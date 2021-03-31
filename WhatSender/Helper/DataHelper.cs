using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace WhatSender
{
    class DataHelper
    {
        public DataTable dt_pending_messages = new DataTable();
        public DataTable dt_sent_messages = new DataTable();

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

        }
        public string deduceMessage(DataTable master_datatable, DataRow dr, String str)
        {
            foreach (DataColumn dc in master_datatable.Columns)
            {
                str = str.Replace("{{" + dc.ColumnName + "}}", dr[dc.ColumnName].ToString());
            }
            return str;
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
