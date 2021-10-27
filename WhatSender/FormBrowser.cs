using CefSharp;
using CefSharp.WinForms;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhatSender.model;

namespace WhatSender
{    
    public partial class FormBrowser : MetroFramework.Forms.MetroForm
    {
        public ChromiumWebBrowser CefBrowser;

        public CefSettings CefBrowserSettings = new CefSettings();
        
        public string loginResult;
        public bool IsWhatsAppLogginIn;
        public bool IsWAPILoggedIn;
        MainForm objMainForm;
        public List<Group> groups = new List<Group>();
        public List<Recipient> recipients = new List<Recipient>();

        public struct WAPI
        {
            public static bool IsWAPILoggedIn;
            public static bool IsWAPIConnected;
        }
        public FormBrowser(MainForm mainForm)
        {
            InitializeComponent();
            objMainForm = mainForm;

            try
            {
                if (Browser.Profile != "")
                    InitializeCef(Browser.GetProfiles() + Browser.Profile);
                else
                    InitializeCef(Browser.GetProfiles() + Application.ProductName);
                CefBrowser = new ChromiumWebBrowser();

                this.Controls.Add(CefBrowser);
                CefBrowser.Dock = DockStyle.Fill;
                CefBrowser.Load("https://web.whatsapp.com");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("CefSharp"))
                {
                    DialogResult dialogResult = MessageBox.Show("Sure", "Application need Visual C++ Redistributable for Visual Studio 2015 to work proprely, do you want download it?", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Process.Start("https://www.microsoft.com/en-in/download/details.aspx?id=48145");
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do something else
                    }
                }
            }
        }

        public void InitializeCef(string Profile = "")
        {
            if (Profile != "")
            {
                try
                {
                    CefBrowserSettings = new CefSettings();
                    CefBrowserSettings.RemoteDebuggingPort = 8080;
                    CefBrowserSettings.UserDataPath = Profile;
                    CefBrowserSettings.CachePath = Profile;
                    CefBrowserSettings.CefCommandLineArgs.Add("user-data-dir", Profile);
                    CefSharp.Cef.Initialize(CefBrowserSettings);
                }
                catch (Exception ex)
                {
                }
            }
        }
        private void FormBrowser_Load(object sender, EventArgs e)
        {
            
        }

