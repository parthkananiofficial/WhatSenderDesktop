
namespace WhatSender
{
    partial class FormBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TimerIsWhatsAppLoggedIn = new System.Windows.Forms.Timer(this.components);
            this.TimerEvent = new System.Windows.Forms.Timer(this.components);
            this.TimerInitiateWAPI = new System.Windows.Forms.Timer(this.components);
            this.TimerReceive = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // TimerIsWhatsAppLoggedIn
            // 
            this.TimerIsWhatsAppLoggedIn.Enabled = true;
            this.TimerIsWhatsAppLoggedIn.Interval = 1000;
            this.TimerIsWhatsAppLoggedIn.Tick += new System.EventHandler(this.TimerIsWhatsAppLoggedIn_Tick);
            // 
            // TimerEvent
            // 
            this.TimerEvent.Enabled = true;
            this.TimerEvent.Interval = 1000;
            this.TimerEvent.Tick += new System.EventHandler(this.TimerEvent_Tick);
            // 
            // TimerInitiateWAPI
            // 
            this.TimerInitiateWAPI.Enabled = true;
            this.TimerInitiateWAPI.Interval = 1000;
            this.TimerInitiateWAPI.Tick += new System.EventHandler(this.TimerInitiateWAPI_Tick);
            // 
            // TimerReceive
            // 
            this.TimerReceive.Interval = 1000;
            this.TimerReceive.Tick += new System.EventHandler(this.TimerReceive_Tick);
            // 
            // FormBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1301, 745);
            this.Name = "FormBrowser";
            this.Text = "WhatsApp";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBrowser_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormBrowser_FormClosed);
            this.Load += new System.EventHandler(this.FormBrowser_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Timer TimerIsWhatsAppLoggedIn;
        public System.Windows.Forms.Timer TimerEvent;
        public System.Windows.Forms.Timer TimerInitiateWAPI;
        public System.Windows.Forms.Timer TimerReceive;
    }
}