using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using ClickNClaim.Setup.DbSetup;

namespace ClickNClaim.Setup
{
    class Program
    {
        private static Exception exception = null;

        static int Main(string[] args)
        {
            var parser = new CommandParse(args);
            var arguments = parser.GetArguments();
            if (arguments.Count == 0)
            {
                TracerConsole.Current.TraceError("Vous devez entrez vos arguments");
                TracerConsole.Current.TraceInformation("/dbenv \"Votre environnement\" par exemple: Dev - Test - Prod. Pour la mise à jour de votre environnement ou la réinstallation d'un environnement\n/dbreinstall Réinstallation de votre environnement");
                TracerConsole.Current.TraceInformation("/wsenv \"Votre environnement pour le webstatic\" Dev - Test - Prod. Pour la mise en place du web Static \n/wsInstall pour reinstaller le web static");
                Console.ReadLine();
                return 1;
            }

          //  StaticWebExec(arguments);
            DbExec(arguments);
            if (!arguments.Keys.Contains("discret"))
            {
                TracerConsole.Current.TraceInformation("Terminé...");
                Console.ReadLine();
            }

            if (exception != null)
                return 1;
            else
                return 0;

        }



        private static void DbExec(Dictionary<string, string> arguments)
        {
            Environment currentEnvironment = Environment.Dev;

            //if (arguments.Keys.Contains("dbrename") && !arguments.Keys.Contains("dbreinstall"))
            //{
            //    TracerConsole.Current.TraceError("/dbrename n'est pas autorisé dans un autre contexte que celui d'un /dbreinstall");
            //    return;
            //}
            
            if (arguments.Keys.Contains("dbenv"))
            {
                if (!Enum.TryParse(arguments["dbenv"], true, out currentEnvironment))
                {
                    TracerConsole.Current.TraceError("Environnement non reconnu: " + arguments["dbenv"]);
                    return;
                }
            }
            else
            {
                TracerConsole.Current.TraceError("Aucun environnement en argument");
                return;
            }

            bool fromScratch = false;
            if (arguments.ContainsKey("dbreinstall"))
                fromScratch = true;

            if (fromScratch)
            {
                if (!arguments.Keys.Contains("discret"))
                {
                    TracerConsole.Current.TraceInformation("Etes-vous certain de vouloir réinstaller la base de données pour l'environnement suivant: " + arguments["dbenv"]);
                    TracerConsole.Current.TraceInformation("[O/N]");


                    string input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        input = input.Trim().ToLowerInvariant();
                        if (!input.Equals("o", StringComparison.InvariantCultureIgnoreCase))
                            return;
                    }
                }
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[currentEnvironment.ToString()];
        
            // Look if DB needs to be rename (only with reinstall mode)
            var dbname = String.Empty;
            if (arguments.Keys.Contains("dbrename"))
                dbname = arguments["dbrename"];
            try
            {
                var dbSetUp = new DbSetUp(connectionStringSettings.ConnectionString, currentEnvironment, dbname);

                if (arguments.Keys.Contains("testdb"))
                    dbSetUp.UpdateStopScriptVersion(int.Parse(arguments["testdb"]));
                
                if (fromScratch)
                {
                    TracerConsole.Current.TraceInformation("Suppression base de données...");
                    dbSetUp.DropDatabase();
                }
                dbSetUp.UpdateDbVersionProd();
                dbSetUp.UpdateDatabase(fromScratch);
            }
            catch (Exception e)
            {
                exception = e;
            }
        }
    }
}
