using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace WhatSender
{
    class Config : ConfigurationSection
    {
        private Config() { }

        #region Public Methods

        ///<summary>
        ///Get this configuration set from the application's default config file
        ///</summary>
        public static Config Open()
        {
            System.Reflection.Assembly assy =
                    System.Reflection.Assembly.GetEntryAssembly();
            return Open(assy.Location);
        }

        ///<summary>
        ///Get this configuration set from a specific config file
        ///</summary>
        public static Config Open(string path)
        {
            if ((object)instance == null)
            {
                if (path.EndsWith(".config",
                            StringComparison.InvariantCultureIgnoreCase))
                    spath = path.Remove(path.Length - 7);
                else
                    spath = path;
                Configuration config = ConfigurationManager.OpenExeConfiguration(spath);
                if (config.Sections["Config"] == null)
                {
                    instance = new Config();
                    config.Sections.Add("Config", instance);
                    config.Save(ConfigurationSaveMode.Modified);
                }
                else
                    instance = (Config)config.Sections["Config"];
            }
            return instance;
        }

        ///<summary>
        ///Create a full copy of the current properties
        ///</summary>
        public Config Copy()
        {
            Config copy = new Config();
            string xml = SerializeSection(this,
                    "Config1", ConfigurationSaveMode.Full);
            System.Xml.XmlReader rdr =
                    new System.Xml.XmlTextReader(new System.IO.StringReader(xml));
            copy.DeserializeSection(rdr);
            return copy;
        }

        ///<summary>
        ///Save the current property values to the config file
        ///</summary>
        public void Save()
        {
            // The Configuration has to be opened anew each time we want to 
            // update the file contents.Otherwise, the update of other custom 
            // configuration sections will cause an exception to occur when we 
            // try to save our modifications, stating that another app has 
            // modified the file since we opened it.
            Configuration config = ConfigurationManager.OpenExeConfiguration(spath);
            Config section =
                    (Config)config.Sections["Config"];
            //
            // TODO: Add code to copy all properties from "this" to "section"
            //
            section.DataSourceConfig = this.DataSourceConfig;
            section.APIConfig = this.APIConfig;
            section.WaitingConfig = this.WaitingConfig;
            section.total_counts = this.total_counts;
            config.Save(ConfigurationSaveMode.Modified);
        }

        #endregion Public Methods

        #region Properties

        public static Config Default
        {
            get { return defaultInstance; }
        }

        // TODO: Add your custom properties and elements here.
        // All properties should have both get and set accessors 
        // to implement the Save function correctly
        [ConfigurationProperty("DataSourceSettings")]

        public DataSourceConfig DataSourceConfig
        {
            get
            {
                return (DataSourceConfig)this["DataSourceSettings"];
            }
            set
            {
                this["DataSourceSettings"] = value;
            }
        }

        [ConfigurationProperty("WaitingSettings")]
        public WaitingConfig WaitingConfig
        {
            get
            {
                return (WaitingConfig)this["WaitingSettings"];
            }
            set
            {
                this["WaitingSettings"] = value;
            }
        }
        [ConfigurationProperty("APISettings")]
        public APIConfig APIConfig
        {
            get
            {
                return (APIConfig)this["APISettings"];
            }
            set
            {
                this["APISettings"] =value;
            }
        }
        [ConfigurationProperty("Total")]
        public TotalConfig total_counts
        {
            get
            {
                return (TotalConfig)this["Total"];
            }
            set
            {
               this["Total"]=value;
            }
        }

        #endregion Properties

        #region Fields
        private static string spath;
        private static Config instance = null;
        private static readonly Config defaultInstance = new Config();
        #endregion Fields
    }
}