using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatSender.model
{
     public class Recipient
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class AllRecipients
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("recipients")]
        public List<Recipient> Recipients { get; set; }
    }
}
