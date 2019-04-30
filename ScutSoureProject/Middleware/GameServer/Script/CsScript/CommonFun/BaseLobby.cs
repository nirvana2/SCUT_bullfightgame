using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 游戏 大厅
    /// </summary>
    public class BaseLobby
    {
        private static BaseLobby _instBase = null;
        /// <summary>
        /// 临时用，后面要修改结构
        /// </summary>
        public static BaseLobby instanceBase
        {
            get
            {
                if (_instBase == null) _instBase = new BaseLobby();
                return _instBase;
            }
        }
        //加锁用的
        protected   object obj = new object();

        /// <summary>
        /// 对应的游戏ID
        /// </summary>
        public   int Gameid = 0;
        /// <summary>
        /// 进入大厅的游戏用户
        /// </summary>
        protected   ConcurrentDictionary<int, UserStatus> _dicUserStatus = new ConcurrentDictionary<int, UserStatus>();

        /// <summary>
        ///  返回NUll表示才登录进来没有值 
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public UserStatus GetUserStatusbyUserID(int UserID)
        {
            if (_dicUserStatus == null) return null;
            UserStatus us = null;
            _dicUserStatus.TryGetValue(UserID, out us);
            return us;
        }
        /// <summary>
        /// 用户登录后，进入房间 调用这个方法 状态为2
        /// 系统自己分配后，开始一桌打牌， 状态为3
        /// 掉线后，设置状态为4
        /// </summary>
        /// <param name="us"></param>
        public void AddorUpdateUserStatus(UserStatus us)
        {
            //ErrorRecord.Record(" AddorUpdateUserStatus   ... _UserID:" + us.UserID + "  us.Status:" + us.Status);
            _dicUserStatus.AddOrUpdate(us.UserID, us, (key, oldValue) => us);
        }  
    }
     
}
