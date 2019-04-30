using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 游戏 大厅
    /// </summary>
    public class TCLobby :BaseLobby
    {
        private static TCLobby _instLandLord = null;
        public static TCLobby instance
        {
            get
            {
                if (_instLandLord == null) _instLandLord = new TCLobby();
                return _instLandLord;
            }
        }
        /// <summary>
        /// 游戏大厅里面的桌子列表
        /// </summary>
        public   ConcurrentDictionary<int, TCRoom> _DicRoom;
        
        /// <summary>
        /// 初始化大厅
        /// </summary>
        public   void Initi()
        {
            lock (obj)
            {
                _DicRoom = new ConcurrentDictionary<int, TCRoom>();

                TCRoom tab1 = new TCRoom(3, 1, 5, 200);//先直接 分配个200桌
                _DicRoom.TryAdd(1, tab1);
                TCRoom tab2 = new TCRoom(3, 2, 10, 200);
                _DicRoom.TryAdd(2, tab2);
                TCRoom tab3 = new TCRoom(3, 3, 20, 200);
                _DicRoom.TryAdd(3, tab3); 
            }
            //_DicDisConnectUser = new ConcurrentDictionary<int, User>();
            _dicUserStatus = new ConcurrentDictionary<int, UserStatus>();
        }
        /// <summary>
        /// 根据房间ID与用户ID 找到房间对象
        /// </summary>
        /// <param name="RoomID"></param>        
        /// <returns></returns>
        public   TCRoom GetRoomByRoomID(int RoomID)
        {
            lock (obj)
            {
                foreach (int key in _DicRoom.Keys)
                {   //1级， 循环大厅里的每一个房间
                    TCRoom r;
                    if (!_DicRoom.TryGetValue(key, out r))
                    {
                        continue;
                    }
                    if (r.mRoomID == RoomID)
                    {
                        return r;
                    }
                }
                ErrorRecord.Record("201207052210 没找到RoomID：" + RoomID);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="TableID"></param>
        /// <returns></returns>
        public   TCTable GetTableByRoomIDandTableID(int RoomID, int TableID)
        {
            if (RoomID == 0 || TableID == 0)  
                return null; 
            lock (obj)
            {
                TCRoom tempR = GetRoomByRoomID(RoomID);
                if (tempR == null)
                {
                    ErrorRecord.Record("21020726151102 运行正常后去掉");
                    return null;
                }
                return tempR.GetTableByTableID(TableID);
            }
        }
        
        /// <summary>
        ///  
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public   TCUser GetUserByRoomIDandTableIDandUserID(int RoomID, int TableID, int UserID)
        {
            lock (obj)
            {
                TCTable myT = GetTableByRoomIDandTableID(RoomID, TableID);
                if (myT == null)
                {
                    ErrorRecord.Record("21020726151103 运行正常后去掉");
                    return null;
                }
                return myT.GetUserByID(UserID);
            }
        }                                     
    
        public void DealEveryRoom(int SecondOne)
        {

        }
        public   bool SendChat(int userid, cs_chat chat)
        {
            TCTable _bftable = GetTableByRoomIDandTableID(chat.levelid, chat.tableid);
            if (_bftable != null)
            {
                _bftable.SendChatBase(userid, chat.content, chat.type);
                return true;
            }
            return false;
        }
    }
}
