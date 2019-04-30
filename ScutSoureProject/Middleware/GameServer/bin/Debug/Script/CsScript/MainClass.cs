/****************************************************************************
Copyright (c) 2013-2015 scutgame.com

http://www.scutgame.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Script;
using System.Threading;
using GameServer.Script.CsScript.Action;
using GameServer.Script.Model;
using System.Collections.Generic;
using ZyGames.Framework.Game.Service;

namespace Game.Script
{
    public class MainClass : GameSocketHost, IMainScript
    { 
        public MainClass()
        {
           
        }

        protected override void OnStartAffer()
        {
            ////    public static void UnBindAllSession()
            ////{
            ////    var sessions = GameSession.GetAll();
            ////    foreach (var session in sessions)
            ////    {
            ////        session.UnBind();
            ////    }
            ////}
            var sessions = GameSession.GetAll();
            foreach (var session in sessions)
            {
                session.UnBind();
            }
            //GMService.Current.Start("http://112.74.175.43", 8080, "Service");
            GMService.Current.Start("http://127.0.0.1", 8088, "Service");
            MyConfig.Initi();
            tb_UserEx.RecoverFromDb(">",-1);
            ////// 获取机器人列表 取一次放到内存里面，
            CommonLogic.InitiRobotList();//所有游戏共用
           
            BF100SendDataServer.instance.StartServer();
            ////TCSendDataServer.instance.StartServer();
            #region only for BullFight test
            ////PokerBullFightType _bulltype = BullFight.GetBullType(new System.Collections.Generic.List<int>() { 409, 206, 202, 108, 106 });      //right  牛1
            ////List<int> _b3lsit01 = BullFight.GetBullTypeHelp(new List<int>() { 409, 206, 202, 108, 106 }); 

            ////PokerBullFightType _bulltype20 = BullFight.GetBullType(new System.Collections.Generic.List<int>() { 412, 208, 308, 408, 108 });      //right 取消牛炸与五小牛
            ////List<int> _b3lsit20 = BullFight.GetBullTypeHelp(new System.Collections.Generic.List<int>() { 412, 208, 308, 408, 108 });

            ////PokerBullFightType _bulltype20 = BullFight.GetBullType(new System.Collections.Generic.List<int>() { 401, 404, 308, 307, 311 });      //right 取消牛炸与五小牛
            ////List<int> _b3lsit20 = BullFight.GetBullTypeHelp(new System.Collections.Generic.List<int>() { 401, 404, 308, 307, 311 });
            #endregion 
        }

        protected override void OnServiceStop()
        {
            GameEnvironment.Stop();
        }

        ////protected override void OnConnectCompleted(object sender, ConnectionEventArgs e)
        ////{
        ////    Console.WriteLine("客户端IP:[{0}]已与服务器连接成功", e.Socket.RemoteEndPoint);
        ////    base.OnConnectCompleted(sender, e);
        ////}

        protected override void OnDisconnected(GameSession session)
        {   //这里处理收到close指令的断线业务
            Console.WriteLine("客户端OnDisconnected UserId:[{0}]{1}已与服务器断开", session.UserId, session.RemoteAddress);
            CommonLogic.ExitRoomByDisConnect(session.UserId);
            base.OnDisconnected(session);
        }
        protected override void OnHeartbeatTimeout(GameSession session)
        {    //这里处理未收到close指令的断线业务  网络不好的断线会进来儿。
            Console.WriteLine("客户端OnHeartbeatTimeout UserId:[{0}]{1}已与服务器断开", session.UserId, session.RemoteAddress);
            CommonLogic.ExitRoomByDisConnect(session.UserId);
            GameSession.ClearSession(m => m.UserId == session.UserId);

            base.OnHeartbeatTimeout(session);
        }
        protected override void OnHeartbeat(GameSession session)
        {
            Console.WriteLine("{0}>>Hearbeat package: {1} userid {2} session count {3}", DateTime.Now.ToString("HH:mm:ss"), session.RemoteAddress, session.UserId, GameSession.Count);
            base.OnHeartbeat(session);
        }
        protected override void BuildHearbeatPackage(RequestPackage package, GameSession session)
        {
            SocketGameResponse response = new SocketGameResponse();
            DataStruct dataStruct = new DataStruct();
            dataStruct.WriteAction(response, package.ActionId, 0, "", package.MsgId);
            var data = response.ReadByte();
            session.SendAsync(data, 0, data.Length);
            base.BuildHearbeatPackage(package, session);
        }
        ////protected override void OnRequested(ActionGetter actionGetter, BaseGameResponse response)
        ////{
        ////    Console.WriteLine("Client {0} request action {1}", actionGetter.GetSessionId(), actionGetter.GetActionId());
        ////}

    }
}