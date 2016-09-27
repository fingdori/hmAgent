using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HyunDaiSecurityAgent
{
    class EventBinding
    {
        static UdpClient _udpClient;
        static String host;
        static int port;
        static UTF8Encoding _encoding = new UTF8Encoding();
        static EventLog _localLog = LogManager.getLocalLog();

        public void Run()
        {
            String logName = "Security";
            // 4624는 로그온, 4634는 로그오프
            String queryString = "*[System[(EventID = 4624 or EventID = 4634)]]";
            EventLogWatcher watcher = null;

            try
            {
                ConfigManager.initConf();
                host = ConfigManager.getIp();
                port = ConfigManager.getPort();

                _localLog.WriteEntry("start HyunDae Application!!!", EventLogEntryType.Information);
                //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "startHyunDai.txt", "hyundae app start!!");
                EventLogQuery eventsQuery = new EventLogQuery(logName,
                    PathType.LogName, queryString);

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
            catch (EventLogReadingException e)
            {
                _localLog.WriteEntry(e.ToString(), EventLogEntryType.Error);               
            }
            catch (UnauthorizedAccessException uae)
            {
                _localLog.WriteEntry(uae.ToString(), EventLogEntryType.Error);
            }
            catch (Exception ee)
            {
                _localLog.WriteEntry(ee.ToString(), EventLogEntryType.Error);
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
                byte[] data = _encoding.GetBytes(xmlString);
                portableSyslogUdpSend(host, port, xmlString);
            }
            else
            {
                Console.WriteLine("The event instance was null.");
            }
        }

        // IP 변경에 대한 이벤트 핸들러
        static void AddressChangedCallback(object sender, EventArgs e)
        {
            String xmlString = "<event><description>network interface change event occur</description>";
            byte[] data = _encoding.GetBytes(xmlString);
            NetworkInterface[] nts = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            xmlString += "<ip>" + ip.Address.ToString() + "</ip>";
                            xmlString += "<mac>" + ni.GetPhysicalAddress().ToString() + "</mac>";
                            xmlString += "</event>";
                        }
                    }
                }
            }
            portableSyslogUdpSend(host, port, xmlString);
        }

        // syslog 형식에 맞춰서 메시지를 만든 후 udp로 asyncronous 하게 전송
        static async void portableSyslogUdpSend(String host, int port, String msg)
        {        
            var process = Process.GetCurrentProcess();
            SyslogRfc5424MessageSerializer syslogRfc5424MessageSerializerUtf8 = new SyslogRfc5424MessageSerializer(Encoding.UTF8);
    
            // making syslog(rfc5424, rfc3164) message format
            var message = new SyslogMessage(DateTime.Now,
            facility: Facility.UserLevelMessages,
            severity: Severity.Debug,
            hostName: process.MachineName,
            procId: process.Id.ToString(),
            appName: process.ProcessName,
            msgId: "hyundai motors security log",
            message: msg);

            try
            {
                // asynchronous communication because of performance
                using (var sender = new AsyncSyslogUdpSender(host, port))
                {
                    await sender.ConnectAsync();
                    await sender.SendAsync(message, syslogRfc5424MessageSerializerUtf8);
                    await sender.DisconnectAsync();
                }
            }
            catch (Exception e) {
                _localLog.WriteEntry("upd send error : " + e.ToString(), EventLogEntryType.Error);
            }

        }        
    }
}
