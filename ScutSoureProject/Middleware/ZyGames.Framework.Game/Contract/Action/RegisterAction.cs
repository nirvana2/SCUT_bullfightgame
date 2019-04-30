﻿/****************************************************************************
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

using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Common.Serialization;

namespace ZyGames.Framework.Game.Contract.Action
{
    /// <summary>
    /// Register action.
    /// </summary>
    public abstract class RegisterAction : BaseStruct
    {
        /// <summary>
        /// The name of the user.
        /// </summary>
        protected string UserName;
        /// <summary>
        /// The sex.
        /// </summary>
        protected int _Sex;
        /// <summary>
        /// The head I.
        /// </summary>
        protected string HeadID;
        /// <summary>
        /// The retail I.
        /// </summary>
        protected string RetailID;
        /// <summary>
        /// The pid.
        /// </summary>
        protected string Pid;
        /// <summary>
        /// The type of the mobile.
        /// </summary>
        protected MobileType MobileType;
        /// <summary>
        /// The screen x.
        /// </summary>
        protected int ScreenX;
        /// <summary>
        /// The screen y.
        /// </summary>
        protected int ScreenY;
        /// <summary>
        /// The req app version.
        /// </summary>
        protected string ReqAppVersion;
        /// <summary>
        /// The game I.
        /// </summary>
        protected int GameID;
        /// <summary>
        /// The server I.
        /// </summary>
        protected string ServerID;
        /// <summary>
        /// The device I.
        /// </summary>
        protected string DeviceID;
        /// <summary>
        /// Gets or sets the guide identifier.
        /// </summary>
        /// <value>The guide identifier.</value>
        public int GuideId { get; set; }
        protected cs_create1005 _createData;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ZyGames.Framework.Game.Contract.Action.RegisterAction"/> class.
        /// </summary>
        /// <param name="aActionId">A action identifier.</param>
        /// <param name="httpGet">Http get.</param>
        protected RegisterAction(short aActionId, ActionGetter httpGet)
            : base(aActionId, httpGet)
        {
        }
        /// <summary>
        /// 创建返回协议内容输出栈
        /// </summary>
        public override void BuildPacket()
        {
            PushIntoStack(GuideId);
        }
        /// <summary>
        /// 接收用户请求的参数，并根据相应类进行检测
        /// </summary>
        /// <returns></returns>
        public override bool GetUrlElement()
        {
            ////if (actionGetter.GetString("UserName", ref UserName) &&
            ////    actionGetter.GetByte("Sex", ref Sex) &&
            ////    actionGetter.GetString("HeadID", ref HeadID) &&
            ////    actionGetter.GetString("RetailID", ref RetailID) &&
            ////    actionGetter.GetString("Pid", ref Pid, 1, int.MaxValue) &&
            ////    actionGetter.GetEnum("MobileType", ref MobileType)
            ////    )
            ////{
            ////    UserName = UserName.Trim();
            ////    actionGetter.GetWord("ScreenX", ref ScreenX);
            ////    actionGetter.GetWord("ScreenY", ref ScreenY);
            ////    actionGetter.GetWord("ClientAppVersion", ref ReqAppVersion);
            ////    actionGetter.GetString("DeviceID", ref DeviceID);
            ////    actionGetter.GetInt("GameID", ref GameID);
            ////    actionGetter.GetInt("ServerID", ref ServerID);
            ////    return GetActionParam();
            ////}
            string _dataEx = "";
            if (actionGetter.GetString("_dataEx", ref _dataEx))
            {
                cs_create1005 _tempdata = JsonUtils.Deserialize<cs_create1005>(_dataEx);
                _createData = _tempdata;
                UserName = _tempdata.roleName;
                ScreenX = _tempdata.ScreenX;
                ScreenY = _tempdata.ScreenY;
                ReqAppVersion = _tempdata.ClientAppVersion;
                DeviceID = _tempdata.ServerID;
                GameID = _tempdata.GameID;
                ServerID = _tempdata.ServerID;
                _Sex = _tempdata._Sex;
                HeadID = _tempdata.HeadID;
                RetailID = _tempdata.RetailID;
                Pid = _tempdata.Pid;
                MobileType = (MobileType)_tempdata.MobileType;
                return GetActionParam();
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CheckAction()
        {
            if (!GameEnvironment.IsRunning)
            {
                ErrorCode = Language.Instance.ErrorCode;
                ErrorInfo = Language.Instance.ServerLoading;
                return false;
            }
            if (Current.UserId <= 0)
            {
                ErrorCode = Language.Instance.ErrorCode;
                ErrorInfo = Language.Instance.UrlElement;
                return false;
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool TakeAction()
        {
            IUser user;
            if (CreateUserRole(out user) && Current != null && user != null)
            {
                Current.Bind(user);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 处理结束执行
        /// </summary>
        /// <param name="state">If set to <c>true</c> state.</param>
        public override void TakeActionAffter(bool state)
        {
        }

        /// <summary>
        /// Gets the action parameter.
        /// </summary>
        /// <returns><c>true</c>, if action parameter was gotten, <c>false</c> otherwise.</returns>
        protected abstract bool GetActionParam();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool CreateUserRole(out IUser user);

    }


    /// <summary>
    /// 创建角色的网络数据结构
    /// </summary>
    public class cs_create1005
    {
        public string roleName;//  writer.writeString("UserName", actionParam.Get<string>("roleName"));
        public int pos_x;//    writer.writeInt32("pos_x", actionParam.Get<int>("pos_x"));
        public int pos_y;//writer.writeInt32("pos_y", actionParam.Get<int>("pos_y"));
        public int pos_z;//writer.writeInt32("pos_z", actionParam.Get<int>("pos_z"));
        public string scene_name;//writer.writeString("scene_name", actionParam.Get<string>("scene_name")); 
        public string RetailID;//writer.writeString("RetailID", GameSetting.Instance.RetailID);
        public string Pid;//writer.writeString("Pid", GameSetting.Instance.Pid);
        public int MobileType;//writer.writeInt32("MobileType", GameSetting.Instance.MobileType);
        public int ScreenX;//writer.writeInt32("ScreenX", GameSetting.Instance.ScreenX);
        public int ScreenY;// writer.writeInt32("ScreenY", GameSetting.Instance.ScreenY);
        public string ClientAppVersion;// writer.writeString("ClientAppVersion", GameSetting.Instance.ClientAppVersion);
        public int GameID;//writer.writeInt32("GameType", GameSetting.Instance.GameID);
        public string ServerID;//writer.writeInt32("ServerID", GameSetting.Instance.ServerID);    
        public int _Sex;     // The sex.       
        public string NickName = string.Empty;   // The name of the nick.       
        public string HeadID = string.Empty;    // The head I.
    }
}