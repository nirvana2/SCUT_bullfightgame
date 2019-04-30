using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ZyGames.Framework.Common.Serialization;
using GameServer.Script.Model;
using ZyGames.Framework.Cache.Generic;
using System.Threading;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 游戏房间
    /// </summary>
    public class BullColorRoom : GameInfoLevel
    {
        private static BullColorRoom _insta;
        /// <summary>
        /// 单例，所有房间只需要一个操作方法
        /// </summary>
        public static BullColorRoom _instance
        {
            // set { }
            get
            {
                if (_insta == null)
                {
                    _insta = new BullColorRoom();
                    roomCache = new MemoryCacheStruct<BullColorRoom>();
                }
                return _insta;
            }
        }
        public static MemoryCacheStruct<BullColorRoom> roomCache;
        public void Awake() { }
        public BullColorRoom()
        {  
           
        }
        public BullColorRoom(tb_gamelevelinfo _roominfo)
        {
            _currRoomInfo = _roominfo;
            RoomId = _roominfo.Id;
            Initi();
            DicUser = new ConcurrentDictionary<int, BullColorUser>();
            DicTable = new ConcurrentDictionary<int, BullColorTable>();
            unusedTableQue = new ConcurrentQueue<int>();
            for (int i = 1; i <= _roominfo.openTableCount; i++)
            {
                unusedTableQue.Enqueue(i);
            }
        }
       
        //加锁用的
        protected readonly object objLock = new object();

        public tb_gamelevelinfo _currRoomInfo;           
        /// <summary>
        /// 用户退出房间时调用
        /// </summary>
        /// <param name="userID"></param>
        public virtual bool ExitTable(int userID)
        {
            return false;
        }
     
        private int _tempwaitAfterLimit = 0;

        /// <summary>
        /// 当前房间  等待分配桌子 的用户  数据队列
        /// </summary> 
        public ConcurrentDictionary<int, BullColorUser> DicUser;
        /// <summary>
        /// 当前房间的桌子列表
        /// </summary>
        public ConcurrentDictionary<int, BullColorTable> DicTable;
  
        /// <summary>
        /// 获取有空位的房间 
        /// </summary>
        /// <returns></returns>
        private BullColorTable GetEmptyPosTable()
        {
            foreach (var btable in DicTable)
            {
                if (btable.Value.HaveEmptyPos()) return btable.Value;
            }
            return null;
        }
        #region enter room   1.先找找是否有空位的房间，如果有直接进入，相当于观战模式。如果没有找到，直接新开一桌进去坐好[即进入就准备]。
        /// <summary>
        ///  用户登录房间时调用
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="UserID"></param>
        /// <param name="ipport"></param>
        /// <returns></returns>
        public int EnterRoom(cs_enterroom _data, int UserID, string ipport)
        {
            lock (objLock)
            {  
                if (GMService.isStop)
                    return -99;
                BullColorRoom roomData;
                if (!roomCache.TryGet(RoomId + "", out roomData)) return -99;

                roomData.EnterRoomBase(UserID, _data.gameid); 
                tb_User tbuser = tb_UserEx.GetFromCachebyUserID(UserID);
                if (tbuser == null)
                {
                    ErrorRecord.Record("201208241558BF tbuser == null  userID:" + UserID);
                    return 0;
                } 

                if (tbuser.UserMoney > _currRoomInfo._max || tbuser.UserMoney < _currRoomInfo._min) return -99;

                UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(UserID);
                if (_us != null)
                {
                    if (_us.Status == UserStatusEnum.InTableDaiPai || _us.Status == UserStatusEnum.InTableDaiPaiDis || _us.Status == UserStatusEnum.InTableWaiting) return -1;
                }
                BullColorUser tempU = new BullColorUser();
                tempU.Initi(ipport, RoomId, tbuser, false);// 当成客户端 的IP：Port用 

                if (!DicUser.TryAdd(UserID, tempU)) ErrorRecord.Record("201208241155BF 已经存在ROOM内了， 添加不成功 逻辑需要处理");
                BullColorTable _findtable = GetEmptyPosTable();
                if (_findtable != null)
                {
                    return DicUser.Count;//直接进入 空桌子  
                }
                return DicUser.Count; //金币模式 等5秒再一起分配
            }
        }

        /// <summary>
        /// 自动2000000 机器人创建一个桌子唯一桌子，系统奖池也是用的这个ID
        /// </summary>
        /// <returns></returns>
        public int CreateTableByRobot( )
        {
            lock (objLock)
            {
                if (GMService.isStop) return 0;
                BullColorRoom roomData; 
                roomCache.TryGet(RoomId+"", out roomData);
                if (roomData == null)  return 0;
               

                int _tid = 0;
                //只有一个桌子 所有不用生产桌子
                if (!unusedTableQue.TryDequeue(out _tid))
                {
                    ErrorRecord.Record("201704011350BFC unusedQue.Count <= 0   桌子不够了，，只是能等待排队...");
                    DicUser = new ConcurrentDictionary<int, BullColorUser>();
                    return 0;
                }
                tb_User tbuser = tb_UserEx.GetFromCachebyUserID(2000000);
                if (tbuser == null)
                {
                    ErrorRecord.Record("没有特殊机器人了系 统不能开始牛牛时时彩");
                    return 0;
                } 

                BullColorUser tempU = new BullColorUser();
                tempU.Initi(tbuser.UserID + "", RoomId , tbuser, true);// 当成客户端 的IP：Port用 
                roomData.EnterRoomBase(tempU._tbUser.UserID, _currRoomInfo.gameid);//处理状态的，不然一直找不到用户                     
                cs_enterroom _data = new cs_enterroom() { baserate = _currRoomInfo.Baserate, gametype = _currRoomInfo.gametype, gameid = 4, gamemodel = 2, levelid = RoomId, numpertable = 500, roomcard = 0, rankertype = 1, tableCount = 1 };
                List<BullColorUser> _tempfirstUser = new List<BullColorUser>();
                _tempfirstUser.Add(tempU);

                BullColorTable tab = new BullColorTable(_currRoomInfo.gameid, this, _tid, _tempfirstUser, _data);
                //添加到当前桌子列表，以便打牌过程好使用                
                if (!DicTable.TryAdd(_tid, tab)) ErrorRecord.Record("add _tableid fial maybe have exist... 201208241601BF");
                return _tid;
            }
        }
    
      
        #endregion

        /// <summary>
        /// 用户退出房间时调用
        /// </summary>
        /// <param name="userID"></param>
        public bool ExitRoom(int userID)
        {
            BullColorRoom roomData;
            if (roomCache.TryGet(RoomId + "", out roomData))
            {
                return roomData.ExitRoomBase(userID);
            } 
            return false;
        }
    
     
        /// <summary>
        /// 循环当前房间 的每一桌     //每桌的裁判[等待时间完了 自动 带打功能] 即时执行的
        /// </summary>
        /// <param name="SecondOne">间隔几秒执行一次</param>
        public void DealEveryTable(int SecondOne)
        {   
            _tempwaitAfterLimit--;//暂时是1S执行一次   

            lock (objLock)
            { 
                foreach (int tid in DicTable.Keys)
                {   //2级， 循环房间里的每一个桌子     
                    BullColorTable _bftable;

                    if (!DicTable.TryGetValue(tid, out _bftable)) continue; //桌子没找到 
                    lock (_bftable)
                    {
                        try
                        {
                            if (_bftable._pos2userbase == null) continue;
                            if (_bftable._pos2userbase.Count == 0) continue;
                             
                            if (_bftable._tablestatus == TableStatusEnum.Playing || _bftable._tablestatus == TableStatusEnum.Initi)
                            {
                                if (_bftable._gameover)
                                {
                                    _bftable.Reset(true);
                                    continue;//解散成功的桌子延时1秒释放 
                                }
                                _bftable.SomeUserIsOverTime();                              
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorRecord.Record(ex, "201207061606BF 暂时可以不去掉");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据房间ID， 桌子ID与用户ID 找到桌子对象
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="TableID"></param>
        /// <returns></returns>
        public BullColorTable GetTableByTableID(int TableID)
        {
            BullColorTable tempT;
            if (!DicTable.TryGetValue(TableID, out tempT))
            {
                ErrorRecord.Record(" 201206092237 在房间RoomID为：" + RoomId + "里没找到TableID为：" + TableID);
                return null;
            }
            //if (tempT._tablestatus == TableStatusEnum.NoBody ) ErrorRecord.Record(" 201208011602BF 房间已经释放了 ");    
            return tempT;
        }
        /// <summary>
        /// 根据房间ID， 桌子ID与用户ID 找到桌子对象
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="TableID"></param>
        /// <returns></returns>
        public BullColorTable GetTableByTableNum(int tablenum)
        {
            foreach (var _tempTable in DicTable)
            {
                if (_tempTable.Value._tableMathCode == tablenum) return _tempTable.Value;
            }
            //ErrorRecord.Record(" 201612041555 在房间RoomID为：" + mRoomID + "里没找到tablenum为：" + tablenum);
            return null;
        }
        public void ResetTableByTableID(int TableID)
        { 
            BullColorRoom   roomdata;
            roomCache.TryGet(RoomId+"", out roomdata);
            lock (DicTable)
            {
                if (!DicTable.ContainsKey(TableID)) return;
                BullColorTable _tempbft;
                ////ErrorRecord.Record("ResetTableByTableID.........._roomid:" + _roomid+"... TableID:" + TableID +" status:"+ DicTable[TableID]._strStatus);
                if (!DicTable.TryRemove(TableID, out _tempbft))
                {
                    ErrorRecord.Record("201208011600BF 释放Table资源失败");
                }
            }
        }
    }
}
