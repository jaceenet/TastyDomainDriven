namespace TastyDomainDriven
{
    using System;

    public static class TheLogManager
    {
        public static Func<string, TheLogger> ResolveLogger;

        public static TheLogger GetLogger(Type type)
        {
            if (ResolveLogger != null)
            {
                return ResolveLogger(type.ToString());
            }

            return NoLogger;
        }

        private static readonly TheLogger NoLogger = new TheLogger();

        public static void LogToConsole()
        {
            ResolveLogger = delegate(string loggername)
            {
                var console = new TheLogger();
                console.LogMessage = (s1, level) => Console.WriteLine(string.Format("{0:hh:mm:ss.FFFFF} : [{1}] ({2}) - {3}", DateTime.Now, loggername, level, s1));
                console.LogException = (s1, ex, level) => Console.WriteLine(string.Format("{0:hh:mm:ss.FFFFF} : [{1}] ({2}) - {3} {4}", DateTime.Now, loggername, level, s1, ex.ToString()));
                return console;
            };
        }
    }
}