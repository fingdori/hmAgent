using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiSecurityAgent
{
    class Utils
    {
        private static EventLog _localLog = LogManager.getLocalLog();

        // 다른 프로세스에서 사용하고 있는 지 체크 (이벤트 핸들러 trigger가 너무 빨라서 변경 완료 하기 전보다 먼저 실행 되어 다른 프로세스가 파일을 사용중이라는 오류 발생)
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        public static String getUUID() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            String guid = assembly.GetType().GUID.ToString();
            
            return guid;
        }

    }
}
