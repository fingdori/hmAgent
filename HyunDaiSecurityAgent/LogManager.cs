using System.Diagnostics;

namespace HyunDaiSecurityAgent
{
    class LogManager
    {
        private static EventLog _localLog = new EventLog("Application", ".", "HyunDai Log Agent");

        public static EventLog getLocalLog() {
            return _localLog;
        }
    }
}
