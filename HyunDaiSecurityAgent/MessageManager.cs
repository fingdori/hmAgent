using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiSecurityAgent
{
    public abstract class MessageManager
    {
        private string _delimiter;

        public MessageManager(string delimiter) {
            this._delimiter = delimiter;
        }

        public string getDelemiter() {
            return this._delimiter;
        }

        public abstract string makeMessage(String xmlString);
    }
}
