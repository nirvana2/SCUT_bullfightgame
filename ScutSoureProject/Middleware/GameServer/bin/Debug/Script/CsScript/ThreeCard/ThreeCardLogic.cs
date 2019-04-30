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
    public class ThreeCardLogic
    {
        public ThreeCardLogic()
        { }

        private string _strIPandPort = "";

        private object obj = new object();

        /// <summary>
        /// 返回指定房间的在线人数
        /// </summary>
        /// <param name="levelid"></param>
        /// <returns></returns>
        public int GetOnlineCount(int levelid)
        {
            TCRoom room = TCLobby.instance.GetRoomByRoomID(levelid);
            return room._curNumberInRoom;
        }
        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        public string EnterRoom(tb_User _user, cs_enterroom _data)
        {                                                 
            sc_enterroom _senddata = new sc_enterroom() { result = 0, fn = "sc_enterroom", cc = 0 };
            TCRoom room = TCLobby.instance.GetRoomByRoomID(_data.levelid);
            if (room == null) return JsonUtils.Serialize(_senddata);
            int WaitUserCount = room.EnterRoom(_data, _user.UserID, _strIPandPort);
            _senddata.waitcount = WaitUserCount;
            _senddata.result = 1;
            _senddata.gameid = _data.gameid;
            _senddata.levelid = _data.levelid;
            return JsonUtils.Serialize(_senddata);        
        }

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

                    case "11"://重新登录===============================      
                        ////  ipmsgList = ReLogin(Convert.ToString(arro[1]), Convert.ToString(arro[2]));    
                        break;
                    case "cs_ready_tc":
                        cs_ready_tc _ready = JsonUtils.Deserialize<cs_ready_tc>(_data);
                        senddata = TableReady(_user, _ready);
                        break;
                    case "cs_gamble_tc":
                        cs_gamble_tc _gamble = JsonUtils.Deserialize<cs_gamble_tc>(_data);
                        senddata = Gamble(_user, _gamble);
                        break;
                    case "cs_showcard_tc": //  
                        cs_showcard_tc _showcard = JsonUtils.Deserialize<cs_showcard_tc>(_data); 
                        senddata = ShowCard(_user, _showcard);
                        break;
                    case "cs_giveup_tc":// 
                        cs_giveup_tc  _giveupobj = JsonUtils.Deserialize<cs_giveup_tc>(_data);
                        senddata = GiveUp(_user, _giveupobj);
                        break;
                    case "cs_compare_tc":// 
                        cs_compare_tc _compare = JsonUtils.Deserialize<cs_compare_tc>(_data);
                        senddata = Compare(_user, _compare);
                        break;
                    default://默认不处理的  发送一个   d- 表示哈
                        ErrorRecord.Record(_basedata.fn + " undeal  201206091508TC ");
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
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        private string TableReady(tb_User _user, cs_ready_tc _data)
        {
            sc_ready_tc _senddata = new sc_ready_tc() { result = 0, fn = "sc_ready_tc", cc = 0 };

            TCTable table = TCLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            table.GetReady(_user.UserID);
            _senddata.result = 1;
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 弃牌
        /// </summary>                          
        /// <returns></returns>
        public string GiveUp(tb_User _user, cs_giveup_tc _data)
        { 
            sc_giveup_tc _senddata = new sc_giveup_tc() { result = 0, fn = "sc_giveup_tc", cc = 0 };
             
            TCTable table = TCLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            table.GiveUp(_user.UserID);
            _senddata.result = 1;
            return JsonUtils.Serialize(_senddata);    
        }
        /// <summary>
        /// 看牌
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        public string ShowCard(tb_User _user, cs_showcard_tc _data)
        {
            sc_showcard_tc _senddata = new sc_showcard_tc() { result = 0, fn = "sc_showcard_tc", cc = 0 };

            TCTable table = TCLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            _senddata.shoupai = table.ShowCard(_user.UserID);
            _senddata.result = 1; 
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 下注
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        public string Gamble(tb_User _user, cs_gamble_tc _data)
        { 
            sc_gamble_tc _senddata = new sc_gamble_tc() { result = 0, fn = "sc_gamble_tc", cc = 0 };

            TCTable table = TCLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
             table.Gamble(_user.UserID, _data.money, _data.addrate);
            _senddata.result = 1;
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 比牌 
        /// </summary>                          
        /// <returns></returns>
        public string Compare(tb_User _user, cs_compare_tc _data)
        {
            sc_compare_tc _senddata = new sc_compare_tc() { result = 0, fn = "sc_compare_tc", cc = 0 };

            TCTable table = TCLobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            table.Compare(_user.UserID, _data.targetpos);
            _senddata.result = 1;
            return JsonUtils.Serialize(_senddata);
        }
    }
}
