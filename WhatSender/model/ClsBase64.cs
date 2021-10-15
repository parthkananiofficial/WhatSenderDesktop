using System;
using System.IO;

public class ClsBase64
{
    public string ConvertFileToBase64(string fileName)
    {
        string Extension = Path.GetExtension(fileName);
        string MimeType = "data:image/png;base64,";
        switch (Extension.ToLower())
        {
            case ".jpg":
                {
                    MimeType = "data:image/jpeg;base64,";
                    break;
                }

            case ".jpeg":
                {
                    MimeType = "data:image/jpeg;base64,";
                    break;
                }

            case ".gif":
                {
                    MimeType = "data:image/gif;base64,";
                    break;
                }

            case ".png":
                {
                    MimeType = "data:image/png;base64,";
                    break;
                }

            case ".bmp":
                {
                    MimeType = "data:image/bmp;base64,";
                    break;
                }

            case ".ico":
                {
                    MimeType = "data:image/x-icon;base64,";
                    break;
                }

            case ".pdf":
                {
                    MimeType = "data:application/pdf;base64,";
                    break;
                }

            case ".mp4":
                {
                    // MimeType = "data:video/mp4;base64,"
                    MimeType = "data:application/mpeg;base64,";
                    break;
                }

            case ".txt":
                {
                    MimeType = "data:text/plain;base64,";
                    break;
                }
        }
        return MimeType + Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
    }
    public string ConvertFileToBase64NoMime(string fileName)
    {
        return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
    }
    public static string EncodeBase64(string stringtoencode)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(stringtoencode);
        return Convert.ToBase64String(data);
    }
    public static string DecodeBase64(string StringtoDecode)
    {
        byte[] data = System.Convert.FromBase64String(StringtoDecode);
        return System.Text.ASCIIEncoding.ASCII.GetString(data);
    }
}

