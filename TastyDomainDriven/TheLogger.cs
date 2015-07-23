namespace TastyDomainDriven
{
    using System;

    public class TheLogger
    {
        private static volatile TheLogger instance;
        private static object syncRoot = new Object();
        //private static log4net.ILog logger = null;

        public static TheLogger Instance()
        {

            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new TheLogger();
                    }
                }
            }

            return instance;
        }

        public Action<string, TheLogLevel> LogMessage;

        public Action<string, Exception, TheLogLevel> LogException;

        public TheLogger()
        {
            LogMessage = (s, level) => { };
            LogException = (s, exception, arg3) => { };
        }

        public TheLogger ToConsole()
        {
            return new TheLogger();
        }

        public void DebugFormat(string msg, params object[] args)
        {
            LogMessage(string.Format(msg, args), TheLogLevel.Debug);
        }

        public void Debug(string msg, Exception ex = null)
        {
            if (ex == null)
            {
                LogMessage(msg, TheLogLevel.Debug);
            }
            else
            {
                LogException(msg, ex, TheLogLevel.Debug);
            }
        }

        public void InfoFormat(string msg, params object[] args)
        {
            LogMessage(string.Format(msg, args), TheLogLevel.Info);
        }

        public void Info(string msg, Exception ex = null)
        {
            if (ex == null)
            {
                LogMessage(msg, TheLogLevel.Info);

            }
            else
            {
                LogException(msg, ex, TheLogLevel.Info);
            }
        }

        public void WarnFormat(string msg, params object[] args)
        {
            LogMessage(string.Format(msg, args), TheLogLevel.Warn);
        }
    }

    public enum TheLogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
    }
}