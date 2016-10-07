using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace HyunDaiSecurityAgent
{
    class ConfigManager
    {
        // C#에서 constant는 pascal case
        private const string DefaultIp = "0.0.0.0";
        private const int DefaultPort = 514;
        private const string DefaultConfigXmlString = "<config>\r\n<server-ip>\r\n0.0.0.0\r\n</server-ip>\r\n<log-on-log-port>\r\n514\r\n</log-on-log-port>"
            + "\r\n<log-off-log-port>\r\n514\r\n</log-off-log-port>\r\n<ip-change-log-port>\r\n514\r\n</ip-change-log-port></config>";
        // private variable은 underscore를 prefix로 씀 (debugging 시 변수 위치가 top에 있음)

        private static String _ip;
        private static int _logOnPort;
        private static int _logOffPort;
        private static int _ipAddressChangePort;
        private static String _configFilePath = AppDomain.CurrentDomain.BaseDirectory + "conf" + Path.DirectorySeparatorChar + "config.xml";
        private static EventLog _localLog = LogManager.getLocalLog();
        private static String _uuid;

        public static String Uuid
        {
            get
            {
                return _uuid;
            }

            set
            {
                _uuid = value;
            }
        }

        public static void initConf() {
#if DEBUG
            // config file change 이벤트 시 UNC path는 읽어들이지 못함
            _configFilePath = "c:\\conf\\config_debug.xml";
#endif         
            XmlDocument xd = new XmlDocument();
            // C#에서는 xml파일을 이용해서 data를 read/write하는 것이 기본적인 방식임 (System.Xml 라이브러리가 잘 되어 있음)
            XmlNode xmlNodeIp;
            XmlNode xmlNodeLogOnPort;
            XmlNode xmlNodeLogOffPort;
            XmlNode xmlNodeIpAddressChangePort;

            if (Uuid == null) {
                Uuid = RegManager.getUuidFromRegistry();
            }

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
                    // set xpath query
                    xmlNodeIp = xd.SelectSingleNode("/config/server-ip[1]");
                    xmlNodeLogOnPort = xd.SelectSingleNode("/config/ports/log-on-log-port[1]");
                    xmlNodeLogOffPort = xd.SelectSingleNode("/config/ports/log-off-log-port[1]");
                    xmlNodeIpAddressChangePort = xd.SelectSingleNode("/config/ports/ip-change-log-port[1]");

                    //  validation check
                    if (xmlNodeIp != null && xmlNodeLogOnPort != null && xmlNodeLogOffPort != null && xmlNodeIpAddressChangePort != null)
                    {
                        _ip = xmlNodeIp.InnerText.Replace("\r\n", "").Trim();
                        _logOnPort = Int32.Parse(xmlNodeLogOnPort.InnerText.Replace("\r\n", "").Trim());
                        _logOffPort = Int32.Parse(xmlNodeLogOffPort.InnerText.Replace("\r\n", "").Trim());
                        _ipAddressChangePort = Int32.Parse(xmlNodeIpAddressChangePort.InnerText.Replace("\r\n", "").Trim());
                    }
                    else
                    {
                        // error log write
                        _localLog.WriteEntry("config xml element count error (must contain field" +
                            "/config/server-ip, /config/ports/log-on-log-port, /config/ports/log-out-log-port" +
                            "/config/ports/ip-change-log-port)", EventLogEntryType.Error);
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
            _logOnPort = DefaultPort;
            _logOffPort = DefaultPort;
            _ipAddressChangePort = DefaultPort;
        }

        public static String getIp() {
            return _ip;
        }

        public static int getLogOnPort()
        {
            return _logOnPort;
        }
        public static int getLogOffPort()
        {
            return _logOffPort;
        }
        public static int getIpAddressChangePort()
        {
            return _ipAddressChangePort;
        }
        public static String getConfigFilePath() {
            return _configFilePath;
        }
    }
}
