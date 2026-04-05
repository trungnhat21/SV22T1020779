using System.Security.Cryptography;
using System.Text;

namespace SV22T1020779.BusinessLayers
{
    public static class SecurityService
    {
        /// <summary>
        /// Mã hóa một chuỗi văn bản (thường là mật khẩu) sang dạng chuỗi MD5 (Hexadecimal)
        /// </summary>
        /// <param name="password">Chuỗi ký tự cần mã hóa</param>
        /// <returns>Chuỗi đã được mã hóa MD5 gồm 32 ký tự viết thường</returns>
        public static string ToMD5(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}