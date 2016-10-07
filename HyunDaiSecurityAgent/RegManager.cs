using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiSecurityAgent
{
    // Registry 접근 클래스
    class RegManager
    {
        private const String _manufacturer = "HyunDaiSecure";

        public static string Manufacturer
        {
            get
            {
                return _manufacturer;
            }
        }

        public static String getUuidFromRegistry() {
            Microsoft.Win32.RegistryKey key;
            // read value from registry
            String uuid = (String)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\" + Manufacturer, "uuid", "");
            // 만약 없는 경우 생성
            if (uuid == null) {
                uuid = Guid.NewGuid().ToString();
                key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SOFTWARE\\" + Manufacturer);
                key.SetValue("uuid", uuid);
                key.Close();
            }

            return uuid;
        }
    }
}
