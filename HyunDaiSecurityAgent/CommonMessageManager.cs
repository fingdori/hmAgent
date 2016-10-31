using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;

namespace HyunDaiSecurityAgent
{
    class CommonMessageManager : MessageManager
    {
        private const int RetryCountMax = 1000;
        private const int SleepDuration = 1000;

        private static EventLog _localLog = LogManager.getLocalLog();

        public CommonMessageManager(string delimiter) : base(delimiter)
        {
            // calling super class constructor
        }

        public override string makeMessage(string xml)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                DateTime now = DateTime.UtcNow;
                sb.Append(getActiveIpsAndMacsMesaage());
                sb.Append(getDelemiter() + "Computer=\"" + Environment.MachineName + "\"");
                sb.Append(getDelemiter() + "TargetUserName=\"" + Environment.UserName + "\"");
                sb.Append(getDelemiter() + "TargetDomainName=" + Environment.UserDomainName + "\"");
                sb.Append(getDelemiter() + "currentSystemTime=\"" + now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff00K",
                                    CultureInfo.InvariantCulture) + "\"");
            }
            catch (Exception e)
            {
                _localLog.WriteEntry("Common Message Make Error : \r\n" + e.ToString(), EventLogEntryType.Error);
                throw new Exception("common message make error occur!!");
            }

            return sb.ToString();            
        }

        public String getActiveIpsAndMacsMesaage()
        {

            NetworkInterface[] nts = NetworkInterface.GetAllNetworkInterfaces();
            DateTime now = DateTime.UtcNow;
            String ipAddress = "";
            String MacAddress = "";
            int retryCount = 0;

            // 최대 1000번의 retry를 1초 간격으로 실시
            while (ipAddress.Equals("") || retryCount > RetryCountMax)
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                if (!ipAddress.Equals(""))
                                {
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
                System.Threading.Thread.Sleep(SleepDuration);
                retryCount++;
            }
            _localLog.WriteEntry("retryCount : " + --retryCount, EventLogEntryType.Information);

            StringBuilder sb = new StringBuilder();

            sb.Append("IpAddress=\"" + ipAddress + "\"");
            sb.Append(getDelemiter() + "MacAddress=\"" + MacAddress + "\"");

            return sb.ToString();
        }
    }
}
