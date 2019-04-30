using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameServer.Script.CsScript.Action
{  
   
    public class UserIDMSG
    {
        public UserIDMSG(int  userid, string senddata, bool robot, bool disconnect)
        {
            _userid = userid;
            _senddata = senddata;
            _isrobot = robot;
            _isDisconnect = disconnect;
        }
        public int _userid;
        public string _senddata;
        public bool _isrobot;
        /// <summary>
        /// 掉线的人，不发数据
        /// </summary>
        public bool _isDisconnect;
    }

}