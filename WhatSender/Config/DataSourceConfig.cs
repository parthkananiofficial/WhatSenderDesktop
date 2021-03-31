using System.Configuration;

namespace WhatSender
{
    public class DataSourceConfig : ConfigurationElement
    {
        public static string API = "api";
        public static string FILE = "file";

        [ConfigurationProperty("type", DefaultValue = "api", IsRequired = true)]
        public string type {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        } 
        
        [ConfigurationProperty("fetchURL", DefaultValue = "https://jsonkeeper.com/b/Z1OL", IsRequired = true)]
        public string fetchURL {
            get
            {
                return (string)this["fetchURL"];
            }
            set
            {
                this["fetchURL"] = value;
            }
        }
                
        [ConfigurationProperty("statusURL", DefaultValue = "https://jsonkeeper.com/b/Z1OL", IsRequired = true)]
        public string statusURL {
            get
            {
                return (string)this["statusURL"];
            }
            set
            {
                this["statusURL"] = value;
            }
        }

    }
}