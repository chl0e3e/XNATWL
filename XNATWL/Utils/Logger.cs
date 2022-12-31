using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class Logger
    {
        public enum Level
        {
            SEVERE,
            INFO,
            WARNING
        }

        public static Dictionary<string, Logger> LOGGERS = new Dictionary<string, Logger>();

        public static Logger GetLogger(Type type)
        {
            if (LOGGERS.ContainsKey(type.FullName))
            {
                return LOGGERS[type.FullName];
            }

            return LOGGERS[type.FullName] = new Logger(type.FullName);
        }

        private string _name;

        public Logger(string name)
        {
            this._name = name;
        }

        public void log(Level level, string message, Exception e)
        {
            this.log(level, message + ", error: " + e.ToString());
            
        }

        public void log(Level level, string message)
        {
            System.Diagnostics.Debug.WriteLine("[" + this._name + "] " + message);
        }
    }
}
