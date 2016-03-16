using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Web;

namespace AutomatedTranslation.Engines
{
    public class GoogleTranslateEngine : ITranslateEngine
    {
        private const string englishCulture = "en";
        private const string googleUrlFormat = "http://translate.google.com/translate_a/single?client=webapp&sl={0}&tl={1}&hl=en&dt=bd&dt=ld&dt=qca&dt=rm&dt=t&source=btn&ssel=5&tsel=5&kc=0&tk=520999|681256&q={2}";
        
        public string FromCulture { get; set; }
        public string ToCulture { get; set; }

        public GoogleTranslateEngine()
        {
            FromCulture = englishCulture;
            ToCulture = englishCulture;
        }

        public string TranslateWordOrPhrase(string wordOrPhraseToTranslate)
        {
            var translatedValue = wordOrPhraseToTranslate;

            try
            {
                var url = String.Format(googleUrlFormat, FromCulture, ToCulture, HttpUtility.UrlEncode(wordOrPhraseToTranslate));
                var webReq = CreateTranslationRequest(url);
                using (var webResponse = webReq.GetResponse())
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new Exception("No response stream found for the given url");

                        var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                        var responseData = streamReader.ReadToEnd();

                        translatedValue = GetTranslatedValueFromResponse(responseData);
                    }
                }                
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Unable to translate due to the follow error.");
                Trace.WriteLine(ex);
                if (Debugger.IsAttached) Debugger.Break();
            }

            return translatedValue;
        }

        private HttpWebRequest CreateTranslationRequest(string url)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            webReq.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            webReq.ContentType = "application/json";
            webReq.Accept = "en-US,en;q=0.5";
            webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0";
            webReq.Referer = "http://translate.google.com/m/translate";
            webReq.Host = "translate.google.com";

            return webReq;
        }

        private string GetTranslatedValueFromResponse(string page)
        {
            var rawValues = page.Split(new [] { @""",""" }, StringSplitOptions.RemoveEmptyEntries);
            if (rawValues.Length < 2)
                return null;
            
            var translatedValue = rawValues.First().Substring(4).TrimEnd('"');
            var englishValue = rawValues[0].TrimEnd(']').Trim('"');

            return (string.IsNullOrWhiteSpace(translatedValue) == false) ? translatedValue : englishValue;
        }
    }
}
