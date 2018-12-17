using System;
using System.IO;

namespace Bugger.Features
{
    public static class BlackBox
    {
        public static void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            File.WriteAllText($"BlackBox-from-{DateTime.Now:MM-dd-yyyy-HH;mm;ss}.log", exception.ToString());
            Environment.Exit(1);
        }
    }
}
