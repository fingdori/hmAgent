using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HyunDaiSecurityAgent
{
    class ConfigManager
    {
        private static String _ip;
        private static int _port;
        private static String _configFilePath = AppDomain.CurrentDomain.BaseDirectory + "conf" + Path.DirectorySeparatorChar + "config.xml";
        static EventLog _localLog = LogManager.getLocalLog();

        public static void initConf() {
#if DEBUG
            _configFilePath = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo di = new DirectoryInfo(_configFilePath);
            DirectoryInfo diParent = di.Parent.Parent.Parent;
            _configFilePath = diParent.FullName + Path.DirectorySeparatorChar + "conf" + Path.DirectorySeparatorChar + "config.xml";
#endif
            
            XmlDocument xd = new XmlDocument();
            // read xml file
            // C#에서는 xml파일을 이용해서 data를 read/write하는 것이 기본적인 방식임 (System.Xml)
            if (System.IO.File.Exists(_configFilePath)) {
                xd.Load(_configFilePath);
                XmlNodeList xmlNodeListIp = xd.GetElementsByTagName("ip");
                XmlNodeList xmlNodeListPort = xd.GetElementsByTagName("port");
                //  validation check
                if (xmlNodeListIp.Count == 1 && xmlNodeListPort.Count == 1) {
                    _ip = xmlNodeListIp[0].InnerText.Replace("\r\n", "").Trim();
                    _port = Int32.Parse(xmlNodeListPort[0].InnerText.Replace("\r\n", "").Trim());
                }
            } else {
                System.IO.File.Create(_configFilePath);
                // setting default value
                _localLog.WriteEntry("no config file in file path : " + _configFilePath, EventLogEntryType.Error);
            }
        }

        public static String getIp() {
            return _ip;
        }

        public static int getPort() {
            return _port;
        }

    }
}
