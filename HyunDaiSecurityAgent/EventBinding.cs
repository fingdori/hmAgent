using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Xml;

namespace HyunDaiSecurityAgent
{
    class EventBinding
    {
        private static UTF8Encoding _encoding = new UTF8Encoding();
        private static EventLog _localLog = LogManager.getLocalLog();
        private static FileSystemWatcher fileSystemWater = new FileSystemWatcher();
        // 4624는 로그온, 4634는 로그오프
        private const string LogName = "Security";
        private const string QueryString = "*[System[(EventID = 4624 or EventID = 4634)]]";
       
        // config file change event handler
        private static void OnChangeConfigFile(object source, FileSystemEventArgs s)
        {
            ConfigManager.initConf();
        }

        // event binding main function
        public void Run()
        {
            EventLogWatcher watcher = null;
            try
            {
                ConfigManager.initConf();

                // configFileChange Event watcher
                fileSystemWater.Path = Path.GetDirectoryName(ConfigManager.getConfigFilePath());
                fileSystemWater.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                fileSystemWater.Changed += OnChangeConfigFile;
                fileSystemWater.Filter = ("*.xml");
                fileSystemWater.EnableRaisingEvents = true;

                EventLogQuery eventsQuery = new EventLogQuery(LogName,
                    PathType.LogName, QueryString);

                watcher = new EventLogWatcher(eventsQuery);

                watcher.EventRecordWritten +=
                        new EventHandler<EventRecordWrittenEventArgs>(
                            EventLogCallback);

                // Begin subscribing to events the events
                watcher.Enabled = true;

                // network address change event handler
                NetworkChange.NetworkAddressChanged += new
                NetworkAddressChangedEventHandler(AddressChangedCallback);

                // Wait for events to occur. 
                Thread.Sleep(Timeout.Infinite);

            }
            catch (Exception e)
            {
                _localLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                // Stop listening to events
                watcher.Enabled = false;

                if (watcher != null)
                {
                    watcher.Dispose();
                }
            }
        }

        // 로그온/오프 이벤트 핸들러
        static void EventLogCallback(object obj,
            EventRecordWrittenEventArgs arg)
        {
            if (arg.EventRecord != null)
            {
                String xmlString = arg.EventRecord.ToXml();
                String resultString;

                try
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(xmlString);
                    LogOnOffMessageManager logonMessage = new LogOnOffMessageManager(" ");
                    resultString = logonMessage.makeMessage(xmlString);
                    String logType = resultString.Split(new String[] {"|"}, StringSplitOptions.None)[1];

                    if (logType.Equals(LogOnOffMessageManager.LogTypeLogOn)) {
                        SyslogManager.portableSyslogUdpSend(ConfigManager.getIp(), ConfigManager.getLogOnPort(), resultString);
                    } else {
                        SyslogManager.portableSyslogUdpSend(ConfigManager.getIp(), ConfigManager.getLogOffPort(), resultString);
                    }                      
                }
                catch (Exception e) {
                    _localLog.WriteEntry("Log on off message make error \r\n" + e.ToString(), EventLogEntryType.Error);
                }
            }
            else
            {
                _localLog.WriteEntry("Event Record is Null", EventLogEntryType.Error);
            }
        }

        // IP 변경에 대한 이벤트 핸들러
        static void AddressChangedCallback(object sender, EventArgs e)
        {
            String xmlString = "<event><description>network interface change event occur</description>";
            byte[] data = _encoding.GetBytes(xmlString);
            NetworkInterface[] nts = NetworkInterface.GetAllNetworkInterfaces();
            DateTime now = DateTime.UtcNow;
            // event Log Time Format
            string currentTimeString = now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff00K",
                                    CultureInfo.InvariantCulture);
            String ipAddress = "";
            String MacAddress = "";
            String hostName = Dns.GetHostName();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            if (!ipAddress.Equals("")) {
                                ipAddress += ",";
                            }
                            ipAddress += ip.Address.ToString();

                            if (!MacAddress.Equals(""))
                            {
                                MacAddress += ",";
                            }
                            MacAddress += ni.GetPhysicalAddress().ToString();
                        }
                    }
                }
            }

            xmlString += "<ip>" + ipAddress + "</ip>";
            xmlString += "<mac>" + MacAddress + "</mac>";
            xmlString += "<hostname>" + hostName + "</hostname>";
            xmlString += "<currentSystemTime>" + currentTimeString + "</currentSystemTime>";
            xmlString += "</event>";

            // xml을 안만들고 바로 message 만들기도 가능
            IpChangeMessageManager ipChangeMessageManager = new IpChangeMessageManager(" ");
            String resultString = ipChangeMessageManager.makeMessage(xmlString);
            SyslogManager.portableSyslogUdpSend(ConfigManager.getIp(), ConfigManager.getIpAddressChangePort(), resultString);
        }
    }  
}
