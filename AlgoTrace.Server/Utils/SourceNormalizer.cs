using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrace.Server.Utils
{
    public static class SourceNormalizer
    {
        public static string NormalizeLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return string.Empty;

            try
            {
                return Regex
                    .Replace(line, @"\s+", "", RegexOptions.None, TimeSpan.FromSeconds(1))
                    .ToLower();
            }
            catch (RegexMatchTimeoutException)
            {
                return line.Replace(" ", "").Replace("\t", "").ToLower();
            }
        }

        public static string[] GetLines(string code)
        {
            return code?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                ?? Array.Empty<string>();
        }
    }
}
