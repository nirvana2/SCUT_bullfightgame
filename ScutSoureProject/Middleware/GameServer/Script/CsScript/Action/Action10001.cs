using System; 
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common;
using ZyGames.Framework.Game.Contract.Action;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Common.Serialization;
using System.Collections.Generic;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Net;
using ZyGames.Framework.RPC.Sockets;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{

    /// <summary>
    /// 基础通信专业做推送
    /// </summary>
    /// <remarks>继续BaseStruct类:不检查用户合法性请求;AuthorizeAction:有验证合法性</remarks>
    public class Action10001 : BaseStruct
    {                               
        private sc_base _temp;       
        private string _dataEx = "";
        private string _senddata="";
        private Dictionary<int, string> _dicUserIdSendData = new Dictionary<int, string>();

        public Action10001(ActionGetter actionGetter) : base(10001, actionGetter)
        {
            
        }

        /// <summary>
        /// 客户端请求的参数较验
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool GetUrlElement()
        { 
            if (actionGetter.GetString("_dataEx", ref _dataEx))
            {
                _temp = JsonUtils.Deserialize<sc_base>(_dataEx);
                if (_temp.fn =="")
                {
                    ErrorRecord.Record(" JSON data error! _dataEx:" + _dataEx);
                    return false;
                }
                _senddata = _dataEx;
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
            return true;
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
            this.PushIntoStack(_senddata);
        }
    }

    
}
