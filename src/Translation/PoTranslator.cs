using System;
using System.IO;
using System.Linq;
using AutomatedTranslation.Engines;

namespace AutomatedTranslation
{
    public class PoTranslator
    {
        private ITranslateEngine translateEngine;


        public ITranslateEngine TranslateEngine
        {
            get { return translateEngine ?? new BingTranslateEngine() { FromCulture = "en-US" }; }
            set { translateEngine = value; }
        }


        public void Translate(DirectoryInfo languageFolder, Action<string, object> writeLine)
        {
            var catalogFileInfo = languageFolder.GetFiles("*.pot", SearchOption.TopDirectoryOnly).First();
            var catalogFile = new CatalogFile(catalogFileInfo.FullName);
            if (catalogFile.IsCatalogValid() == false)
                throw new Exception("The catalog file is not valid!  Check for duplicates in the file or other discrepancies.");                                                                       

            var catalogKeys = catalogFile.GetEnglishKeys().ToArray();

            var translationFiles = languageFolder.GetFiles("*.po", SearchOption.TopDirectoryOnly)
                                                 .Select(tf => new TranslationFile(tf.FullName));
            foreach (var translationFile in translationFiles)
            {
                if (translationFile.IsTranslationFileValid() == false)
                    throw new Exception(string.Format("The translation file '{0}' is not valid!  Check for duplicates in the file or other discrepancies.", translationFile.Filename));

                writeLine?.Invoke(" -> Processing language file: {0}", translationFile.Filename);

                translateEngine.ToCulture = translationFile.CountryCode.Replace('_', '-');
                var existingKeys = translationFile.TranslationDictionary.Keys.ToArray();
                var newCatalogKeys = catalogKeys.Where(ck => existingKeys.Contains(ck.EnglishKey) == false).ToArray();
                foreach (var newKey in newCatalogKeys)
                    translationFile.TranslationDictionary.Add(newKey.EnglishKey, new TranslatedValue {
                            IsNew = true,
                            Key = newKey.EnglishKey,
                            Locations = newKey.Locations
                        });
                
                var keysToTranslate = translationFile.TranslationDictionary.Where(d => d.Value.IsNew).Select(k => k.Key).ToArray();
                foreach (var englishTranslation in keysToTranslate)
                    translationFile.TranslationDictionary[englishTranslation].Value = TranslateEngine.TranslateWordOrPhrase(englishTranslation);

                translationFile.SaveTranslations();
            }
        }
    }
}
