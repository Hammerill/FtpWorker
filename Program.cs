using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace FTP
{
    class Program
    {
        public static void UploadFtpFile(string folderName, string fileName)
        {
            FtpWebRequest request;

            string absoluteFileName = Path.GetFileName(fileName);

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/{2}", "127.0.0.1", folderName, absoluteFileName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = true;
            request.Credentials = new NetworkCredential("user", "user");

            using var fileStream = File.OpenRead(fileName);
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();
            Stream ftpStream = request.GetRequestStream();
            ftpStream.Write(buffer, 0, buffer.Length);
            ftpStream.Flush();
            ftpStream.Close();
        }
        public static void DownloadFtpFile(string folderName, string fileName)
        {
            FtpWebRequest request;

            string absoluteFileName = Path.GetFileName(fileName);

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/{2}", "127.0.0.1", folderName, absoluteFileName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential("user", "user");

            using (var ftpStream = request.GetResponse().GetResponseStream())
            {
                using (var fileStream = File.Create(fileName))
                {
                    ftpStream.CopyTo(fileStream);
                }
            }
        }
        public static void DeleteFtpDir(string folderName)
        {
            FtpWebRequest request;

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/", "127.0.0.1", folderName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            request.Credentials = new NetworkCredential("user", "user");

            var reg = new Regex(@"^(.{1}).+\d{2}:\d{2}\s(.+)$");

            foreach (var item in GetFtpFileList(folderName))
            {
                var m = reg.Match(item);
                if (m.Groups[1].Value == "d")
                {
                    DeleteFtpDirRecursively(folderName + "/" + m.Groups[2].Value);
                }
                else if (m.Groups[1].Value == "-")
                {
                    DeleteFtpFile(folderName + "/" + m.Groups[2].Value);
                }
            }

            request.GetResponse().Close();
        }
        public static void DeleteFtpDirRecursively(string folderName)
        {
            FtpWebRequest request;

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/", "127.0.0.1", folderName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            request.Credentials = new NetworkCredential("user", "user");

            var reg = new Regex(@"^(.{1}).+\d{2}:\d{2}\s(.+)$");

            foreach (var item in GetFtpFileList(folderName))
            {
                var m = reg.Match(item);
                if (m.Groups[1].Value == "d")
                {
                    DeleteFtpDirRecursively(folderName + "/" + m.Groups[2].Value);
                }
                else if (m.Groups[1].Value == "-")
                {
                    DeleteFtpFile(folderName + "/" + m.Groups[2].Value);
                }
            }

            request.GetResponse().Close();
        }
        public static void DeleteFtpFile(string fileName)
        {
            FtpWebRequest request;

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}", "127.0.0.1", fileName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential("user", "user");

            request.GetResponse().Close();
        }
        public static List<string> GetFtpFileList(string folderName)
        {
            FtpWebRequest request;

            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/", "127.0.0.1", folderName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential("user", "user");

            var responseStream = request.GetResponse().GetResponseStream();
            var reader = new StreamReader(responseStream);
            string names = reader.ReadToEnd();

            reader.Close();

            return names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        static void Main(string[] args)
        {
            DeleteFtpDir("/Windows");
            Console.WriteLine("deleted");
        }
    }
}
