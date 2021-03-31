using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatSender
{
    public class WhatsApp
    {
        private String id;
        public string Id
        {
            get;set;
        }

        private String message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private String phone;
        public string Phone
        {
            get { return phone; }
            set { phone = value; }
        }
        private String attachment;
        public string Attachment
        {
            get { return attachment; }
            set { attachment = value; }
        }
        private Boolean withMedia;
        public Boolean WithMedia
        {
            get { return withMedia; }
            set { withMedia = value; }
        }

        private Boolean withDocument;
        public Boolean WithDocument
        {
            get { return withDocument; }
            set { withDocument = value; }
        }
        private String timeSent;
        public string TimeSent
        {
            get { return timeSent; }
            set { timeSent = value; }
        }

        private Boolean status;
        public Boolean Status
        {
            get { return status; }
            set { status = value; }
        }
        private String error;
        public string Error
        {
            get { return error; }
            set { error = value; }
        }
    }
}
