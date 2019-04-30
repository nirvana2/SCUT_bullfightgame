using GameServer.Script.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common.Serialization;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 游戏 大厅
    /// </summary>
    public class BullFight100Lobby : BaseLobby
    {
        private static BullFight100Lobby _inst = null;
        public static BullFight100Lobby instance
        {
            get
            {
                if (_inst == null) _inst = new BullFight100Lobby();
                return _inst;
            }
        } 

        /// <summary>
        /// 初始化大厅
        /// </summary>
        public void Initi()
        {
            Gameid = 42;
            var roomInfo = new ShareCacheStruct<tb_GameInfo>();
            //DOTO 取了一个房间配置 后面在根据情况在来处理
            //====================begin============================================
            BullFight100Room._instance.Awake();
            tb_gamelevelinfoEx.TryRecoverFromDb();//先从数据库恢复一次
            var roomList = new ShareCacheStruct<tb_gamelevelinfo>().FindAll((g) => { return g.gameid == Gameid; });
            foreach (var _roomInfo in roomList)
            {
                var roomData = new BullFight100Room(_roomInfo);
                BullFight100Room.roomCache.TryAdd(roomData.mRoomID + "", roomData);
                roomData.CreateTableByRobot();
            }
            //===================end
            BullFight.InitRate();
            _dicUserStatus = new ConcurrentDictionary<int, UserStatus>();
        }
        /// <summary>
        /// 根据房间ID与用户ID 找到房间对象
        /// </summary>
        /// <param name="levelid">roomid 一个意思 </param>        
        /// <returns></returns>
        public BullFight100Room GetRoomByRoomID(int levelid)
        {   
            var br = BullFight100Room.roomCache.Find((r) => { return r.mRoomID == levelid; });
            if (br == null) ErrorRecord.Record("201207052210 没找到RoomID：" + levelid); 
            return br;
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="TableID"></param>
        /// <returns></returns>
        public BullFight100Table GetTableByRoomIDandTableID(int RoomID, int TableID)
        { 
            BullFight100Room tempR = GetRoomByRoomID(RoomID);
            if (tempR == null) return null;

            return tempR.GetTableByTableID(TableID); 
        }

        /// <summary>
        /// 根据生成的房号找到房间
        /// </summary>
        /// <param name="tablenum"></param>
        /// <returns></returns>
        public BullFight100Table GetTableByTableNum(int tablenum)
        {
            ////foreach (var roominfo in _dicLevelInfo)
            ////{
            ////    BullFightRoom tempR = GetRoomByRoomID(roominfo.Value.id);
            ////    if (tempR == null) continue;
            ////    BullFightTable _tempTable  = tempR.GetTableByTableNum(tablenum);
            ////    if (_tempTable != null)
            ////    {
            ////        return _tempTable;
            ////    }
            ////}
            return null; 
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public BullFight100User GetUserByRoomIDandTableIDandUserID(int RoomID, int TableID, int UserID)
        {
            BullFight100Table _bftable = GetTableByRoomIDandTableID(RoomID, TableID);
            if (_bftable == null)
            {
                ErrorRecord.Record("21020726151103 RoomID:"+ RoomID + ",  TableID:"+ TableID + ",  UserID:"+ UserID);
                return null;
            }
            lock (_bftable)
            {
                return _bftable.GetUserByID(UserID);
            }
        }


        public   bool SendChat(int userid, cs_chat chat)
        {
            BullFight100Table _bftable = GetTableByRoomIDandTableID(chat.levelid, chat.tableid);
    
            if (_bftable != null)
            {
                lock (_bftable)
                {
                    _bftable.SendChatBase(userid, chat.content, chat.type);
                }
                return true;
            }      
            return false;
        }
        public void SendTransferMsg(int userid, sc_askmoneytrading_n msg)
        {
            List<UserIDMSG> imList = new List<UserIDMSG>();
            imList.Add(new UserIDMSG(userid, JsonUtils.Serialize(msg), false, false));
            BF100SendDataServer.instance.SendDataDelay(imList);
        }
        public void SendMoneyTradinMsg(int userId, sc_ensuremoneytrading_n msg)
        {
            List<UserIDMSG> imList = new List<UserIDMSG>();
            imList.Add(new UserIDMSG(userId, JsonUtils.Serialize(msg), false, false));
            BF100SendDataServer.instance.SendDataDelay(imList);
        }
    }
}
