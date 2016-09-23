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
        static String host = "192.168.1.200";
        static int port = 514;
        static UTF8Encoding _encoding = new UTF8Encoding();
        static EventLog _localLog = new EventLog("Application", ".", "HyunDai Log Agent");

        public void Run()
        {
            String logName = "Security";
            // 4624는 로그온, 4634는 로그오프
            String queryString = "*[System[(EventID = 4624 or EventID = 4634)]]";
            EventLogWatcher watcher = null;

            try
            {
                
                _localLog.WriteEntry("start HyunDae Application!!!", EventLogEntryType.Information);
                //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "startHyunDai.txt", "hyundae app start!!");
                EventLogQuery eventsQuery = new EventLogQuery(logName,
                    PathType.LogName, queryString);

                watcher = new EventLogWatcher(eventsQuery);

                watcher.EventRecordWritten +=
                        new EventHandler<EventRecordWrittenEventArgs>(
                            EventLogEventRead);

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
                Console.WriteLine("Error reading the log: {0}", e.Message);
                _localLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
                System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "startHyunDai.txt", "hyundae app 11111!!");
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("UnauthorizedAccessException occur log: {0}", uae.Message);
                _localLog.WriteEntry(uae.ToString(), EventLogEntryType.Error);
                System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "startHyunDai.txt", "hyundae app 2222222!!");
            }
            catch (Exception ee)
            {
                Console.WriteLine("error : " + ee.ToString());
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
        static void EventLogEventRead(object obj,
            EventRecordWrittenEventArgs arg)
        {
            if (arg.EventRecord != null)
            {
                String xmlString = arg.EventRecord.ToXml();
                Console.WriteLine("==================================== Log on/off Event Occur ======================================");
                Console.WriteLine("Event XML : {0}", xmlString);
                byte[] data = _encoding.GetBytes(xmlString);

                //sendUdp(host, port, data);
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
            Console.WriteLine("==================================== Network Change Event Occur ======================================");
            String xmlString = "<data>network interface change event occur</data>";
            byte[] data = _encoding.GetBytes(xmlString);
            //sendUdp(host, port, data);
            portableSyslogUdpSend(host, port, xmlString);

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
                            Console.WriteLine(ip.Address.ToString());
                            Console.WriteLine(ni.GetPhysicalAddress().ToString());
                        }
                    }
                }
            }
        }

        public static void testfunc(Exception e)
        {
            if (e != null)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static async void portableSyslogUdpSend(String host, int port, String msg)
        {
            _localLog.WriteEntry("HyunDae send Udp data!!!", EventLogEntryType.Information);

            var process = Process.GetCurrentProcess();
            SyslogRfc5424MessageSerializer syslogRfc5424MessageSerializerUtf8 = new SyslogRfc5424MessageSerializer(Encoding.UTF8);

            var message = new SyslogMessage(DateTime.Now,
            facility: Facility.UserLevelMessages,
            severity: Severity.Debug,
            hostName: process.MachineName,
            procId: process.Id.ToString(),
            appName: process.ProcessName,
            msgId: "Hello",
            message: msg);

            // asynchronous communication because of performance
            using (var sender = new AsyncSyslogUdpSender(host, port))
            {
                await sender.ConnectAsync();
                await sender.SendAsync(message, syslogRfc5424MessageSerializerUtf8);
                await sender.DisconnectAsync();
            }
        }

        //// UDP send
        //static void sendUdp(String ip, int port, byte[] data)
        //{
        //    try
        //    {
        //        _udpClient = new UdpClient();
        //        _udpClient.Connect(host, port);
        //        _udpClient.Send(data, data.Length);
        //        _udpClient.Close();
        //    }
        //    catch (SocketException se)
        //    {
        //        Console.WriteLine(se.ToString());
        //    }
        //    catch (NullReferenceException ne)
        //    {
        //        Console.WriteLine(ne.ToString());
        //    }
        //}
    }
}
