using System;
using System.Text;

namespace webapi_csharp.utils {
    public class Utils {
        public static string bytesToB64(byte[] bytes) {
            string fileData = Encoding.UTF8.GetString(bytes);
            byte[] charFileData = Convert.FromBase64String(fileData);
            string base64String = Convert.ToBase64String(charFileData);
            return base64String;
        }
    }
}