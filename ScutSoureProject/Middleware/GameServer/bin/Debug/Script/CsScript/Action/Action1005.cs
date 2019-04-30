using System;
using System.Collections.Generic;
using GameServer.Script.Model;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.Action;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Model;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Game.Cache;

namespace GameServer.Script.CsScript.Action
{

    /// <summary>
    /// 1005_创建角色接口
    /// </summary>
    public class Action1005 : RegisterAction
    {
        private string _senddata;

        public Action1005(ActionGetter actionGetter)
            : base((short)ActionType.CreateRote, actionGetter)
        {

        }

        protected override bool GetActionParam()
        {
            return true;
        }

        protected override bool CreateUserRole(out IUser user)
        {
            user = null;
            var cacheSet = new PersonalCacheStruct<GameUser>();
            GameUser gameUser;
            if (cacheSet.TryFindKey(UserId + "", out gameUser) == LoadingStatus.Success)
            {
                if (gameUser == null)
                {
                    gameUser = new GameUser { UserId = UserId, PassportId = Pid, RetailId = RetailID, NickName = Pid };
                    cacheSet.Add(gameUser);
                } 
                var tbuserCache = new GameDataCacheSet<tb_User>();
                if (gameUser.CurrRoleId == 0)
                {
                    tb_User _tempuser = tbuserCache.FindKey(UserId);
                    if (_tempuser == null)
                    {//自动注册同个角色到游戏库中
                     // VALUES(2, 'j222222', '', 1000.0000, 100000.0000, '', '', '', 'wechatname', '114.114.114.114', 'desc', 0, 0, 1, 1380002, 1000)
                      
                        int genCodeUserId = GameServerManager.GetGenerateUserId(UserId);
                        _tempuser = new tb_User()
                        {
                            Desc = "desc",
                            diamond = 0,
                            IP = Current.RemoteAddress == null ? string.Empty : Current.RemoteAddress,
                            isRobot = 0,
                            LastLotinTime1 = "",
                            LastLotinTime2 = "",
                            wechatName = _createData.roleName == "" ?  UserId + "wechat" : _createData.NickName,
                            RegTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            RobotLevel = 0,
                            Status = 0,
                            UserID = UserId,
                            UserMaxMoney = 20 * 10000 * 10000,
                            UserMoney = 2000, //t_anythingList.GetInt("init_gold"),
                            UserName = _createData.roleName,
                            UserPassword = "",
                            Sex = _createData._Sex,
                            wechatHeadIcon = _createData.HeadID, 
                        };                                
                        tbuserCache.Add(_tempuser);
                                                          
                        //InitiData.InitiRoleData(UserId);

                        //触发邀请玩家任务计数
                        //if (genCodeUserId > 0) TaskHelper.SetTaskCount(genCodeUserId, TaskTypeEnum.User);
                    }
                    gameUser.CurrRoleId = UserId;
                    sc_create _create = new sc_create() { fn = "sc_create", result = 1 };
                    _senddata = JsonUtils.Serialize(_create);
                    return true;
                }
            }
            Console.WriteLine("10000 CreateUserRole  is fail......................................");
            return false;
        }
        ////protected override bool CreateUserRole(out IUser user)
        ////{
        ////    user = null;
        ////    if (UserName.Length < 2 || UserName.Length > 12)
        ////    {
        ////        ErrorCode = Language.Instance.ErrorCode;
        ////        ErrorInfo = Language.Instance.St1005_UserNameNotEnough;
        ////        return false;
        ////    }
        ////    var userCache = new PersonalCacheStruct<GameUser>();
        ////    var roleCache = new PersonalCacheStruct<UserRole>();
        ////    GameUser gameUser;
        ////    if (userCache.TryFindKey(UserId.ToString(), out gameUser) == LoadingStatus.Success)
        ////    {
        ////        if (gameUser == null)
        ////        {
        ////            gameUser = new GameUser
        ////            {
        ////                UserId = UserId,
        ////                PassportId = Pid,
        ////                RetailId = RetailID,
        ////                NickName = Pid
        ////            };
        ////            userCache.Add(gameUser);
        ////        }
        ////        user = new SessionUser(gameUser);
        ////        UserRole role;
        ////        if (roleCache.TryFind(gameUser.PersonalId, r => r.RoleName == UserName, out role) == LoadingStatus.Success)
        ////        {
        ////            if (role == null)
        ////            {
        ////                role = new UserRole()
        ////                {
        ////                    RoleId = (int)roleCache.GetNextNo(),
        ////                    UserId = UserId,
        ////                    RoleName = UserName,
        ////                    HeadImg = HeadID,
        ////                    Sex = Sex.ToBool(),
        ////                    LvNum = 1,
        ////                    ExperienceNum = 0,
        ////                    LifeNum = 100,
        ////                    LifeMaxNum = 100
        ////                };
        ////                roleCache.Add(role);
        ////                gameUser.CurrRoleId = role.RoleId;
        ////            }
        ////            return true;
        ////        }
        ////    }
        ////    return false;
        ////}

        public override void BuildPacket()
        {
            if (_senddata != "") this.PushIntoStack(_senddata);
        }

        ////public override void TakeActionAffter(bool state)
        ////{
        ////    var notifyUsers = new List<IUser>();
        ////    notifyUsers.Add(Current.User);
        ////    ActionFactory.SendAsyncAction(notifyUsers, (int)ActionType.World, null, t => { });
        ////    base.TakeActionAffter(state);
        ////}
    }
    public class sc_create : sc_base
    { }
}
