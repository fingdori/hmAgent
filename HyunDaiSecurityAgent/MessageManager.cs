using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiSecurityAgent
{
    public abstract class MessageManager
    {
        private string _delimiter;
        private const String _logTypeDelimeter = "|";
        private const String _logTypeLogOn = "Logon";
        private const String _logTypeLogOff = "Logoff";
        private const String _logTypeIpChange = "IpChanged";

        public static string LogTypeLogOn
        {
            get
            {
                return _logTypeLogOn;
            }
        }

        public static string LogTypeLogOff
        {
            get
            {
                return _logTypeLogOff;
            }
        }


        public static string LogTypeDelimeter
        {
            get
            {
                return _logTypeDelimeter;
            }
        }

        public static string LogTypeIpChange
        {
            get
            {
                return _logTypeIpChange;
            }
        }

        public MessageManager(string delimiter) {
            this._delimiter = delimiter;
        }

        public string getDelemiter() {
            return this._delimiter;
        }

        public abstract string makeMessage(String xmlString);
    }
}
