using System;
using System.Linq;

namespace Bugger.Configuration
{
    public class ApplicationSettings
    {
        public readonly bool Headless;
        public readonly bool Verbose;
        public readonly int CacheSize;
        public readonly bool LoggerDownloadingAttachment;
        public readonly bool LogIntoFile;
        public readonly bool LogIntoConsole;

        private readonly static string[] helpKeywords = new []{ "-help", "-h", "-info", "-i" };
        private const string HeadlessArg = "-hl";
        private const string VerboseArg = "-vb";
        private const string AttachementsArg = "-att";
        private const string CacheSizeArg = "-cs=";
        private const string LogDestinationArg = "-log=";
        private const string TokenArg = "-token=";

        public ApplicationSettings(string [] args)
        {
            if (args.Any(arg => helpKeywords.Contains(arg)))
            {
                Console.WriteLine(
                    "Possible arguments you can provide are:\n" +
                    "-help | -h | -info -i  : shows this help\n" +
                    "-hl                    : run in headless mode (no output to console)\n" +
                    "-vb                    : run with verbose discord logging\n" +
                    "-token=<token>         : run with specific token instead of the saved one in bot configs\n" +
                    "-cs=<number>           : message cache size per channel (defaults to 0)" +
                    "-log=<f | c>           : log into a (f)ile, (c)onsole  or both. Default is console"
                );
                
                // Makes sure the help notice stays up even when
                // running an .exe with this argument.
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (args.Contains(HeadlessArg)) Headless = true;

            if (args.Contains(VerboseArg)) Verbose = true;

            // Cachesize argument handling -cs=<cacheSize>
            if (args.Any(arg => arg.StartsWith(CacheSizeArg)))
            {
                var numberString = GetArgumentContent(args, CacheSizeArg);
                int.TryParse(numberString, out CacheSize);
            }
            else
            {
                CacheSize = 500;
            }

            // Token argument handling -token=YOUR.TOKEN.HERE
            var tokenString = args.FirstOrDefault(arg => arg.StartsWith(TokenArg));
            if (string.IsNullOrWhiteSpace(tokenString) == false)
            {
                BotSettings.config.Token = GetArgumentContent(args, TokenArg);
            }

            // Downloading Attachemnts for Activity Logger -att
            if (args.Contains(AttachementsArg)) LoggerDownloadingAttachment = true;

            // Log output handling -log=<f | c>
            // f = file c = console
            // Default is (c)onsole
            if (args.Any(arg => arg.StartsWith(LogDestinationArg)))
            {
                var options = GetArgumentContent(args, LogDestinationArg);
                
                LogIntoConsole = options.Contains("c");
                LogIntoFile = options.Contains("f");
            }
            else
            {
                LogIntoConsole = true;
            }
        }

        ///<summary>Finds the first matching argument, removes the prefix and returns the rest.</summary>
        private static string GetArgumentContent(string[] args, string prefix)
        {
            return args.FirstOrDefault(arg => arg.StartsWith(prefix))?.Replace(prefix, "");
        }
    }
}
