using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Net;
using ZyGames.Framework.Common.Timing;
using GameServer.Script.Model;
using System.Linq;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 自动服务类 即，服务启动就要运行 包括 以下功能：
    /// 1.自动消息队列。
    /// 2.机器人的配置。
    /// 3.玩家满四个分配到一桌。
    /// 
    /// </summary>
    public class BF100SendDataServer : BaseSendDataServer 
    {
        private static BF100SendDataServer _ins;
        public static BF100SendDataServer instance
        {
            get
            {
                if (_ins == null) _ins = new BF100SendDataServer();
                return _ins;
            }
        }
        public BF100SendDataServer()
        {
            TurnWaitTime = 60*60*24*100;//去掉用户超时功能 100天
        }
        /// <summary>
        /// 允许一个房间配置最大机器人的数量
        /// </summary>
        public  int maxRobotCount = 20;
      
        /// <summary>
        /// 机器人处理的线程
        /// </summary>
        private  WaitCallback callBack = new WaitCallback(BullFight100Robot.RobotDealMSG);

        /// <summary>
        /// 开始就自动运行
        /// 消息队列处理
        /// </summary>
        private  void StartAutoSendData()
        {
            while (true)
            {
                //即时执行的发送消息模块
                if (_waittoSendData.Count <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                #region  //服务端 主动发消息

                List<UserIDMSG> imlist;
                if (!_waittoSendData.TryDequeue(out imlist)) continue;

                foreach (UserIDMSG im in imlist)
                {   //给指定的人发消息                           
                    try
                    {
                        // 分两种情况 1.给机器人发消息   2.给指定的人发消息
                        if (im._isrobot)
                        {   // 机器人ID  //这儿需要设置机器人的AI 如果是用户掉线， 机器人代打， 是了低级的AI
                            int UserID = im._userid;
                            object[] objArr = new object[3];
                            objArr[0] = (object)UserID;
                            objArr[1] = (object)im._senddata;
                            objArr[2] = (object)1;//机器人的AI等级
                            lock (_objLock)
                            {
                                ThreadPool.QueueUserWorkItem(callBack, (object)objArr);
                                // Robot.RobotDealMSG(UserID, im.strMSG);//这儿涉及到 处理时间长，，且需要 多用户同时处理，故使用多线程，线程池的方法
                            }

                            Thread.Sleep(1);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorRecord.Record(ex, "201206052240 可能的未知错误");
                        continue;
                    }
                }
                AutoSendData(imlist);

                #endregion
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 向里面压入数据 即会发送消息  不能放父类，否则会消息乱传了，，，，，-------------
        /// </summary> 
        private  ConcurrentQueue<List<UserIDMSG>> _waittoSendData = new ConcurrentQueue<List<UserIDMSG>>();
        public override void SendDataDelay(List<UserIDMSG> imList)
        {
            _waittoSendData.Enqueue(imList);
        }

        #region robot 

        public override void StartServer()
        {
            // 新建一个后台处理线程  进行初始化相关的 与服务器自己运行的， 及机器人等逻辑 
            Thread.CurrentThread.IsBackground = true;
            Thread processor = new Thread(new ThreadStart(StartAutoSendData));//
            processor.Start();//线程开始 


            ////// 新建一个后台处理线程  服务器向客户端 主动发信息 保证发送消息的流畅   
            ////Thread.CurrentThread.IsBackground = true;
            ////Thread msgProcessor = new Thread(new ThreadStart(StartRobotTimer));//
            ////msgProcessor.Start();//线程开始 
            BullFight100Lobby.instance.Initi();
            InitiRobotList();// 获取机器人列表 
            //间隔1秒执行，延迟2000毫秒开始启动，执行10分钟超时    测试时候用
            SyncTimer testTimer = new SyncTimer(StartRobotTimer, 5000, 1000, 10 * 60 * 1000);
            testTimer.Start();

            SyncTimer _syncTimer = new SyncTimer(AddOnlineInformation, 1, 60000, 60 * 1000);
            _syncTimer.Start();
            base.StartServer();
        }

        /// <summary>
        /// 开始就自动运行
        /// 消息队列处理
        /// </summary>
        private void StartRobotTimer(object state)
        {
            CommonLogic.AutoHeartBeat(); 
            var _roomlist = BullFight100Room.roomCache.FindAll();
            foreach (var _tempRoom in _roomlist)
            {   //1级， 循环大厅里的每一个房间   
                lock (_tempRoom)
                {
                    _tempRoom.DealEveryTable(0); //每桌的裁判[等待时间完了 自动 带打功能] 即时执行的
                }
            }
        }
        private void AddOnlineInformation(object state)
        {
            var bll_online = new BLL_OnlineInformation();
            lock (_objLock)
            {
                try
                { 
                    var data = new List<tb_OnlineInformation>();
                    var brlist = BullFight100Room.roomCache.FindAll();
                    foreach (var _tempbfroom in brlist)
                    {
                        int gameModel = 2; 
                        int userCount = 0; 
                        // var count = _tempbfroom.DicTable.Count * 4 + _tempbfroom.DicUser.Count;
                        if (_tempbfroom.DicTable.Count > 0)
                        {
                            _tempbfroom.DicTable.Values.ToList().ForEach(d =>
                            { 
                                userCount += d._pos2userbase.Values.Where(w => !w._isRobot).Count(); 
                            });
                            //._pos2userbase.Values.Where(w => !w._isRobot).Count();
                            BullFight100Table _tempbfTable = _tempbfroom.DicTable.Values.FirstOrDefault();
                            if (_tempbfTable != null && _tempbfTable._judge != null)
                            {
                                gameModel = _tempbfTable._judge._gameCoin2Room1;
                            }
                        }
                        if (userCount > 0)
                        {
                            data.Add(new tb_OnlineInformation()
                            {
                                CreateTime = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"),
                                GameModel = gameModel,
                                RoomId = _tempbfroom.mRoomID,
                                OnlineCount = userCount,
                                GameType = 1
                            });
                        }
                    }
                    bll_online.AddRange(data); 
                }
                catch (Exception ex)
                {
                    ErrorRecord.Record(new Exception(), ex.Message);
                }
            }
        }

        /// <summary>
        /// 初始化机器人队列，MySQLDAL.Model.tb_User 数据 
        /// </summary> 
        private  void InitiRobotList()
        { 
            base.InitiRobot();
            ////BullFightRobot.Type = 1;    //设置机器人AI值  
            ////BullFightRobot.AIValue = 10;
        }
        #endregion
    }
}

