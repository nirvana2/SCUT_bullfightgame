using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZyGames.Framework.Common.Configuration;

namespace GameServer.Script.CsScript.Action
{
    public class ToolsEx
    {
        /// <summary>
        /// 随机打乱数组中数据顺序
        /// </summary>
        /// <param name="arr">要处理的数组</param>
        /// <param name="type">要处理的数组值类型</param>
        /// <returns></returns>
        public static Array MixArray(Array arr)
        {
            Array goal = Array.CreateInstance(typeof(int), arr.Length);
            Random rnd = new Random();//实例化一个伪随机数生成器
            for (int i = 0; i < arr.Length; i++)
            { //循环要处理的数组
                //随机生成一个数组索引号，注意每循环一次，将范围缩小一个
                int pos = rnd.Next(0, arr.Length - i - 1);
                //将随机的索引号pos所在的值 赋给输出数组中的当前循环索引（i）
                goal.SetValue(arr.GetValue(pos), i);
                //由于每次循环，范围都缩小了一个，而在该范围外的一个值，可能会丢掉了。
                //所以要将原数组pos位置的值更改为该范围外的那个值，当前位置的值已传给输出数组了
                arr.SetValue(arr.GetValue(arr.Length - 1 - i), pos);
            }
            return goal;
        }

        private static readonly Object _objLock = new object();
        /// <summary>
        /// 全局种子 用静态的才能有用
        /// </summary>
        private static int Seeds = 0;

        /// <summary>
        /// 返回一个指定范围内的随机数。
        /// </summary>
        /// <param name="max">返回的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于 minValue。</param>
        /// <param name="min">返回的随机数的下界（随机数可取该下界值）。</param>
        /// <returns></returns>
        public static int GetRandomSys(int min, int max)
        { 
            lock(_objLock)
            {
                if (max < min)
                {
                    return min;
                }
                Seeds += Convert.ToInt32(DateTime.Now.Ticks & 0xffff); 
                if (Seeds >= int.MaxValue)
                {
                    Seeds = 0;
                }
            }
            return new Random(Seeds).Next(min, max);
        }
        private static ConcurrentDictionary<int, bool> _SixRoomID = new ConcurrentDictionary<int, bool>();
        /// <summary>
        /// 获取进入房间的6位ID, 缺陷比较多，每周维护重启一次应该没什么问题
        /// </summary>
        /// <returns></returns>
        public static int GetRoomEnterSixID()
        {                       
            return GetIDloop();   
        }
        private static int GetIDloop()
        {
            Random rd = new Random(Guid.NewGuid().GetHashCode());
            int num = rd.Next(100000, 1000000);   
            if(! _SixRoomID.TryAdd(num, true)) GetIDloop();
            return num;      
        }

        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        /// <returns></returns>
        public static  string GetIpAddress()
        {
            //获取本地的IP地址
            string AddressIP = ConfigUtils.GetSetting("ServerIp") + "/";
            //foreach (var _IPAddress in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
            //{
            //    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
            //    {
            //        AddressIP = _IPAddress.ToString();
            //    }
            //}
            return AddressIP;
        }
        public static string IsHandlePhoto(int _isRobot, string wechatHeadIcon)
        {
            if (_isRobot == 1)
            {
                var serverIp = ToolsEx.GetIpAddress();
                return "" + serverIp + "/fordlc/wechat/" + wechatHeadIcon;
            }
            else return wechatHeadIcon;
        }
    }
}


////public class RandomUtil
////{
////    public static Queue q;
////    public static Dictionary<int, bool> _SixRoomID = new Dictionary<int, bool>();
////    public static int GetID()
////    {
////        if (q == null)
////        {
////            Init();
////        }
////        return (int)q.Dequeue();
////    }
////    public static bool SetID(int num)
////    {
////        if (q == null)
////        {
////            return false;
////        }
////        if (_SixRoomID.ContainsKey(num))
////        {
////            return false;
////        }
////        q.Enqueue(num);
////        return true;
////    }
////    public static void Init()
////    {
////        q = new Queue();
////        for (int i = 0; i < 2000; i++)
////        {
////            GetRandom();
////        }
////    }
////    public static void GetRandom()
////    {
////        System.Random rd = new System.Random(Guid.NewGuid().GetHashCode());
////        int num = rd.Next(100000, 1000000);
////        if (_SixRoomID.ContainsKey(num))
////        {
////            GetRandom();
////        }
////        else
////        {
////            _SixRoomID.Add(num, true);
////            q.Enqueue(num);
////        }
////    }

////}