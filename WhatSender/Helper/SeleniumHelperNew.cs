using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WhatSender.model;

namespace WhatSender
{
    class SeleniumHelperNew
    {
        IWebDriver Driver;

        //check login
        string MENU_BUTTON = "//*[@data-testid='menu']";

        string INVALID_NUMBER = "/html/body/div/div[1]/span[2]/div[1]/span/div[1]/div/div/div/div/div[1]";


        //Attachment
        string CLIP_BUTTON = "span[data-icon='clip']";
        string DOCUMENT_BUTTON_XPATH = "//*[@data-testid='attach-document']";
        string VIDEO_OR_PICTURE_BUTTON = "//*[@data-testid='attach-image']";

        //document send
        string FILE_SEND_BUTTON = "/html/body/div/div[1]/div[1]/div[2]/div[2]/span/div[1]/span/div[1]/div/div[2]/span/div";
        string DOCUMENT_FILE_Xpath = "/html/body/div/div[1]/div[1]/div[4]/div[1]/footer/div[1]/div[1]/div[2]/div/span/div[1]/div/ul/li[3]/button/input";


        //image send
        string IMAGE_SEND_BUTTON = "/html/body/div/div[1]/div[1]/div[2]/div[2]/span/div[1]/span/div[1]/div/div[2]/div/div[2]/div[2]/div";   
        //string IMAGE_SEND_BUTTON = "/html/body/div/div[1]/div[1]/div[2]/div[2]/span/div[1]/span/div[1]/div/div[2]/span/div";   
        string VIDEO_OR_PICTURE_FILE = "/html/body/div/div[1]/div[1]/div[4]/div[1]/footer/div[1]/div[1]/div[2]/div/span/div[1]/div/ul/li[1]/button/input";        

        //text send
        string SEND_BUTTON = "/html/body/div/div[1]/div[1]/div[4]/div[1]/footer/div[1]/div[2]/div/div[2]/button";


        Boolean status;
        Boolean isLoggedIn = false;
        int waiting_time = 20;


        public List<Group> groups = new List<Group>();
        public List<Recipient> recipients = new List<Recipient>();
        public SeleniumHelperNew()
        {
            //CreateSession();
        }
        public void CreateSession()
        {
            

            var driverService = ChromeDriverService.CreateDefaultService();
            //var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            LogHelper.Write("Creating new session");


            //FirefoxOptions options = new FirefoxOptions();
            ChromeOptions options = new ChromeOptions();
            options.SetLoggingPreference("performance", LogLevel.All);

            try
            {
                Driver = new ChromeDriver(driverService, options);
                //Driver = new FirefoxDriver(driverService, options);
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                Driver.Navigate().GoToUrl("https://web.whatsapp.com");
            }
            catch (Exception excp)
            {
                MessageBox.Show(excp.Message);
            }
        }


        public void getLogs()
        {
            try
            {
                var logs = Driver.Manage().Logs.GetLog("performance");
                foreach (var log in logs)
                {
                    if(log.Message.Contains("participants") || log.Message.Contains("recipients")) //this is for group only
                    {
                        GroupLog groupLog = JsonConvert.DeserializeObject<GroupLog>(log.Message);

                        String payload = groupLog.Message.Params.Response.PayloadData;
                        payload = payload.Substring(payload.IndexOf(',') + 1);

                        if (log.Message.Contains("participants"))
                        {

                            Group group = JsonConvert.DeserializeObject<Group>(payload);
                            if (!isGroupExist(group))
                                groups.Add(group);
                        }
                        if (log.Message.Contains("recipients"))
                        {
                            AllRecipients allRecipients = JsonConvert.DeserializeObject<AllRecipients>(payload);
                            foreach(Recipient recipient in allRecipients.Recipients)
                            {
                                if (!isRecipientExist(recipient))
                                    recipients.Add(recipient);
                            }                            
                        }
                        Console.WriteLine(log.Message);
                    }
                    //Console.WriteLine(entry.ToString());
                }
            }
            catch (Exception excp)
            {
                //MessageBox.Show(excp.Message);
            }
        }

        public bool isGroupExist(Group group)
        {
            bool found = false;
            foreach(Group g in groups)
            {
                if (g.Id.Equals(group.Id))
                    return true;
            }
            return found;
        }
        public bool isRecipientExist(Recipient recipient)
        {
            bool found = false;
            foreach (Recipient r in recipients)
            {
                if (r.Id.Equals(recipient.Id))
                    return true;
            }
            return found;
        }


