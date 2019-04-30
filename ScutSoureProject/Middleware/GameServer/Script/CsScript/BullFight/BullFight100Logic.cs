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
    public class BullFight100Logic
    {
        public BullFight100Logic()
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
                    case "cs_entertable_bf100":
                        cs_entertable_bf100 _entertable = JsonUtils.Deserialize<cs_entertable_bf100>(_data);
                        senddata = EnterTableAdd(_user, _entertable);
                        break;
                    case "cs_applybanker_bf100": //  cs_gamble_bf
                        cs_applybanker_bf100 _enterroom = JsonUtils.Deserialize<cs_applybanker_bf100>(_data);
                        senddata = GetBanker(_user, _enterroom);
                        break; 
                   
                    case "cs_gambleone_bf100"://     cs_gamble_bf  
                        cs_gambleone_bf100 _gambleone = JsonUtils.Deserialize<cs_gambleone_bf100>(_data);
                        return GambleOne(_user, _gambleone);
                  
                     
                    case "cs_bankergetbonuspot_bf100":
                        cs_bankergetbonuspot_bf100 _getPot = JsonUtils.Deserialize<cs_bankergetbonuspot_bf100>(_data);
                        senddata = BankerGetBonusPot(_user, _getPot);
                        break;
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
            BullFight100Room room = BullFight100Lobby.instance.GetRoomByRoomID(levelid);
            if (room == null) return 0;
            return room._curNumberInRoom;
        }
        /// <summary>
        /// 进入房间 1.如果是房卡模式就直接进入房间，2.如果是金币模式就返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        public  string EnterRoom(tb_User _user, cs_enterroom _data)
        {
            sc_enterroom _senddata = new sc_enterroom() { result = 0, fn = "sc_enterroom", cc = 0 };
            BullFight100Room room = BullFight100Lobby.instance.GetRoomByRoomID(_data.levelid);
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
        /// 进入房间中的指定桌子
        /// </summary>                          
        /// <returns></returns>
        public string EnterRoomTable(tb_User _user, cs_enterroomtable _data)
        {
            sc_enterroomtable _senddata = new sc_enterroomtable() { result = 0, fn = "sc_enterroomtable", cc = 0 };
            
            int _tablenum = 0;
            if (int.TryParse(_data.tablenum, out _tablenum))
            {
                BullFight100Table _findtable = BullFight100Lobby.instance.GetTableByTableNum(_tablenum);
                if (_findtable == null) return JsonUtils.Serialize(_senddata);
                BullFight100Room room = BullFight100Lobby.instance.GetRoomByRoomID(_findtable._roomid);
                if (room == null) return JsonUtils.Serialize(_senddata);
                int WaitUserCount = room.EnterRoomTable(_findtable._roomid, _user);
                _senddata.result = WaitUserCount;
                _senddata.tableid = _findtable._tableid;
                _senddata.levelid = _findtable._roomid;
                _senddata.numpertable = _findtable._num_max;
                _senddata.gametype = _findtable._judge._gametype;
                _senddata.gameModel = _findtable._judge._gameCoin2Room1;
            }
            return JsonUtils.Serialize(_senddata);
        }    
        /// <summary>
        ///   
        /// </summary>                          
        /// <returns></returns>
        private string EnterTableAdd(tb_User _user, cs_entertable_bf100 _data)
        {
            sc_entertable_bf100 _senddata = new sc_entertable_bf100() { result = 0, fn = "sc_entertable_bf100", cc = 0 };

            BullFight100Table table = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            bool _succes =  table.EnterTableAdditive(_user);
            _senddata.result = _succes?1:0;
            string _redata = JsonUtils.Serialize(_senddata);
            table.AddSendDataRecord(_user.UserID, _redata);
            return _redata;
        }

        /// <summary>
        ///   
        /// </summary>                          
        /// <returns></returns>
        private string GetBanker(tb_User _user, cs_applybanker_bf100 _data)
        {
            sc_applybanker_bf100 _senddata = new sc_applybanker_bf100() { result = 0, fn = "sc_applybanker_bf100", cc = 0 };

            BullFight100Table table = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            if (table.ApplyGetBanker(_user.UserID, true)) _senddata.result = 1;
            string _redata = JsonUtils.Serialize(_senddata);
            table.AddSendDataRecord(_user.UserID, _redata);
            return _redata;
        }

        /// <summary>
        /// 下注一次，只有升庄牛牛才会有的
        /// </summary>                          
        /// <returns></returns>
        private string GambleOne(tb_User _user, cs_gambleone_bf100 _data)
        {
            sc_gambleone_bf100 _senddata = new sc_gambleone_bf100() { result = 0, fn = "sc_gambleone_bf100", cc = 0 };

            BullFight100Table table = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            if (table.GambleOne(_user.UserID, _data.targetpos, _data.gamble)) _senddata.result = 1;
            string _redata = JsonUtils.Serialize(_senddata);
            table.AddSendDataRecord(_user.UserID, _redata);
            return _redata;
        }

        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        private string BankerGetBonusPot(tb_User _user, cs_bankergetbonuspot_bf100 _data)
        {
            sc_bankergetbonuspot_bf100 _senddata = new sc_bankergetbonuspot_bf100() { result = 0, fn = "sc_bankergetbonuspot_bf100", cc = 0 };

            BullFight100Table table = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_data.levelid, _data.tableid);
            if (table == null) return JsonUtils.Serialize(_senddata);
            if (table.BankerGetBonusPot(_user.UserID)) _senddata.result = 1;
            string _redata = JsonUtils.Serialize(_senddata);
            table.AddSendDataRecord(_user.UserID, _redata);
            return _redata;
        }
    }
    
}

