using System;
using System.IO;
using System.Linq;
using AutomatedTranslation.Engines;
using AutomatedTranslation.Helpers;

namespace AutomatedTranslation
{
    public class StringResourceTranslator
    {
        private ITranslateEngine translateEngine;

        private const string SOURCE_RESOURCE_FILE = "Strings.resx";

        public ITranslateEngine TranslateEngine
        {
            get { return translateEngine ?? new BingTranslateEngine() { FromCulture = "en-US" }; }
            set { translateEngine = value; }
        }


        public void Translate(DirectoryInfo languageFolder, Action<string, object> writeLine)
        {
            var sourceFile = languageFolder.GetFiles(SOURCE_RESOURCE_FILE, SearchOption.TopDirectoryOnly).First().FullName;
            var translationFiles = languageFolder.GetFiles("*.resx", SearchOption.TopDirectoryOnly)
                                                 .Where(tf => tf.Name.Equals(SOURCE_RESOURCE_FILE, StringComparison.InvariantCultureIgnoreCase) == false);
            foreach (var translationFile in translationFiles)
            {
                if (writeLine != null)
                    writeLine(" -> Processing language file: {0}", translationFile.Name);

                using (var resourceFileHelper = new ResourceFileHelper(sourceFile, translationFile.FullName))
                {
                    foreach (var sourcePair in resourceFileHelper.GetAllNameValuesFromSource())
                    {
                        translateEngine.ToCulture = translationFile.Name.Replace(".resx", string.Empty).Replace('_', '-');
                        var translatedValue = translateEngine.TranslateWordOrPhrase(sourcePair.Value);
                        var existingTargetValue = resourceFileHelper.GetValueFromTargetUsingKey(sourcePair.Key);
                        if (translatedValue.Equals(existingTargetValue, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        resourceFileHelper.WriteNameValuePairToTarget(sourcePair.Key, translatedValue, true);
                    }
                }
            }
        }
    }
}
