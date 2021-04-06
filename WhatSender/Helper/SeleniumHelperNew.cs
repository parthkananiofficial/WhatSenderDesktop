using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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

namespace WhatSender
{
    class SeleniumHelperNew
    {
        IWebDriver Driver;
        string MENU_BUTTON = "//*[@data-testid='menu']";
        //string FILE_SEND_BUTTON = "/html/body/div[1]/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/span/div/div";
        string FILE_SEND_BUTTON = "/html/body/div/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/span/div/div";
        //string SEND_BUTTON = "/html/body/div[1]/div/div/div[4]/div/footer/div[1]/div[3]/button";
        string SEND_BUTTON = "/html/body/div/div/div/div[4]/div/footer/div[1]/div[3]/button";
        string IMAGE_SEND_BUTTON = "/html/body/div/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/span/div";
        //string SEND_BUTTON = "span[data-icon='send']";
        string CLIP_BUTTON = "span[data-icon='clip']";
        string VIDEO_OR_PICTURE_BUTTON = "/html/body/div/div/div/div[4]/div/footer/div[1]/div[1]/div[2]/div/span/div/div/ul/li[1]/button";
        string VIDEO_OR_PICTURE = "/html/body/div/div/div/div[4]/div/footer/div[1]/div[1]/div[2]/div/span/div/div/ul/li[1]/button/input";
        //string VIDEO_OR_PICTURE = "input[type='file']";
        //string DOCUMENT_Xpath = "/html/body/div[1]/div/div/div[4]/div/footer/div[1]/div[1]/div[2]/div/span/div/div/ul/li[3]/button/input";
        string DOCUMENT_BUTTON_Xpath = "/html/body/div/div[1]/div[1]/div[4]/div[1]/footer/div[1]/div[1]/div[2]/div/span/div[1]/div/ul/li[3]/button";
        string DOCUMENT_Xpath = "/html/body/div/div[1]/div[1]/div[4]/div[1]/footer/div[1]/div[1]/div[2]/div/span/div[1]/div/ul/li[3]/button/input";
        //string DOCUMENT_Xpath = "/html/body/div/div/div/div[4]/div/footer/div[1]/div[1]/div[2]/div/span/div/div/ul/li[3]/button/input";
        Boolean status;
        Boolean isLoggedIn = false;
        int waiting_time = 20;
        public SeleniumHelperNew()
        {
            //CreateSession();
        }
        public void CreateSession()
        {
            

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            LogHelper.Write("Creating new session");


            ChromeOptions options = new ChromeOptions();
            options.SetLoggingPreference("performance", LogLevel.All);

            try
            {
                Driver = new ChromeDriver(driverService, options);
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
                foreach (var entry in logs)
                {
                    if(entry.Message.Contains("participants")) //this is for group only
                        Console.WriteLine(entry.Message);
                    //Console.WriteLine(entry.ToString());
                }
            }
            catch (Exception excp)
            {
                MessageBox.Show(excp.Message);
            }
        }

        public Boolean IsLoggedIn()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waiting_time+50));
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
            if(!whatsApp.Status)
            {
                if(!isNumberValid())
                {
                    whatsApp.Error = "Number is Invalid";
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
                Driver.FindElement(By.XPath(VIDEO_OR_PICTURE)).SendKeys(whatsApp.Attachment);//Web.whatsapp input image or video file path
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
                wait.Until(c => c.FindElement(By.XPath(DOCUMENT_BUTTON_Xpath)));
                Driver.FindElement(By.XPath(DOCUMENT_Xpath)).SendKeys(whatsApp.Attachment);//Web.whatsapp input File(pdf,rar,zip,exe...) file path
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
            try
            {
                Driver.FindElement(By.XPath("//*[contains(text(),'Phone number shared via url is invalid')]"));
            }
            catch(Exception ex)
            {
                return true;
            }
            return false;
        }
        public void deduceContactsFromGroup(String message)
        {

        }
    }


    

}
