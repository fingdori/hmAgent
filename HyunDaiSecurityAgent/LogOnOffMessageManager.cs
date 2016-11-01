using System;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace HyunDaiSecurityAgent
{
    class LogOnOffMessageManager : MessageManager
    {
        private static EventLog _localLog = LogManager.getLocalLog();

        public LogOnOffMessageManager(string delimiter) : base(delimiter)
        {
            // calling super class constructor
        }

        public override string makeMessage(string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument xd = new XmlDocument();
            CommonMessageManager commonMessageManager = new CommonMessageManager(getDelemiter());
            try
            {
                xd.LoadXml(xmlString);
                String eventId = xd.GetElementsByTagName("EventID")[0].InnerText;

                switch (eventId) {
                    // Log On Message

                    //Event > System > EventID
                    //Event > System > TimeCreated : 이벤트 수신 시간과 차이점은 없는지
                    //Event > System > EventRecordID
                    //Event > System > Execution > ProcessID ?????
                    //Event > System > Computer
                    //Event > EventData > SubjectUserSid
                    //Event > EventData > SubjectUserName
                    //Event > EventData > SubjectDomainName
                    //Event > EventData > SubjectLogonId
                    //Event > EventData > TargetUserSid
                    //Event > EventData > TargetUserName
                    //Event > EventData > TargetDomainName
                    //Event > EventData > TargetLogonId
                    //Event > EventData > WorkstationName
                    //Event > EventData > LogonGuid
                    //Event > EventData > ProcessId
                    //Event > EventData > ProcessName
                    //Event > EventData > IpAddress
                    //Event > EventData > IpPort
                    //예)
                    //Logon|EventID=4624(공백)TimeCreated=2016-09-21T05:16:59.504547000Z(공백)EventRecordID=xxx
                    case "4624": // Log on
                        sb.Append("UUID=" + ConfigManager.Uuid + LogTypeDelimeter + LogTypeLogOn + LogTypeDelimeter);

                        sb.Append(addSingleNodeInnerText("EventID", xd));
                        sb.Append(addSingleNodeAttributeValue("TimeCreated", "TimeCreated", "SystemTime", xd));
                        sb.Append(addSingleNodeInnerText("EventRecordID", xd));
                        sb.Append(addSingleNodeAttributeValue("ProcessID", "Execution", "ProcessID", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("SubjectUserSid", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("SubjectUserName", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("SubjectDomainName", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("TargetUserSid", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("TargetLogonId", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("WorkstationName", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("LogonGuid", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("ProcessId", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("ProcessName", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("IpPort", xd));
                        sb.Append(commonMessageManager.makeMessage(null) + getDelemiter());
                        sb.Remove(sb.Length - 1, 1);
                        break;

                    // Log Off Message

                    //Event > System > EventID
                    //Event > System > TimeCreated : 이벤트 수신 시간과 차이점은 없는지
                    //Event > System > EventRecordID
                    //Event > System > ExecutionID > ProcessID ?????
                    //Event > System > ExecutionID > Computer
                    //Event > EventData > TargetUserSid
                    //Event > EventData > TargetUserName
                    //Event > EventData > TargetDomainName
                    //Event > EventData > TargetLogonId
                    //Event > EventData > LogonType
                    //예)
                    //LogOff|EventID=4624(공백)TimeCreated=2016-09-21T05:16:59.504547000Z(공백)EventRecordID=xxx

                    case "4634": // Log out
                        sb.Append("UUID=" + ConfigManager.Uuid + LogTypeDelimeter + LogTypeLogOff + LogTypeDelimeter);

                        sb.Append(addSingleNodeInnerText("EventID", xd));
                        sb.Append(addSingleNodeAttributeValue("TimeCreated", "TimeCreated", "SystemTime", xd));
                        sb.Append(addSingleNodeInnerText("EventRecordID", xd));
                        sb.Append(addSingleNodeAttributeValue("ProcessID", "Execution", "ProcessID", xd));
                        sb.Append(addSingleNodeInnerText("Computer", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("TargetUserSid", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("TargetLogonId", xd));
                        sb.Append(addDataElementValueMatchNameAttribute("LogonType", xd));
                        sb.Append(commonMessageManager.makeMessage(null) + getDelemiter());
                        sb.Remove(sb.Length - 1, 1);

                        break;
                }  
            }
            catch (Exception e)
            {
                _localLog.WriteEntry("Xml parsing error : \r\n" + e.ToString(), EventLogEntryType.Error);
                throw new Exception("xml parsing error occur!!");
            }

            return sb.ToString();            
        }

        private String getDataNameAttributeXpathQuery(string attr) {
            StringBuilder sb = new StringBuilder();

            sb.Append("//*[name() = 'Data'][@Name='");
            sb.Append(attr);
            sb.Append("']");

            return sb.ToString();
        }

        private String addSingleNodeInnerText(String elementName, XmlDocument xd) {
            StringBuilder sb = new StringBuilder();
            sb.Append(elementName + "=");
            sb.Append("\"");
            sb.Append(xd.GetElementsByTagName(elementName)[0].InnerText);
            sb.Append("\"");
            sb.Append(getDelemiter());
            return sb.ToString();
        }

        private String addSingleNodeAttributeValue(String name, String elementName, String attr, XmlDocument xd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name + "=");
            sb.Append("\"");
            sb.Append(xd.GetElementsByTagName(elementName)[0].Attributes[attr].Value);
            sb.Append("\"");
            sb.Append(getDelemiter());
            return sb.ToString();
        }

        private String addDataElementValueMatchNameAttribute(String attrName, XmlDocument xd) {
            StringBuilder sb = new StringBuilder();
            sb.Append(attrName + "=");
            sb.Append("\"");
            sb.Append(xd.SelectSingleNode(getDataNameAttributeXpathQuery(attrName)).InnerText);
            sb.Append("\"");
            sb.Append(getDelemiter());
            return sb.ToString();
        }
    }
}
