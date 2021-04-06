using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatSender.model
{
    public class Group
    {

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Response
        {
            [JsonProperty("mask")]
            public bool Mask { get; set; }

            [JsonProperty("opcode")]
            public int Opcode { get; set; }

            [JsonProperty("payloadData")]
            public string PayloadData { get; set; }
        }

        public class Params
        {
            [JsonProperty("requestId")]
            public string RequestId { get; set; }

            [JsonProperty("response")]
            public Response Response { get; set; }

            [JsonProperty("timestamp")]
            public double Timestamp { get; set; }
        }

        public class Message
        {
            [JsonProperty("method")]
            public string Method { get; set; }

            [JsonProperty("params")]
            public Params Params { get; set; }
        }

        public class Root
        {
            [JsonProperty("message")]
            public Message Message { get; set; }

            [JsonProperty("webview")]
            public string Webview { get; set; }
        }

    }
}
