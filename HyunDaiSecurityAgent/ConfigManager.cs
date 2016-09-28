using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace HyunDaiSecurityAgent
{
    class ConfigManager
    {
        // C#에서 constant는 pascal case
        private const string DefaultIp = "0.0.0.0";
        private const int DefaultPort = 514;
        private const string DefaultConfigXmlString = "<config>\r\n<ip>\r\n0.0.0.0\r\n</ip>\r\n<port>\r\n0\r\n</port>\r\n</config>";
        // private variable은 underscore를 prefix로 씀 (debugging 시 변수 위치가 top에 있음)
        private static String _ip;
        private static int _port;
        private static String _configFilePath = AppDomain.CurrentDomain.BaseDirectory + "conf" + Path.DirectorySeparatorChar + "config.xml";
        private static EventLog _localLog = LogManager.getLocalLog();

        public static void initConf() {
#if DEBUG
            // config file change 이벤트 시 UNC path는 읽어들이지 못함
            _configFilePath = "c:\\conf\\config_debug.xml";
#endif
            
            XmlDocument xd = new XmlDocument();
            // C#에서는 xml파일을 이용해서 data를 read/write하는 것이 기본적인 방식임 (System.Xml 라이브러리가 잘 되어 있음)
            XmlNodeList xmlNodeListIp;
            XmlNodeList xmlNodeListPort;

            if (File.Exists(_configFilePath)) {
                try
                {
                    // wait file is readable
                    while (Utils.IsFileLocked(new FileInfo(_configFilePath)))
                    {
                        // do something, for example wait a second
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    }

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
                sw.WriteLine(DefaultConfigXmlString);
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

        public static String getConfigFilePath() {
            return _configFilePath;
        }
    }
}
