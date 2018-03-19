using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace AutomatedTranslation.Engines
{
    class TranslateTranslationEngine : ITranslateEngine
    {
        private const string DEFAULT_CULTURE_ENGLISH = "en";
        private const string URL_TRANSLATION = "https://www.translate.com/translator/ajax_translate";

        private readonly JavaScriptSerializer javaScriptSerializer;

        public string FromCulture { get; set; }
        public string ToCulture { get; set; }

        public TranslateTranslationEngine()
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
                var shortToCulture = ToCulture.Contains("-") ? ToCulture.Split('-')[0] : ToCulture;
                var postData = Encoding.UTF8.GetBytes($@"text_to_translate=""{wordOrPhraseToTranslate}""&source_lang={FromCulture}&translated_lang={shortToCulture}&use_cache_only=false");

                var webReq = (HttpWebRequest)WebRequest.Create(URL_TRANSLATION);
                webReq.Method = "POST";
                //webReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                webReq.ContentType = @"application/x-www-form-urlencoded";
                webReq.ContentLength = postData.Length;
                webReq.Accept = @"application/json";
                var stream = webReq.GetRequestStream();
                stream.Write(postData, 0, postData.Length);

                using (var webResponse = webReq.GetResponse()) {
                    using (var responseStream = webResponse.GetResponseStream()) {
                        if (responseStream == null)
                            throw new Exception("No response stream found for the given url");

                        var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                        var responseData = streamReader.ReadToEnd();

                        translatedValue = GetTranslatedValueFromJson(responseData);
                    }
                }
            }
            catch (Exception ex) {
                Trace.WriteLine("Unable to translate due to the follow error.");
                Trace.WriteLine(ex);
                if (Debugger.IsAttached) Debugger.Break();
            }

            return translatedValue;
        }

        private string GetTranslatedValueFromJson(string page)
        {
            var json = javaScriptSerializer.Deserialize<TranslateDotComResult>(page);
            return json.result == "success" ? json.translated_text: json.original_text;
        }

        class TranslateDotComResult
        {
            public string result { get; set; }
            public string original_text { get; set; }
            public string translated_text { get; set; }
            public int translation_id { get; set; }
            public string uri_slug { get; set; }
            public string seo_directory_url { get; set; }
            public string translation_source { get; set; }
            public string request_source { get; set; }
            public bool is_favorite { get; set; }
            public bool human_translaton_possible { get; set; }
        }
    }
}
