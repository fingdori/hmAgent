using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
