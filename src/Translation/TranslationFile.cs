using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutomatedTranslation
{
    class TranslationFile
    {
        private readonly string filePath;
        private Dictionary<string, TranslatedValue> translationDictionary;

        private readonly Func<string, bool> isLocationHeader = str => str.StartsWith("#: ");
        private readonly Func<string, bool> isTranslationKey = str => str.StartsWith("msgid");
        private readonly Func<string, bool> isTranslationValue = str => str.StartsWith("msgstr");
        
        private readonly Func<string, string> getTranslationKey = str => str.Substring(7).TrimEnd('"');
        private readonly Func<string, string> getTranslationValue = str => str.Substring(8).TrimEnd('"');

        public string Filename
        {
            get
            {
                return (new FileInfo(filePath).Name);
            }
        }

        public string CountryCode
        {
            get
            {
                var fileInfo = new FileInfo(filePath);
                var countryCode = fileInfo.Name.Remove(fileInfo.Name.Length - 3);
                
                var fileLines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var line in fileLines)
                {
                    if (line.StartsWith("\"Language:"))
                    {
                        countryCode = line.Substring(11).Replace("\\n\"", string.Empty);
                        break;
                    }
                }

                return countryCode;
            }
        }

        public Dictionary<string, TranslatedValue> TranslationDictionary
        {
            get
            {
                if (translationDictionary == null)
                    InitalizeDictionary();

                return translationDictionary;
            }
        }
        
        public TranslationFile(string translationFilePath)
        {
            filePath = translationFilePath;
        }

        public bool IsTranslationFileValid()
        {
            try
            {
                var valid = File.ReadAllLines(filePath, Encoding.UTF8)
                                .Where(ln => IsLineValid(ln) && isTranslationKey(ln))
                                .ToDictionary(ln => getTranslationKey(ln), ln => string.Empty);
                return (valid.Keys.Any());
            }
            catch { return false; }
        }

        public void SaveTranslations()
        {
            var newTranslations = translationDictionary.Where(v => v.Value.IsNew).ToDictionary(k => k.Key, v => v.Value);
            if (newTranslations.Any() == false)
                return;

            var workingKey = string.Empty;
            var lines = File.ReadAllLines(filePath).ToList();
            for (int nIndex = 0; nIndex < lines.Count(); nIndex++)
            {
                if (IsLineValid(lines[nIndex])) {
                    if (isTranslationKey(lines[nIndex]))
                        workingKey = getTranslationKey(lines[nIndex]);
                    else if (isTranslationValue(lines[nIndex]) && newTranslations.ContainsKey(workingKey)) {
                        lines[nIndex] = string.Format("msgstr \"{0}\"", newTranslations[workingKey].Value);
                        newTranslations.Remove(workingKey);
                    }
                }
            }

            foreach (var translation in newTranslations)
            {
                lines.Add(string.Empty);
                lines.AddRange(translation.Value.Locations);
                lines.Add(string.Format("msgid \"{0}\"", translation.Key));
                lines.Add(string.Format("msgstr \"{0}\"", translation.Value.Value));
            }

            File.Delete(filePath);
            File.WriteAllLines(filePath, lines);
        }

        private void InitalizeDictionary()
        {
            translationDictionary = new Dictionary<string, TranslatedValue>();

            var translationValue = new TranslatedValue();
            var lines = File.ReadAllLines(filePath, Encoding.UTF8)
                            .Where(IsLineValid);
            foreach (var line in lines)
            {
                if (isLocationHeader(line))
                    translationValue.Locations.Add(line);
                else if (isTranslationKey(line)) {
                    var key = getTranslationKey(line);
                    if (translationDictionary.ContainsKey(key))
                        throw new Exception("The key already exists in the translation file.  The translation file is not valid!");

                    translationValue.Key = key;
                }
                else if (isTranslationValue(line)) {
                    translationValue.Value = getTranslationValue(line);
                    if (string.IsNullOrWhiteSpace(translationValue.Value))
                        translationValue.IsNew = true;
                    translationDictionary.Add(translationValue.Key, translationValue);
                    translationValue = new TranslatedValue();
                }
            }
        }

        private bool IsLineValid(string lineOfText)
        {
            if (string.IsNullOrWhiteSpace(lineOfText))
                return false;
            if (lineOfText.StartsWith("\""))
                return false;
            if (lineOfText.Trim().Equals("msgid \"\"", StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (lineOfText.Trim().Equals("msgstr \"\"", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
    }
}