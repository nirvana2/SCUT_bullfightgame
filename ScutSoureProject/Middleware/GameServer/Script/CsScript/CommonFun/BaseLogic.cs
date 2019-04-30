using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Model;
using ZyGames.Framework.Data;
using ZyGames.Framework.Net;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{      
    /// <summary>
    /// 逻辑消息进来 的接口处理
    /// </summary>
    public abstract class BaseLogic
    {
        private string _strIPandPort = "";
        private object obj = new object();
        private int _gameid  = 1;  //使用继承的方法分发出去
        public BaseLogic()
        {                                
            //_DicIPPortRobotIndex = new ConcurrentDictionary<string, int>();
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="clientcommand"></param>
        /// <returns></returns>
        public virtual string DealDataEx(string _data, string _ipport, tb_User _user)
        {
            return "";
        }
         
    }
} 