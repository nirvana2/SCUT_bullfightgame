using GameServer.Script.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common.Serialization;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 百人牛牛的通用处理，非占用模式，非房卡模式
    /// </summary>
   public class BaseColorTable
    {
        public BaseColorTable()
        {

        }

        //加锁用的
        protected readonly object objLock = new object();
        protected BaseSendDataServer _bsDataServer;

        protected int _gameid;
        /// <summary>
        /// 是否处理了，客户端超时的动作，只处理一次，如果出现服务器第一次没处理，直接把这桌意外结束
        /// </summary>
        public bool _DealWaitEndFirst;

        /// <summary>
        /// 桌子的状态 
        /// </summary>
        public TableStatusEnum _tablestatus;
        /// <summary>
        /// LOG用的状态
        /// </summary>
        public string _strStatus;
        public int _roomid;
        /// <summary>
        /// 桌号
        /// </summary>
        public int _tableid;
        /// <summary>
        /// 当前桌进了好多人 在进行游戏
        /// </summary>
        public int _numpertable;

        /// <summary>
        /// 此桌的最小人数，才能开局
        /// </summary>
        public int _num_min;
        /// <summary>
        /// 此桌的最大人数
        /// </summary>
        public int _num_max;
        /// <summary>
        /// 当前是谁的令牌，即该谁正常出牌   1 2 3  
        /// 特殊赋值的地方  庄第一次初始赋值  每MoveNextUserToken  加1   
        /// </summary>
        public int _userTokenPos;

        /// <summary>
        /// 是否已扣出房卡      12或24局才扣一次
        /// </summary>
        public bool _haveCheckRoomCard = false;
        /// <summary>
        /// 有人申请解散游戏了
        /// </summary>
        public bool _applyExitTable = false;
        /// <summary>
        /// 正常结束，n-1个人认输了，解散成功
        /// </summary>
        public bool _gameover = false;
        /// <summary>
        ///  此卓对面的位置与USER字典 
        ///  pos -> BaseUser
        /// </summary>
        public ConcurrentDictionary<int, BaseUser> _pos2userbase;
        /// <summary>
        /// 一桌的所有钱记录 1付2，或2付1
        /// </summary>
        protected ConcurrentQueue<MoneyRecord> MoneyRecordList;
        /// <summary>
        /// 这一桌的录相记录 在开始一桌的时候 new 这个对象。   
        /// 每一步操作写成序列值 存入ActionList		在一桌结束的时候写入初始数据库
        /// </summary>
        protected tb_tablerecord _tablRecord;

        /// <summary>
        /// User 数组 基于POS顺序
        /// </summary> 
        protected Dictionary<int, int> _pos2UserID { get; set; }

        /// <summary>
        /// IP and port  基于POS顺序
        /// </summary> 
        protected Dictionary<int, string> _pos2IPPort { get; set; }

        /// <summary>
        /// 增加或减少的钱  基于POS顺序
        /// </summary> 
        protected Dictionary<int, decimal> _pos2Money { get; set; }
        /// <summary>
        /// 最后一局完后是否是胜利 _pos2Money在裁判中是否够基础分 
        /// </summary>
        protected Dictionary<int, bool> _pos2Win { get; set; }
        /// <summary>
        /// 纸牌数组 基于POS顺序
        /// </summary> 
        protected Dictionary<int, List<int>> _pos2CardList { get; set; }
        /// <summary>
        /// 纸牌对应牛的倍率   基于POS顺序
        /// </summary>
        protected Dictionary<int, int> _pos2BullRate { get; set; }
        /// <summary>
        /// 是否爆分，当观众了   基于POS顺序
        /// </summary>
        protected Dictionary<int, int> _pos2Watch { get; set; }
        /// <summary>
        /// 当前这一桌发送的所有消息 列表 录相功能 时使用   不包括进入人数的统计
        /// </summary>
        protected List<List<UserIDMSG>> _tableSendData = new List<List<UserIDMSG>>();
        /// <summary>
        /// 进入人数的功能，要大结算才能清理 ，
        /// </summary>
        protected List<List<UserIDMSG>> _tableEnterSendData = new List<List<UserIDMSG>>();

        /// <summary>
        /// 一局的生存时间
        /// </summary>
        protected DateTime _aliveTime;
        public int _tableMathCode;
        public string _guid;
        protected int _gametype;
        public int _tableMaxCount;
        /// <summary>
        /// 超时的时间
        /// </summary>
        public int _TurnWaitTime;
        /// <summary>
        /// 房间位置
        /// </summary>
        public int _masterPos;
        protected void Initi(ConcurrentDictionary<int, BaseUser> _pos2user, int minnum, int maxnum, int gameid, BaseSendDataServer _bsds, TimerCallback callback)
        {
            lock (objLock)
            {
                _tablRecord = new tb_tablerecord();
                Interlocked.Exchange(ref _userTokenPos, 0);
                MoneyRecordList = new ConcurrentQueue<MoneyRecord>();
                //记录
                DateTime tempDT = DateTime.Now;

                _tableMathCode = ToolsEx.GetRoomEnterSixID();
                _guid = Guid.NewGuid().ToString("N");
                _haveCheckRoomCard = false;
                _applyExitTable = false;
                _pos2userbase = _pos2user;
                _num_max = maxnum;
                _num_min = minnum;
                _gameid = gameid;
                _tablestatus = TableStatusEnum.Initi;
                _bsDataServer = _bsds;
                ForeashAllDoBase((i) =>
                {
                    _pos2userbase[i]._WaitSecond = _TurnWaitTime;  //20
                });
                _tableSendData.Clear();
                _tableEnterSendData.Clear();
            }
        }
        /// <summary>
        /// 基础遍历方法
        /// </summary>
        /// <param name="match"></param>
        private void ForeashAllDoBase(Action<int> match)
        {
            if (match == null) return;
            if (_pos2userbase == null)
            {
                ErrorRecord.Record("201611151413 fetal error  _pos2userbase is null.............................");
                return;
            }
            lock (objLock)
            {
                foreach (var _tempbuser in _pos2userbase)
                {
                    if (_pos2userbase == null) return;
                    match(_tempbuser.Key);
                }
            }
        }
        /// <summary>
        /// 是否有位置 
        /// </summary>
        /// <returns></returns>
        public bool HaveEmptyPos()
        {
            if (_numpertable >= _num_max) return false;
            return true;
        }
        protected void InitiAdd(int key, BaseUser _buser)
        {
            _pos2userbase.TryAdd(key, _buser);
            _pos2userbase[key]._Pos = key;
            if (_tablestatus == TableStatusEnum.Playing)
            {
                _pos2userbase[key]._isWatch = true;
            }
        }
        protected void StartBase(int second)
        {
            _aliveTime = DateTime.Now.AddSeconds(second);
            ForeashAllDoBase((i) =>
            {
                UserStatus us = BaseLobby.instanceBase.GetUserStatusbyUserID(_pos2userbase[i]._userid);
                if (us == null)
                {
                    ErrorRecord.Record("201208311452basetable 必须找到的UserID： " + _pos2userbase[i]._userid);
                    return;
                }
                if (us.Status != UserStatusEnum.InTableDaiPaiDis)
                    us.Status = UserStatusEnum.InTableDaiPai;
                BaseLobby.instanceBase.AddorUpdateUserStatus(us);
            });
            _tablestatus = TableStatusEnum.Playing;
        }
        /// <summary>
        ///  初始化进入的人，现在只有一个， 金币模式可能会有多个
        /// </summary>
        protected void EnterTable()
        {
            List<OtherUserInfoSD> oUserlist = new List<OtherUserInfoSD>();
            ForeashAllDoBase((i) =>
            {
                BaseUser _tempBUser = _pos2userbase[i];
                _tempBUser._tbwechatposData.pos = i;
                _tempBUser._tbwechatposData._isDisconnet = _tempBUser._isDisconnet ? 1 : 0;
                _tempBUser._tbwechatposData._isReady = _tempBUser._isReady ? 1 : 0;
                oUserlist.Add(_tempBUser._tbwechatposData);

                UserStatus us = BaseLobby.instanceBase.GetUserStatusbyUserID(_tempBUser._userid);
                if (us == null)
                {
                    ErrorRecord.Record("201611301736 basetable 必须找到的UserID： " + _tempBUser._userid);
                    return;
                }
                us.TableID = _tableid;
                us.Status = UserStatusEnum.InTableWaiting;
                BaseLobby.instanceBase.AddorUpdateUserStatus(us);
            });
            //通知所有人，有人进入桌子了    
            List<UserIDMSG> imList = new List<UserIDMSG>();

            ForeashAllDoBase((i) =>
            {
                BaseUser tempUser = _pos2userbase[i];
                tempUser._Pos = i;

                sc_entertable_n _canReady = new sc_entertable_n() { fn = "sc_entertable_n", result = 1, _msgid = 8 };
                _canReady.pos = tempUser._Pos;
                _canReady.tableid = _tableid;
                _canReady.gameid = _gameid;//客服端好做分发
                _canReady.gametype = _gametype;
                _canReady.palyerlist = oUserlist;
                _canReady.MatchCode = _tableMathCode + "";
                _canReady.maxCount = _tableMaxCount;

                imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_canReady), tempUser._isRobot, tempUser._isDisconnet));

            });

            _bsDataServer.SendDataDelay(imList);
            _tableEnterSendData.Add(imList);
        }
        /// <summary>
        /// 后面进入的房间的人消息处理
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected void EnterTableAdditive(int key)
        {
            List<OtherUserInfoSD> oUserlist = new List<OtherUserInfoSD>();
            ForeashAllDoBase((i) =>
            {
                _pos2userbase[i]._tbwechatposData.pos = i;
                _pos2userbase[i]._tbwechatposData._isDisconnet = _pos2userbase[i]._isDisconnet ? 1 : 0;
                _pos2userbase[i]._tbwechatposData._isReady = _pos2userbase[i]._isReady ? 1 : 0;
                oUserlist.Add(_pos2userbase[i]._tbwechatposData);
                UserStatus us = BaseLobby.instanceBase.GetUserStatusbyUserID(_pos2userbase[i]._userid);
                if (us == null)
                {
                    ErrorRecord.Record("201611301740 basetable 必须找到的UserID： " + _pos2userbase[i]._userid);
                    return;
                }
                us.TableID = _tableid;
                us.Status = UserStatusEnum.InTableWaiting;
                BaseLobby.instanceBase.AddorUpdateUserStatus(us);
            });
            //通知所有人，有人进入桌子了   
            List<UserIDMSG> imList = new List<UserIDMSG>();
            ForeashAllDoBase((i) =>
            {
                BaseUser tempUser = _pos2userbase[i];
                sc_entertable_n _canReady = new sc_entertable_n() { fn = "sc_entertable_n", result = 1, _msgid = 8 };
                _canReady.pos = key;
                _canReady.tableid = _tableid;
                _canReady.gameid = _gameid;//客服端好做分发
                _canReady.gametype = _gametype;

                _canReady.palyerlist = oUserlist;
                _canReady.MatchCode = _tableMathCode + "";
                _canReady.maxCount = _tableMaxCount;
                imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_canReady), tempUser._isRobot, tempUser._isDisconnet));
            });

            _bsDataServer.SendDataDelay(imList);
            _tableEnterSendData.Add(imList);
            NotifyWarning();
        }
        /// <summary>
        /// 掉线 状态通知
        /// </summary>
        /// <param name="userID"></param>
        public void NotifyDisBase(int userid, int _isreconnect)
        {
            BaseUser myu = GetBaseUserByID(userid);
            if (myu == null) return;
            if (_isreconnect == 0) myu._isDisconnet = true;     //断线的，不用给他推送了

            sc_disconnect_n _exitForce = new sc_disconnect_n() { fn = "sc_disconnect_n", result = 1 };
            _exitForce._msgid = 1;
            _exitForce.gameid = _gameid;
            _exitForce.pos = myu._Pos;
            _exitForce.tableid = _tableid;
            _exitForce.reconnect = _isreconnect;

            List<UserIDMSG> imList = new List<UserIDMSG>();//给剩下的人发送消息，说明有人离开房间了，，

            ForeashAllDoBase((i) =>
            {
                BaseUser tempUser = _pos2userbase[i];
                imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_exitForce), _pos2userbase[i]._isRobot, tempUser._isDisconnet));
            });
            _bsDataServer.SendDataDelay(imList);
            _tableSendData.Add(imList);
        }
        /// <summary>
        /// 同位置，与IP警告
        /// </summary> 
        public void NotifyWarning()
        {
            //check ip 
            bool _havesameip = false;
            ForeashAllDoBase((i) =>
            {
                if (_pos2userbase[i]._isRobot) return;
                if (!_pos2userbase[i]._IPandPort.Contains(":")) return;
                ForeashAllDoBase((j) =>
                {
                    if (_pos2userbase[j]._isRobot) return;
                    if (!_pos2userbase[j]._IPandPort.Contains(":")) return;
                    if (_pos2userbase[j]._IPandPort.Split(':')[0] == _pos2userbase[i]._IPandPort.Split(':')[0]) _havesameip = true;
                });
            });
            if (_havesameip)
            {
                sc_warning_n _exitForce = new sc_warning_n() { fn = "sc_warning_n", result = 1 };
                _exitForce._msgid = 1;
                _exitForce.gameid = _gameid;
                _exitForce.type = _havesameip ? 1 : 0;
                _exitForce.content = "有两人在同一IP";

                List<UserIDMSG> imList = new List<UserIDMSG>();//给剩下的人发送消息，说明有人离开房间了，，

                ForeashAllDoBase((i) =>
                {
                    BaseUser tempUser = _pos2userbase[i];
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_exitForce), _pos2userbase[i]._isRobot, tempUser._isDisconnet));
                });
                _bsDataServer.SendDataDelay(imList);
                _tableSendData.Add(imList);
            }
        }
        /// <summary>
        /// 用户退出桌子   只是通用的发消息
        /// </summary>
        /// <param name="userID"></param>
        protected void ExitTableall(List<int> _posdisagree, bool _needDealEnd)
        {
            if (!_applyExitTable) return;       //必须 桌子为 
            sc_exittable_n _exitForce = new sc_exittable_n() { fn = "sc_exittable_n", result = 1 };
            _exitForce._msgid = 1;
            _exitForce.gameid = _gameid;
            _exitForce.disagree = _posdisagree;
            _exitForce._showResult = _needDealEnd;

            List<UserIDMSG> imList = new List<UserIDMSG>();//给剩下的人发送消息，说明有人离开房间了，，

            ForeashAllDoBase((i) =>
            {
                imList.Add(new UserIDMSG(_pos2userbase[i]._userid, JsonUtils.Serialize(_exitForce), _pos2userbase[i]._isRobot, _pos2userbase[i]._isDisconnet));
            });
            _bsDataServer.SendDataDelay(imList);

            //处理一桌清理========================
            if (_posdisagree.Count == 0)
            { //退出游戏                    
                //_gameover = true;//暂时不用这个标识
            }
            else
            {    //允许 第二次申请处理
                ForeashAllDoBase((i) =>
                {
                    _pos2userbase[i]._isAgreeExit = 2;
                });
                _applyExitTable = false;
            }
        }

        /// <summary>
        /// 用户退出桌子   只是通用的发消息
        /// </summary>
        /// <param name="userID"></param>
        protected void ExitTableOne(int _exitpos, int userid)
        {
            sc_one_exittable_n _exitForce = new sc_one_exittable_n() { fn = "sc_one_exittable_n", result = 1 };
            _exitForce._msgid = 1;
            _exitForce.gameid = _gameid;
            _exitForce.pos = _exitpos;
            _exitForce.userid = userid;

            List<UserIDMSG> imList = new List<UserIDMSG>();//给剩下的人发送消息，说明有人离开房间了，，

            ForeashAllDoBase((i) =>
            {
                if (i == _exitpos && _pos2userbase[i]._isRobot) return; //什么器人不能再收到消息了，否则找不到数据 
                imList.Add(new UserIDMSG(_pos2userbase[i]._userid, JsonUtils.Serialize(_exitForce), _pos2userbase[i]._isRobot, _pos2userbase[i]._isDisconnet));
            });
            _bsDataServer.SendDataDelay(imList);

            ////foreach (var _templist in _tableEnterSendData)
            ////{
            ////    foreach (var _usermsg in _templist)
            ////    {
            ////        if (_usermsg._userid == userid && _usermsg._senddata != "") _usermsg._senddata = "";
            ////    }
            ////}
            _numpertable -= 1;//不移出，就不能再次进来
            UserStatus us = BaseLobby.instanceBase.GetUserStatusbyUserID(userid);
            if (us != null) us.Status = UserStatusEnum.InLobby;
        }

        /// <summary>
        /// 获取此桌中指定用户ID的对象
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public BaseUser GetBaseUserByID(int userID)
        {
            if (_pos2userbase == null)
            {
                ErrorRecord.Record("201611102028bs _DicPos2User == null fetal Error 运行正常后去掉 userID:" + userID);
                return null;
            }
            foreach (var _tempbuser in _pos2userbase)
            {
                if (_tempbuser.Value._userid == userID) return _tempbuser.Value;
            }
            ErrorRecord.Record("201611102029bs fetal Error 必需处理   没找到userID:" + userID);
            return null;
        }
        /// <summary>
        /// 一秒检测一次的一不会调用，只是强制加一个最大限制 
        /// </summary>
        protected void DealAliveTime()
        {
        }
        /// <summary>
        /// 获取下家的位置 序列号， DicUser的索引号
        /// </summary>
        /// <returns></returns>
        protected void SetNextDicUserIndex()
        {
            lock (objLock)
            {
                do
                {
                    if (_userTokenPos == _num_max)
                    {
                        Interlocked.Exchange(ref _userTokenPos, 1);//就是_userTokenPosition = 1;的效果
                    }
                    else
                    {
                        Interlocked.Increment(ref _userTokenPos);//就是_userTokenPosition++;的效果 
                    }
                }
                while (!_pos2userbase.ContainsKey(_userTokenPos));//中间有空位，要跳过
            }
        }
        /// <summary>
        /// 找最到小的非观众位置
        /// </summary>
        /// <returns></returns>
        public int GetFirstValidPos()
        {
            int _pos = 1;
            while (_pos2userbase[_pos]._isWatch)//中间有空位，要跳过
            { _pos++; }
            if (_pos > _numpertable)
            {
                ErrorRecord.Record("fetal logic error................201612062021");
                _pos = _numpertable;
            }
            return _pos;
        }
        /// <summary>                                                        
        /// 1.房卡次数到了。
        /// 2.n-1家认输了。
        /// 一桌完未重置之前使用
        /// </summary>
        /// <returns></returns>
        protected void CheckResetTable()
        {
            int _playerCount = 0;
            ForeashAllDoBase((i) =>
            {
                if (_pos2userbase[i]._isWatch) _playerCount++;
            });
            if (_playerCount == _numpertable - 1) _gameover = true;//n-1家认输了         
        }
        protected void ResetBase(bool _no_again)
        {
            if (_numpertable < _num_min) _tablestatus = TableStatusEnum.WaitforReady;  //人数不够了，，停止循环        

            MoneyRecordList = new ConcurrentQueue<MoneyRecord>();
            List<int> _removeKey = new List<int>();

            for (int i = 1; i <= _num_max; i++)
            {
                foreach (int key in _removeKey)
                {
                    if (i != key) continue;
                    BaseUser _buser;
                    _pos2userbase.TryRemove(i, out _buser);
                }
            }
            _numpertable -= _removeKey.Count;//看一下 下一局还有多少人在够格继续
            _tablRecord = new tb_tablerecord();
            ForeashAllDoBase((i) =>
            {
                _pos2userbase[i].ResetBase();
            });
            Interlocked.Exchange(ref _userTokenPos, 0);
            _tableSendData.Clear();      //一局完了清理，断线后的逻辑
            if (_no_again)
            {
                _tableEnterSendData.Clear();      //一局完了清理，断线后的逻辑 
                                                  //// _isUsed = false;    //房卡模式需要全踢
                ForeashAllDoBase((i) =>
                {
                    if (_pos2userbase[i]._isRobot)
                    {
                        _bsDataServer.QueRobotUser.Reverse();
                        //回收机器人 //1.断开机器人的连接  
                        _bsDataServer.QueRobotUser.Enqueue(_pos2userbase[i]._tbUser);
                        _bsDataServer.RobotExistNumReduceOne();
                    }
                    UserStatus us = BaseLobby.instanceBase.GetUserStatusbyUserID(_pos2userbase[i]._userid);
                    if (us == null)
                    {
                        ErrorRecord.Record("201612052103BaseTable 必须找到的UserID： " + _pos2userbase[i]._userid);
                        return;
                    }
                    us.Status = UserStatusEnum.InLobby;
                    us.RoomID = 0;
                    BaseLobby.instanceBase.AddorUpdateUserStatus(us);
                });
            }
        }

        /// <summary>
        ///  发送聊天信息
        /// </summary>
        public void SendChatBase(int userid, string content, int _type)
        {
            int pos = 0;
            ForeashAllDoBase((i) =>
            {
                if (_pos2userbase[i]._userid == userid) pos = i;
            });
            if (0 == pos)
            {
                ErrorRecord.Record("201611031040BaseTable  发送消息的User 找不到。 userid:" + userid);
                return;
            }
            List<UserIDMSG> imList = new List<UserIDMSG>();
            ForeashAllDoBase((i) =>
            {
                BaseUser tempUser = _pos2userbase[i];

                sc_chat_n _chat_n = new sc_chat_n() { fn = "sc_chat_n", result = 1 };
                _chat_n.pos = pos;
                _chat_n.content = content;
                _chat_n.type = _type;
                _chat_n.gameid = _gameid;

                imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_chat_n), tempUser._isRobot, tempUser._isDisconnet));
            });
            _bsDataServer.SendDataDelay(imList);
            // _tableSendData.Add(imList); //聊天信息不加入记录     
        }
        //public void SendTransferMsgBase(int userid, sc_askmoneytrading_n msg)
        //{
        //    List<UserIDMSG> imList = new List<UserIDMSG>();
        //    imList.Add(new UserIDMSG(userid, JsonUtils.Serialize(msg), false, false));
        //    _bsDataServer = BFSendDataServer.instance;
        //    _bsDataServer.SendDataDelay(imList);
        //}

        #region   断线相关
        /// <summary>
        /// 获取断线前的所有进入人的消息列表 
        /// </summary>
        /// <param name="userid"></param>
        public void ClearBaseLimit()
        {
            ForeashAllDoBase((i) =>
            {
                BaseUser tempUser = _pos2userbase[i];
                tempUser._WaitClientLimitCount = 0;
            });
        }
        /// <summary>
        /// 获取断线前的所有进入人的消息列表 
        /// </summary>
        /// <param name="userid"></param>
        public string GetEnterDisListBase(int pos)
        {
            List<OtherUserInfoSD> oUserlist = new List<OtherUserInfoSD>();
            ForeashAllDoBase((i) =>
            {
                oUserlist.Add(_pos2userbase[i]._tbwechatposData);
            });

            sc_entertable_n _canReady = new sc_entertable_n() { fn = "sc_entertable_n", result = 1, _msgid = 8 };
            _canReady.pos = pos;
            _canReady.tableid = _tableid;
            _canReady.gameid = _gameid;//客服端好做分发
            _canReady.gametype = _gametype;
            _canReady.palyerlist = oUserlist;
            _canReady.MatchCode = _tableMathCode + "";
            _canReady.maxCount = _tableMaxCount;

            return JsonUtils.Serialize(_canReady);
        }
        /// <summary>
        /// 获取断线前的所有消息列表  一局的游戏操作逻辑
        /// </summary>
        /// <param name="userid"></param>
        public List<string> GetDisListBase(int userid)
        {
            List<string> _imlist = new List<string>();
            //通知其他人有人上线了      
            foreach (var _templist in _tableSendData)
            {
                foreach (var _usermsg in _templist)
                {
                    if (_usermsg._userid == userid) _imlist.Add(_usermsg._senddata);
                }
            }
            return _imlist;
        }
        #endregion

        #region 延时相关
        /// <summary>
        /// 当前动作等待时间已到 // 按各个用户处理， 
        /// </summary>
        /// <returns></returns>
        public bool SomeUserIsOverTime()
        {
            bool _tempHaveTimeOut = false;
            if (_tablestatus == TableStatusEnum.Playing || _tablestatus == TableStatusEnum.Initi)
            {
                ForeashAllDoBase((i) =>
                {
                    if (_pos2userbase[i]._WaitClientLimitCount == 0) return;
                    if (_pos2userbase[i]._waitUserAction == "0") return;
                    if (DateTime.Now >= _pos2userbase[i]._WaitStartTime)
                    {
                        DealEveryUser();
                        _tempHaveTimeOut = true;
                        return;
                    }
                });
            }
            return _tempHaveTimeOut;
        }
        #endregion

        #region Timer

        public Timer _timer;
        private int _timerRunning = 0;
        private int _timerPeriod = 0;
        public bool IsTimeRunning
        {
            get { return _timerRunning == 1; }
            set { Interlocked.Exchange(ref _timerRunning, value ? 1 : 0); }
        }
        /// <summary>
        /// 出牌操作超时时间(5秒)
        /// </summary>
        private const int OperationSecTimeout = 5000;
        /// <summary>
        /// 操作计数
        /// </summary>
        private int _timeNumber = 0;
        public void StartTimer(int period = 5000)
        {
            //Console.WriteLine("{0}>>Table:{1} in {2} room timer is started", DateTime.Now.ToString("HH:mm:ss"), _tableId, _roomId);
            _timerPeriod = period;
            _timer.Change(period, period);
            _timeNumber = 0;
            IsTimerStarted = true;
        }

        public void ReStartTimer(int period)
        {
            //Console.WriteLine("{0}>>Table:{1} in {2} room timer is restarted", DateTime.Now.ToString("HH:mm:ss"), _tableId, _roomId);
            _timerPeriod = period;
            _timer.Change(-1, -1);
            _timer.Change(period, period);
            _timeNumber = 0;
        }

        public void StopTimer()
        {
            // Console.WriteLine("{0}>>Table:{1} in {2} room timer is stoped", DateTime.Now.ToString("HH:mm:ss"), _tableId, _roomId);
            _timer.Change(-1, -1);
            _timeNumber = 0;
            IsTimerStarted = false;
        }

        /// <summary>
        /// 定时器计数
        /// </summary>
        public void DoTimeNumber()
        {
            Interlocked.Exchange(ref _timeNumber, _timeNumber + _timerPeriod);
        }

        /// <summary>
        /// 操作是否超时
        /// </summary>
        public bool IsOperationTimeout
        {
            get { return _timeNumber > OperationSecTimeout; }
        }

        /// <summary>
        /// 定时器是否开始
        /// </summary>
        public bool IsTimerStarted
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// 写入一桌的金钱交易记录，每个人一条
        /// </summary>
        protected void LogWriteToDB(tb_tablerecord tr)
        {
            BLL_Record bll = new BLL_Record();
            BLL_MoneyLog money = new BLL_MoneyLog();
            //var cacheSet = new ShareCacheStruct<tb_tablerecord>();
            bll.Add(tr);
            //cacheSet.Add(tr);
            var cacheSetmoneylog = new ShareCacheStruct<tb_TableMoneyLog>();
            try
            {
                ForeashAllDoBase((i) =>
                {
                    tb_TableMoneyLog _moneylogtemp = new tb_TableMoneyLog();
                    if (_pos2Money.ContainsKey(i)) _moneylogtemp.AddorReduceMoney = _pos2Money[i];
                    if (_pos2Win.ContainsKey(i)) _moneylogtemp._win = _pos2Win[i];
                    _moneylogtemp.gameid = _gameid;
                    _moneylogtemp.MatchCode = tr.MatchCode;
                    _moneylogtemp._guid = tr._guid;
                    _moneylogtemp.TableRecordID = tr.id;
                    if (_pos2UserID.ContainsKey(i)) _moneylogtemp.UserID = _pos2UserID[i];
                    if (_pos2IPPort.ContainsKey(i)) _moneylogtemp._ipport = _pos2IPPort[i];
                    _moneylogtemp._pos = i;
                    if (_pos2CardList.ContainsKey(i)) _moneylogtemp._cardList = _pos2CardList[i];
                    _moneylogtemp._isover = tr._isover;
                    if (_pos2BullRate.ContainsKey(i)) _moneylogtemp._bullrate = _pos2BullRate[i];
                    if (_pos2Watch.ContainsKey(i)) _moneylogtemp._isWatch = _pos2Watch[i];
                    var result = money.Add(_moneylogtemp);//直接写入数据库 待测
                                                          //cacheSetmoneylog.Add(_moneylogtemp);
                });
            }
            catch (Exception ex)
            {


            }
        }
        public virtual void DealEveryUser()
        { }

    }
}
