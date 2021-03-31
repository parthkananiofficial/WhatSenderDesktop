using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WhatSender
{
    public class WebClientHelper
    {
        public static string webGetMethod(string URL)
        {
            try {
                string jsonString = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
                //request.Accept = "/";
                request.ContentType = "application/json";
                WebResponse response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                jsonString = sr.ReadToEnd();
                sr.Close();
                return jsonString;
            }
            catch(Exception e)
            {
                return e.Message;
            }            
        }

        public static bool checkMessageSchema(String payload)
        {
            try
            {
                JSchema schema = JSchema.Parse(File.ReadAllText("messageSchema.json"));
                JObject messages = JObject.Parse(payload);
                return messages.IsValid(schema);
            }catch(Exception ex)
            {
                return false;
            }
        }
        public static bool isValidJSON(string json)
        {
            try {
                JObject.Parse(json);
            }
            catch (JsonSchemaException ex)
            {
                return false;
            }catch(Exception ex)
            {                
                return false;
            }
            return true;
        }
    }
}
