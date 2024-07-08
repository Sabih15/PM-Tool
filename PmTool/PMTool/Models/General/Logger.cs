using System;
using System.Configuration;
using System.IO;

namespace PMTool.General
{
    public class Logging
    {
        public enum LogType
        {
            Error = 1,
            Warning = 2,
            Info = 3,
            Critical = 4
        }
        public static void WriteLog(LogType logType, string text)
        {
            try
            {
                string LogPath = ConfigurationManager.AppSettings["LogPath"];
                string file = AppDomain.CurrentDomain.BaseDirectory + LogPath;
                string dir = file.Substring(0, file.LastIndexOf(@"\"));
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                FileStream fStream;
                if (File.Exists(file))
                    fStream = File.Open(file, FileMode.Append, FileAccess.Write);
                else
                    fStream = File.Open(file, FileMode.CreateNew, FileAccess.ReadWrite);
                StreamWriter objStreamWriter = new StreamWriter(fStream, System.Text.Encoding.UTF8);
                objStreamWriter.Close();
                objStreamWriter.Dispose();
            }
            catch { }

        }
        public static void WriteExLog(LogType logType, Exception ex, string methodname)
        {
            try
            {
                string text = "Method Name: " + methodname + " -- " + ex.Message + (ex.InnerException == null ? " InnerException: null" : " InnerException:" + ex.InnerException.Message);

                string LogPath = ConfigurationManager.AppSettings["LogPath"];
                string file = AppDomain.CurrentDomain.BaseDirectory + LogPath;
                string dir = file.Substring(0, file.LastIndexOf(@"\"));
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                FileStream fStream;
                if (File.Exists(file))
                    fStream = File.Open(file, FileMode.Append, FileAccess.Write);
                else
                    fStream = File.Open(file, FileMode.CreateNew, FileAccess.ReadWrite);
                StreamWriter objStreamWriter = new StreamWriter(fStream, System.Text.Encoding.UTF8);
                string date = DateTime.Now.ToShortDateString();
                objStreamWriter.WriteLine(date + " -------- " + text);
                objStreamWriter.Close();
                objStreamWriter.Dispose();
            }
            catch { }

        }
    }
}
