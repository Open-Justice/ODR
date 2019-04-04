// -----------------------------------------------------------------------
// <copyright file="CommandParse.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ClickNClaim.Setup
{
    using System.Collections.Generic;

    /// <summary>
    /// Parsing command line arguments
    /// </summary>
    public class CommandParse
    {
        private readonly string[] _args;

        public CommandParse(string[] args)
        {
            _args = args;
        }

        public Dictionary<string, string> GetArguments()
        {
            const string commandPrefix = "/";
            var commandDictionary = new Dictionary<string, string>();
            for (int i = 0; i < _args.Length; i++)
            {
                if (_args[i].StartsWith(commandPrefix))
                {
                    string key = _args[i].Substring(commandPrefix.Length, _args[i].Length - 1);
                    string val = string.Empty;
                    if (i + 1 < _args.Length)
                    {
                        if (!_args[i + 1].StartsWith(commandPrefix))
                        {
                            val = _args[i + 1];
                        }
                    }
                    commandDictionary.Add(key, val);
                }
            }
            return commandDictionary;
        }
    }
}
