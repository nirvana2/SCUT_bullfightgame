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
    public class BullColorLobby : BaseLobby
    {
        private static BullColorLobby _instLandLord = null;
        public static BullColorLobby instance
        {
            get
            {
                if (_instLandLord == null) _instLandLord = new BullColorLobby();
                return _instLandLord;
            }
        } 

        /// <summary>
        /// 初始化大厅
        /// </summary>
        public void Initi()
        {
            Gameid = 41;
            var roomInfo = new ShareCacheStruct<tb_GameInfo>();
            var temp= roomInfo.FindAll(false);//DOTO 取了一个房间配置 后面在根据情况在来处理
            //====================begin============================================
            BullColorRoom._instance.Awake();
            var roomList = new ShareCacheStruct<tb_gamelevelinfo>().FindAll((g)=> { return g.gameid == Gameid; });
            
            foreach (var _roomInfo in roomList)
            {
                var roomData = new BullColorRoom(_roomInfo);
                BullColorRoom.roomCache.TryAdd(roomData.RoomId + "", roomData);
                roomData.CreateTableByRobot();
            }
            //===================end
            _dicUserStatus = new ConcurrentDictionary<int, UserStatus>();
        }

        /// <summary>
        /// 根据房间ID与用户ID 找到房间对象
        /// </summary>
        /// <param name="levelid">roomid 一个意思 </param>        
        /// <returns></returns>
        public BullColorRoom GetRoomByRoomID(int levelid)
        {
            //BullColorRoom.roomCache
            
           
            return null;
        }
 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="TableID"></param>
        /// <returns></returns>
        public BullColorTable GetTableByRoomIDandTableID(int RoomID, int TableID)
        { 
            BullColorRoom tempR = GetRoomByRoomID(RoomID);
            if (tempR == null) return null;

            return tempR.GetTableByTableID(TableID); 
        }

    
        /// <summary>
        ///  
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public BullColorUser GetUserByRoomIDandTableIDandUserID(int RoomID, int TableID, int UserID)
        {
            BullColorTable _bftable = GetTableByRoomIDandTableID(RoomID, TableID);
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
            BullColorTable _bftable = GetTableByRoomIDandTableID(chat.levelid, chat.tableid);
    
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
            BFColorSendDataServer.instance.SendDataDelay(imList);
        }
        public void SendMoneyTradinMsg(int userId, sc_ensuremoneytrading_n msg)
        {
            List<UserIDMSG> imList = new List<UserIDMSG>();
            imList.Add(new UserIDMSG(userId, JsonUtils.Serialize(msg), false, false));
            BFColorSendDataServer.instance.SendDataDelay(imList);
        }
    }
}
