using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;
using System;
using System.Diagnostics;
using System.Text;

namespace HyunDaiSecurityAgent
{
    class SyslogManager
    {
        private static EventLog _localLog = LogManager.getLocalLog();

        // syslog 형식에 맞춰서 메시지를 만든 후 udp로 asyncronous 하게 전송
        public static async void portableSyslogUdpSend(String host, int port, String msg)
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
            catch (Exception e)
            {
                _localLog.WriteEntry("upd send error : " + e.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
