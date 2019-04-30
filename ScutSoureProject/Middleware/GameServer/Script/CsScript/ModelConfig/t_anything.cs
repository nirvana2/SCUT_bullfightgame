/*
 * Name:  app 的配置
 * Date: 2013.09.23
 * Author：jiangshengwei
*/
using System.Collections.Generic;
using System;
using ZyGames.Framework.Common;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 杂项表
    /// </summary>
    public class t_anythingList
    {

        public static Dictionary<string, string> mList;

        private static void Init(t_anything cfgDatas)
        {
            //mList = cfgDatas;
        }

        public static void LoadDataSync()
        {
            List<t_anything> data = ConfigLoader.LoadJsonListFile<t_anything>("t_anything.bytes");
            mList = new Dictionary<string, string>();
            foreach (var item in data)
            {
                mList.Add(item.key, item._val);
            }
        }

        public static int GetInt(string key, int defValue = 0)
        {
            var item = mList[key];
            return item == null ? defValue : item.ToInt();
        }

        public static byte GetByte(string key, byte defValue = 0)
        {
            var item = mList[key];
            return item == null ? defValue : item.ToByte();
        }

        public static decimal GetDecimal(string key, decimal defValue = 0)
        {
            var item = mList[key];
            return item == null ? defValue : item.ToDecimal();
        }
        public static double GetDouble(string key, double defValue = 0)
        {
            var item = mList[key];
            return item == null ? defValue : item.ToDouble();
        }

        public static string GetString(string key, string defValue = "")
        {
            var item = mList[key];
            return item == null ? defValue : item.ToNotNullString();
        }

        ////public static t_anything Current
        ////{
        ////    get { return mList; }
        ////    set { mList = value; }
        ////}
    }

    public class t_anything
    {                
        ////"key": "lift_cost_max",
        ////"value": 9,
        ////"desc": "每次成功送客户上电梯的小费最大值"
        

        /// <summary>
        ///  
        /// </summary>
        public string key;
        /// <summary>
        ///  
        /// </summary>
        public string _val;        
        /// <summary>
        ///   
        /// </summary>
        public string desc;      
    }

}