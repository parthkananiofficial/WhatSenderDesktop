using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatSender.model
{
    public class Participant
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("isSuperAdmin")]
        public bool IsSuperAdmin { get; set; }
    }

    public class Group
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("creation")]
        public int Creation { get; set; }

        [JsonProperty("participants")]
        public List<Participant> Participants { get; set; }

        [JsonProperty("subjectTime")]
        public int SubjectTime { get; set; }

        [JsonProperty("subjectOwner")]
        public string SubjectOwner { get; set; }
    }


}
