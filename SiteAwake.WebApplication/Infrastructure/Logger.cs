using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;

namespace SiteAwake.WebApplication.Infrastructure
{
    public static class Logger
    {        
        public enum EventType
        {
            Debug,
            Error,
            Fatal,
            Info,
            Warning            
        }

        /// <summary>
        /// Logs the specified declaring type.
        /// </summary>
        /// <param name="declaringType">Type of the declaring.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Log(Type declaringType, EventType eventType, object message, Exception exception)
        {
            ILog Logger = LogManager.GetLogger(declaringType);

            switch (eventType)
            {
                case EventType.Debug:
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(message, exception);
                    }
                    break;
                case EventType.Error:
                    if (Logger.IsErrorEnabled)
                    {
                        Logger.Error(message, exception);
                    }
                    break;
                case EventType.Fatal:
                    if (Logger.IsFatalEnabled)
                    {
                        Logger.Fatal(message, exception);
                    }
                    break;
                case EventType.Info:
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(message, exception);
                    }
                    break;
                case EventType.Warning:
                    if (Logger.IsWarnEnabled)
                    {
                        Logger.Warn(message, exception);
                    }
                    break;
                default:
                    if (Logger.IsErrorEnabled)
                    {
                        Logger.Error(message, exception);
                    }
                    break;
            }            
        }
    }
}