        public Boolean IsLoggedIn()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waiting_time+500));
                wait.Until(c => c.FindElement(By.XPath(MENU_BUTTON)));
            }
            catch (NoSuchElementException)
            {
                isLoggedIn = false;
                return false;
            }
            isLoggedIn = true;
            return true;
        }

        public WhatsApp SendMessage(WhatsApp whatsApp)
        {
            LogHelper.Write("Phone :" + whatsApp.Phone + "\n Message : " + whatsApp.Message);
            Driver.Navigate().GoToUrl("https://web.whatsapp.com/send?phone=" + whatsApp.Phone + "&text=" + Uri.EscapeDataString(whatsApp.Message) + "&source=&data=");
                        
            if (!isNumberValid())
            {
                whatsApp.Status = false;
                whatsApp.Error = "Number is Invalid";
            }
            else
            {
                if (!whatsApp.WithMedia && !whatsApp.WithDocument)//Sending Text Message
                {
                    whatsApp.Status = SendText(whatsApp);
                }
                else if (whatsApp.WithMedia)//Sending Image or Video 
                {
                    if (SendImageOrVideo(whatsApp))
                    {
                        whatsApp.Status = true;
                    }
                    else
                    {
                        whatsApp.Status = false;
                    }
                }
                else if (whatsApp.WithDocument) //Sending File
                {
                    if (SendFile(whatsApp) && SendText(whatsApp))
                    {
                        whatsApp.Status = true;
                    }
                    else
                    {
                        whatsApp.Status = false;
                    }
                }
            }            
            return whatsApp;
        }

        public Boolean SendText(WhatsApp whatsApp)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waiting_time));
                wait.Until(c => c.FindElement(By.XPath(SEND_BUTTON)));

                Driver.FindElement(By.XPath(SEND_BUTTON)).Click();
                //Driver.FindElement(By.ClassName("_2Ujuu")).Click();//Whatsapp send button click
                return true;
            }
            catch (ElementClickInterceptedException)
            {
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waiting_time));
                wait.Until(c => c.FindElement(By.ClassName("_2Ujuu")));
                Driver.FindElement(By.XPath(SEND_BUTTON)).Click();
                //Driver.FindElement(By.ClassName("_2Ujuu")).Click();
                return true;
            }
            catch (NoSuchElementException)
            {
                LogHelper.Write("Send button not found");
                return false;
            }
            catch (Exception excp)
            {
                //MessageBox.Show(excp.Message);
                LogHelper.Write("Error in SendText :" + excp.Message);
                return false;
            }
        }

        public Boolean SendImageOrVideo(WhatsApp whatsApp)
        {
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waiting_time));
            try
            {
                wait.Until(c => c.FindElement(By.CssSelector(CLIP_BUTTON)));            
                Driver.FindElement(By.CssSelector(CLIP_BUTTON)).Click();
                //Driver.FindElement(By.XPath(CLIP_BUTTON)).Click();//Web.whatsapp 'CLİP' button click
            }
            catch (ElementClickInterceptedException)
            {
                Driver.FindElement(By.CssSelector(CLIP_BUTTON)).Click();
            }
            catch (ElementNotInteractableException)
            {
                Driver.FindElement(By.CssSelector(CLIP_BUTTON)).Click();
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
            //Thread.Sleep(3000);
            try
            {
                wait.Until(c => c.FindElement(By.XPath(VIDEO_OR_PICTURE_BUTTON)));
                Driver.FindElement(By.XPath(VIDEO_OR_PICTURE_FILE)).SendKeys(whatsApp.Attachment);//Web.whatsapp input image or video file path
                wait.Until(c => c.FindElement(By.XPath(IMAGE_SEND_BUTTON)));
                Driver.FindElement(By.XPath(IMAGE_SEND_BUTTON)).Click();
                //Driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/span/div/div")).Click();//Click send button
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (Exception Excp)
            {
                //MessageBox.Show(Excp.Message);
                LogHelper.Write("Error in SendImageOrVideo :" + Excp.Message);
                return false;
            }
            //Thread.Sleep(3000);
        }

        public Boolean SendFile(WhatsApp whatsApp)
        {
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waiting_time));
            try
            {
                wait.Until(c => c.FindElement(By.CssSelector(CLIP_BUTTON)));
                Driver.FindElement(By.CssSelector(CLIP_BUTTON)).Click();
            }
            catch (ElementClickInterceptedException)
            {
                Driver.FindElement(By.CssSelector(CLIP_BUTTON)).Click();
            }
            catch (ElementNotInteractableException)
            {
                Driver.FindElement(By.CssSelector(CLIP_BUTTON)).Click();
            }
            catch (NoSuchElementException)
            {
                //Thread.Sleep(2000);
                // FailedProcess++;
                return false;
            }
            catch(Exception e)
            {
                return false;
            }
            //Thread.Sleep(3000);
            try
            {
                wait.Until(c => c.FindElement(By.XPath(DOCUMENT_BUTTON_XPATH)));
                Driver.FindElement(By.XPath(DOCUMENT_FILE_Xpath)).SendKeys(whatsApp.Attachment);//Web.whatsapp input File(pdf,rar,zip,exe...) file path
                //wait.Until(c => c.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/span/div/div")));
                wait.Until(c => c.FindElement(By.XPath(FILE_SEND_BUTTON)));
                //wait.Until(c => c.FindElement(By.CssSelector(SEND_BUTTON)));
                //Driver.FindElement(By.CssSelector(FILE_SEND_BUTTON)).Click();
                Driver.FindElement(By.XPath(FILE_SEND_BUTTON)).Click();
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (Exception Excp)
            {
                //MessageBox.Show(Excp.Message);
                LogHelper.Write("Error in SendFile :" + Excp.Message);
                return false;
            }
            //Thread.Sleep(3000);
        }

        public Boolean CanSendMessage()
        {
            if(IsLoggedIn())
            {
                MessageBox.Show("You must logged in into Your WhatsApp Web Acccount");
                return false;
            }
            else
            {
                return true;
            }            
        }
        public Boolean isNumberValid()
        {

            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(2));
            try
            {
                wait.Until(c => c.FindElement(By.XPath(INVALID_NUMBER)));
                Driver.FindElement(By.XPath(INVALID_NUMBER)).Click();
            }
            catch (Exception ex)
            {
                return true;
            }
            return false;

            //try
            //{
            //    //_3SRfO
            //    Driver.FindElement(By.XPath("//*[contains(text(),'Phone number shared via url is invalid')]"));
            //}
            //catch(Exception ex)
            //{
            //    return true;
            //}
            //return false;
        }
        public void grabAllGroup()
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("var objDiv = document.getElementById('pane - side'); objDiv.scrollTop = objDiv.scrollHeight;");
            }
            catch (Exception e)
            {
            }
        }
    }
}
