﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using AutomatedTranslation.Infos;

namespace AutomatedTranslation.Engines
{
    public class BingTranslateEngine : ITranslateEngine
    {
        private const string DEFAULT_CULTURE_ENGLISH = "en";
        private const string URL_TRANSLATION = "https://www.bing.com/ttranslate?&IG=EB0A092B0DA749B1B157DB6A0F40C32A&IID=translator.5032.1";
        
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

            try {
                var postData = Encoding.ASCII.GetBytes($"text={wordOrPhraseToTranslate}&from={FromCulture}&to={ToCulture}");

                var webReq = (HttpWebRequest)WebRequest.Create(URL_TRANSLATION);
                webReq.Method = "POST";
                webReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
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
            var json = javaScriptSerializer.Deserialize<BingTranslationResult>(page);
            return json.statusCode == HttpStatusCode.OK ? json.translationResponse : null;
        }
    }

    class BingTranslationResult
    {
        public HttpStatusCode statusCode { get; set; }
        public string translationResponse { get; set; }
    }
}
