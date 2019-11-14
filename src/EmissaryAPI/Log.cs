using System;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace EmissaryApi
{
    public class Log
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void SetNLogConfig()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("target1")
            {
                Layout = @"${date:format=yyy-MM-dd HH\:mm\:ss} ${level} ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);
            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/file.txt",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForOneLevel(LogLevel.Error, fileTarget); // only errors to file
            config.AddRuleForAllLevels(consoleTarget); // all to console

            LogManager.Configuration = config;
        }

    }
}
