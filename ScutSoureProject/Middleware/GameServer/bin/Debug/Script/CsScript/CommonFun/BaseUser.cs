using System;
using System.Collections.Generic;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 虚拟 用户   处理通用超时功能
    /// </summary>
    public class BaseUser
    {
        public BaseUser()
        { }
        #region 属性
        public object obj = new object();

        /// <summary>
        /// 一局的收支
        /// </summary>
        public decimal _Income;
        /// <summary>
        /// 当前用户进入房间，等队列的时间
        /// </summary>
        public DateTime _enterTime;
        /// <summary>
        /// //当前所处房间id
        /// </summary>
        public int _roomid = 0;
        /// <summary>
        /// 如果玩家已经在打牌了就会存在桌子ID 必须先根据房间ID 才能得到桌子ID 对应的对象
        /// </summary>
        public int _tableID = 0;
        /// <summary>
        /// 是否已准备
        /// </summary>
        public bool _isReady = false;
        /// <summary>
        /// 是否正在打牌
        /// </summary>
        public bool _isPlaying = false;
        /// <summary>
        /// 是否后面进来的，只是能看  此用户不处理延时操作的
        /// </summary>
        public bool _isWatch = false;
        /// <summary>
        /// 是否机器人
        /// </summary>
        public bool _isRobot = false;
        /// <summary>
        /// 是否掉线了
        /// </summary>
        public bool _isDisconnet = false;        
                                                
        /// <summary>
        /// tb_user中的UserID 而不是id
        /// </summary>
        public int _userid = 0;                           
      
        /// <summary>
        /// 1 2 3 4 也可以对应东 南 西 北
        /// </summary>
        public int _Pos = 0;
        /// <summary>
        /// 是否同意解散一桌游戏    2表示未回答 ，1表示同意，0表示不同意。
        /// </summary>
        public int _isAgreeExit = 2;
        /// <summary>
        /// 用户数据库的对应值 ， 机器人也存在的
        /// </summary>
        public  tb_User _tbUser = null;
        public OtherUserInfoSD _tbwechatposData;
        public string _IPandPort = "";
        private int _curGold;
        /// <summary>
        /// 在一局中的钱的变量，增加或减少的。
        /// </summary>
        public int _CurrentGold
        {
            set
            {     
                //_curGold = value; 
                _tbUser.UserMoney = value;
                _tbwechatposData.otherpalyer.Money = (int)_tbUser.UserMoney;
            }
            get
            {
                return (int)_tbUser.UserMoney;//不是房间模式了，金币模式
                //return _curGold;
            }
        }
        #endregion
        public void Initi(string ipport, int roomID, tb_User tbuser, bool robot)
        {
            _IPandPort = ipport;
            _roomid = roomID;
            _tbUser = tbuser;
            _userid = tbuser.UserID;
            _isRobot = robot;
            _isPlaying = false;
            _enterTime = DateTime.Now;
            _WaitStartTime = DateTime.Now;
            _SysDealTimeOutCount = 0;
            _waitUserAction = "0";

            _tbwechatposData = new OtherUserInfoSD();     
            _tbwechatposData.pos = 0;            //位置暂时用    字典添加的先后顺利      
            _tbwechatposData.otherpalyer = new PlayerInfoSD()
            {
                accountId = "",
                Diamond = _tbUser.diamond,// (int)tbuser.diamond,     //========================================================
                lastLoginTime = 1,// tbuser.LastLotinTime1.Ticks,
                level = 1,
                Money = (int)_tbUser.UserMoney,
                SignInCount = 5,
                state = 0,
                uName = _tbUser.wechatName,
                userid = _tbUser.UserID,
                vipLevel = 11,
                _wechat = new WechatInfoSD() { HeadIconURL = IsHandlePhoto(_tbUser.isRobot,_tbUser.wechatHeadIcon).Trim(), Sex = _tbUser.Sex, wechatName = _tbUser.wechatName.Trim() }
            };
        }
        private string IsHandlePhoto(int _isRobot, string wechatHeadIcon)
        {
            if (_isRobot == 1)
            {
                var serverIp = ToolsEx.GetIpAddress();
                return "" + serverIp + "/fordlc/wechat/" + wechatHeadIcon.Trim();
            }
            else return wechatHeadIcon.Trim();
        }


        public virtual void ResetBase()
        {
            _isPlaying = false;
            //_isWatch = false;
            _isReady = false;
                       
            _isAgreeExit = 2;
            _SysDealTimeOutCount = 0;
        }
        /// <summary>
        /// 同一用户只允许提交一次
        /// </summary> 
        /// <returns></returns>
        public bool CheckFirstDeal()
        {
            if (1 == _WaitClientLimitCount)
            {   //执行一次后设置为，就不会多次执行了
                _WaitClientLimitCount = 0;
                _waitUserAction = "0";
                return true;
            }
            ErrorRecord.Record(new Exception(), " 201207031139BaseUser 同一操作多次提交...............userid:" + _userid); //客户端不处理延时打牌的话 就会报错-----------------------
            return false;
        }
        /// <summary>
        /// 观众不允许操作操作
        /// </summary>
        /// <returns></returns>
        public bool CheckisWatch()
        {
            if (_isWatch)
            {
                return true;
            }
            return false;
        }
        #region timeout 相关
        /// <summary>
        ///系统处理超时的次数，，，，暂定大于5次，，，会在下一局自动踢出桌子
        /// </summary>
        public int _SysDealTimeOutCount;
          /// <summary>
        /// 从动作开始计算等待时间，即任何一个动作开始赋值，
        /// </summary>
        public DateTime _WaitStartTime;
        /// <summary>  
        /// 使用前为1 提交一次后修改为0
        /// 为  限制 一个用户不能重复提交 ，第二无效处理 add by jsw 201206281620
        /// </summary>
        public int _WaitClientLimitCount;
        /// <summary>
        /// 需要用户返回的Action  
        /// MJ:14定缺  15摸牌后打牌 150其他人打牌自己有碰杠胡  171 其他人明杠，自己可以胡 
        /// BF:发牌后， 抢庄，下注
        /// TC:
        /// </summary>
        public string _waitUserAction; 
        //每步延时需要等待的时间      
        public int _WaitSecond;
        public void SetTimeOutAction(int WaitClientLimit, string waitUserAction, int waitSecond =15)
        {
            if (_isWatch) return;//观看用户没有延时操作
            if (waitSecond == 15) _WaitStartTime = DateTime.Now.AddSeconds(_WaitSecond);
            else    _WaitStartTime = DateTime.Now.AddSeconds(waitSecond);
            _WaitClientLimitCount = WaitClientLimit;
            _waitUserAction = waitUserAction;   
        }
        public int GetCurrentTimeDown()
        {
            TimeSpan _ts = _WaitStartTime - DateTime.Now;
            return (int)_ts.TotalSeconds;
        }
        public void RecordTimeoutCount()
        {
            if (_isRobot) return;//机器人不统计
            _SysDealTimeOutCount++;
        }
        #endregion
    }


    public class UserStatus
    {
        public UserStatus()
        {
        }

        public UserStatus(UserStatusEnum _Status, int _gameid, int _RoomID, int _UserID)
        {
            Status = _Status;
            RoomID = _RoomID;           
            UserID = _UserID;
            Gameid = _gameid;
        }

        public int UserID { get; set; }

        /// <summary>
        /// 在大厅1，在房间2，在桌子上打牌3, 在打牌但是断线了4
        /// </summary>
        public UserStatusEnum Status { get; set; }   

        public int Gameid { get; set; }      

        public int RoomID { get; set; }           

        public int TableID { get; set; }         
    }
    public enum UserStatusEnum
    {
        InLobby = 1,
        InRoom = 2,
        /// <summary>
        /// 在桌子上打牌
        /// </summary>
        InTableDaiPai = 3,
        /// <summary>
        /// 在桌子上打牌，但断线了
        /// </summary>
        InTableDaiPaiDis = 4,
        /// <summary>
        /// 在桌子上等待，
        /// </summary>
        InTableWaiting = 5
    }
}
