using System;
using System.IO;

namespace avenabot.Log
{
    static class Logger
    {
        private static readonly string logDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\Log\\log.txt";
        private static readonly StreamWriter File = new StreamWriter(logDirectory, true);
        internal static void Log(string log)
        {
            Console.Write(DateTime.Now + ":");
            Console.WriteLine(log);
            File.Write(DateTime.Now + ":");
            File.WriteLine(log);
        }

        internal static void Dispose()
        {
            File.Flush();
            File.Dispose();
        }
    }
}
