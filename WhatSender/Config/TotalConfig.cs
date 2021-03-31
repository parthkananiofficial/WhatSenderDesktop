using System.Configuration;

namespace WhatSender
{
    internal class TotalConfig : ConfigurationElement
    {
        [ConfigurationProperty("success", DefaultValue = 0, IsRequired = true)]
        public int success
        {
            get
            {
                return (int)this["success"];
            }
            set
            {
                this["success"] = value;
            }
        }
        [ConfigurationProperty("error", DefaultValue = 0, IsRequired = true)]
        public int error
        {
            get
            {
                return (int)this["error"];
            }
            set
            {
                this["error"]= value;
            }
        }
    }
}