using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutomatedTranslation.Engines;

namespace AutomatedTranslation
{
    enum ArgumentType { PoLanguage = 0, StringResource, TranslationEngine }

    class Program
    {
        private const string PARAMETER_LANGUAGE_PATH = "/languagePath=";
        private const string PARAMETER_STRING_RESOURCE_PATH = "/strResourcePath=";
        private const string PARAMETER_TRANSLATION_ENGINE = "/engine=";

        private const int S_OK = 0;


        public static ITranslateEngine TranslationEngine { get; set; }


        static int Main(string[] args)
        {
            var resultCode = S_OK;

            try
            {
                var validatedArgs = ValidateArguments(args);
                ValidateTranslationEngineArgument(args);
                Console.WriteLine(" -> All Translations Done Using the '{0}'", TranslationEngine.GetType().Name);

                if (validatedArgs.ContainsKey(ArgumentType.PoLanguage)) {
                    var poTranslator = new PoTranslator { TranslateEngine = TranslationEngine };
                    poTranslator.Translate(validatedArgs[ArgumentType.PoLanguage], Console.WriteLine);
                }

                if (validatedArgs.ContainsKey(ArgumentType.StringResource)) {
                    var strResourceTranslator = new StringResourceTranslator { TranslateEngine = TranslationEngine };
                    strResourceTranslator.Translate(validatedArgs[ArgumentType.StringResource], Console.WriteLine);
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

        private static Dictionary<ArgumentType, DirectoryInfo> ValidateArguments(string[] args)
        {
            var result = new Dictionary<ArgumentType, DirectoryInfo>();

            if (args.Any() == false)
                throw new Exception("Unable to find any arguments.  Please make sure you include at least one argument.");

            if (args.Any(a => a.StartsWith(PARAMETER_LANGUAGE_PATH)))
                result.Add(ArgumentType.PoLanguage, ValidatePoLanguageArgument(args.Single(a => a.StartsWith(PARAMETER_LANGUAGE_PATH))));

            if (args.Any(a => a.StartsWith(PARAMETER_STRING_RESOURCE_PATH)))
                result.Add(ArgumentType.StringResource, ValidateStringResourceArgument(args.Single(a => a.StartsWith(PARAMETER_STRING_RESOURCE_PATH))));
            
            return result;
        }

        private static DirectoryInfo ValidatePoLanguageArgument(string arg)
        {
            var languageRootInfo = new DirectoryInfo(arg.Split('=')[1]);
            if (languageRootInfo.Exists == false)
                throw new DirectoryNotFoundException("The PO Language Folder specified cannot be found!  Please verify the (/languagePath=) argument and try the operation again.");

            var files = languageRootInfo.GetFiles();
            var fileExtensions = files.Select(f => f.Extension).ToArray();
            if (fileExtensions.Count(fe => fe.Equals(".pot", StringComparison.InvariantCultureIgnoreCase)) != 1)
                throw new Exception("Unable to find the catalog file in the specified folder.  Please reference a valid path.");

            if (fileExtensions.Any(fe => fe.Equals(".po")) == false)
                throw new Exception("Unable to find any translation files in the specified folder.  Please reference a valid path.");

            Console.WriteLine(" -> Language Root Folder: {0}", languageRootInfo.FullName);

            return languageRootInfo;
        }

        private static DirectoryInfo ValidateStringResourceArgument(string arg)
        {
            var strResourceRootInfo = new DirectoryInfo(arg.Split('=')[1]);
            if (strResourceRootInfo.Exists == false)
                throw new DirectoryNotFoundException("The String Resource Folder specified cannot be found!  Please verify the (/strResourcePath=) argument and try the operation again.");

            var files = strResourceRootInfo.GetFiles();
            var fileExtensions = files.Select(f => f.Extension).ToArray();
            if (fileExtensions.Any(fe => fe.Equals(".resx", StringComparison.InvariantCultureIgnoreCase)) == false)
                throw new Exception("Unable to find the string resource files in the specified folder.  Please reference a valid path.");

            Console.WriteLine(" -> String Resource Root Folder: {0}", strResourceRootInfo.FullName);

            return strResourceRootInfo;
        }

        private static void ValidateTranslationEngineArgument(string[] args)
        {
            TranslationEngine = new BingTranslateEngine { FromCulture = "en-US" };

            if (args.Any(a => a.StartsWith(PARAMETER_TRANSLATION_ENGINE)) == false)
                return;
            
            var engineArg = args.Single(a => a.StartsWith(PARAMETER_TRANSLATION_ENGINE));
            var engineName = engineArg.Split('=')[1];
            var validEngineNames = new[] { "bing", "google" };
            if (validEngineNames.Contains(engineName.ToLowerInvariant()) == false)
                throw new ArgumentOutOfRangeException("/engine", "The specified translation engine is invalid.  Please use one of the following values:\n" + string.Join("\n", validEngineNames));

            if (engineName.Equals("Google", StringComparison.CurrentCultureIgnoreCase))
                TranslationEngine = new GoogleTranslateEngine { FromCulture = "en" };
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
