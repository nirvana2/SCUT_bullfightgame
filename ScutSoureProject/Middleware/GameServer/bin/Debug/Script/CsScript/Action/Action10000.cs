using System;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Game.Service;
using GameServer.Script.Model;
using ZyGames.Framework.Game.Contract;
using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    //201611081602解绑IP与UserID//201611081602解绑IP与UserID//201611081602解绑IP与UserID//201611081602解绑IP与UserID//201611081602解绑IP与UserID//201611081602解绑IP与UserID
    /// <summary>
    /// 基础通信请求接收后分发          适配以前写法，，，尝试一下先
    /// </summary>
    /// <remarks>继续BaseStruct类:不检查用户合法性请求;AuthorizeAction:有验证合法性</remarks>
    public class Action10000 : BaseStruct
    {
        private int _headUserID;
        private cs_base _temp;
        private CommonLogic _commonLogic;
        private string _dataEx = "";
        private string _senddata = "";//如果只有一个role就直接返回不走推送接口
        private string _dicUserIdSendData = "";

        public Action10000(ActionGetter actionGetter) : base(10000, actionGetter)
        {
            _commonLogic = new CommonLogic();
        }

        /// <summary>
        /// 客户端请求的参数较验
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool GetUrlElement()
        {
            _headUserID = actionGetter.GetUserId();
            if (actionGetter.GetString("_dataEx", ref _dataEx))
            {
                _temp = JsonUtils.Deserialize<cs_base>(_dataEx);
                if (_temp.fn == "")
                {
                    ErrorRecord.Record(" JSON data error! _dataEx:" + _dataEx);
                    return false;
                }
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
            //自己处理一次 RPC
            //ErrorCode = Language.Instance.ErrorCode;
            if (_dataEx == "") return false;
            ErrorCode = 0;
            if (_temp._userid == 1)
            {//表示需要断线重库设置
                _commonLogic.SetNotifyReConnect(Current.UserId);
            }
            ////var sessionList = GameSession.GetAll(); //使用GameSession类可以获得，它包括有在线的玩家和不在线的玩家，代码如下：
            ////if (Current.UserId == 0)
            ////{//处理短暂的断线重连，绑定userid与user，删除上个Session对象。
            ////    GameSession _oldsession = GameSession.Get(_temp._userid);
            ////    if (_oldsession != null)
            ////    {
            ////        Current.User = _oldsession.User;
            ////        //if(!_oldsession.IsClosed) //同一帐号在登录  可能会踢人处理
            ////        ////_oldsession.User = null;
            ////        ////_oldsession.SetExpired();
                    
            ////        _mjLogic.SetNotifyReConnect(Current.UserId);
            ////    }
            ////}
            string _ipport = Current.RemoteAddress;

            try
            {
                return _commonLogic.DealDataEx(_dataEx, _ipport, Current.UserId, out _dicUserIdSendData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("10000  "+ ex.Message);
                ErrorRecord.Record(ex, " errorloc 201610231256 ");
                return false;
            }         
        }


        public override   void TakeActionAffter(bool state)
        {         
            if (Current.User == null && _temp.fn != "cs_gm_chesscard")
            {
                Console.WriteLine("10000 Current.User == null......................................");  
            }


            _senddata = _dicUserIdSendData;//只有一个，
                
             
                ////string _senddata01 = _dicUserIdSendData[key];
                ////var notifyUsers = new List<IUser>();
                ////GameSession _tempSession = GameSession.Get(key);   
                //////test  fun  1.
                ////if (_tempSession != null)
                ////{
                ////    notifyUsers.Add(_tempSession.User);
                ////    Parameters param = new Parameters();
                ////    param.Add("_dataEx", _senddata01);
                ////    await ActionFactory.SendAsyncAction(notifyUsers, (int)ActionType.BaseMsgNotify, param, t =>
                ////    {
                ////        Console.WriteLine(" 推送10001结果:{0}", t.Result.ToString());
                ////    });
                ////} 
            base.TakeActionAffter(state);
        }

        //https://github.com/ScutGame/Scut/wiki/ActionPush  消息 推送示例

        ////public override void TakeActionAffter(bool state)
        ////{
        ////    int usserId = 138000;
        ////    var user = PersonalCacheStruct.Get<UserRole>(usserId.ToString());
        ////    var userList = new List<IUser>();
        ////    userList.Add(new SessionUser(user));

        ////    var parameters = new Parameters();
        ////    parameters["ID"] = 123;
        ////    ActionFactory.SendAction(userList, 1002, parameters, (asyncResult) =>
        ////    {
        ////        Console.WriteLine("Action 1002 send result:{0}", asyncResult.Result == ResultCode.Success ? "ok" : "fail");

        ////    }, httpGet.OpCode, 0);
        ////    base.TakeActionAffter(state);
        ////}

        /// <summary>
        /// 下发给客户的包结构数据
        /// </summary>
        public override void BuildPacket()
        {
            if(_senddata != "") this.PushIntoStack(_senddata);
        }
    }
}
