using System;
using System.IO;
using System.Xml.Serialization;
using InfoParser.Models;

namespace DodjiParser.Models
{
    public sealed class Configuration
    {
        #region Access logic

        private const string CONFIGURATION_FILE_NAME = "configuration.xml";

        private static readonly Lazy<Configuration> _lazy = new Lazy<Configuration>(Load);

        public static Configuration Instance => _lazy.Value;

        private static Configuration Load()
        {
            var fi = new FileInfo(CONFIGURATION_FILE_NAME);

            if (!fi.Exists)
            {
                var conf = new Configuration();
                conf.Save();
                return conf;
            }

            var ser = new XmlSerializer(typeof(Configuration));
            using (var fs = new FileStream(CONFIGURATION_FILE_NAME, FileMode.Open))
            {
                return ser.Deserialize(fs) as Configuration;
            }
        }

        private Configuration()
        {
        }

        public void Save()
        {
            var ser = new XmlSerializer(typeof(Configuration));

            using (var fs = new FileStream(CONFIGURATION_FILE_NAME, FileMode.OpenOrCreate))
            {
                ser.Serialize(fs, this);
            }
        }

        #endregion Access logic

        public ExhentaiConfiguration ExhentaiConfiguration { get; set; }
    }
}
