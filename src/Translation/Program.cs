using AutomatedTranslation.Engines;
using System;
using System.IO;
using System.Linq;

namespace AutomatedTranslation
{
    class Program
    {
        private const int S_OK = 0;

        static int Main(string[] args)
        {
            var resultCode = S_OK;

            try
            {
                var languageFolder = ValidateArguments(args);
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

                    Console.WriteLine(" -> Processing language file: {0}", translationFile.Filename);
                    
                    var existingKeys = translationFile.TranslationDictionary.Keys.ToArray();
                    var newCatalogKeys = catalogKeys.Where(ck => existingKeys.Contains(ck.EnglishKey) == false).ToArray();
                    foreach (var newKey in newCatalogKeys)
                        translationFile.TranslationDictionary.Add(newKey.EnglishKey, new TranslatedValue
                                                                                        {
                                                                                            IsNew = true,
                                                                                            Key = newKey.EnglishKey,
                                                                                            Locations = newKey.Locations
                                                                                        });

                    var engine = new GoogleTranslateEngine { FromCulture = "en_US", ToCulture = translationFile.CountryCode };
                    var keysToTranslate = translationFile.TranslationDictionary.Where(d => d.Value.IsNew).Select(k => k.Key).ToArray();
                    foreach (var englishTranslation in keysToTranslate)
                        translationFile.TranslationDictionary[englishTranslation].Value = engine.TranslateWordOrPhrase(englishTranslation);

                    translationFile.SaveTranslations();
                }

                Console.WriteLine(" -> Translations Finished");
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
                resultCode = 87;
            }

            return resultCode;
        }

        private static DirectoryInfo ValidateArguments(string[] args)
        {
            if (args.Count() != 1)
                throw new Exception(
                    "Unable to find the /path=<folder> argument.  Please pass this argument pointing to the language root directory.");

            var languageRoot = args[0].Split('=')[1];

            var languageRootInfo = new DirectoryInfo(languageRoot);
            if (languageRootInfo.Exists == false)
                throw new Exception(string.Format("The path '{0}' does not exist!  Please reference a valid path.", languageRoot));

            var files = languageRootInfo.GetFiles();
            var fileExtensions = files.Select(f => f.Extension).ToArray();
            if (fileExtensions.Count(fe => fe.Equals(".pot", StringComparison.InvariantCultureIgnoreCase)) != 1)
                throw new Exception("Unable to find the catalog file in the specified folder.  Please reference a valid path.");
            if (fileExtensions.Any(fe => fe.Equals(".po")) == false)
                throw new Exception(
                    "Unable to find any translation files in the specified folder.  Please reference a valid path.");

            Console.WriteLine(" -> Language Root Folder: {0}", languageRootInfo.FullName);

            return languageRootInfo;
        }

        private static void WriteError(string errorMessage)
        {
            var currentConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ForegroundColor = currentConsoleColor;
        }
    }
}
