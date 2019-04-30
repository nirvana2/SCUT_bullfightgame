using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Net;
using GameServer.Script.Model;
using ZyGames.Framework.Common.Timing;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 自动服务类 即，服务启动就要运行 包括 以下功能：
    /// 1.自动消息队列。
    /// 2.机器人的配置。3.玩家满四个分配到一桌。
    /// 
    /// </summary>
    public class TCSendDataServer : BaseSendDataServer
    {
        private static TCSendDataServer _ins;
        public static TCSendDataServer instance
        {
            get
            {
                if (_ins == null) _ins = new TCSendDataServer();
                return _ins;
            }
        }
        /// <summary>
        /// 允许一个房间配置最大机器人的数量
        /// </summary>
        public  int maxRobotCount = 2;
        /// <summary>
        /// 机器人处理的线程
        /// </summary>
        private  WaitCallback callBack = new WaitCallback(TCRobot.RobotDealMSG);
        
        
        /// <summary>
        /// 开始就自动运行
        /// 消息队列处理
        /// </summary>
        public  void StartAutoSendData()
        {
            while (true)
            {
                //即时执行的发送消息模块
                if (_waittoSendData.Count <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }
                lock (_objLock)
                {
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
                                objArr[1] = (object)im._senddata;         //===========================
                                objArr[2] = (object)1;//机器人的AI等级
                                ThreadPool.QueueUserWorkItem(callBack, (object)objArr);
                                // Robot.RobotDealMSG(UserID, im.strMSG);//这儿涉及到 处理时间长，，且需要 多用户同时处理，故使用多线程，线程池的方法
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
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 向里面压入数据 即会发送消息  不能放父类，否则会消息发送传了，，，，，-------------
        /// </summary> 
        private  ConcurrentQueue<List<UserIDMSG>> _waittoSendData = new ConcurrentQueue<List<UserIDMSG>>();
        public override void SendDataDelay(List<UserIDMSG> imList)
        {
            _waittoSendData.Enqueue(imList);
        }

        #region  robot
        public override void StartServer()
        {
            //建一个后台处理线程  进行初始化相关的 与服务器自己运行的， 及机器人等逻辑       
            Thread.CurrentThread.IsBackground = true;
            Thread processor = new Thread(new ThreadStart(StartAutoSendData));//
            processor.Start();//线程开始 

                          
            TCLobby.instance.Initi();     // 初始化 大厅 及下面的 房间 与桌子
            InitiRobotList();// 获取机器人列表 
            //间隔1秒执行，延迟2000毫秒开始启动，执行10分钟超时    测试时候用
            SyncTimer testTimer = new SyncTimer(StartRobotTimer, 2000, 1000, 10 * 60 * 1000);
            testTimer.Start();

            base.StartServer();
        }
        /// <summary>
        /// 开始就自动运行
        /// 消息队列处理
        /// </summary>
        private void StartRobotTimer(object state)
        {
            //begin   服务自动初始化的一些数据       
            CommonLogic.AutoHeartBeat();
            foreach (int key in TCLobby.instance._DicRoom.Keys)
            {   //1级， 循环大厅里的每一个房间         
                TCRoom _bfroom;
                if (!TCLobby.instance._DicRoom.TryGetValue(key, out _bfroom)) continue;
                if (_bfroom != null)
                {
                    //每桌的裁判[等待时间完了 自动 带打功能] 即时执行的
                    _bfroom.DealEveryTable(0);
                }
            }
        }


        /// <summary>
        /// 初始化机哭人队列，MySQLDAL.Model.tb_User 数据 
        /// </summary> 
        private  void InitiRobotList()
        {
            //int count = TCLobby.instance._DicRoom.Count * maxRobotCount;
            base.InitiRobot();
            ////TCRobot.Type = 1;    //设置机器人AI值  
            ////TCRobot.AIValue = 10;
        }
        #endregion
    }
}

