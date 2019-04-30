using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ZyGames.Framework.Common.Serialization;
using GameServer.Script.Model;
using ZyGames.Framework.Cache.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 游戏房间
    /// </summary>
    public class BullFight100Room : BaseRoom
    {
        private static BullFight100Room _insta;
        /// <summary>
        /// 单例，所有房间只需要一个操作方法
        /// </summary>
        public static BullFight100Room _instance
        {
            // set { }
            get
            {
                if (_insta == null)
                {
                    _insta = new BullFight100Room();
                    roomCache = new MemoryCacheStruct<BullFight100Room>();
                }
                return _insta;
            }
        }
        public static MemoryCacheStruct<BullFight100Room> roomCache;
        public void Awake() { }
        public BullFight100Room()
        { }

        /// <summary>
        /// 机器人开启标识
        /// </summary>
        private static bool _openRobot = true;
        private int _tempwaitAfterLimit = 0;
        public tb_gamelevelinfo _currRoomInfo;
        /// <summary>
        /// 分配完成，就不再分配了，
        /// </summary>
        public bool _alocOver = false;

        /// <summary>
        /// 当前房间  等待分配桌子 的用户  数据队列
        /// </summary> 
        public ConcurrentDictionary<int, BullFight100User> DicUser;
        /// <summary>
        /// 当前房间的桌子列表
        /// </summary>
        public ConcurrentDictionary<int, BullFight100Table> DicTable;
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gamelevelid"></param>       
        /// <param name="tableNum">先弄个一百桌，后面做到配置里面去</param>
        public BullFight100Room(tb_gamelevelinfo _roominfo)
        {           
            _currRoomInfo = _roominfo; 
            _roomid = _roominfo.Id;
            _gameid = _roominfo.gameid; 
            base.Initi();
            DicUser = new ConcurrentDictionary<int, BullFight100User>();
            DicTable = new ConcurrentDictionary<int, BullFight100Table>();
            unusedTableQue = new ConcurrentQueue<int>();
            for (int i = 1; i <= _roominfo.openTableCount; i++)
            {
                unusedTableQue.Enqueue(i);
            } 
        }
        /// <summary>
        /// 获取有空位的房间 
        /// </summary>
        /// <returns></returns>
        private BullFight100Table GetEmptyPosTable()
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
                base.EnterRoomBase(UserID, _data.gameid);

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
                BullFight100User tempU = new BullFight100User();
                tempU.Initi(ipport, mRoomID, tbuser, false);// 当成客户端 的IP：Port用 

                ////if (!DicUser.TryAdd(UserID, tempU)) ErrorRecord.Record("201208241155BF 已经存在ROOM内了， 添加不成功 逻辑需要处理");
                if (!DicUser.ContainsKey(UserID)) DicUser.TryAdd(UserID, tempU);

                return DicUser.Count;//直接进入 空桌子          
            }
        }

        /// <summary>
        /// l输入 房间编号进到指定的桌子  1.玩家进入房间触发。 
        /// </summary>
        /// <returns></returns>
        public int EnterRoomTable(int roomid, tb_User tbUser)
        {
            lock (objLock)
            { 
                if (tbUser.UserMoney > _currRoomInfo._max || tbUser.UserMoney < _currRoomInfo._min) return -99; 
                base.EnterRoomBase(tbUser.UserID, _gameid);
                ////BullFightUser tempU = new BullFightUser();
                ////tempU.Initi(tbUser.IP, mRoomID, tbUser, false);// 当成客户端 的IP：Port用   
                ////_bftable.AllocationtoTable(tempU);
                return 1;
            }
        }

        /// <summary>
        /// 自动分配 3个人 到一桌  1.玩家进入房间触发。2.系统增加机器人触发
        /// 分配机器人也在这儿处理 
        /// </summary>
        /// <returns></returns>
        private int CreateTableByHuman(BullFight100User tempU, int _tid, cs_enterroom _data)
        {
            lock (objLock)
            {
                ////if (GMService.isStop)
                ////    return -1;
                ////BullFight100Table _bftable;
                ////if (DicTable.TryGetValue(_tid, out _bftable))
                ////{//已存在流程是错误的
                ////    ErrorRecord.Record("fetal error 201601101429BF 必须处理 ");
                ////    return -1;
                ////} 
                ////BullFight100Table tab = new BullFight100Table(_gameid, this, _tid, tempU, _data);
                ////int _checkCode = tab.CheckRoomCard(tempU._userid);
                ////if (_checkCode != 1) return _checkCode;
                ////  //移出已经分配了的用户
                ////    BullFight100User temp02 = null;// new BullFightUser();
                ////    DicUser.TryRemove(tempU._userid, out temp02);
                 
                //////添加到当前桌子列表，以便打牌过程好使用                
                ////if (!DicTable.TryAdd(_tid, tab)) ErrorRecord.Record("add _tableid fial maybe have exist... 201208241601BF");
                return 1;
            }
        }
        /// <summary>
        /// 自动分配 1个人机器人创建一个桌子 仅金币模式
        /// </summary>
        /// <returns></returns>
        public int CreateTableByRobot()
        {
            lock (objLock)
            {
                if (GMService.isStop) return 0;
                BullFight100Room roomData;
                roomCache.TryGet(mRoomID + "", out roomData);
                if (roomData == null) return 0;

                int _tid = 0;
                //只有一个桌子 所有不用生产桌子
                if (!unusedTableQue.TryDequeue(out _tid))
                {
                    ErrorRecord.Record("201704011350BFC unusedQue.Count <= 0   桌子不够了，，只是能等待排队...");
                    DicUser = new ConcurrentDictionary<int, BullFight100User>();
                    return 0;
                }
                tb_User tbuser = tb_UserEx.GetFromCachebyUserID(2000000);
                if (tbuser == null)
                {
                    ErrorRecord.Record("没有特殊机器人了系 统不能开始牛牛时时彩");
                    return 0;
                }
                tbuser.UserMoney = 2345;//===================================测试
                BullFight100User tempU = new BullFight100User();
                tempU.Initi(tbuser.UserID + "", mRoomID, tbuser, true);// 当成客户端 的IP：Port用 
                roomData.EnterRoomBase(tempU._tbUser.UserID, _currRoomInfo.gameid);//处理状态的，不然一直找不到用户                     
                cs_enterroom _data = new cs_enterroom() { baserate = _currRoomInfo.Baserate, gametype = _currRoomInfo.gametype, gameid = 42, gamemodel = 2, levelid = mRoomID, numpertable = 500, roomcard = 0, rankertype = 1, tableCount = 1 };
                

                BullFight100Table tab = new BullFight100Table(_currRoomInfo.gameid, this, _tid, tempU, _data);
                //添加到当前桌子列表，以便打牌过程好使用                
                if (!DicTable.TryAdd(_tid, tab)) ErrorRecord.Record("add _tableid fial maybe have exist... 201208241601BF");
                return _tid;
            }
        }
        /// <summary>
        /// 自动分配人 到一桌  1.玩家进入房间触发。 
        /// </summary>
        /// <returns></returns>
        private void AutoAlloc2TableForHuman(BullFight100Table _bftable, BullFight100User tempU)
        {
            lock (objLock)
            {
                if (!_openRobot) return;

                if (_bftable._judge._gameCoin2Room1 == 1) return;//房卡模式不能进机器人了   
                base.EnterRoomBase(tempU._tbUser.UserID, _gameid);

                //移出已经分配了的用户
                BullFight100User temp02 = null;// new BullFightUser();
                DicUser.TryRemove(tempU._userid, out temp02);
                _bftable.AllocationtoTable(tempU);
                BF100SendDataServer.instance.RobotExistNumAddOne();
            }
        }
        /// <summary>
        /// 自动分配人 到一桌  1.玩家进入房间触发。 分配机器人也在这儿处理 
        /// </summary>
        /// <returns></returns>
        private void AutoAlloc2TableByRobot(BullFight100Table _bftable)
        {
            lock (objLock)
            {
                if (!_openRobot) return; 

                //一次性分配足额机器人 
                int _needCount = _bftable._num_min - _bftable._pos2userbase.Count;//一般最多为2人
                if (BF100SendDataServer.instance.QueRobotUser.Count < _needCount) return;
                for (int i = 0; i < _needCount; i++)
                {  
                    tb_User tbRobotuser;
                    if (!BF100SendDataServer.instance.QueRobotUser.TryDequeue(out tbRobotuser)) return;
                    //把tbuser 机器在的金币限制在当前Level内，，， 
                    if (tbRobotuser.UserMoney < _currRoomInfo._min || tbRobotuser.UserMoney > _currRoomInfo._max)
                    {
                        if (tbRobotuser.isRobot == 1)
                        {
                            var temp = tbRobotuser.UserMoney;
                            var raMoney = ToolsEx.GetRandomSys(_currRoomInfo._min, _currRoomInfo._max);
                            tbRobotuser.UserMoney = raMoney;
                            ////var cacheUserSet = new ShareCacheStruct<tb_User>();  //100MS会自动存库
                            ////cacheUserSet.AddOrUpdate(tbuser); 
                        }
                    }

                    BullFight100User tempU = new BullFight100User();
                    tempU.Initi(tbRobotuser.UserID + "", mRoomID, tbRobotuser, true);// 当成客户端 的IP：Port用 
                    base.EnterRoomBase(tbRobotuser.UserID, _gameid);
                    _bftable.AllocationtoTable(tempU);
                    BF100SendDataServer.instance.RobotExistNumAddOne();
                }
            }
        }

        /////// <summary>
        /////// 当在一个桌子里一桌结束需要处理进入处理时的分配功能
        /////// </summary>
        ////private void AutoAlloc2TableForReady(BullFightTable bftable)
        ////{//直接把排队的人放进有空位的桌子
        ////    if (bftable._tablestatus == TableStatusEnum.WaitforReady)
        ////    {
        ////        if (bftable.GetRobotCount() == bftable._numpertable)
        ////        {//全机器人了，，释放掉桌子
        ////            unusedTableQue.Enqueue(bftable._tableid);
        ////            BFSendDataServer.instance.RobotExistNumReduceOne();
        ////            DicTable.TryRemove(bftable._tableid, out bftable);
        ////            return;
        ////        }
        ////    }
        ////    //人员满了就不分配了，
        ////    if (bftable._numpertable >= bftable._num_max) return;
        ////    if (DicUser.Count == 0) return;//没有等待的人
        ////    foreach (int key in DicUser.Keys)
        ////    {
        ////        bftable.AllocationtoTable(DicUser[key]);
        ////        BullFightUser temp02 = null;// new BullFightUser();
        ////        DicUser.TryRemove(key, out temp02);
        ////        break;//一次只分配一个，，省力~.~
        ////    }
        ////}
        #endregion


        /// <summary>
        /// 用户退出房间时调用
        /// </summary>
        /// <param name="userID"></param>
        public bool ExitRoom(int userID)
        {
            return this.ExitRoomBase(userID);
        }
        /// <summary>
        /// 用户退出房间时调用  手动退出，掉线（清理Session里要调用）       
        /// </summary>
        /// <param name="userID"></param>
        public override bool ExitRoomBase(int userID)
        {
            lock (objLock)
            {
                if (base.ExitRoomBase(userID))
                {
                    BullFight100User _tempcu;
                    if (!DicUser.TryRemove(userID, out _tempcu))
                    {
                        ErrorRecord.Record("用户列表清理失败 201611051448BF   可能是已不桌子上，再次请求退出 ，，，，");
                    }
                    return true;
                }
                return false;
            }
        } 
        /// <summary>
        /// 循环当前房间 的每一桌     //每桌的裁判[等待时间完了 自动 带打功能] 即时执行的
        /// </summary>
        /// <param name="SecondOne">间隔几秒执行一次</param>
        public void DealEveryTable(int SecondOne)
        {    
            _tempwaitAfterLimit--;//暂时是1S执行一次   

            lock (objLock)
            {//未加锁要出错，，，============
                foreach (int tid in DicTable.Keys)
                {   //2级， 循环房间里的每一个桌子     
                    BullFight100Table _bftable;

                    if (!DicTable.TryGetValue(tid, out _bftable)) continue; //桌子没找到 
                    lock (_bftable)
                    {
                        try
                        {
                            if (_bftable._pos2userbase == null) continue;
                            if (_bftable._pos2userbase.Count == 0) continue;
                            if (_bftable._pos2userbase.Count < 2) //====================测试
                            { 
                                if (_bftable._tablestatus == TableStatusEnum.Initi) AutoAlloc2TableByRobot(_bftable);
                            }
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
        public BullFight100Table GetTableByTableID(int TableID)
        {
            BullFight100Table tempT;
            if (!DicTable.TryGetValue(TableID, out tempT))
            {
                ErrorRecord.Record(" 201206092237 在房间RoomID为：" + _roomid + "里没找到TableID为：" + TableID);
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
        public BullFight100Table GetTableByTableNum(int tablenum)
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
            base.ReleaseTable(TableID);
            lock(DicTable)
            {
                if (!DicTable.ContainsKey(TableID)) return;
                BullFight100Table _tempbft;
                ErrorRecord.Record("ResetTableByTableID.........._roomid:" + _roomid+"... TableID:" + TableID +" status:"+ DicTable[TableID]._strStatus);
                if (!DicTable.TryRemove(TableID, out _tempbft))
                {
                    ErrorRecord.Record("201208011600BF 释放Table资源失败");
                }
            }
        }
    }
}
