using System.Security.Cryptography;
using System.Text;

namespace Shared
{
    public class MD5
    {
        public MD5() { }

        public string Checksum(string filename)
        {
            StringBuilder sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);

                    foreach (byte hex in hash)
                        sb.Append(hex.ToString("x2"));
                    string md5sum = sb.ToString();
                    return md5sum;
//                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }
    }
}
