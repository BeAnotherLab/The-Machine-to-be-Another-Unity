using UnityEngine;

namespace Uduino
{
    public static class Log 
    {
        private static LogLevel _debugLevel;

        static Log()
        {
        }

        public static void Error(object message, bool removeNewLines = false)
        {
            if (removeNewLines) message.ToString().RemoveLineEndings();

            if ((int)_debugLevel <= (int)LogLevel.Error && (int)_debugLevel != 0)
                UnityEngine.Debug.LogError(message);
        }

        public static void Warning(object message, bool removeNewLines = false)
        {
            if (removeNewLines) message.ToString().RemoveLineEndings();

            if ((int)_debugLevel <= (int)LogLevel.Warning && (int)_debugLevel != 0)
                UnityEngine.Debug.LogWarning(message);
        }

        public static void Info(object message,  bool removeNewLines = false)
        {
            if (removeNewLines) message.ToString().RemoveLineEndings();

            if ((int)_debugLevel <= (int)LogLevel.Info && (int)_debugLevel != 0)
              UnityEngine.Debug.Log(((string)message).RemoveLineEndings());
        }

        public static string TrimStartString(string sourceString, char[]  trimed)
        {
            sourceString = sourceString.TrimStart(trimed);
            return sourceString;
        }

        public static void Debug(object message, bool removeNewLines = false)
        {
            if (removeNewLines) message.ToString().RemoveLineEndings();
            if ((int)_debugLevel <= (int)LogLevel.Debug && (int)_debugLevel !=0)
                UnityEngine.Debug.Log(message);
        }

        public static void SetLogLevel(LogLevel level)
        {
            _debugLevel = level;
        }

        public static string RemoveLineEndings(this string value)
        {
            if (System.String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);
        }
    }

}
