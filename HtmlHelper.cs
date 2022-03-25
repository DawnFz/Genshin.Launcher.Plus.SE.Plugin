using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    public class HtmlHelper
    {
        private const string Url = "https://www.cnblogs.com/DawnFz/p/7271382.html";

        private static async Task<string> ReadHTMLAsTextAsync(string url)
        {
            try
            {
                using (Stream stream = await new HttpClient().GetStreamAsync(url))
                {
                    using (StreamReader sr = new(stream, Encoding.UTF8))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string Mid(string str, string preStr, string nextStr)
        {
            try
            {
                string trimFront = str[(str.IndexOf(preStr) + preStr.Length)..];
                return trimFront[..trimFront.IndexOf(nextStr)];
            }
            catch
            {
                return string.Empty;
            }
        }
        public static async Task<string> GetInfoFromHtmlAsync(string tag)
        {
            return Mid(await ReadHTMLAsTextAsync(Url), $"[${tag}$]", $"[#{tag}#]");
        }
    }
}
