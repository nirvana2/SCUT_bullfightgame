using System;
using System.Collections.Generic;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.Action;
using ZyGames.Framework.Game.Service;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{

    /// <summary>
    /// 1004_用户登录
    /// </summary>
    public class Action1004 : LoginExtendAction
    {
        public Action1004(ActionGetter actionGetter)
            : base((short)ActionType.Login, actionGetter)
        {
        }


        protected override bool DoSuccess(int userId, out IUser user)
        {
            Console.WriteLine("1004 顺序测试:DoSuccess ");
            user = null;
            var cacheSet = new PersonalCacheStruct<GameUser>();
            GameUser gameUser = cacheSet.FindKey(userId.ToString());
            if (gameUser == null)
            {
                GuideId = 1005;
                return true;
            }
            //var roleCache = new PersonalCacheStruct<UserRole>();
            //var tbuserCache = new ShareCacheStruct<tb_User>();
            tb_User _tempuser = GameDataCache.Get<tb_User>(userId);
            if (_tempuser == null)
            {//自动注册同个角色到游戏库中
                ErrorRecord.Record("201702161640 tb_User is null...");
            }
            else
            {
                //_tempuser.lockTime = "2017-05-27 11:47:35";
                if (!string.IsNullOrEmpty(_tempuser.lockTime))
                {
                    if (Convert.ToDateTime(_tempuser.lockTime) > DateTime.Now)
                    {
                        ErrorCode = SimplifiedLanguage.Instance.LockTimeoutCode;
                        ErrorInfo = SimplifiedLanguage.Instance.AcountIsLocked;
                        return false;
                    }
                }
            }
            GameSession.ClearSession(m => m.UserId == userId);//add by jsw 0418
            user = new SessionUser(gameUser);
            if (gameUser.CurrRoleId == 0)
            {
                gameUser.CurrRoleId = _tempuser.UserID;     // gameUser.CurrRoleId = roleList[0].RoleId;
            }

            return true;
        }

        ////public override async void TakeActionAffter(bool state)
        ////{
        ////    Console.WriteLine("1004>发送World通知...");
        ////    var notifyUsers = new List<IUser>();
        ////    notifyUsers.Add(Current.User);      
        ////    //Current.SendAsync() //当前推送，，
        ////    //给所有指定User推送
        ////    await ActionFactory.SendAsyncAction(notifyUsers, (int)ActionType.World, null, t =>
        ////   {
        ////       Console.WriteLine("1004>发送World通知结果:{0}", t.Result.ToString());
        ////   });
        ////    base.TakeActionAffter(state);
        ////}
    }
}
