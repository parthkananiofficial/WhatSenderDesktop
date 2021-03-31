using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhatSender
{
   
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        Config configuration;
        DataTable dt_file_messages = new DataTable();
        DataHelper dataHelper = new DataHelper();
        public Boolean isSending = false;
        DataHelper objDataHelper;
        SeleniumHelper objSeleniumHelper;
        int ProcessCounter = 0;
        int sent = 0;
        int failed = 0;
        int queue_total = 0;
        bool isLoggedIn = false;


        public MainForm()
        {
            InitializeComponent();
            this.components.SetStyle(this);
            this.components.SetTheme(this);
            //configuration = ConfigurationManager.GetSection("Config") as Config;
            loadConfig();// load UI based on configuration

            metroGridPending.DataSource = dataHelper.dt_pending_messages;
            metroGridSent.DataSource = dataHelper.dt_sent_messages;
            isSending = false;
            objDataHelper = new DataHelper();
            objSeleniumHelper = new SeleniumHelper();

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
            refresh_tiles();
            frontEndLogs("Application Started");
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
            openFileDialogFileUpload.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialogFileUpload.Title = "Choose Numbers List..";
            openFileDialogFileUpload.RestoreDirectory = true;
            openFileDialogFileUpload.ShowDialog();
            metroTextBoxPhonenumbersFile.Text = openFileDialogFileUpload.FileName;
            ImportNumbers(metroTextBoxPhonenumbersFile.Text);
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
                if (filePath.EndsWith(".csv"))
                {
                    // open the file "data.csv" which is a CSV file with headers
                    using (CsvReader csv = new CsvReader(new StreamReader(filePath), true))
                    {
                        dt_file_messages.Clear();
                        dt_file_messages.Load(csv);
                    }
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
                else
                {
                    MessageBox.Show("Selected File is Invalid, Please Select valid csv file.", "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception " + ex);
            }
        }

        private void metroRadioButtonMessageText_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxAttachment.Enabled = metroRadioButtonMessageText.Checked ? false: true;
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
                if (configuration.DataSourceConfig.type == DataSourceConfig.API && backgroundWorkerFetchData.IsBusy != true)
                {
                    fetchDataWorker(true);
                }

                if (!mainWorker(true))
                {
                    MessageBox.Show("Queue is busy, Try again after sometime!");
                }
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
                if (!isLoggedIn)
                {
                    MessageBox.Show("Please login into WhatsApp");
                    return false;
                }                    
                if (backgroundWorkerMain.IsBusy != true && isLoggedIn)
                {
                    backgroundWorkerMain.RunWorkerAsync();
                    frontEndLogs("worker:SendMessage Status:Started");
                    metroButtonStartStop.Text = "Stop";
                    metroLabelStatus.Text = "Sending...";
                    return true;
                }
                return false;
            }
            else
            {
                if (backgroundWorkerMain.WorkerSupportsCancellation == true)
                {
                    backgroundWorkerMain.CancelAsync();
                    metroButtonStartStop.Text = "Start";
                    metroLabelStatus.Text = "Stopped";
                    frontEndLogs("worker:SendMessage Status:Stopped");
                    return true;
                }
                else
                    return true;
            }
        }

        private Boolean fetchDataWorker(bool action)
        {
            if (action)
            {
                if (backgroundWorkerFetchData.IsBusy != true)
                {
                    backgroundWorkerFetchData.RunWorkerAsync();
                    frontEndLogs("worker:FetchData Status:Started");
                    return true;
                }
                return false;
            }
            else
            {
                if (configuration.DataSourceConfig.type == DataSourceConfig.API && backgroundWorkerFetchData.WorkerSupportsCancellation == true)
                {
                    backgroundWorkerFetchData.CancelAsync();
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

            if(configuration.DataSourceConfig.type == DataSourceConfig.FILE && dt_file_messages.Rows.Count >0)
            {
                if(objDataHelper.dt_pending_messages.Rows.Count > 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want to Reload the Queue ?", "Are you Sure ??", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //user wants to reload the queue
                        objDataHelper.dt_pending_messages.Clear();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do nothing
                    }
                }
                dumpDataToWaitingQueue();
            }
            else if (configuration.DataSourceConfig.type == DataSourceConfig.API) {
                if (objDataHelper.dt_pending_messages.Rows.Count > 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want to Reload the Queue ?", "Are you Sure ??", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //user wants to reload the queue
                        objDataHelper.dt_pending_messages.Clear();
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do nothing
                    }
                }
                dumpDataToWaitingQueue();
            }

            if(isReadyToSend())
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
                for(int i=0;i<dt_file_messages.Rows.Count;i++)
                {
                    DataRow dr= objDataHelper.dt_pending_messages.NewRow();
                    dr["Id"] = dt_file_messages.Rows[i]["phone"];
                    dr["Number"] = dt_file_messages.Rows[i]["phone"];
                    dr["Message"] = objDataHelper.deduceMessage(dt_file_messages, dt_file_messages.Rows[i], metroTextBoxMessage.Text);
                    dr["Attachment"] = objDataHelper.deduceMessage(dt_file_messages, dt_file_messages.Rows[i], metroTextBoxAttachment.Text);
                    dr["Media"] = metroRadioButtonMessagePhotoVideo.Checked;
                    dr["Document"] = metroRadioButtonMessageDocument.Checked;
                    dr["Time"] = DateTime.Now;
                    objDataHelper.dt_pending_messages.Rows.Add(dr);
                    queue_total++;
                }

            }
            else if(configuration.DataSourceConfig.type == DataSourceConfig.API)
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
                        DataRow dr = objDataHelper.dt_pending_messages.NewRow();
                        dr["Id"] = item.id;
                        dr["Number"] = item.phone;
                        dr["Message"] = item.message;
                        dr["Attachment"] = item.attachment;
                        dr["Media"] = item.isMedia;
                        dr["Document"] = item.isDocument;
                        dr["Time"] = DateTime.Now;
                        objDataHelper.dt_pending_messages.Rows.Add(dr);
                        queue_total++;
                        i++;
                    }
                    frontEndLogs("Dumping " + i + " Records from:" + configuration.DataSourceConfig.type);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            metroGridPending.DataSource = objDataHelper.dt_pending_messages;
            metroGridSent.DataSource = objDataHelper.dt_sent_messages;
            refresh_tiles();
            if (configuration.DataSourceConfig.type == DataSourceConfig.API) // assume that auto start when queue is waiting
            {
                mainWorker(true);
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (Int32 i = 0; i < objDataHelper.dt_pending_messages.Rows.Count;)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                if (i != objDataHelper.dt_pending_messages.Rows.Count)
                {
                    WhatsApp whatsApp = new WhatsApp();
                    whatsApp.Id = objDataHelper.dt_pending_messages.Rows[i]["Id"].ToString();
                    whatsApp.Phone = objDataHelper.dt_pending_messages.Rows[i]["Number"].ToString();
                    whatsApp.Message = objDataHelper.dt_pending_messages.Rows[i]["Message"].ToString();
                    whatsApp.Attachment = objDataHelper.dt_pending_messages.Rows[i]["Attachment"].ToString();
                    whatsApp.WithMedia = Convert.ToBoolean(objDataHelper.dt_pending_messages.Rows[i]["Media"]);
                    whatsApp.WithDocument = Convert.ToBoolean(objDataHelper.dt_pending_messages.Rows[i]["Document"]);
                    whatsApp = WhatsAppValidation(whatsApp);
                    //whatsApp.Status = true;//generaterandomBool();
                    if(!whatsApp.Status)
                    {
                        //dont' send message if validation failed.
                    }
                    else
                    {
                        whatsApp = objSeleniumHelper.SendMessage(whatsApp);
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
                    DataRow dr = objDataHelper.dt_sent_messages.NewRow();


                    dr["Number"] = whatsApp.Phone;
                    dr["Message"] = whatsApp.Message;
                    dr["Attachment"] = whatsApp.Attachment;
                    dr["Media"] = whatsApp.WithMedia;
                    dr["Document"] = whatsApp.WithDocument;
                    dr["Time"] = DateTime.Now;
                    dr["Status"] = whatsApp.Status;
                    dr["Error"] = whatsApp.Error;

                    objDataHelper.dt_sent_messages.Rows.Add(dr);
                    configuration.Save();

                    if (configuration.DataSourceConfig.type == DataSourceConfig.API)
                    {
                        //create webhook for the status update if any.
                        string url = configuration.DataSourceConfig.statusURL;
                        url = url + "?id=" + whatsApp.Id + "&status=" + whatsApp.Status + "&error=" + whatsApp.Error;
                        string response = WebClientHelper.webGetMethod(url);
                    }
                }
                objDataHelper.dt_pending_messages.Rows[i].Delete();
                objDataHelper.dt_pending_messages.AcceptChanges();

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

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {            
            metroGridSent.Refresh();
            metroGridPending.Refresh();
            refresh_tiles();
            if (e.ProgressPercentage == 0)
            {
                
            }
            else if (e.ProgressPercentage == 100)
            {
                //metroButtonStartStop.Text = "Start";
                metroLabelStatus.Text = "Completed";
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
            metroTilePending.TileCount = objDataHelper.dt_pending_messages.Rows.Count;

            metroTileTotalFailed.TileCount = configuration.total_counts.error;

            metroTileTotalSent.TileCount = configuration.total_counts.success;

            metroTileFailed.TileCount = failed;
            metroTileSent.TileCount = sent;

            metroTilePending.Refresh();
            metroTileTotalFailed.Refresh();
            metroTileTotalSent.Refresh();
            metroTileFailed.Refresh();
            metroTileSent.Refresh();
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
            objSeleniumHelper.CreateSession();
            if (metroButtonLogin.Text == "Login")
            {
                if (backgroundWorkerCheckLoggedIn.IsBusy != true)
                {
                    backgroundWorkerCheckLoggedIn.RunWorkerAsync();
                }
            }
        }

        private void backgroundWorkerCheckLoggedIn_DoWork(object sender, DoWorkEventArgs e)
        {
            while(!isLoggedIn)
            {
                isLoggedIn = objSeleniumHelper.IsLoggedIn();
            }
        }

        private void backgroundWorkerCheckLoggedIn_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            metroButtonLogin.Text = "Logged In";
            metroButtonLogin.Enabled = false;
        }

        private void metroButtonLogin_EnabledChanged(object sender, EventArgs e)
        {
            metroButtonStartStop.Enabled = !metroButtonLogin.Enabled;
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

        private void backgroundWorkerMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            frontEndLogs("worker:SendMessage Status:Completed");
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
                Thread.Sleep(Convert.ToInt16(configuration.WaitingConfig.lumpsum_delay) * 1000);
            }
        }

        private void backgroundWorkerFetchData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 10)
            {
                frontEndLogs("worker:FetchData Status:CheckIn");
                dumpDataToWaitingQueue();
            }
            
        }
        private void frontEndLogs(string str)
        {
            richTextBoxLogs.AppendText(DateTime.Now.ToString("hh:mm:ss.ff") + " "+ str +"\n");
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
                objDataHelper.dt_sent_messages.WriteToCsvFile(saveFileDialog1.FileName);
                MessageBox.Show("Export completed");
            }
        }
    }
}
