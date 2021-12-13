using System;
using System.IO;

public class ClsBase64
{
    public string ConvertFileToBase64(string fileName)
    {
        string Extension = Path.GetExtension(fileName);
        string MimeType = "data:document;base64,";
        switch (Extension.ToLower())
        {
            case ".aac": { MimeType = "data:audio/aac;base64,"; break; }
            case ".abw": { MimeType = "data:application/x-abiword;base64,"; break; }
            case ".arc": { MimeType = "data:application/x-freearc;base64,"; break; }
            case ".avi": { MimeType = "data:video/x-msvideo;base64,"; break; }
            case ".azw": { MimeType = "data:application/vnd.amazon.ebook;base64,"; break; }
            case ".bin": { MimeType = "data:application/octet-stream;base64,"; break; }
            case ".bmp": { MimeType = "data:image/bmp;base64,"; break; }
            case ".bz": { MimeType = "data:application/x-bzip;base64,"; break; }
            case ".bz2": { MimeType = "data:application/x-bzip2;base64,"; break; }
            case ".cda": { MimeType = "data:application/x-cdf;base64,"; break; }
            case ".csh": { MimeType = "data:application/x-csh;base64,"; break; }
            case ".css": { MimeType = "data:text/css;base64,"; break; }
            case ".csv": { MimeType = "data:text/csv;base64,"; break; }
            case ".doc": { MimeType = "data:application/msword;base64,"; break; }
            case ".docx": { MimeType = "data:application/vnd.openxmlformats-officedocument.wordprocessingml.document;base64,"; break; }
            case ".eot": { MimeType = "data:application/vnd.ms-fontobject;base64,"; break; }
            case ".epub": { MimeType = "data:application/epub+zip;base64,"; break; }
            case ".gz": { MimeType = "data:application/gzip;base64,"; break; }
            case ".gif": { MimeType = "data:image/gif;base64,"; break; }
            case ".htm": { MimeType = "data:text/html;base64,"; break; }
            case ".html": { MimeType = "data:text/html;base64,"; break; }
            case ".ico": { MimeType = "data:image/vnd.microsoft.icon;base64,"; break; }
            case ".ics": { MimeType = "data:text/calendar;base64,"; break; }
            case ".jar": { MimeType = "data:application/java-archive;base64,"; break; }
            case ".jpg": { MimeType = "data:image/jpeg;base64,"; break; }
            case ".jpeg": { MimeType = "data:image/jpeg;base64,"; break; }
            case ".js": { MimeType = "data:text/javascript;base64,"; break; }
            case ".json": { MimeType = "data:application/json;base64,"; break; }
            case ".jsonld": { MimeType = "data:application/ld+json;base64,"; break; }
            case ".midi": { MimeType = "data:audio/midi audio/x-midi;base64,"; break; }
            case ".mid": { MimeType = "data:audio/midi audio/x-midi;base64,"; break; }
            case ".mjs": { MimeType = "data:text/javascript;base64,"; break; }
            case ".mp3": { MimeType = "data:audio/mpeg;base64,"; break; }
            case ".mp4": { MimeType = "data:video/mpeg;base64,"; break; }
            case ".mpeg": { MimeType = "data:video/mpeg;base64,"; break; }
            case ".mpkg": { MimeType = "data:application/vnd.apple.installer+xml;base64,"; break; }
            case ".odp": { MimeType = "data:application/vnd.oasis.opendocument.presentation;base64,"; break; }
            case ".ods": { MimeType = "data:application/vnd.oasis.opendocument.spreadsheet;base64,"; break; }
            case ".odt": { MimeType = "data:application/vnd.oasis.opendocument.text;base64,"; break; }
            case ".oga": { MimeType = "data:audio/ogg;base64,"; break; }
            case ".ogv": { MimeType = "data:video/ogg;base64,"; break; }
            case ".ogx": { MimeType = "data:application/ogg;base64,"; break; }
            case ".opus": { MimeType = "data:audio/opus;base64,"; break; }
            case ".otf": { MimeType = "data:font/otf;base64,"; break; }
            case ".png": { MimeType = "data:image/png;base64,"; break; }
            case ".pdf": { MimeType = "data:application/pdf;base64,"; break; }
            case ".php": { MimeType = "data:application/x-httpd-php;base64,"; break; }
            case ".ppt": { MimeType = "data:application/vnd.ms-powerpoint;base64,"; break; }
            case ".pptx": { MimeType = "data:application/vnd.openxmlformats-officedocument.presentationml.presentation;base64,"; break; }
            case ".rar": { MimeType = "data:application/vnd.rar;base64,"; break; }
            case ".rtf": { MimeType = "data:application/rtf;base64,"; break; }
            case ".sh": { MimeType = "data:application/x-sh;base64,"; break; }
            case ".svg": { MimeType = "data:image/svg+xml;base64,"; break; }
            case ".swf": { MimeType = "data:application/x-shockwave-flash;base64,"; break; }
            case ".tar": { MimeType = "data:application/x-tar;base64,"; break; }
            case ".tiff": { MimeType = "data:image/tiff;base64,"; break; }
            case ".tif": { MimeType = "data:image/tiff;base64,"; break; }
            case ".ts": { MimeType = "data:video/mp2t;base64,"; break; }
            case ".ttf": { MimeType = "data:font/ttf;base64,"; break; }
            case ".txt": { MimeType = "data:text/plain;base64,"; break; }
            case ".vsd": { MimeType = "data:application/vnd.visio;base64,"; break; }
            case ".wav": { MimeType = "data:audio/wav;base64,"; break; }
            case ".weba": { MimeType = "data:audio/webm;base64,"; break; }
            case ".webm": { MimeType = "data:video/webm;base64,"; break; }
            case ".webp": { MimeType = "data:image/webp;base64,"; break; }
            case ".woff": { MimeType = "data:font/woff;base64,"; break; }
            case ".woff2": { MimeType = "data:font/woff2;base64,"; break; }
            case ".xhtml": { MimeType = "data:application/xhtml+xml;base64,"; break; }
            case ".xls": { MimeType = "data:application/vnd.ms-excel;base64,"; break; }
            case ".xlsx": { MimeType = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,"; break; }
            case ".xml": { MimeType = "data:application/xml;base64,"; break; }
            case ".xul": { MimeType = "data:application/vnd.mozilla.xul+xml;base64,"; break; }
            case ".zip": { MimeType = "data:application/zip;base64,"; break; }
            case ".3gp": { MimeType = "data:video/3gpp;base64,"; break; }
            case ".3g2": { MimeType = "data:video/3gpp2;base64,"; break; }
            case ".7z": { MimeType = "data:application/x-7z-compressed;base64,"; break; }
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

