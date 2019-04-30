using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 配置文件相关静态数据 开始运行时就读取
    /// </summary>
    public class MyConfig
    {
        
        public static void Initi()
        {
            IsOpenTip = false;
            TipRate = 5;
            ////LogURL = ConfigurationManager.AppSettings["LogURL"];
            ErrorRecord.directorypath = MyConfig.LogURL;
        }
        public static string LogURL = System.Environment.CurrentDirectory + "\\Log\\";
        public static string ConnetionString = "";

        /// <summary>
        /// 是否开启 抽水功能
        /// </summary>
        public static bool IsOpenTip = false;
        /// <summary>
        /// 抽水千分比
        /// </summary>
        public static int TipRate = 0;
        /// <summary>
        /// 账号登录 密码是否加密
        /// </summary>
        public static bool IsEncrypt = false; 
    }
}
