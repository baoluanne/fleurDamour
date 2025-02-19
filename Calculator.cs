using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace fleurDamour.Utilities
{
    public static class Calculator
    {
        public static readonly string[] SecurityQuestion = new string[5]{
            "What was your first pet's name?",
            "What is the name of your best friend from childhood?",
            "What is the name of your favorite restaurant?",
            "What is the name of your favorite movie or book?",
            "What city were you born in?"
        };

        public static string SHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static readonly Dictionary<char, char> VietnameseDiacriticsRemovalMap = new Dictionary<char, char>
        {
            { 'á', 'a' }, { 'à', 'a' }, { 'ả', 'a' }, { 'ã', 'a' }, { 'ạ', 'a' },
            { 'ă', 'a' }, { 'ắ', 'a' }, { 'ặ', 'a' }, { 'ằ', 'a' }, { 'ẳ', 'a' }, { 'ẵ', 'a' },
            { 'â', 'a' }, { 'ấ', 'a' }, { 'ầ', 'a' }, { 'ẩ', 'a' }, { 'ẫ', 'a' }, { 'ậ', 'a' },
            { 'é', 'e' }, { 'è', 'e' }, { 'ẻ', 'e' }, { 'ẽ', 'e' }, { 'ẹ', 'e' },
            { 'ê', 'e' }, { 'ế', 'e' }, { 'ề', 'e' }, { 'ể', 'e' }, { 'ễ', 'e' }, { 'ệ', 'e' },
            { 'í', 'i' }, { 'ì', 'i' }, { 'ỉ', 'i' }, { 'ĩ', 'i' }, { 'ị', 'i' },
            { 'ó', 'o' }, { 'ò', 'o' }, { 'ỏ', 'o' }, { 'õ', 'o' }, { 'ọ', 'o' },
            { 'ô', 'o' }, { 'ố', 'o' }, { 'ồ', 'o' }, { 'ổ', 'o' }, { 'ỗ', 'o' }, { 'ộ', 'o' },
            { 'ơ', 'o' }, { 'ớ', 'o' }, { 'ờ', 'o' }, { 'ở', 'o' }, { 'ỡ', 'o' }, { 'ợ', 'o' },
            { 'ú', 'u' }, { 'ù', 'u' }, { 'ủ', 'u' }, { 'ũ', 'u' }, { 'ụ', 'u' },
            { 'ư', 'u' }, { 'ứ', 'u' }, { 'ừ', 'u' }, { 'ử', 'u' }, { 'ữ', 'u' }, { 'ự', 'u' },
            { 'ý', 'y' }, { 'ỳ', 'y' }, { 'ỷ', 'y' }, { 'ỹ', 'y' }, { 'ỵ', 'y' },
            { 'đ', 'd' }
        };

        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = text.ToLowerInvariant();
            var stringBuilder = new StringBuilder();
            foreach (char c in text)
            {
                if (VietnameseDiacriticsRemovalMap.ContainsKey(c))
                {
                    stringBuilder.Append(VietnameseDiacriticsRemovalMap[c]);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString();
        }

        public static string ConvertNameSong(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace(' ', '-').RemoveDiacritics();
        }
    }
}
