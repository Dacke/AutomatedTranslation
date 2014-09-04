using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutomatedTranslation
{
    class CatalogFile
    {
        private readonly string[] lines;

        private readonly Func<string, bool> isLocationHeader = str => str.StartsWith("#: ");
        private readonly Func<string, bool> isTranslationKey = str => str.StartsWith("msgid");
        private readonly Func<string, string> getTranslationKey = str => str.Substring(7).TrimEnd('"');
        

        public CatalogFile(string catalogFilePath)
        {
            lines = File.ReadAllLines(catalogFilePath)
                        .Where(IsLineValid)
                        .ToArray();
        }

        public bool IsCatalogValid()
        {
            try
            {
                var valid = lines.Where(k => isTranslationKey(k)).ToDictionary(k => k, v => string.Empty);
                return (valid.Keys.Any());
            }
            catch { return false; }
        }

        public IEnumerable<CatalogKey> GetEnglishKeys()
        {
            var catalogKeys = new List<CatalogKey>();

            var key = new CatalogKey();
            foreach (var line in lines)
            {
                if (isLocationHeader(line))
                    key.Locations.Add(line);
                else if (isTranslationKey(line)) {
                    key.EnglishKey = getTranslationKey(line);
                    catalogKeys.Add(key);
                    key = new CatalogKey();
                }
            }

            return catalogKeys;
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


    class CatalogKey
    {
        public List<string> Locations { get; set; }
        public string EnglishKey { get; set; }

        public CatalogKey()
        {
            Locations = new List<string>();
        }
    }
}
