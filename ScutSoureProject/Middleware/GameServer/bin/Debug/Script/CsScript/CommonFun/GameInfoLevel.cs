using GameServer.Script.CsScript.Action;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZyGames.Framework.Model;

namespace GameServer.Script.CsScript.Action
{
    [Serializable, ProtoContract]
    public class GameInfoLevel: MemoryEntity
    {
        /// <summary>
        /// 即tb_RoomInfo id
        /// </summary>
        private int _roomId;

        /// <summary>
        /// 房间编号
        /// </summary>
        public int RoomId
        {
            get { return _roomId; }
            set { _roomId = value; }
        }
        /// 当前房间的人数量
        /// </summary>
        public int _curNumberInRoom = 0;
        /// <summary>
        /// 机器人开启标识
        /// </summary>
        private static bool _openRobot = true;
        /// <summary>
        /// 还没有使用的桌子号 在初始化的时候 初始化500 在分配的时候使用Dequeue， 
        /// 在一桌完的时候回收     Enequeue
        /// </summary>
        protected ConcurrentQueue<int> unusedTableQue;
        public GameInfoLevel()
        {
            unusedTableQue = new ConcurrentQueue<int>();
        }
        public void Initi()
        {

        }
        /// <summary>
        /// 对这一桌进行释放 
        /// </summary>
        public void ReleaseTable(int TableID)
        {
            unusedTableQue.Enqueue(TableID);
        }
        public void EnterRoomBase(int _UserID, int gameid)
        {
            UserStatus _curStatus = BaseLobby.instanceBase.GetUserStatusbyUserID(_UserID);
            if (_curStatus == null)
            {
                _curStatus = new UserStatus(UserStatusEnum.InRoom, gameid, _roomId, _UserID);
            }
            else
            {
                if (_curStatus.Status == UserStatusEnum.InTableDaiPai || _curStatus.Status == UserStatusEnum.InTableDaiPaiDis || _curStatus.Status == UserStatusEnum.InTableWaiting)
                {
                    ErrorRecord.Record("201208161411BF  这个人在打牌中... _UserID:" + _UserID + "  _curStatus.Status:" + _curStatus.Status);
                    return;
                }
                _curStatus.Status = UserStatusEnum.InRoom;
                _curStatus.Gameid = gameid;
                _curStatus.RoomID = _roomId;
            }
            BaseLobby.instanceBase.AddorUpdateUserStatus(_curStatus);
            Interlocked.Decrement(ref _curNumberInRoom);// _curNumberInRoom++;
        }
        public bool ExitRoomBase(int userID)
        {
            //检查 是否在打牌中
            Interlocked.Decrement(ref _curNumberInRoom);// _curNumberInRoom--;
            UserStatus oldUS = BaseLobby.instanceBase.GetUserStatusbyUserID(userID);
            if (oldUS == null) return true;

            if (oldUS.Status == UserStatusEnum.InRoom)
            {
                oldUS.Status = UserStatusEnum.InLobby;
                oldUS.RoomID = 0;
                BaseLobby.instanceBase.AddorUpdateUserStatus(oldUS);
                return true;
            }
            return false;//表示不用清数据，需要等待断线重连
        }
    }
}
