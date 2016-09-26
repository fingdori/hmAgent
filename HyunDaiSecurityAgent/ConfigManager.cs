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
        // C#에서 constant는 pascal case
        private const string DefaultIp = "0.0.0.0";
        private const int DefaultPort = 514;
        private static String _ip;
        private static int _port;
        private static String _configFilePath = AppDomain.CurrentDomain.BaseDirectory + "conf" + Path.DirectorySeparatorChar + "config.xml";
        static EventLog _localLog = LogManager.getLocalLog();
        static String defaultConfigXmlString = "<config>\r\n<ip>\r\n0.0.0.0\r\n</ip>\r\n<port>\r\n0\r\n</port>\r\n</config>"; 

        public static void initConf() {
#if DEBUG
            _configFilePath = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo di = new DirectoryInfo(_configFilePath);
            DirectoryInfo diParent = di.Parent.Parent.Parent;
            _configFilePath = diParent.FullName + Path.DirectorySeparatorChar + "conf" + Path.DirectorySeparatorChar + "config_debug.xml";
#endif
            
            XmlDocument xd = new XmlDocument();
            // read xml file
            // C#에서는 xml파일을 이용해서 data를 read/write하는 것이 기본적인 방식임 (System.Xml)
            XmlNodeList xmlNodeListIp;
            XmlNodeList xmlNodeListPort;

            if (System.IO.File.Exists(_configFilePath)) {
                try
                {
                    xd.Load(_configFilePath);
                    xmlNodeListIp = xd.GetElementsByTagName("ip");
                    xmlNodeListPort = xd.GetElementsByTagName("port");

                    //  validation check
                    if (xmlNodeListIp.Count == 1 && xmlNodeListPort.Count == 1)
                    {
                        _ip = xmlNodeListIp[0].InnerText.Replace("\r\n", "").Trim();
                        _port = Int32.Parse(xmlNodeListPort[0].InnerText.Replace("\r\n", "").Trim());
                    }
                    else
                    {
                        // error log write
                        _localLog.WriteEntry("config xml element count error (# of ip element : " + xmlNodeListIp.Count
                            + " # of port element : " + xmlNodeListPort.Count + ")", EventLogEntryType.Error);
                    }
                }
                catch (Exception xmle) {
                    _localLog.WriteEntry("xml parsing exception occur!!! ip and port will be default setting : " 
                        + xmle.ToString(), EventLogEntryType.Error);
                    // default setting
                    setDefaultValue();
                }
                
            } else {
                StreamWriter sw = File.CreateText(_configFilePath);
                sw.WriteLine(defaultConfigXmlString);
                sw.Flush();
                // setting default value
                _localLog.WriteEntry("no config file in file path : " + _configFilePath 
                    + "ip and port will be default setting(ip: 0.0.0.0, port: 0)", EventLogEntryType.Error);
                // default setting
                setDefaultValue();
            }
        }

        private static void setDefaultValue() {
            _ip = DefaultIp;
            _port = DefaultPort;
        }

        public static String getIp() {
            return _ip;
        }

        public static int getPort() {
            return _port;
        }

    }
}