        private void FormBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            CefBrowser.Dispose();
            this.Dispose();
        }

        private void FormBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void TimerEvent_Tick(object sender, EventArgs e)
        {
            if (IsWAPILoggedIn)
            {
                objMainForm.lblReady.BackColor = Color.DarkSeaGreen;
                objMainForm.lblReady.Text = "ONLINE";
            }
            else
            {
                objMainForm.lblReady.BackColor = Color.OrangeRed;
                objMainForm.lblReady.Text = "OFFLINE";
                if(objMainForm.timerWorkerMain.Enabled)
                {
                    objMainForm.timerWorkerMain.Enabled = false;
                }
            }
        }

        private void TimerInitiateWAPI_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("IsWhatsAppLogginIn :" + IsWhatsAppLogginIn.ToString() + " IsWAPILoggedIn :" + IsWAPILoggedIn.ToString());
            if (IsWhatsAppLogginIn)
            {
                TimerInitiateWAPI.Enabled = false;
                for (var i = 1; i <= 300; i++)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }

                CefBrowser.ExecuteScriptAsync(SenderModule.WAPIScript);
                Console.WriteLine("WAPI binded completely");
                CefBrowser.JavascriptObjectRepository.Register("boundAsync", new CefSharpJavascriptObj(), true);
                //TimerReceive.Enabled = true;
            }
        }

        private async void TimerIsWhatsAppLoggedIn_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("IsWhatsAppLogginIn :" + IsWhatsAppLogginIn.ToString() + " IsWAPILoggedIn :" + IsWAPILoggedIn.ToString());
            try
            {
                var a = await CefBrowser.EvaluateScriptAsync("document.getElementsByClassName('" + webConfig.loginTag + "').length");
                if(Convert.ToInt32(a.Result)>0)
                    IsWhatsAppLogginIn = true;
                else
                    IsWhatsAppLogginIn = false;
            }
            catch (Exception ex)
            {
                IsWhatsAppLogginIn = false;
            }
            try
            {
                //Console.WriteLine("WAPI called");
                var b = await CefBrowser.EvaluateScriptAsync("var a; WAPI.isLoggedIn(a)");

                IsWAPILoggedIn = System.Convert.ToBoolean(b.Result);
            }
            catch (Exception ex)
            {
            }
        }

        private void TimerReceive_Tick(object sender, EventArgs e)
        {

        }

        public List<Recipient> GetAllContact()
        {

            try
            {
                dynamic ContactResult = CefBrowser.EvaluateScriptAsync("var result; WAPI.getAllContacts(result)");
                ContactResult.Wait();
                recipients = new List<Recipient>();
                foreach (dynamic contact in ContactResult.Result.Result)
                {
                    Application.DoEvents();
                    // MsgBox(Contact.isUser)
                    if (contact.id.server.ToString() == "c.us")
                    {
                        Recipient recipient = new Recipient();
                        recipient.Name = contact.formattedName;
                        recipient.Id = contact.id._serialized;
                        recipient.Number = contact.id.user;
                        recipients.Add(recipient);
                    }
                }
                return recipients;
            }
            catch (Exception ex)
            {
                return new List<Recipient>();
            }
        }
        public List<Group> GetAllGroups()
        {
            try
            {
                dynamic ContactResult = CefBrowser.EvaluateScriptAsync("var result; WAPI.getAllContacts(result)");
                ContactResult.Wait();
                groups = new List<Group>();
                foreach (dynamic contact in ContactResult.Result.Result)
                {
                    Application.DoEvents();
                    // MsgBox(Contact.isUser)
                    if (contact.id.server.ToString() == "g.us")
                    {
                        Group group = new Group();
                        group.Subject = contact.formattedName;
                        group.Id = contact.id._serialized;
                        group.Owner = contact.id.user;
                        groups.Add(group);
                    }
                }
                return groups;
            }
            catch (Exception ex)
            {
                return new List<Group>();
            }
        }
        public void CheckWhatsAppAccount(string WhatsAppAccount)
        {
            try
            {
                CefBrowser.ExecuteScriptAsync("WAPI.checkNumberStatus('" + WhatsAppAccount + "',(async function(result) {await CefSharp.BindObjectAsync('boundAsync','bound');boundAsync.checkNumberStatus(result);}))");
            }
            catch (Exception ex)
            {
            }
        }

        public bool SendMessage(string WhatsAppAccount, string Message)
        {
            try
            {
                string[] a = Strings.Split(WhatsAppAccount, ";");
                Message = Message.Replace("'", @"\'");
                Message = Message.Replace(Constants.vbNewLine, @"\n");
                CefBrowser.ExecuteScriptAsync("WAPI.sendMessageToID('" + a[1] + "','" + Message + "',(async function(result) {await CefSharp.BindObjectAsync('boundAsync','bound');boundAsync.sendMessage('" + a[0] + "' + ';' + result);}))");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void getGroupParticipantIDs(string id)
        {
            try
            {                
                CefBrowser.ExecuteScriptAsync("WAPI.getGroupParticipantIDs('" + id + "', (async function(result) { await CefSharp.BindObjectAsync('boundAsync','bound'); boundAsync.getGroupParticipantIDs(result);}))");
            }
            catch (Exception ex)
            {
                
            }            
        }
        public void SendSeen(string id)
        {
            try
            {
                CefBrowser.ExecuteScriptAsync("WAPI.sendSeen('" + id + "')");
            }
            catch (Exception ex)
            {
            }
        }
        // Public Async Sub getGroupParticipantIDs(ByVal id As String)
        // Dim a As Task(Of JavascriptResponse) = CefBrowser.EvaluateScriptAsync("WAPI.getGroupParticipantIDs('97455512686-1452990249@g.us');")
        // Await a

        // MsgBox(Newtonsoft.Json.JsonConvert.SerializeObject(a.Result.Result))

        // End Sub
        public void SendMessageToID(string WhatsAppAccount, string Message)
        {
            try
            {
                Message = Message.Replace("'", @"\'");
                Message = Message.Replace(Constants.vbNewLine, @"\n");
                CefBrowser.ExecuteScriptAsync("WAPI.sendMessageToID('" + WhatsAppAccount + "','" + Message + "')");
            }
            catch (Exception ex)
            {
            }
        }
        public bool SendFile(string FileName, string WhatsAppAccount, string Caption)
        {
            try
            {
                ClsBase64 Base64converter = new ClsBase64();
                string Base64File = Base64converter.ConvertFileToBase64(FileName);
                string[] a = Strings.Split(FileName, @"\");
                Caption = Caption.Replace(Constants.vbNewLine, @"\n");
                Caption = Caption.Replace("'", @"\'");
                string _filename = a[Information.UBound(a)];
                string js = "WAPI.sendImage('" + Base64File + "','" + WhatsAppAccount + "','" + _filename + "','" + Caption + "')";
                CefBrowser.ExecuteScriptAsync(js);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void SendStickers(string FileName, string WhatsAppAccount)
        {
            try
            {
                ClsBase64 Base64converter = new ClsBase64();
                string Base64File = Base64converter.ConvertFileToBase64NoMime(FileName);
                CefBrowser.ExecuteScriptAsync("WAPI.sendImageAsSticker('" + Base64File + "','" + WhatsAppAccount + "')");
            }
            catch (Exception ex)
            {
            }
        }
        public async Task<string> SendMessageAsync(string Message, string Destination)
        {
            try
            {
                var sendResult = await CefBrowser.EvaluateScriptAsync("var a; WAPI.sendMessage('" + Destination + "@c.us','" + Message + "',a);");
                return sendResult.Result.ToString();
            }
            catch (Exception ex)
            {
                Interaction.MsgBox(ex.Message);
                return "";
            }
        }
    }
    static class Browser
    {
        public static string Profile;
        public static string GetProfiles()
        {
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\whatsender\Profiles"))
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\whatsender\Profiles");
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\whatsender\Profiles\";
        }
    }

    public class CefSharpJavascriptObj
    {
        public void checkNumberStatus(object e)
        {
            CefSharpModule.NumberCheckedList = Newtonsoft.Json.JsonConvert.SerializeObject(e) + "|" + CefSharpModule.NumberCheckedList;
        }
        public void sendMessage(object e)
        {
            CefSharpModule.SentMessageslist = CefSharpModule.SentMessageslist + "|" + Newtonsoft.Json.JsonConvert.SerializeObject(e);
        }
        public void getGroupParticipantIDs(object e)
        {
            CefSharpModule.GroupsParticipant = Newtonsoft.Json.JsonConvert.SerializeObject(e);
        }
    }
    static class CefSharpModule
    {
        public static string NumberCheckedList;
        public static string SentMessageslist;
        public static string ReceivedMessages;
        public static string allReceivedMessage;
        public static string GroupsParticipant;
    }
    static class webConfig
    {
        public static string loginTag = "_1lPgH";
    }
}
