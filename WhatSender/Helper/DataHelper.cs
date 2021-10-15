using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Text;
using System.Windows;

namespace WhatSender
{
    class DataHelper
    {
        public DataTable dt_pending_messages = new DataTable();
        public DataTable dt_sent_messages = new DataTable();
        public DataTable dt_group = new DataTable();
        public DataTable dt_recipient = new DataTable();

        public DataHelper()
        {            
            dt_pending_messages.Columns.Add("ID");
            dt_pending_messages.Columns.Add("Number");
            dt_pending_messages.Columns.Add("Message");
            dt_pending_messages.Columns.Add("Attachment");
            dt_pending_messages.Columns.Add("Media");
            dt_pending_messages.Columns.Add("Document");
            //dt_pending_messages.Columns.Add("Status");
            dt_pending_messages.Columns.Add("Time");

            
            dt_sent_messages.Columns.Add("Number");
            dt_sent_messages.Columns.Add("Message");
            dt_sent_messages.Columns.Add("Attachment");
            dt_sent_messages.Columns.Add("Media");
            dt_sent_messages.Columns.Add("Document");
            dt_sent_messages.Columns.Add("Status");
            dt_sent_messages.Columns.Add("Error");
            dt_sent_messages.Columns.Add("Time");


            dt_group.Columns.Add("phone");

            dt_recipient.Columns.Add("name");
            dt_recipient.Columns.Add("phone");

        }
        public string deduceMessage(DataTable master_datatable, DataRow dr, String str)
        {
            foreach (DataColumn dc in master_datatable.Columns)
            {
                str = str.Replace("{{" + dc.ColumnName + "}}", dr[dc.ColumnName].ToString());
            }
            return str;
        }

        public DataTable ReadExcel(string fileName, string fileExt)
        {
            string conn = string.Empty;
            DataTable dtexcel = new DataTable();
            if (fileExt.CompareTo(".xls") == 0)
                conn = @"provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007  
            else
                conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=NO';"; //for above excel 2007  
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1  
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable  
                }
                catch(Exception ex) {
                    MessageBox.Show("Please install Excel in your computer");
                    return new DataTable();
                }
            }
            return dtexcel;
        }
    }
    public static class DataTableExtensions
    {
        public static void WriteToCsvFile(this DataTable dataTable, string filePath)
        {
            StringBuilder fileContent = new StringBuilder();

            foreach (var col in dataTable.Columns)
            {
                fileContent.Append(col.ToString() + ",");
            }

            fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (var column in dr.ItemArray)
                {
                    fileContent.Append("\"" + column.ToString() + "\",");
                }

                fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);
            }

            System.IO.File.WriteAllText(filePath, fileContent.ToString());
        }
    }
}
