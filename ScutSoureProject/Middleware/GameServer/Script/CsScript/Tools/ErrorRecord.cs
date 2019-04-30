/*********************************************************   
 * 作    者：jsw
 * 模    块：记录错误
 *  创建时间：20090715
 *  功能描述：将错误记录到错误文件
 *******************************************************
 */
using System;
using System.IO;
using System.Diagnostics;
namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 在需要的地方调用    
    /// </summary>
    public class ErrorRecord
    {
        private static object obj = new object();

        //HttpContext.Current.Server.MapPath("App_Data");
        //private static string directorypath = string.Format(@"C:\SiteErrorRecord\{0}\{1}\{2}",
        //    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        // public static string directorypath = @"C:\MJServerError";
        public static string directorypath = System.Environment.CurrentDirectory + "\\Log\\";
        /// <summary>
        /// 默认目录在C根目录下，可以自己传文件位置修改
        /// </summary>
        public static string strPath
        {
            get { return directorypath; }
            set { directorypath = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ErrorRecord()
        { }

        //先屏避此方法;根据需要再加上; 
        ///// <summary>
        ///// 记录错误信息
        ///// </summary>
        ///// <param name="ex">错误实例</param>
        //public void Record(Exception ex)
        //{
        //    try
        //    {
        //        string directorypath = string.Format(@"C:\XZErrorRecord\{0}\{1}\{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        //        //不存在则创建一个目录
        //        if (!Directory.Exists(directorypath))
        //        {
        //            Directory.CreateDirectory(directorypath);
        //        }
        //        FileStream fs = null;
        //        string strFilePath = string.Format(@"{0}\Error.txt", directorypath);
        //        if (!File.Exists(strFilePath))
        //        {
        //            fs = new FileStream(strFilePath, FileMode.Create);
        //        }
        //        else
        //        {
        //            fs = new FileStream(strFilePath, FileMode.Append);
        //        }
        //        StackTrace st = new StackTrace(true);

        //        StreamWriter sw = new StreamWriter(fs);
        //        sw.WriteLine("—————————————————————————————–");
        //        sw.WriteLine(string.Format("日    期：{0}", DateTime.Now.ToString("G")));
        //        sw.WriteLine(string.Format(string.Format("发生在：{0}的{1}行", st.GetFrame(1).GetFileName(), st.GetFrame(1).GetFileLineNumber())));  
        //        sw.WriteLine(string.Format("错误消息：{0}", ex.Message));
        //        sw.Close();
        //    }
        //    catch (IOException ioe)
        //    {
        //        //MessageBox.Show("错误未被记录！原因：" + ioe.Message.ToString());
        //    }
        //}
        /// <summary>
        /// 记录错误信息  附加备注
        /// </summary>
        /// <param name="ex">错误实例</param>
        /// <param name="Desc">备注说明</param>
        public static void Record(Exception ex, string Desc)
        {
            lock (obj)
            {
                try
                {
                    //不存在则创建一个目录
                    if (!Directory.Exists(directorypath))
                    {
                        Directory.CreateDirectory(directorypath);
                    }
                    FileStream fs = null;
                    string strFilePath = string.Format(@"{0}\{1}_{2}_{3}Error.txt", directorypath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    if (!File.Exists(strFilePath))
                    {
                        fs = new FileStream(strFilePath, FileMode.Create);
                    }
                    else
                    {
                        fs = new FileStream(strFilePath, FileMode.Append);
                    }
                    StackTrace st = new StackTrace(true);

                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("—————————————Exception—begin————————————————————————————–");
                    sw.WriteLine("—————————————调用堆栈Begin—————————————————————————————–");
                    for (int i = 0; i < st.FrameCount; i++)
                    {
                        int line = st.GetFrame(i).GetFileLineNumber();
                        if (line != 0) sw.WriteLine(string.Format("发生在：{0}的{1}行", st.GetFrame(i).GetFileName(), line));
                    }
                    sw.WriteLine("—————————————调用堆栈End—————————————–");
                    sw.WriteLine("—————————————被调用堆栈Begin—————————————–");
                    sw.WriteLine(string.Format("ex.StackTrace：{0}", ex.StackTrace));
                    sw.WriteLine("—————————————被调用堆栈End—————————————–");
                    sw.WriteLine(string.Format("日    期：{0}", DateTime.Now.ToString("G"))); 
                    sw.WriteLine(string.Format("错误消息：{0}", ex.Message));
                    sw.WriteLine(string.Format("错误编号：No.{0}", Desc));
                    sw.WriteLine("—————————————Exception—begin————————————————————————————–");
                    sw.Close();
                }
                catch //(IOException ioe)
                {
                   
                }
            }
        }
        /// <summary>
        /// Log 记录    添加备注
        /// </summary>
        /// <param name="Desc"></param>
        public static void Record(string Desc)
        {
            lock (obj)
            {
                //if (XmlOpe.GetConfigValue("DebugLog").ToLower() != "true")
                //{
                //    return;  //加了要死循环
                //}
                try
                {
                    //不存在则创建一个目录
                    if (!Directory.Exists(directorypath))
                    {
                        Directory.CreateDirectory(directorypath);
                    }
                    FileStream fs = null;
                    string strFilePath = string.Format(@"{0}\{1}_{2}_{3}Error.txt", directorypath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    if (!File.Exists(strFilePath))
                    {
                        fs = new FileStream(strFilePath, FileMode.Create);
                    }
                    else
                    {
                        fs = new FileStream(strFilePath, FileMode.Append);
                    }
                    StackTrace st = new StackTrace(true);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("—————————————Log begin———————————————–");
                    for (int i = 0; i < st.FrameCount; i++)
                    {
                        int line = st.GetFrame(i).GetFileLineNumber();
                        if (line != 0) sw.WriteLine(string.Format("发生在：{0}的{1}行", st.GetFrame(i).GetFileName(), line));
                    } 
                    // MethodInfo method0 = (MethodInfo)(st.GetFrame(0).GetMethod()); 
                    sw.WriteLine(string.Format("日    期：{0}", DateTime.Now.ToString("G")));
                    sw.WriteLine(string.Format("错误编号：No.{0}", Desc));
                    sw.WriteLine("—————————————Log end———————————————–");
                    sw.Close();
                }
                catch (Exception ioe) //(Exception ex) //(IOException ioe)
                {
                   //// Game.Script.MainClass.WL(ioe.Message, "ErrorRecord 197 line"); 
                }
            }
        }

    }
}