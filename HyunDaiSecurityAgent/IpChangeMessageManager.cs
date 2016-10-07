using System;
using System.Text;
using System.Xml;

namespace HyunDaiSecurityAgent
{
    class IpChangeMessageManager : MessageManager
    {
        public IpChangeMessageManager(string delimiter) : base(delimiter)
        {
        }

        //IpChanged|ip=(공백)mac=(공백)hostname=(공백)currentSystemTime= 
        public override string makeMessage(string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xmlString);
            sb.Append("UUID=" + ConfigManager.Uuid + LogTypeDelimeter + LogTypeIpChange + LogTypeDelimeter);
            sb.Append(addSingleNodeInnerText("ip", xd));
            sb.Append(addSingleNodeInnerText("mac", xd));
            sb.Append(addSingleNodeInnerText("hostname", xd));
            sb.Append(addSingleNodeInnerText("currentSystemTime", xd));
            sb.Remove(sb.Length-1, 1);

            return sb.ToString();
        }

        private String addSingleNodeInnerText(String elementName, XmlDocument xd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(elementName + "=");
            sb.Append(xd.GetElementsByTagName(elementName)[0].InnerText);
            sb.Append(getDelemiter());
            return sb.ToString();
        }
    }
}
