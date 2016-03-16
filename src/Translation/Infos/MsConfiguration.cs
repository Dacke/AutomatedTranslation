using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AutomatedTranslation.Infos
{
    class MsConfiguration
    {
        public string serviceName { get; set; }
        public string serviceURL { get; set; }
        public string baseURL { get; set; }
        public string locale { get; set; }
        public string referrer { get; set; }
        public string appId { get; set; }
        public int maxNumberOfChars { get; set; }

        public MsConfiguration() : this(null) { }
        public MsConfiguration(string configuation)
        {
            if (string.IsNullOrWhiteSpace(configuation))
                return;

            var propInfos = typeof(MsConfiguration).GetProperties();
            var configItems = configuation.Split(',');
            foreach (var item in configItems)
            {
                var itemPairs = item.Split(':');
                var kvp = new KeyValuePair<string, object>(itemPairs[0], itemPairs[1]);
                var prop = propInfos.SingleOrDefault(p => p.Name.Equals(kvp.Key));
                if (prop == null)
                    continue;

                if (prop.Name.Equals("maxNumberOfChars", StringComparison.CurrentCultureIgnoreCase)) {
                    var maxNum = Int32.Parse(kvp.Value.ToString(), NumberStyles.HexNumber);
                    prop.SetValue(this, maxNum);
                }
                else
                    prop.SetValue(this, Convert.ChangeType(kvp.Value.ToString().Trim('"'), prop.PropertyType));
            }
        }
    }
}
