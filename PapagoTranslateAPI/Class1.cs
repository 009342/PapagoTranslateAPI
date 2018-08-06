using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PapagoTranslateAPI
{
    public class PapagoTranslate
    {
        public PapagoTranslate()
        {

        }
        /// <summary>
        /// 번역 요청을 한 뒤, 결과를 받아옵니다.
        /// </summary>
        /// <param name="source">원본 문장의 언어입니다.</param>
        /// <param name="target">번역되고자 하는 언어입니다.</param>
        /// <param name="text">번역할 문장입니다.</param>
        /// <param name="mode">n2mt, nsmt중 하나입니다. 기본값은 n2mt입니다.</param>
        /// <returns></returns>
        public string Translate(string source, string target, string text, string mode = "n2mt")
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers["accept"] = "application/json";
            wc.Headers["content-type"] = "application/x-www-form-urlencoded; charset=UTF-8";
            wc.Headers["origin"] = "https://papago.naver.com/";
            wc.Headers["referer"] = "https://papago.naver.com/";
            string param = "data=" + GenerateDataParam(GenerateJSON(source, target, text));
            string result = wc.UploadString("https://papago.naver.com/apis/" + mode + "/translate", param);
            return JObject.Parse(result)["translatedText"].ToString();
        }
        private string GenerateJSON(string source, string target, string text)
        {
            JObject jObject = new JObject();
            jObject.Add("deviceId", Guid.NewGuid().ToString());
            jObject.Add("dict", false);
            jObject.Add("source", source);
            jObject.Add("target", target);
            jObject.Add("text", text);
            return jObject.ToString();
        }
        private string GenerateDataParam(string query)
        {
            string range1 = "abcdefghijklmABCDEFGHIJKLM";
            string range2 = "nopqrstuvwxyzNOPQRSTUVWXYZ";
            string encoded = Base64Encode(query);
            StringBuilder s = new StringBuilder(encoded.Substring(0, 16));
            for (int i = 0; i < 16; i++)
            {
                if (range1.Contains(s[i]))
                {
                    s[i] = (char)(s[i] + 13);
                }
                else if (range2.Contains(s[i]))
                {
                    s[i] = (char)(s[i] - 13);
                }
            }
            return s.ToString() + encoded.Substring(16, encoded.Length - 16);
        }
        private string Base64Encode(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
    }
}
