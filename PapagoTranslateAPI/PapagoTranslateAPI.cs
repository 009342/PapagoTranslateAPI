using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace PapagoTranslateAPI
{
    public class PapagoTranslate
    {
        private static readonly HashSet<string> SMTLanguages = new HashSet<string> { "hi", "pt" };

        private static readonly string UrlBase = "https://papago.naver.com";
        private static readonly string UrlN2MT = "/apis/n2mt/translate"; // Neural Machine Translation
        private static readonly string UrlNSMT = "/apis/nsmt/translate"; // Statistical Machine Translation
        private static readonly string FormUrlEncodedTemplate = "source={0}&target={1}&text={2}";
        private static readonly Guid UUID = Guid.NewGuid();

        private static readonly Regex patternSource = new Regex(@"/vendors~main[^""]+", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex patternVersion = new Regex(@"v\d\.\d\.\d_[^""]+", RegexOptions.Compiled | RegexOptions.Singleline);

        private double nextUpdate = 0;
        private string version; // HMAC 키

        /// <summary>
        /// 번역 요청을 한 뒤, 결과를 받아옵니다.
        /// </summary>
        /// <param name="source">원본 문장의 언어입니다.</param>
        /// <param name="target">번역되고자 하는 언어입니다.</param>
        /// <param name="text">번역할 문장입니다.</param>
        /// <param name="mode">n2mt, nsmt중 하나입니다. 기본 값이 없으면 자동으로 선택합니다.</param>
        /// <returns></returns>
        public string Translate(string source, string target, string text, string mode = "")
        {
            var wc = new WebClient
            {
                Encoding = Encoding.UTF8
            };

            // 현재 유닉스 밀리 초
            var timestamp = Math.Truncate(DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds);

            // 버전이 없거나 마지막으로 버전 확인하고 10분 지났다면 업데이트하기
            if (nextUpdate == 0 || nextUpdate <= timestamp)
            {
                nextUpdate = timestamp + (60 * 10 * 1000);
                updateVersion(wc);
            }

            var isSMT = SMTLanguages.Contains(source) || SMTLanguages.Contains(target);
            if (mode != "")
            {
                isSMT = mode == "nsmt";
            }

            var url = UrlBase + (isSMT ? UrlNSMT : UrlN2MT);
            var param = string.Format(FormUrlEncodedTemplate, source, target, text);

            // 버전을 키로 사용한 HmacMD5 으로 인증 토큰 만들기
            var key = Encoding.UTF8.GetBytes(version);
            var data = Encoding.UTF8.GetBytes($"{UUID}\n{url}\n{timestamp}");
            var token = Convert.ToBase64String(new HMACMD5(key).ComputeHash(data));

            // 최소 헤더 보내기
            wc.Headers.Set("Authorization", $"PPG {UUID}:{token}");
            wc.Headers.Set("Timestamp", timestamp.ToString());

            var result = wc.UploadString(url, param);

            return JObject.Parse(result)["translatedText"].ToString();
        }

        private void updateVersion(WebClient wc)
        {
            // 메인 페이지에서 버전 불러올 수 있는 자바스크립트 주소 찾아오기
            var main = wc.DownloadString(UrlBase);
            var mainMatch = patternSource.Match(main);

            // 자바스크립트 속에서 현재 파파고 버전 확인하기
            var script = wc.DownloadString(UrlBase + mainMatch.Value);
            var scriptMatch = patternVersion.Match(script);

            version = scriptMatch.Value;
        }
    }
}
