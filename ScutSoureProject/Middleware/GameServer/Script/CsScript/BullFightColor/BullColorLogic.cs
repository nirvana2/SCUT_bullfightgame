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
    public class BullColorLogic
    {
        public BullColorLogic()
        { }

        private string _strIPandPort = "";

        private object obj = new object();

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="clientcommand"></param>
        /// <returns></returns>
        public string DealDataEx(string _data, string _ipport, tb_User _user)
        {
            string senddata = "";
            try
            {
                _strIPandPort = _ipport;
                cs_base _basedata = JsonUtils.Deserialize<cs_base>(_data);
                switch (_basedata.fn)
                {
                    case "cs_entertable_bfc":
                        cs_entertable_bfc _entertable = JsonUtils.Deserialize<cs_entertable_bfc>(_data);
                        senddata = EnterTableAdd(_user, _entertable);
                        break;
                    case "cs_gambleone_bfc"://     cs_gamble_bf  
                        cs_gambleone_bfc _gambleone = JsonUtils.Deserialize<cs_gambleone_bfc>(_data);
                        return GambleOne(_user, _gambleone);
                    default:
                        ErrorRecord.Record(_basedata.fn + " undeal  201611062128BF ");
                        break;
                }
                return senddata;
            }
            catch (Exception ex)
            {
                ErrorRecord.Record(ex, " 201206091508BF ");
                return "";
            }
        }
      

        /// <summary>
        /// 返回指定房间的在线人数
        /// </summary>
        /// <param name="levelid"></param>
        /// <returns></returns>
        public int GetOnlineCount(int levelid)
        {
            //BullColorRoom room = BullColorLobby.instance.GetRoomByRoomID(levelid);
            //if (room == null) return 0;
            //return room._curNumberInRoom;
            return 0;
        }
        /// <summary>
        /// 进入房间 1.如果是房卡模式就直接进入房间，2.如果是金币模式就返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        public  string EnterRoom(tb_User _user, cs_enterroom _data)
        {
            sc_enterroom _senddata = new sc_enterroom() { result = 0, fn = "sc_enterroom", cc = 0 };
            BullColorRoom room = BullColorLobby.instance.GetRoomByRoomID(_data.levelid);
            if (room == null) return JsonUtils.Serialize(_senddata);
            int WaitUserCount = room.EnterRoom(_data, _user.UserID, _strIPandPort);
            _senddata.waitcount = WaitUserCount;
            _senddata.result = 1;
            _senddata.gameid = _data.gameid;
            _senddata.levelid = _data.levelid;
            _senddata.gamemodel = _data.gamemodel;
            _senddata.numpertable = _data.numpertable;
            return JsonUtils.Serialize(_senddata);                                                                          
        }
     
        /// <summary>
        ///   
        /// </summary>                          
        /// <returns></returns>
        private string EnterTableAdd(tb_User _user, cs_entertable_bfc _data)
        {
            sc_entertable_bfc _senddata = new sc_entertable_bfc() { result = 0, fn = "sc_entertable_bf", cc = 0 };

            BullColorTable table = BullColorLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            bool _succes =  table.EnterTableAdditive(_user);
            _senddata.result = _succes?1:0;
            string _redata = JsonUtils.Serialize(_senddata);
            table.AddSendDataRecord(_user.UserID, _redata);
            return _redata;
        } 

        /// <summary>
        /// 下注一次，只有升庄牛牛才会有的
        /// </summary>                          
        /// <returns></returns>
        private string GambleOne(tb_User _user, cs_gambleone_bfc _data)
        {
            sc_gambleone_bfc _senddata = new sc_gambleone_bfc() { result = 0, fn = "sc_gambleone_bfc", cc = 0 };

            BullColorTable table = BullColorLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            table.GambleOne(_user.UserID, _data.targetpos, _data.rate, _data.lx, _data.ly);
            _senddata.result = 1;
            _senddata.lx = _data.lx;
            _senddata.ly = _data.ly;
            string _redata = JsonUtils.Serialize(_senddata);
            table.AddSendDataRecord(_user.UserID, _redata);
            return _redata;                                                                          
        }
       
    }
    
}

