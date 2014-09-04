using System.Collections.Generic;

namespace AutomatedTranslation
{
    class TranslatedValue
    {
        public List<string> Locations { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsNew { get; set; }

        public TranslatedValue()
        {
            Locations = new List<string>();
        }

        public override string ToString()
        {
            return string.Format("IsNew: {0}, Value: {1}", IsNew, Value);
        }
    }
}