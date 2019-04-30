using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Net;
using GameServer.Script.Model;
using ZyGames.Framework.RPC.Sockets;
using ZyGames.Framework.Common.Timing;
using ZyGames.Framework.Script;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 自动服务类 即，服务启动就要运行 包括 以下功能：
    /// 1.自动消息队列。
    /// 2.机器人的配置。
    /// 3.玩家满四个分配到一桌。 
    /// </summary>
    public abstract class BaseSendDataServer //: IBSDataServer
    { 
        protected readonly object _objLock = new object();
        //全局配置信息
        private  int _gameid;
        /// <summary>
        /// 用户等待多长时间就添加一个机器人 秒  设置成5秒， 测试用1秒
        /// </summary>
        private int _waitTimetoAddRobot = 1;//设置成5秒， 测试用1秒

        /// <summary>
        /// 15 每个桌子的每个动作等待时间 暂定15秒，若超时，进行默认处理
        /// </summary>
        private int _TurnWaitTime = 15; //--------------------------------------------------------------


        /// <summary>
        /// 全局集合
        /// 获取机器人列表
        /// 开始时初始化一次  如有需要 ， 以后再动态初始化
        /// </summary>
        private ConcurrentQueue<tb_User> _QueRobotUser;
        /// <summary>
        /// 已经存在的机器人
        /// </summary>
        private int _RobotExistNum = 0;

        protected int gameid
        {
            get{return _gameid;}
            set{_gameid = value;}
        }

        public int waitTimetoAddRobot
        {
            get{return _waitTimetoAddRobot;}
            set{_waitTimetoAddRobot = value;}
        }

        /// <summary>
        /// 操作的等待时间  房间模式会很长很长的
        /// </summary>
        public  int TurnWaitTime
        {
            get{return _TurnWaitTime;}
            set{_TurnWaitTime = value;}
        }

        public ConcurrentQueue<tb_User> QueRobotUser
        {
            get{return _QueRobotUser;}
            set{_QueRobotUser = value;}
        }

        public int GetRobotExistNum()
        {
             return _RobotExistNum; 
             
        }
        public void RobotExistNumAddOne()
        {
            Interlocked.Increment(ref _RobotExistNum);
        }
        public void RobotExistNumReduceOne()
        {
            Interlocked.Decrement(ref _RobotExistNum);
        }
        /// <summary>
        /// 初始化机哭人队列，MySQLDAL.Model.tb_User 数据 
        /// </summary> 
        public void InitiRobot(   )
        {
            _QueRobotUser = new ConcurrentQueue<tb_User>();            
            foreach (tb_User tu in CommonLogic._robotUserList)
            {   //会出现在不同游戏中有，有相同的机器人的情况，，，， 
                //if (tu.GameID != gameid) continue;                
                _QueRobotUser.Enqueue(tu);
            }
        }             

        public virtual void StartServer()
        {                 
            //每天0点执行
            TimeListener.Append(PlanConfig.EveryDayPlan(DoEveryDayExecute, "EveryDayTask", "00:00"));
        }
        private static void DoEveryDayExecute(PlanConfig planconfig)
        {
            if (ScriptEngines.IsCompiling)  return; //因为代码热更新 需要先处理掉

            //do something
            tb_RankEx.SetRankListEveryDay();
        }
        public async void AutoSendData(List<UserIDMSG> imlist)
        {
             List<UserIDMSG> _umsg = imlist.FindAll((msg) => { return !msg._isrobot; });

            foreach (UserIDMSG msg in _umsg)
            {
                if (msg._isrobot || msg._isDisconnect) continue;

                var notifyUsers = new List<IUser>();
                GameSession _tempSession = GameSession.Get(msg._userid);
                //test  fun  1.
                if (_tempSession != null)
                {
                    notifyUsers.Add(_tempSession.User);
                    Parameters param = new Parameters();
                    param.Add("_dataEx", msg._senddata);
                    await ActionFactory.SendAsyncAction(notifyUsers, (int)ActionType.BaseMsgNotify, param, t =>
                    {
                        if (ResultCode.Error == t.Result)
                        {
                            ErrorRecord.Record("201704181628,  SendAsyncAction error , data:" + msg._senddata);
                        }
                        else Console.WriteLine("BaseSendDataServer 推送10001结果:{0}", t.Result.ToString());
                    });
                }
            }
        }

        public virtual void SendDataDelay(List<UserIDMSG> imList)
        {
           
        }
        /// <summary>
        /// 给在线的用户推送消息
        /// </summary>
        /// <param name="senddata"></param>
        ////public static async void AutoNotifySendData(string senddata)
        ////{
        ////    List<GameSession> sessionList = GameSession.GetOnlineAll(1*1000) as List<GameSession>;

        ////    Parameters parameter = new Parameters();
        ////    parameter.Add("_dataEx", senddata);
        ////    sbyte opCode = OpCode.Binary;
        ////    if (sessionList.Count == 0) return;

        ////    RequestPackage package = parameter is Parameters
        ////   ? ActionFactory.GetResponsePackage((int)ActionType.BaseMsgNotify, sessionList[0], parameter as Parameters, opCode, null)
        ////   : ActionFactory.GetResponsePackage((int)ActionType.BaseMsgNotify, sessionList[0], null, opCode, parameter);

        ////    await ActionFactory.BroadcastAction((int)ActionType.BaseMsgNotify, sessionList, package, (session, asyncResult) =>
        ////    {
        ////        Console.WriteLine("Action 1002 send result:{0}", asyncResult.Result == ResultCode.Success ? "ok" : "fail");
        ////    }, 0);
        ////}


        public static async void AutoNotifySendData(string senddata)
        {
            List<GameSession> sessionList = GameSession.GetOnlineAll(10 * 1000) as List<GameSession>; //心跳间隔10s发送
            if (sessionList == null) return;
            var parameters = new Parameters();
            parameters.Add("_dataEx", senddata);
            await ActionFactory.SendAction(sessionList, 1002, parameters, (session, asyncResult) =>
            {
                if (ResultCode.Error == asyncResult.Result)
                {
                    ErrorRecord.Record("201704181638,  SendAsyncAction error , data:" + senddata);
                }
                else Console.WriteLine("Action 1002 send result:{0}", asyncResult.Result == ResultCode.Success ? "ok" : "fail");

            }, OpCode.Binary, 0); 
        }
    }
}

