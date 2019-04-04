// -----------------------------------------------------------------------
// <copyright file="DbRun.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace ClickNClaim.Setup.DbSetup
{
    using System;
    using System.Data.SqlClient;
    using Microsoft.SqlServer.Management.Smo;
    using Microsoft.SqlServer.Management.Common;
    using System.Data;
using System.Configuration;

    /// <summary>
    /// Run Command into Sql Server
    /// </summary>
    public class DbRun
    {
        public static string DbName = ConfigurationManager.AppSettings["DbName"];
        private string GetDbVersionCmd = "USE [" + DbName + "] SELECT name, value FROM fn_listextendedproperty(default, default, default, default, default, default, default)";

        private string _dropDbCmd = "USE [master] IF EXISTS (SELECT name FROM sys.databases WHERE name = N'" + DbName + "') BEGIN EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'" + DbName + "' ALTER DATABASE [" + DbName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE ALTER DATABASE [" + DbName + "] SET SINGLE_USER DROP DATABASE [" + DbName + "] END";
        private string DropDbCmd
        {
            get
            {
                return _dropDbCmd;
            }
        }
        private string CreateDbVersionProdCmd = "USE [" + DbName + "] EXEC sp_addextendedproperty @name = N'DbVersion', @value ='3'";

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="DbRun"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DbRun(string connectionString, string dbName)
        {
            ConnectionString = connectionString;
            DbName = DbName;
        }

        /// <summary>
        /// Drops the database.
        /// </summary>
        public void DropDb()
        {
            RunCommand(DropDbCmd);
        }

        public void UpdateDbName(string dbName)
        {
            DbName = dbName;
        }

        internal void SetDbVersionProd()
        {
            RunCommand(CreateDbVersionProdCmd);
        }

        /// <summary>
        /// Runs Sql command
        /// </summary>
        /// <param name="commandText">The command text.</param>
        public void RunCommand(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                TracerConsole.Current.TraceError("CommandText null");
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Connection.Open();
                    var server = new Server(new ServerConnection(connection));
                    server.ConnectionContext.ExecuteNonQuery(commandText);
                }
            }
        }

        public void RunCommandTransaction(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                TracerConsole.Current.TraceError("CommandText null");
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var server = new Server(new ServerConnection(connection));
                try
                {
                    
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = CommandType.Text;
                        server.ConnectionContext.BeginTransaction();
                        server.ConnectionContext.ExecuteNonQuery(commandText);
                        server.ConnectionContext.CommitTransaction();
                    }
                }
                catch (SqlException)
                {
                    server.ConnectionContext.RollBackTransaction();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the db version.
        /// </summary>
        public int DbVersion
        {
            get
            {
                int version = 0;
                using (var connection = new SqlConnection(ConnectionString))
                {
                    using (var command = new SqlCommand(GetDbVersionCmd, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Connection.Open();
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            string name = (string)reader["name"];
                            string value = (string)reader["value"];
                            if (name.ToLowerInvariant().Equals("dbversion", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (int.TryParse(value, out version))
                                    return version;
                            }
                        }
                    }
                }
                return version;
            }
        }
    }
}
