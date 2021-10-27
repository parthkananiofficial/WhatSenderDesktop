using LumenWorks.Framework.IO.Csv;
using MetroFramework.Controls;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhatSender.model;

namespace WhatSender
{

    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        Config configuration;
        DataTable dt_file_messages = new DataTable();
        DataHelper dataHelper = new DataHelper();
        public Boolean isSending = false;
        
        SeleniumHelperNew objSeleniumHelper;
        //SeleniumHelper objSeleniumHelper;
        int ProcessCounter = 0;
        int sent = 0;
        int failed = 0;
        int queue_total = 0;

        public List<Group> groups = new List<Group>();
        public List<Recipient> recipients = new List<Recipient>();

        FormBrowser formBrowser;

        public MainForm()
        {
            InitializeComponent();

        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            //this.components.SetStyle(this);
            //this.components.SetTheme(this);
            //configuration = ConfigurationManager.GetSection("Config") as Config;
            loadConfig();// load UI based on configuration

            metroGridPending.DataSource = dataHelper.dt_pending_messages;
            metroGridSent.DataSource = dataHelper.dt_sent_messages;
            isSending = false;
            
            objSeleniumHelper = new SeleniumHelperNew();
            //objSeleniumHelper = new SeleniumHelper();

            if (configuration.DataSourceConfig.type == DataSourceConfig.API)
            {
                groupBoxFileUploadConfig.Enabled = false;
                groupBoxAPIConfig.Enabled = true;
            }
            if (configuration.DataSourceConfig.type == DataSourceConfig.FILE)
            {
                groupBoxFileUploadConfig.Enabled = true;
                groupBoxAPIConfig.Enabled = false;
            }
            metroTextBoxSampleFetchedData.Text = File.ReadAllText("sample.json");
            frontEndLogs("Application Started");
            refresh_tiles();

            if (SenderModule.WAPIScript == "")
            {
                MessageBox.Show("Unable to load latest API...");
                this.Close();
            }
            formBrowser = new FormBrowser(this);
            backgroundWorkerUpdateTiles.RunWorkerAsync();

        }
        private void metroRadioAPI_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxAPIConfig.Enabled = metroRadioAPI.Checked;
        }

        private void metroRadioFileUpload_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxFileUploadConfig.Enabled = metroRadioFileUpload.Checked;
        }

        private void metroButtonFileUploadBrowse_Click(object sender, EventArgs e)
        {
            openFileDialogFileUpload.Filter = "CSV files (*.csv)|*.csv|*.xls|*.xlsx|All files (*.*)|*.*";
            openFileDialogFileUpload.Title = "Choose Numbers List..";
            openFileDialogFileUpload.RestoreDirectory = true;
            if (openFileDialogFileUpload.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                metroTextBoxPhonenumbersFile.Text = openFileDialogFileUpload.FileName;
                ImportNumbers(metroTextBoxPhonenumbersFile.Text);
            }
        }

        private void MetroButtonAttachmentBrowse_Click(object sender, EventArgs e)
        {
            openFileDialogAttachment.Filter = "All files (*.*)|*.*";
            openFileDialogAttachment.Title = "Choose Image, Video or File";
            openFileDialogAttachment.RestoreDirectory = true;
            openFileDialogAttachment.ShowDialog();
            metroTextBoxAttachment.Text = openFileDialogAttachment.FileName;
        }
        public void ImportNumbers(string filePath)
        {
            try
            {
                string fileExt = string.Empty;
                fileExt = Path.GetExtension(filePath);
                if (filePath.EndsWith(".xls") || filePath.EndsWith(".xlsx"))
                {
                    try
                    {
                        dt_file_messages = new DataTable();
                        dt_file_messages = dataHelper.ReadExcel(filePath, fileExt); //read excel file

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                        return;
                    }
                }
                else if (filePath.EndsWith(".csv"))
                {
                    // open the file "data.csv" which is a CSV file with headers
                    using (CsvReader csv = new CsvReader(new StreamReader(filePath), true))
                    {
                        //dt_file_messages.Clear();
                        dt_file_messages = new DataTable();
                        dt_file_messages.Load(csv);
                    }
                }
                else
                {
                    MessageBox.Show("Please choose .xls .xlsx or csv file only.", "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //validation started
                if (Convert.ToString(dt_file_messages.Columns[0]).ToLower() != "phone")
                {
                    MessageBox.Show("Invalid First column in File");
                    return;
                }
                if (dt_file_messages.Rows != null && dt_file_messages.Rows.ToString() != String.Empty)
                {
                    metroGridPhoneNumbers.DataSource = dt_file_messages;
                }
                if (metroGridPhoneNumbers.Rows.Count == 0)
                {
                    MessageBox.Show("There is no data in this file", "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception " + ex);
            }
        }

        private void metroRadioButtonMessageText_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxAttachment.Enabled = metroRadioButtonMessageText.Checked ? false : true;
        }

        private void metroRadioButtonMessagePhotoVideo_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxAttachment.Enabled = metroRadioButtonMessagePhotoVideo.Checked;
        }

        private void metroRadioButtonMessageDocument_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxAttachment.Enabled = metroRadioButtonMessageDocument.Checked;
        }

        private void metroButtonStartStop_Click(object sender, EventArgs e)
        {

            if (metroButtonStartStop.Text == "Start")
            {
                frontEndLogs("Starting the task");
                if (configuration.DataSourceConfig.type == DataSourceConfig.API)
                {
                    //dumpDataToWaitingQueue();
                   
                        fetchDataWorker(true);
                 
                }

                mainWorker(true);
                
            }
            else
            {                
                    fetchDataWorker(false);
             

                 mainWorker(false);
                               
            }
        }

        private Boolean mainWorker(bool action)
        {
            if (action)
            {
                if (!formBrowser.IsWAPILoggedIn)
                {
                    MessageBox.Show("Please login into WhatsApp");
                    return false;
                }
                if (formBrowser.IsWAPILoggedIn)
                {
                    backgroundWorkerMain.RunWorkerAsync(2000);
                    //timerWorkerMain.Enabled = true;
                    frontEndLogs("worker:SendMessage Status:Started");
                    metroButtonStartStop.Text = "Stop";
                    metroLabelStatus.Text = "Sending...";
                    return true;
                }
                return false;
            }
            else
            {
                backgroundWorkerMain.Abort();
                backgroundWorkerMain.CancelAsync();
                //timerWorkerMain.Enabled = false;
                metroButtonStartStop.Text = "Start";
                metroLabelStatus.Text = "Stopped";
                frontEndLogs("worker:SendMessage Status:Stopped");
                return true;
            }
        }

        private Boolean fetchDataWorker(bool action)
        {
            if (action)
            {
                if (backgroundWorkerFetchData.IsBusy != true)
                {
                    timerDataFetch.Enabled = true;
                    frontEndLogs("worker:FetchData Status:Started");
                    return true;
                }
                return false;
            }
            else
            {
                if (configuration.DataSourceConfig.type == DataSourceConfig.API)
                {
                    timerDataFetch.Enabled = false;
                    frontEndLogs("worker:FetchData Status:Stopped");
                    return true;
                }
                else
                    return false;
            }

        }



        private void metroButtonSaveConfig_Click(object sender, EventArgs e)
        {
            frontEndLogs("Saving configuration");
            if (metroRadioAPI.Checked)
            {
                configuration.DataSourceConfig.type = DataSourceConfig.API;
                configuration.DataSourceConfig.fetchURL = metroTextBoxFetchURL.Text;
                configuration.DataSourceConfig.statusURL = metroTextBoxStatusURL.Text;
            }
            else if (metroRadioFileUpload.Checked)
            {
                configuration.DataSourceConfig.type = DataSourceConfig.FILE;
            }
            configuration.WaitingConfig.lumpsum_delay = int.Parse(metroTextBoxWait_lumpsum_delay.Text);
            configuration.WaitingConfig.count_of_message = int.Parse(metroTextBox_count_of_message.Text);
            configuration.WaitingConfig.wait_after_every_message = int.Parse(metroTextBox_wait_after_every_message.Text);
            configuration.Save();
            MessageBox.Show("Configuration has been saved Successfully");

            if (configuration.DataSourceConfig.type == DataSourceConfig.FILE && dt_file_messages.Rows.Count > 0)
            {
                if (dataHelper.dt_pending_messages.Rows.Count > 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want to Reload the Queue ?", "Are you Sure ??", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //user wants to reload the queue
                        dataHelper.dt_pending_messages.Clear();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do nothing
                    }
                }
                dumpDataToWaitingQueue();
            }
            else if (configuration.DataSourceConfig.type == DataSourceConfig.API)
            {
                if (dataHelper.dt_pending_messages.Rows.Count > 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want to Reload the Queue ?", "Are you Sure ??", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //user wants to reload the queue
                        dataHelper.dt_pending_messages.Clear();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do nothing
                    }
                }
                dumpDataToWaitingQueue();
            }

            if (isReadyToSend())
            {
                //go to second tab
                metroTabControl.SelectedIndex = 1;
            }
            else
            {
                //check the error
            }

        }
        public void loadConfig()
        {
            configuration = Config.Open();

            if (configuration.DataSourceConfig.type == DataSourceConfig.API)
            {
                metroRadioAPI.Checked = true;
            }
            else if (configuration.DataSourceConfig.type == "file")
            {
                metroRadioFileUpload.Checked = true;
            }

            metroTextBoxFetchURL.Text = configuration.DataSourceConfig.fetchURL;
            metroTextBoxStatusURL.Text = configuration.DataSourceConfig.statusURL;
            metroTextBoxWait_lumpsum_delay.Text = configuration.WaitingConfig.lumpsum_delay.ToString();
            metroTextBox_count_of_message.Text = configuration.WaitingConfig.count_of_message.ToString();
            metroTextBox_wait_after_every_message.Text = configuration.WaitingConfig.wait_after_every_message.ToString();
            frontEndLogs("Configuration Loaded");
        }
        public Boolean isReadyToSend()
        {
            //validate the configurations
            return true;
        }
        public void dumpDataToWaitingQueue()
        {
            if (configuration.DataSourceConfig.type == DataSourceConfig.FILE)
            {
                // copy the data to waiting queue
                frontEndLogs("Dumping " + dt_file_messages.Rows.Count + " Records from:" + configuration.DataSourceConfig.type);
                for (int i = 0; i < dt_file_messages.Rows.Count; i++)
                {
                    DataRow dr = dataHelper.dt_pending_messages.NewRow();
                    dr["Id"] = dt_file_messages.Rows[i]["phone"];
                    dr["Number"] = dt_file_messages.Rows[i]["phone"];
                    dr["Message"] = dataHelper.deduceMessage(dt_file_messages, dt_file_messages.Rows[i], metroTextBoxMessage.Text);
                    dr["Attachment"] = dataHelper.deduceMessage(dt_file_messages, dt_file_messages.Rows[i], metroTextBoxAttachment.Text);
                    dr["Media"] = metroRadioButtonMessagePhotoVideo.Checked;
                    dr["Document"] = metroRadioButtonMessageDocument.Checked;
                    dr["Time"] = DateTime.Now;
                    dataHelper.dt_pending_messages.Rows.Add(dr);
                    queue_total++;
                }

            }
            else if (configuration.DataSourceConfig.type == DataSourceConfig.API)
            {
                // API connection
                //queue_total++;
                string response = WebClientHelper.webGetMethod(configuration.DataSourceConfig.fetchURL);
                try
                {
                    dynamic messagesObject = JsonConvert.DeserializeObject(response);
                    int i = 0;
                    foreach (var item in messagesObject.messages)
                    {
                        DataRow dr = dataHelper.dt_pending_messages.NewRow();
                        dr["Id"] = item.id;
                        dr["Number"] = item.phone;
                        dr["Message"] = item.message;
                        dr["Attachment"] = item.attachment;
                        dr["Media"] = item.isMedia;
                        dr["Document"] = item.isDocument;
                        dr["Time"] = DateTime.Now;
                        dataHelper.dt_pending_messages.Rows.Add(dr);
                        queue_total++;
                        i++;
                    }
                    frontEndLogs("Dumping " + i + " Records from:" + configuration.DataSourceConfig.type);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            //metroGridPending.DataSource = dataHelper.dt_pending_messages;
            //metroGridSent.DataSource = dataHelper.dt_sent_messages;
            refresh_tiles();
            if (configuration.DataSourceConfig.type == DataSourceConfig.API && dataHelper.dt_pending_messages.Rows.Count > 0) // assume that auto start when queue is waiting
            {
                mainWorker(true);
            }
        }



        private Boolean generaterandomBool()
        {
            Random gen = new Random();
            int prob = gen.Next(100);
            return prob <= 20;
        }
        private void refresh_tiles()
        {
            lblPending.Text = dataHelper.dt_pending_messages.Rows.Count.ToString();

            lblTotalFailed.Text = configuration.total_counts.error.ToString();
            lblTotalSuccess.Text = configuration.total_counts.success.ToString();

            lblFailed.Text = failed.ToString();
            lblSuccess.Text = sent.ToString();
        }

        private DataRow WhatsAppToDataRow(WhatsApp whatsapp)
        {
            DataRow dr = null;
            dr["Number"] = whatsapp.Phone;
            dr["Message"] = whatsapp.Message;
            dr["Attachment"] = whatsapp.Attachment;
            dr["Media"] = whatsapp.WithMedia;
            dr["Document"] = whatsapp.WithDocument;
            dr["Time"] = DateTime.Now;
            dr["Status"] = whatsapp.Status;
            return dr;
        }

        private void metroButtonLogin_Click(object sender, EventArgs e)
        {
            formBrowser.Show();
        }

        private void metroButtonLogin_EnabledChanged(object sender, EventArgs e)
        {
            metroButtonStartStop.Enabled = metroButtonLogin.Enabled;
        }

        private void metroGridPending_DataSourceChanged(object sender, EventArgs e)
        {

        }

        private void metroTextBoxStatusURL_KeyUp(object sender, KeyEventArgs e)
        {
            metroTextBoxStatusURLDemo.Text = metroTextBoxStatusURL.Text + "?id={{ID}}&status={{STATUS}}";
        }

        private void metroButtonTestButton_Click(object sender, EventArgs e)
        {
            string response = WebClientHelper.webGetMethod(metroTextBoxFetchURL.Text);
            // var messages = JsonConvert.DeserializeObject<List<WhatsApp>>(messagesObject.messages);

            htmlLabelTestAPIStatus.Visible = true;
            if (WebClientHelper.checkMessageSchema(response))
            {
                htmlLabelTestAPIStatus.Text = "Success !";
            }
            else
            {
                //schema validation failed
                htmlLabelTestAPIStatus.Text = "Invalid Response";
            }
            metroTextBoxSampleFetchedData.Text = response;
        }

        private void backgroundWorkerFetchData_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)//endless
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                //fetch logic will be here



                worker.ReportProgress(10);
                //Thread.Sleep(Convert.ToInt16(configuration.WaitingConfig.lumpsum_delay) * 1000);
                Thread.Sleep(1 * 1000);
            }
        }

        private void backgroundWorkerFetchData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 10)
            {
                if (dataHelper.dt_pending_messages.Rows.Count == 0)
                {
                    frontEndLogs("worker:FetchData Status:CheckIn");
                    dumpDataToWaitingQueue();
                }
            }

        }
        private void frontEndLogs(string str)
        {
            richTextBoxLogs.AppendText(DateTime.Now.ToString("hh:mm:ss.ff") + " " + str + "\n");
        }

        private void backgroundWorkerFetchData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            frontEndLogs("worker:FetchData Status:Completed");
        }
        private WhatsApp WhatsAppValidation(WhatsApp whatsapp)
        {
            //if((whatsapp.Attachment == "" && (whatsapp.WithMedia || whatsapp.WithDocument)) || (whatsapp.Attachment != "" && (!whatsapp.WithMedia && !whatsapp.WithDocument)))
            if (whatsapp.Attachment == "" && (whatsapp.WithMedia || whatsapp.WithDocument))
            {
                whatsapp.Status = false;
                whatsapp.Error = "Attachment Value missing";
            }
            else
            {
                whatsapp.Status = true;
            }

            return whatsapp;
        }

        private void metroButtonExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = "Save Information status";
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dataHelper.dt_sent_messages.WriteToCsvFile(saveFileDialog1.FileName);
                MessageBox.Show("Export completed");
            }
        }

        private void grabGroupButton_Click(object sender, EventArgs e)
        {
            //objSeleniumHelper.getLogs();
        }

        private void metroButtonGrabGroup_Click(object sender, EventArgs e)
        {
            objSeleniumHelper.getLogs();
        }

        //private void backgroundWorkerGroup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{            
        //    if (e.ProgressPercentage == 10)
        //    {
        //        metroLabelGroupCount.Text = objSeleniumHelper.groups.Count().ToString();
        //        metroListViewGroup.Items.Clear();
        //        foreach (Group group in objSeleniumHelper.groups)
        //        {
        //            ListViewItem item = new ListViewItem();
        //            item.Text = group.Subject;
        //            item.SubItems.Add(group.Participants.Count().ToString());
        //            metroListViewGroup.Items.Add(item);
        //        }

        //        metroLabelRecepientCount.Text = objSeleniumHelper.recipients.Count().ToString();
        //        metroListRecipient.Items.Clear();
        //        foreach (Recipient recipient in objSeleniumHelper.recipients)
        //        {
        //            ListViewItem item = new ListViewItem();
        //            item.Text = recipient.Id.Split('@')[0];
        //            metroListRecipient.Items.Add(item);
        //        }
        //    }        
        //}



        private void metroButtonExportRecipient_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = "Save Recipients";
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = "WhatSender Recipient";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                DataTable dt_recipient = new DataTable();
                dt_recipient.Columns.Add("name");
                dt_recipient.Columns.Add("phone");
                foreach (Recipient recipient in recipients)
                {
                    DataRow dr = dt_recipient.NewRow();
                    dr[0] = recipient.Name;
                    dr[1] = recipient.Number;
                    dt_recipient.Rows.Add(dr);
                }

                dt_recipient.WriteToCsvFile(saveFileDialog1.FileName);
                MessageBox.Show("Recipient Export completed");
            }
        }


        private void cbShowLogs_CheckedChanged(object sender, EventArgs e)
        {
            if (cbShowLogs.Checked)
            {
                this.Size = this.MaximumSize;
            }
            else
            {
                this.Size = this.MinimumSize;
            }
        }

        private void metroButtonRefreshGroups_Click(object sender, EventArgs e)
        {
            groups = formBrowser.GetAllGroups();

            metroLabelGroupCount.Text = groups.Count().ToString();
            metroListViewGroup.Items.Clear();
            foreach (Group group in groups)
            {
                ListViewItem item = new ListViewItem();
                item.Text = group.Subject;
                item.SubItems.Add(group.Id);
                metroListViewGroup.Items.Add(item);
            }


            recipients = formBrowser.GetAllContact();

            metroLabelRecipientCount.Text = recipients.Count().ToString();
            metroListRecipient.Items.Clear();
            foreach (Recipient recipient in recipients)
            {
                ListViewItem item = new ListViewItem();
                item.Text = recipient.Name;
                item.SubItems.Add(recipient.Number);
                metroListRecipient.Items.Add(item);
            }
        }

        private void btnExportGroups_Click(object sender, EventArgs e)
        {
            DataTable dt_group = new DataTable();
            dt_group.Columns.Add("phone");
            int i = 0;
            foreach (ListViewItem li in metroListViewGroup.SelectedItems)
            {
                i = 0;
                ToolStripStatusLabel3.Text = "Fetching From:" + li.Text;
                if (li.Selected)
                {
                    CefSharpModule.GroupsParticipant = "";

                    Application.DoEvents();
                    formBrowser.getGroupParticipantIDs(li.SubItems[1].Text);
                    do
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(200);
                        i = i + 1;
                        if (i == 10)
                            break;
                    }
                    while (CefSharpModule.GroupsParticipant == "");

                    List<GroupParticipant> groupParticipants = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GroupParticipant>>(CefSharpModule.GroupsParticipant);

                    if (!Information.IsNothing(groupParticipants))
                    {
                        foreach (GroupParticipant groupParticipant in groupParticipants)
                        {
                            DataRow dr = dt_group.NewRow();
                            dr[0] = groupParticipant.user;
                            dt_group.Rows.Add(dr);
                        }
                    }
                }

            }
            ToolStripStatusLabel3.Text = "";

            //group.Participants
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = "Save Group";
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = metroListViewGroup.SelectedItems[0].Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dt_group.WriteToCsvFile(saveFileDialog1.FileName);
                MessageBox.Show("Group Export completed");
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            //formBrowser.Show();
        }

        private void backgroundWorkerMain_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!backgroundWorkerMain.CancellationPending)
            {
                if (dataHelper.dt_pending_messages.Rows.Count == 0)
                {
                    backgroundWorkerMain.ReportProgress(100);
                }

                for (Int32 i = 0; i < dataHelper.dt_pending_messages.Rows.Count;)
                {
                    if (backgroundWorkerMain.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                    if (i != dataHelper.dt_pending_messages.Rows.Count)
                    {
                        WhatsApp whatsApp = new WhatsApp();
                        whatsApp.Id = dataHelper.dt_pending_messages.Rows[i]["Id"].ToString();
                        whatsApp.Phone = dataHelper.dt_pending_messages.Rows[i]["Number"].ToString();
                        whatsApp.Message = dataHelper.dt_pending_messages.Rows[i]["Message"].ToString();
                        whatsApp.Attachment = dataHelper.dt_pending_messages.Rows[i]["Attachment"].ToString();
                        whatsApp.WithMedia = Convert.ToBoolean(dataHelper.dt_pending_messages.Rows[i]["Media"]);
                        whatsApp.WithDocument = Convert.ToBoolean(dataHelper.dt_pending_messages.Rows[i]["Document"]);
                        whatsApp = WhatsAppValidation(whatsApp);

                        whatsApp.Message = whatsApp.Message.Replace("'", @"\'");
                        whatsApp.Message = whatsApp.Message.Replace(Constants.vbNewLine, @"\n");
                        whatsApp.Message = whatsApp.Message.Replace(Constants.vbTab, @"\t");
                        //whatsApp.Status = true;//generaterandomBool();
                        if (!whatsApp.Status)
                        {
                            //dont' send message if validation failed.
                        }
                        else
                        {
                            //whatsApp = objSeleniumHelper.SendMessage(whatsApp);
                            
                            if (whatsApp.WithMedia)
                            {
                                //whatsApp.Status = formBrowser.SendFile(whatsApp.Attachment, whatsApp.Phone + "@c.us", whatsApp.Message);
                                whatsApp.Status = formBrowser.SendFile(whatsApp.Attachment, whatsApp.Phone + "@c.us", whatsApp.Message);
                            }
                            else if (whatsApp.WithDocument)
                            {
                                whatsApp.Status = formBrowser.SendFile(whatsApp.Attachment, whatsApp.Phone + "@c.us", whatsApp.Message)
                                                    &&
                                                formBrowser.SendMessage(i + ";" + whatsApp.Phone + "@c.us", whatsApp.Message);
                            }
                            else
                            {
                                whatsApp.Status = formBrowser.SendMessage(i + ";" + whatsApp.Phone + "@c.us", whatsApp.Message);
                            }

                        }

                        if (whatsApp.Status)
                        {
                            sent++;
                            configuration.total_counts.success++;
                        }
                        else
                        {
                            failed++;
                            configuration.total_counts.error++;
                        }
                        DataRow dr = dataHelper.dt_sent_messages.NewRow();


                        dr["Number"] = whatsApp.Phone;
                        dr["Message"] = whatsApp.Message;
                        dr["Attachment"] = whatsApp.Attachment;
                        dr["Media"] = whatsApp.WithMedia;
                        dr["Document"] = whatsApp.WithDocument;
                        dr["Time"] = DateTime.Now;
                        dr["Status"] = whatsApp.Status;
                        dr["Error"] = whatsApp.Error;

                        dataHelper.dt_sent_messages.Rows.Add(dr);
                        configuration.Save();

                        if (configuration.DataSourceConfig.type == DataSourceConfig.API)
                        {
                            //create webhook for the status update if any.
                            string url = configuration.DataSourceConfig.statusURL;
                            url = url + "?id=" + whatsApp.Id + "&status=" + whatsApp.Status + "&error=" + whatsApp.Error;
                            string response = WebClientHelper.webGetMethod(url);
                        }
                    }
                    dataHelper.dt_pending_messages.Rows[i].Delete();
                    dataHelper.dt_pending_messages.AcceptChanges();

                    ProcessCounter++;
                    int progress = (int)((sent + failed) * 100 / queue_total);

                    backgroundWorkerMain.ReportProgress(progress);

                    Thread.Sleep(Convert.ToInt16(configuration.WaitingConfig.wait_after_every_message) * 1000);

                    if (ProcessCounter == configuration.WaitingConfig.count_of_message)
                    {
                        Thread.Sleep(Convert.ToInt16(configuration.WaitingConfig.lumpsum_delay) * 1000);
                        ProcessCounter = 0;
                    }

                }
            }
        }

        private void backgroundWorkerMain_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //metroGridSent.Refresh();
            //metroGridPending.Refresh();
            //refresh_tiles();
        }

        private void backgroundWorkerMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            metroButtonStartStop.Text = "Start";

            if (e.Cancelled)
            {
                metroLabelStatus.Text = "Paused";
                MessageBox.Show("Operation was cancelled");
            }

            else if (e.Error != null)
            {
                metroLabelStatus.Text = "Error";
                MessageBox.Show(e.Error.Message);
            }
            else {
                metroLabelStatus.Text = "Ideal";
                //MessageBox.Show(e.Result.ToString());
            }
            metroGridSent.Refresh();
            metroGridPending.Refresh();
            refresh_tiles();
        }

        private void backgroundWorkerUpdateTiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                metroLabelStatus.Text = "Cancelled";
                MessageBox.Show("Operation was cancelled");
            }

            else if (e.Error != null)
            {
                metroLabelStatus.Text = "Error";
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                //MessageBox.Show(e.Result.ToString());
            }
        }
        private void backgroundWorkerUpdateTiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //metroGridSent.Refresh();
            //metroGridPending.Refresh();            
            refresh_tiles();            
        }

        private void backgroundWorkerUpdateTiles_DoWork(object sender, DoWorkEventArgs e)
        {   
            do
            {
                backgroundWorkerUpdateTiles.ReportProgress(10);
                Thread.Sleep(1000);
            }
            while (true);
        }

    }
}

        public static class SenderModule
        {
            public static string WAPIScript = GetWapi();
            public static string GetWapi()
            {
                try
                {
                    WebClient wc = new WebClient();

                    // Return IO.File.ReadAllText("parthkanani.js")
                    return wc.DownloadString("https://versionhash.s3.ap-south-1.amazonaws.com/whatsender.js");
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
        }       
    
