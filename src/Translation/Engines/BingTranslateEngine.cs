using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using AutomatedTranslation.Infos;

namespace AutomatedTranslation.Engines
{
    public class BingTranslateEngine : ITranslateEngine
    {
        private const string DEFAULT_CULTURE_ENGLISH = "en";
        private const string URL_TRANSLATION = "http://api.microsofttranslator.com/v2/ajax.svc/TranslateArray?appId=%22{3}%22&texts=%5B%22{2}%22%5D&from=%22{0}%22&to=%22{1}%22";
//                                              http://api.microsofttranslator.com/v2/ajax.svc/TranslateArray?appId=%22{3}%22&texts=%5B%22{2}%22%5D&from=%22{0}%22&to=%22{1}%22&options=%7B%7D&oncomplete=onComplete_0&onerror=onError_0&_=1431706249820
        private const string URL_GET_APP_ID = @"http://www.bing.com/translator/dynamic/217311/js/MobileLandingPage.js?loc={0}";

        private readonly JavaScriptSerializer javaScriptSerializer;

        public string FromCulture { get; set; }
        public string ToCulture { get; set; }

        public BingTranslateEngine()
        {
            FromCulture = DEFAULT_CULTURE_ENGLISH;
            ToCulture = DEFAULT_CULTURE_ENGLISH;
            
            javaScriptSerializer = new JavaScriptSerializer();
        }

        public string TranslateWordOrPhrase(string wordOrPhraseToTranslate)
        {
            var translatedValue = wordOrPhraseToTranslate;

            try
            {
                var msConfig = GetMicrosoftConfig();
                if (wordOrPhraseToTranslate.Length > msConfig.maxNumberOfChars)
                    throw new ArgumentOutOfRangeException("wordOrPhraseToTranslate", string.Format("The word or phrase is too long to translate.  It exceeds {0} characters in length.", msConfig.maxNumberOfChars));

                var url = String.Format(URL_TRANSLATION, FromCulture, ToCulture, HttpUtility.UrlEncode(wordOrPhraseToTranslate), msConfig.appId);
                var webReq = CreateGetRequest(url);
                //webReq.Referer = msConfig.referrer;
                using (var webResponse = webReq.GetResponse())
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new Exception("No response stream found for the given url");

                        var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                        var responseData = streamReader.ReadToEnd();

                        translatedValue = GetTranslatedValueFromJson(responseData);
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

        private MsConfiguration GetMicrosoftConfig()
        {
            var result = new MsConfiguration();

            const string CONFIG = ".Configurations=";
            
            var url = string.Format(URL_GET_APP_ID, FromCulture);
            var webReq = CreateGetRequest(url);
            using (var webResponse = webReq.GetResponse())
            {
                using (var responseStream = webResponse.GetResponseStream())
                {
                    if (responseStream == null)
                        throw new Exception("No response stream found for the given url");

                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    var responseData = streamReader.ReadToEnd();

                    var translatorIndex = responseData.IndexOf("Microsoft.Translator=");
                    var configurationStartIndex = responseData.IndexOf(CONFIG, translatorIndex) + CONFIG.Length + 1;
                    var configurationEndIndex = responseData.IndexOf("};", configurationStartIndex);
                    var configurationJson = responseData.Substring(configurationStartIndex, (configurationEndIndex - configurationStartIndex));

                    result = new MsConfiguration(configurationJson);
                }
            }

            return result;
        }

        private HttpWebRequest CreateGetRequest(string url)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            webReq.ContentType = "application/json";
            webReq.UserAgent = "Mozilla/5.0 (Linux; U; Android 4.4.4; Nexus 5 Build/KTU84P) AppleWebkit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
            webReq.Referer = "http://www.bing.com/translator/";

            return webReq;
        }

        private string GetTranslatedValueFromJson(string page)
        {
            var json = javaScriptSerializer.Deserialize<BingTranslationResult[]>(page);
            if (json.Any())
                return json[0].TranslatedText;

            return null;
        }
    }
}
