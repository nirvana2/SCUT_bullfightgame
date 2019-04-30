using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ZyGames.Framework.Common.Serialization;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 游戏房间
    /// </summary>
    public class TCRoom : BaseRoom
    {
        public TCRoom()
        { }

        /// <summary>
        /// 机器人开启标识
        /// </summary>
        private static bool _openRobot = true;            
        private int _tempwaitAfterLimit = 0;
        /// <summary>
        /// 分配完成，就不再分配了，
        /// </summary>
        public bool _alocOver = false;

        /// <summary>
        /// 当前房间  等待分配桌子 的用户  数据队列
        /// </summary> 
        private ConcurrentDictionary<int, TCUser> DicUser;
        /// <summary>
        /// 当前房间的桌子列表
        /// </summary>
        private ConcurrentDictionary<int, TCTable> DicTable;
             
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomid"></param>
        /// <param name="baserate"></param>
        /// <param name="tableNum">先弄个一百桌，后面做到配置里面去</param>
        public TCRoom(int gameid,  int roomid, int _baserate, int tableNum)
        {
            base.Initi();
            _gameid = gameid;
            _roomid = roomid; 
            DicUser = new ConcurrentDictionary<int, TCUser>();
            DicTable = new ConcurrentDictionary<int, TCTable>();
            unusedTableQue = new ConcurrentQueue<int>();
            for (int i = 1; i <= tableNum; i++)
            {
                unusedTableQue.Enqueue(i);
            }
            BlockingCollection<int> tempbc = new BlockingCollection<int>();
            List<int> tmpi = new List<int>();
        }
        /// <summary>
        /// 获取有空位的房间 
        /// </summary>
        /// <returns></returns>
        private TCTable GetEmptyPosTable()
        {
            foreach (int key in DicTable.Keys)
            {
                if (DicTable[key].HaveEmptyPos()) return DicTable[key];
            }
            return null;
        }
        #region enter room       1.先找找是否有空位的房间，如果有直接进入，相当于观战模式。如果没有找到，直接新开一桌进去坐好[即进入就准备]。
        /// <summary>
        ///  用户登录房间时调用
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="_UserID"></param>
        /// <param name="ipport"></param>
        /// <returns></returns>
        public int EnterRoom(cs_enterroom _data, int UserID, string ipport)
        { 
            lock (objLock)
            {
                base.EnterRoomBase(UserID, _data.gameid);
                tb_User tbuser = tb_UserEx.GetFromCachebyUserID(UserID); 
                if (tbuser == null)
                {
                    ErrorRecord.Record("201208241558TC tbuser == null  userID:" + UserID);
                    return 0;
                }
                UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(UserID);
                if (_us != null)
                {
                    if (_us.Status == UserStatusEnum.InTableDaiPai || _us.Status == UserStatusEnum.InTableDaiPaiDis || _us.Status == UserStatusEnum.InTableWaiting) return -1;
                }
                TCUser tempU = new TCUser();
                tempU.Initi(ipport, mRoomID, tbuser, false);   // 当成客户端 的IP：Port用 

                if (!DicUser.TryAdd(UserID, tempU))     ErrorRecord.Record("201208241155TC  已经存在ROOM内了， 添加不成功");
                TCTable _findtable = GetEmptyPosTable();
                if (_findtable != null)
                {
                    ////_findtable.AllocationtoTable(DicUser[UserID]);
                    ////BullFightUser temp02 = null;// new BullFightUser();
                    ////DicUser.TryRemove(UserID, out temp02);
                    ////return 1;//直接进入 空桌子  //开房模式不要这个功能了，
                }
                int _tableid = 0;
                if (!unusedTableQue.TryDequeue(out _tableid))
                {
                    ErrorRecord.Record("201611141831TC unusedQue.Count <= 0   桌子不够了，，只是能等待排队...");
                    return 999;
                }
                return AutoAlloctoTable(tempU, _tableid, _data);
                //return DicUser.Count; 
            }
        }
        /// <summary>
        /// l输入 房间编号进到指定的桌子  1.玩家进入房间触发。 
        /// </summary>
        /// <returns></returns>
        public int EnterRoomTable(TCTable _bftable, tb_User tbUser)
        {
            lock (objLock)
            {
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
        private int AutoAlloctoTable(TCUser tempU, int _tid, cs_enterroom _data)
        {
            lock (objLock)
            {
                TCTable _bftable;
                if (DicTable.TryGetValue(_tid, out _bftable))
                {//已存在流程是错误的
                    ErrorRecord.Record("fetal error 201601101429TC 必须处理 ");
                    return -1;
                }
                List<TCUser> _tempfirstUser = new List<TCUser>();
                _tempfirstUser.Add(tempU);
                TCTable tab = new TCTable(_gameid, _roomid, _tid, _tempfirstUser, _data);
                int _checkCode = tab.CheckRoomCard(tempU._userid);
                if (_checkCode != 1) return _checkCode;
                for (int i = 0; i < _tempfirstUser.Count; i++)
                {   //移出已经分配了的用户
                    TCUser temp02 = null;// new BullFightUser();
                    DicUser.TryRemove(_tempfirstUser[i]._userid, out temp02);
                }
                //添加到当前桌子列表，以便打牌过程好使用                
                if (!DicTable.TryAdd(_tid, tab)) ErrorRecord.Record("add _tableid fial maybe have exist... 201208241601TC");
                return 1;
            }
        }
        /// <summary>
        /// 满足条件 就添加 机器人   
        /// </summary>
        /// <param name="waitTimetoAddRobot"></param>
        public void RobotEnterRoom(int waitTimetoAddRobot)
        {
            lock (objLock)
            {
                if (DicUser.Count == 0 || DicUser == null)  return;

                DateTime lastDt = DateTime.Now;
                foreach (int tempUserID in DicUser.Keys)
                {
                    lastDt = DicUser[tempUserID]._enterTime;
                    break;  //===================不严谨 =======================
                }
                TimeSpan ts = DateTime.Now - lastDt;
                //如果大于5秒了， 就分配机器人 
                if (ts.TotalSeconds < waitTimetoAddRobot)  return;
             
                if (TCSendDataServer.instance.GetRobotExistNum() > TCSendDataServer.instance.maxRobotCount || TCSendDataServer.instance.QueRobotUser.Count <= 0)
                {   //现在人数大于多少了 就不增加 机器人了     
                   //// ErrorRecord.Record(" 201206121556TC 仅作提示使用， 即没有机器人添加的情况 ，正式运行时，需要去掉");
                    return;
                }
                tb_User tbuser;
                if (!TCSendDataServer.instance.QueRobotUser.TryDequeue(out tbuser)) return;

                TCUser tempU = new TCUser();
                tempU.Initi(tbuser.UserID + "", mRoomID, tbuser, true);      // 当成客户端 的IP：Port用
                if (!DicUser.TryAdd(tbuser.UserID, tempU)) ErrorRecord.Record("201208061154 已经存在， 添加不成功");

                base.EnterRoomBase(tbuser.UserID, _gameid);
                // _alocOver = true;//默认分配一个就不再分配了， //================================= 
                TCRobot.AddtoDicIDtoUser(tbuser.UserID, tempU);//机器人特有的触发动作
               //// sc_entertable_n(); //需要在分配之前
                AutoAllocationtoTable();//触发分配一下
                TCSendDataServer.instance.RobotExistNumAddOne();         
            }
        }
       
        /// <summary>
        /// 自动分配 3个人 到一桌  1.玩家进入房间触发。2.系统增加机器人触发
        /// 分配机器人也在这儿处理
        /// </summary>
        /// <returns></returns>
        private void AutoAllocationtoTable(bool start = false)
        {
            
        }

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
                    TCUser _tempcu;
                    if (!DicUser.TryRemove(userID, out _tempcu))
                    {
                        ErrorRecord.Record("用户列表清理失败 201611051448TC");
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 循环当前房间 的每一桌     //每桌的裁判[等待时间完了 自动 带打功能] 即时执行的
        /// </summary>
        public void DealEveryTable(int SecondOne)
        {
            if (0 != SecondOne) return;
            lock (objLock)
            {
                _tempwaitAfterLimit--;//暂时是1S执行一次  ------------------------------------------
                if (_tempwaitAfterLimit <= 0) AutoAllocationtoTable(true); //再执行一次分配
                try
                {
                    foreach (int rid in DicTable.Keys)
                    {   //2级， 循环房间里的每一个桌子  
                        if (0 != SecondOne) continue;
                        TCTable t;
                        if (!DicTable.TryGetValue(rid, out t)) continue;        //桌子没找到 
                        if (t._pos2userbase == null || t._pos2userbase.Count == 0) continue;
                        if (t._tablestatus == TableStatusEnum.Playing || t._tablestatus == TableStatusEnum.Initi)
                        {
                            if (!t.SomeUserIsOverTime())
                            {//等时间还没到
                                ; //也许 有动作 
                            }
                            else
                            {   //等待时间完了  

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorRecord.Record(ex, "201207061606TC 暂时可以不去掉");
                }
            }
        } 
        /// <summary>
        /// 根据房间ID， 桌子ID与用户ID 找到桌子对象
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="TableID"></param>
        /// <returns></returns>
        public TCTable GetTableByTableID(int TableID)
        {
            TCTable tempT;
            if (!DicTable.TryGetValue(TableID, out tempT))
            {
                ErrorRecord.Record(" 201206092237TC 在房间RoomID为：" + mRoomID + "里没找到TableID为：" + TableID);
                return null;
            }
           //// if (!tempT._isUsed) ErrorRecord.Record(" 201208011602TC 房间已经释放了 ");
            return tempT; 
        }
        public void ResetTableByTableID(int TableID)
        {   //不知道为什么这儿更新不了 -===================================================      
            TCTable  _tempbft;
            if (!DicTable.TryRemove(TableID, out _tempbft))
            {
                ErrorRecord.Record("201208011600TC 释放Table资源失败");
            }
        }
   
    }
}
