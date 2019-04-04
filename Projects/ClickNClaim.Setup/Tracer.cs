// -----------------------------------------------------------------------
// <copyright file="Tracer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ClickNClaim.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class TracerConsole : ITracer
    {
        private static readonly TracerConsole Instance = new TracerConsole();

        public static TracerConsole Current
        {
            get { return Instance; }
        }

        public void TraceInformation(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }

        public void TraceError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }


        public void TraceWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
        }
    }

    public interface ITracer
    {
        void TraceInformation(string message);
        void TraceError(string message);
        void TraceWarning(string message);
    }
}
