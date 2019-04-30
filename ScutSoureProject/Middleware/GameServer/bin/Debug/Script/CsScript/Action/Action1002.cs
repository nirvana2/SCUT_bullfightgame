using System;
using ZyGames.Framework.Common.Security;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Game.Sns;
using ZyGames.Framework.Common.Serialization;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{ 
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>继续BaseStruct类:不检查用户合法性请求;AuthorizeAction:有验证合法性</remarks>
    public class Action1002 : BaseStruct
    {
        private string passport = string.Empty;
        private string password = string.Empty;
        private string deviceID = string.Empty;
        private int mobileType = 0;
        private int gameID = 0;
        private string retailID = string.Empty;
        private string clientAppVersion = string.Empty;
        private int ScreenX = 0;
        private int ScreenY = 0;
        private string _openid;
        private string _senddata = "";//如果只有一个role就直接返回不走推送接口
        public Action1002(ActionGetter actionGetter)
            : base((short)ActionType.Regist, actionGetter)
        {
        }

        /// <summary>
        /// 客户端请求的参数较验
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool GetUrlElement()
        {      
            string _dataEx = "";
            if (actionGetter.GetString("_dataEx", ref _dataEx))
            {
                cs_device _temp =  JsonUtils.Deserialize<cs_device>(_dataEx);
                mobileType = _temp.MobileType;
                gameID = _temp.GameID;
                retailID = _temp.RetailID;
                clientAppVersion = _temp.ClientAppVersion;
                deviceID = _temp.DeviceID;
                ScreenX = _temp.ScreenX;
                ScreenY = _temp.ScreenY;
                _openid = _temp._openid;
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
            try
            {
                if (_openid != "")
                {
                    var q = SnsManager.Register(_openid, "123456", "", true);
                    var s = SnsManager.RegisterWeixin(_openid, "123456", "", _openid);
                    SnsUser _tempu = SnsManager.LoginByWeixin(_openid);
                    passport = _tempu.PassportId;
                    password = _tempu.Password;
                    _tempu.RetailUser = _openid;
                    _tempu.RetailID = retailID;
                    _tempu.WeixinCode = _openid;
                    SnsManager.ChangeUserInfo(passport, _tempu);
                }
                else
                {
                    string[] userList = SnsManager.GetRegPassport(deviceID);
                    passport = userList[0];
                    password = userList[1];
                }
                sc_device _scd = new sc_device() { fn= "sc_device", result = 1 };
                _scd.passportid = passport;
                _scd.password = password;
                _senddata = JsonUtils.Serialize(_scd);
                return true;
            }
            catch (Exception ex)
            {
                this.SaveLog(ex);
                this.ErrorCode = Language.Instance.ErrorCode;
                this.ErrorInfo = Language.Instance.St1002_GetRegisterPassportIDError;
                return false;
            }
        }
        public override   void TakeActionAffter(bool state)
        {
           
        }
        /// <summary>
        /// 下发给客户的包结构数据
        /// </summary>
        public override void BuildPacket()
        {
            if (_senddata != "") this.PushIntoStack(_senddata);
        }    
    }


    public class cs_device : cs_base
    {
        public int MobileType;
        public int GameID;
        public string RetailID;
        public string ClientAppVersion;
        public string DeviceID;
        public int ScreenX;
        public int ScreenY;
        public int ServerID;
        public string _openid;
    }                              

    public class sc_device : sc_base
    {
        public string passportid;
        public string password;
    }
}
