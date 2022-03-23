using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    public class ReadHtmlHelper
    {
        public string ReadHTML(string url)
        {
            string strHTML = "";
            try
            {
                WebClient myWebClient = new();
                myWebClient.Proxy = null;
                Stream myStream = myWebClient.OpenRead(url);
                StreamReader sr = new StreamReader(myStream, Encoding.UTF8);
                strHTML = sr.ReadToEnd();
                myStream.Close();
            }
            catch
            {
                return "";
            }
            return strHTML;
        }
        public string MiddleText(string Str, string preStr, string nextStr)
        {
            try
            {
                string tempStr = Str.Substring(Str.IndexOf(preStr) + preStr.Length);
                tempStr = tempStr.Substring(0, tempStr.IndexOf(nextStr));
                return tempStr;
            }
            catch
            {
                return "";
            }
        }
        public string GetFromHtml(string tag)
        {
            return MiddleText(ReadHTML("https://www.cnblogs.com/DawnFz/p/7271382.html"), $"[${tag}$]", $"[#{tag}#]");
        }
    }
}
