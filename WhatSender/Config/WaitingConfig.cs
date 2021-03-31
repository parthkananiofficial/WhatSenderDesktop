using System.Configuration;

namespace WhatSender
{
    internal class WaitingConfig : ConfigurationElement
    {
        [ConfigurationProperty("lumpsum_delay", DefaultValue = 4, IsRequired = true)]
        public int lumpsum_delay
        {
            get
            {
                return (int)this["lumpsum_delay"];
            }
            set
            {
                this["lumpsum_delay"] = value;
            }
        }

        [ConfigurationProperty("count_of_message", DefaultValue = 10, IsRequired = true)]
        public int count_of_message
        {
            get
            {
                return (int)this["count_of_message"];
            }
            set
            {
               this["count_of_message"] = value;
            }
        }

        [ConfigurationProperty("wait_after_every_message", DefaultValue = 5, IsRequired = true)]
        public int wait_after_every_message
        {
            get
            {
                return (int)this["wait_after_every_message"];
            }
            set
            {
                this["wait_after_every_message"] = value;
            }
        }
    }
}