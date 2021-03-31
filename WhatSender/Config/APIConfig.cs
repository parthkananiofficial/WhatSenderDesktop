using System.Configuration;

namespace WhatSender
{
    public class APIConfig : ConfigurationElement
    {
        [ConfigurationProperty("udlfilelocation", DefaultValue = "", IsRequired = true)]
        public string udlfilelocation
        {
            get
            {
                return (string)this["udlfilelocation"];
            }
            set
            {
                this["udlfilelocation"] = value;
            }
        }

    }
}