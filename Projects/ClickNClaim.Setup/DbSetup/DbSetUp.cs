// -----------------------------------------------------------------------
// <copyright file="DbSetUp.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace ClickNClaim.Setup.DbSetup
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.SqlServer.Management.Common;
    using System.Data.SqlClient;
    using ClickNClaim.Setup;
    using System.Text;

    /// <summary>
    /// Setup for database.
    /// </summary>
    public class DbSetUp
    {
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// Gets the current environment.
        /// </summary>
        public ClickNClaim.Setup.Environment CurrentEnvironment { get; private set; }

        public int StopScriptVersion { get; set; }
        public string DbName { get; set; }

        public DbSetUp(string connectionString, ClickNClaim.Setup.Environment environment, string dbName)
        {
            ConnectionString = connectionString;
            CurrentEnvironment = environment;
            DbName = dbName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSetUp"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="environment">The environment.</param>
        public DbSetUp(string connectionString, ClickNClaim.Setup.Environment environment)
        {
            ConnectionString = connectionString;
            CurrentEnvironment = environment;
        }

        /// <summary>
        /// Drops the database.
        /// </summary>
        public void DropDatabase()
        {
            var dbRun = new DbRun(ConnectionString,DbName);
            if (!String.IsNullOrEmpty(DbName))
                dbRun.UpdateDbName(DbName);
            dbRun.DropDb();
        }

        /// <summary>
        /// Updates the database.
        /// </summary>
        /// <param name="fromScratch">if set to <c>true</c> [from scratch].</param>
        public void UpdateDatabase(bool fromScratch = false)
        {
            var directoryInfo = new DirectoryInfo(this.GetType().Assembly.Location.Replace("Agantio.Setup.exe", "Scripts\\"));
            var dbRun = new DbRun(ConnectionString, DbName);
            var files = directoryInfo.GetFileSystemInfos();
        
            if (fromScratch == false)
            {
                int dbVersion = dbRun.DbVersion + 1;
                files = files.Where(f => Filter(f.Name, dbVersion)).ToArray();
            }
            else
            {
                files = files.Where(f => Filter(f.Name, 0)).ToArray();
            }
          
            var filteredFiles = files.OrderBy(f => OrderedName(f.Name));
         
            foreach (var fileInfo in filteredFiles)
            {
                string[] actions;
                CheckFilename(fileInfo.FullName, out actions);
                if (StopScriptVersion > 0 && int.Parse(actions[0].Split('\\').Last().Replace("v","")) > StopScriptVersion)
                    return;
                var result = CanRunScript(fileInfo.Name);
                if (!result.Item1)
                {
                    if (result.Item2 == ScriptAction.BadEnvironment)
                        continue;

                    if (result.Item2 == ScriptAction.FilenameError)
                    {
                        TracerConsole.Current.TraceError("Mauvais nom de fichier. Ne peut executer le script. Arrêt de la mise à jour de la base.");
                        break;
                    }
                }

                TracerConsole.Current.TraceInformation("En cours de traitement: " + fileInfo.Name);
                using (var file = File.OpenRead(Path.Combine(directoryInfo.FullName, fileInfo.FullName)))
                {
                    using (var sr = new StreamReader(file, Encoding.Default))
                    {
                        try
                        {
                            string script = sr.ReadToEnd();
                            if (!String.IsNullOrEmpty(DbName))
                            {
                               script = script.Replace("AgantioDb", DbName);
                            }

                            if (fileInfo.Name.StartsWith("v1_")) //create table not supported by transaction
                                dbRun.RunCommand(script);
                            else
                                dbRun.RunCommandTransaction(script);
                        }
                        catch (ExecutionFailureException e)
                        {
                            var sqlException = e.InnerException as SqlException;
                            if (sqlException != null)
                            {
                                if (sqlException.Number == 2714) //number used for RaisError
                                {
                                    TracerConsole.Current.TraceWarning(fileInfo.FullName + "\n" + "DbVersion ne possède pas la bonne version pour executer le script\nDbVersion:" + dbRun.DbVersion);
                                    throw e;
                                }
                            }
                            TracerConsole.Current.TraceError(fileInfo.FullName);
                            WriteAllNestedError(e);
                            throw e;
                        }
                        catch (Exception e)
                        {
                            TracerConsole.Current.TraceError(fileInfo.FullName);
                            WriteAllNestedError(e);
                            throw e;
                        }
                    }
                }
            }
        }

        public void UpdateStopScriptVersion(int scriptVersion)
        {
            StopScriptVersion = scriptVersion;
        }

        public void UpdateDbVersionProd()
        {
            if (CurrentEnvironment != ClickNClaim.Setup.Environment.Prod)
                return;

            var dbRun = new DbRun(ConnectionString, DbName);
            if (dbRun.DbVersion == 0)
                dbRun.SetDbVersionProd();
        }

        private Tuple<bool, ScriptAction> CanRunScript(string scriptName)
        {
            string[] actions;
            if (!CheckFilename(scriptName, out actions))
                return new Tuple<bool, ScriptAction>(false, ScriptAction.FilenameError);

            string env = actions[1].ToLowerInvariant();
            if (env.Equals(ClickNClaim.Setup.Environment.All.ToString().ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase))
                return new Tuple<bool, ScriptAction>(true, ScriptAction.NoError);

            if (env.Equals(CurrentEnvironment.ToString().ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase))
                return new Tuple<bool, ScriptAction>(true, ScriptAction.NoError);

            return new Tuple<bool, ScriptAction>(false, ScriptAction.BadEnvironment);
        }

        private static void WriteAllNestedError(Exception exception)
        {
            Exception e = exception;
            while (e.InnerException != null)
            {
                TracerConsole.Current.TraceError("Source:" + e.InnerException.Source + "\n" + "Message: " + e.InnerException.Message);
                e = exception.InnerException;
            }
        }

        private static bool Filter(string filename, int dbVersion)
        {
            string[] actions;
            if (!CheckFilename(filename, out actions))
                return false;

            string nb = actions[0];
            if (string.IsNullOrWhiteSpace(nb))
                return false;

            nb = nb.Replace("v", "");
            int scriptNumber;
            if (!int.TryParse(nb, out scriptNumber))
                return false;

            if (scriptNumber >= dbVersion)
                return true;
            return false;
        }

        /// <summary>
        /// Checks the filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="actions">The actions.</param>
        /// <returns></returns>
        private static bool CheckFilename(string filename, out string[] actions)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                actions = new string[] { };
                return false;
            }

            actions = filename.Split('_');
            if (actions.Length < 4)
            {
                TracerConsole.Current.TraceError("Nom de fichier non conforme. Il ne sera pas traité.");
                return false;
            }
            return true;
        }

        private int OrderedName(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return 0;

            string[] actions;
            if (!CheckFilename(filename, out actions))
                return 0;

            string nb = actions.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(nb))
                return 0;

            nb = nb.Replace("v", "");
            int scriptNumber;
            if (!int.TryParse(nb, out scriptNumber))
                return 0;
            return scriptNumber;
        }

        private enum ScriptAction
        {
            NoError = 0,
            FilenameError = 1,
            BadEnvironment = 2
        }
    }
}
