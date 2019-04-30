using System;
using System.Collections.Generic; 
using System.Collections.Concurrent;
using System.Linq;
using ZyGames.Framework.Common.Serialization;
using System.Threading;
using ZyGames.Framework.Common.Timing;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    ///  虚拟桌子      
    /// </summary>
    public class BullColorTable : BaseColorTable
    {
        public BullColorTable()
        { }

        //成员 变量    
        /// <summary>
        /// 1~500 key 
        /// 把int 表示 pos  因为人员进来不位置不是固定的，不能循环处理
        /// </summary>
        public ConcurrentDictionary<int, BullColorUser> _DicPos2User;

        //底分 暂时用不到        
        public int _baseMoney;        
        public int _bankpos; // 庄的位置         
        public BullColorJudge _judge;   // 此桌对应的打牌裁判
        public BullColorRoom _room;
        public void ForeashAllDo(Action<int> match)
        {
            if (match == null) return;
            if (_DicPos2User == null)
            {
                ErrorRecord.Record("201611151415 fetal error  _DicPos2User is null.............................");
                return;
            }
          
            lock(objLock)
            {
                foreach (var _tempuser in _DicPos2User)
                {
                    if (_DicPos2User == null) return;
                    match(_tempuser.Key);
                }
            }
        }
        

        /// <summary>
        /// 进入1~1006人后处理的
        /// </summary>
        /// <param name="tablenum"></param>
        /// <param name="userList"></param>
        public BullColorTable(int gameid, BullColorRoom _room2, int tablenum, List<BullColorUser> userList, cs_enterroom _data)
        {
            _numpertable = userList.Count;
            _roomid = _room2.RoomId;
            _room = _room2 as BullColorRoom;
            _tableid = tablenum;
            _baseMoney = 1;
            _bankpos = 1;
            if (_DicPos2User == null) _DicPos2User = new ConcurrentDictionary<int, BullColorUser>();

            List<BullColorUser> _fuserlist = new List<BullColorUser>(userList);
            ConcurrentDictionary<int, BaseUser> _temppos2user = new ConcurrentDictionary<int, BaseUser>();
            for (int i = 0; i < _fuserlist.Count; i++)
            {
                _fuserlist[i]._tableID = tablenum;        //赋值桌子号 

                _DicPos2User.TryAdd(i + 1, _fuserlist[i]);//1~3表示位置 
                _temppos2user.TryAdd(i + 1, _fuserlist[i]);
            }

            _judge = new BullColorJudge(this);
            _judge.InitiArgs(_data);
            base._gametype = _judge._gametype;
            _tableMaxCount = _judge.GetTableorBankerMaxCount;

            _TurnWaitTime = BFColorSendDataServer.instance.TurnWaitTime;

            base.Initi(_temppos2user, _judge._minLimit, _judge._maxLimit, gameid, BFColorSendDataServer.instance, DoTableTimer);
            base.EnterTable(); 
            _DicPos2User[_bankpos].SetTimeOutAction(1, "sc_ready_bfc_n", 1 * 60);//1分钟等待时间
            _strStatus = "BullColorTable...1";   
        }
         

        /// <summary>
        /// 非房主进入桌子 房间模式的
        /// </summary>
        /// <param name="userid"></param>
        public bool EnterTableAdditive(tb_User tbUser)
        {
            //不限制客户端的个数 可能 有破坏的外挂===========
            if (_numpertable >= _num_max) return false;
            BullColorUser myu = new BullColorUser();
            myu.Initi(tbUser.IP, _roomid, tbUser, false);// 当成客户端 的IP：Port用   
            AllocationtoTable(myu);
            return true;
        }

        /// <summary>
        /// 分配到 桌子上的空位，
        /// </summary>
        public void AllocationtoTable(BullColorUser _adduser)
        {
            _adduser._tableID = _tableid;
            _numpertable += 1;
            int key = -1;
            for (int i = 1; i <= _num_max; i++)
            {
                if (_DicPos2User.ContainsKey(i)) continue;
                key = i;
                break;
            }
            _adduser._WaitSecond = _TurnWaitTime;  //20 
            _DicPos2User.TryAdd(key, _adduser);
            if (key == -1)
            {
                ErrorRecord.Record("201611172219 fetal error!!!");
                return;
            }
            base.InitiAdd(key, _adduser as BaseUser); 
            base.EnterTableAdditive(key); 
            
            _strStatus = "BullColorTable...1.1 for auto ready...";
        }

        #region 非功能 成员 方法
        /// <summary>
        /// 重置此桌信息
        /// </summary>
        public void Reset(bool _no_again)
        {
            lock(objLock)
            {
                _strStatus = "Reset...     ";
                ForeashAllDo((i) =>
                    {
                        BullColorUser _tempuser = _DicPos2User[i];
                        _tempuser._isBanker = false; 
                        _tempuser._shouPaiArr = new List<int>();
                        _tempuser._bulltype = PokerBullFightType.Bull_No; 
                        _tempuser._gambleTotal = 0; 
                        _tempuser._SysDealTimeOutCount = 0;
                        _tempuser._gambletime = DateTime.Now.AddYears(100);
                    // _tempuser._WaitClientLimitCount = 0;       
                });
                base.ResetBase(_no_again);

                //不在的用户处理掉
                ForeashAllDo((i) =>
                {
                    if (!_pos2userbase.ContainsKey(i))
                    {
                        BullColorUser _tempu;
                        _DicPos2User.TryRemove(i, out _tempu);
                    }
                });
                if (_numpertable < base._num_min)
                {
                    _tablestatus = TableStatusEnum.WaitforReady;  //人数不够了，，停止下局的自动准备 
                }
                if (_no_again)
                { 
                    _DicPos2User = null;
                    _judge = null;
                    BullColorRoom myr = BullColorLobby.instance.GetRoomByRoomID(_roomid);
                    if (myr != null) myr.ResetTableByTableID(_tableid);
                    _tableid = 0;
                }
            }
        }

        /// <summary>
        /// 目前仅用于cs的返回
        /// </summary>
        /// <param name="imList"></param>
        public void AddSendDataRecord(int userid, string senddata)
        {
            UserIDMSG _usermsg = new UserIDMSG(userid, senddata, false, false);     
            _tableSendData.Add(new List<UserIDMSG>() { _usermsg });
        }

        /// <summary>
        /// 获取此桌中指定用户ID的对象
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public BullColorUser GetUserByID(int userID)
        { 
            lock(objLock)
            {
                if (_DicPos2User == null)
                {
                    ErrorRecord.Record("201704151616 fetal Error 必需处理   _DicPos2User is null" + userID);
                    return null;
                }
                foreach (var _tempUser2 in _DicPos2User)
                {
                    if (_tempUser2.Value._userid == userID) return _tempUser2.Value;
                }
                ErrorRecord.Record("201208221538bf fetal Error 必需处理   没找到userID:" + userID);
                return null;
            }
        }
        /// <summary>
        /// 获取此桌中指定用户ID的对象
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public BullColorUser GetUserByPos(int pos)
        { 
            BullColorUser _tempUser;
            _DicPos2User.TryGetValue(pos, out _tempUser);
            if (_tempUser == null) ErrorRecord.Record("201708221538bf fetal Error 必需处理   pos:" + pos);
            return _tempUser;
        }
        #endregion

        #region  进入桌子->允许开始下注->开牌
          
        /// <summary>
        /// 至少两人到位了进行 开局 ，开局就会扣掉配置钱===============================================
        /// </summary>
        private void Start( )
        {
            lock (objLock)
            {
                if (_numpertable < _num_min) return;

                base.StartBase(60 * 4);
                _pos2UserID = new Dictionary<int, int>();
                _pos2IPPort = new Dictionary<int, string>();
                _pos2Money = new Dictionary<int, decimal>();
                _pos2Win = new Dictionary<int, bool>();
                _pos2CardList = new Dictionary<int, List<int>>();
                _pos2BullRate = new Dictionary<int, int>();
                _pos2Watch = new Dictionary<int, int>();

                Queue<int> _tcardLeft = new Queue<int>();
                Dictionary<int, List<int>> _pokerList = new Dictionary<int, List<int>>();

                _pokerList[0] = BullFight.GetPokerbyBullFightType(PokerBullFightType.Bull_Bull);

                Dictionary<int, int> _pos2RobotAI = new Dictionary<int, int>();
                ForeashAllDo((i) =>
                {
                    _pos2RobotAI.Add(i, _DicPos2User[i]._tbUser.winpercent);
                });
                //  _judge.ChangePokerByRobotAI(_pos2RobotAI, _pokerList);

                List<int> _temppoker0 = new List<int>() { 0, 0, 0, 0, 0 };
                List<UserIDMSG> imList = new List<UserIDMSG>();
                List<CommonPosValSD> _uplist = new List<CommonPosValSD>();
                ForeashAllDo((i) =>
                {
                    _uplist.Add(new CommonPosValSD() { pos = i, val = _DicPos2User[i]._userid });
                }); 
                
                _DicPos2User[0]._isBanker = true;
                //同时把每位玩家的数据确认修改了 //发送消息到所有玩家   
                ForeashAllDo((i) =>
                {
                    BullColorUser tempUser = _DicPos2User[i];

                    tempUser._isPlaying = true;
                    tempUser._shouPaiArr = _pokerList[i];
                    _DicPos2User[i] = tempUser;//必须要赋值回去 要求有3个返馈才处理    

                    _tablRecord.MatchCode = _tableMathCode;
                    _tablRecord._guid = _guid;
                    _tablRecord.StartTime = DateTime.Now;
                    _pos2UserID.Add(i, tempUser._userid);
                    _pos2IPPort.Add(i, tempUser._IPandPort);
                    //发送通知消息
                    sc_tablestart_bfc_n _start = new sc_tablestart_bfc_n() { result = 1, fn = "sc_tablestart_bfc_n", _msgid = _TurnWaitTime };
                    _start.tableid = _tableid;
                    _start.pos = i;

                    _start._canGetBanker = false;
                    _start._pos2userid = _uplist;//所有玩家及对应 的位置信息
                    _start._curTableCount = _judge._curTableOverCount;
                    _start._curBankerCount = 1;
                    _start.ShowCardList = _temppoker0;
                    _start.closefun = false;//需要报价抢庄，不关闭自动显示功能
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_start), tempUser._isRobot, tempUser._isDisconnet));
                });

                BFColorSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                _DicPos2User[_bankpos].SetTimeOutAction(1, "sc_gambleover_bfc_n", 4 * 60);//1分钟等待时间
                _strStatus = "Start...3  "; 
            }
        } 

        /// <summary>
        /// 下注   1次    庄比牛牛特有
        /// </summary>
        public void GambleOne(int userid, int _targetpos,  int _gamble, int lx=10000, int ly=10000)
        {
            BullColorUser myu = GetUserByID(userid);
            if (myu == null) return;
            if (_DicPos2User[myu._Pos]._isBanker) return; //庄不能下注
            if (myu.CheckisWatch()) return;

            _DicPos2User[myu._Pos]._gambleTotal += _gamble; //设置累计下注数      
            _DicPos2User[myu._Pos]._CurrentGold -= _gamble;
            _DicPos2User[_bankpos]._CurrentGold += _gamble;//奖池
            tb_UserEx.UpdateData(_DicPos2User[myu._Pos]._tbUser);  
        } 

        private void NotifyGambleOne(int _pos, int _targetpos, int _gamble, int lx = 10000, int ly = 10000)
        {
            lock (objLock)
            {
                List<UserIDMSG> imList = new List<UserIDMSG>();
                int _tempallrate = _DicPos2User[_pos]._gambleTotal;
                if (_tempallrate == 0) _tempallrate = _DicPos2User[_pos]._gambleTotal;

                List<CommonPosValListSD> _pos2OtherPosRate_order = new List<CommonPosValListSD>();

                ForeashAllDo((i) =>
                {
                    BullColorUser tempUser = _DicPos2User[i];

                    sc_gambleone_bfc_n _gambleone_n = new sc_gambleone_bfc_n() { fn = "sc_gambleone_bfc_n", result = 1, _msgid = _TurnWaitTime };
                    _gambleone_n.pos = _pos;
                    _gambleone_n.targetpos = _targetpos;
                    _gambleone_n.rate = _gamble;
                    _gambleone_n.allrate = _tempallrate;
                    _gambleone_n._curGold = _DicPos2User[_pos]._CurrentGold - _tempallrate;
                    _gambleone_n._pos2Gamble_order = _pos2OtherPosRate_order;
                    _gambleone_n.lx = lx;
                    _gambleone_n.ly = ly;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gambleone_n), tempUser._isRobot, tempUser._isDisconnet));
                });
                BFColorSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
            }
        }

        /// <summary>
        /// 处理的所有人的下注倍数
        /// </summary>
        private void DoExecuteDealGamble()
        {
            lock (objLock)
            {
                List<UserIDMSG> imList = new List<UserIDMSG>();
                List<CommonPosValSD> _dicpos2rate = new List<CommonPosValSD>();
                ForeashAllDo((i) =>
                {
                    _dicpos2rate.Add(new CommonPosValSD() { pos = i, val = _DicPos2User[i]._gambleTotal });
                });
                List<int> _dicelist = new List<int>(); 
                ForeashAllDo((i) =>
                {
                    BullColorUser tempUser = _DicPos2User[i];

                    ////sc_gambleover_bfc_n _gamble_n = new sc_gambleover_bfc_n() { fn = "sc_gambleover_bfc_n", result = 1, _msgid = 60 };
                    ////_gamble_n.pos2rate = _dicpos2rate;
                    ////_gamble_n.firstfapaipos = 1;
                    ////_gamble_n.dicelist = _dicelist;
                    ////_gamble_n._shoupai = tempUser._shouPaiArr;
                    ////imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gamble_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BFColorSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                       
            }
        }
         
        /// <summary>
        /// 处理的所有人的摊牌与结算了， 
        /// </summary>
        private void DoExecuteDealShowDown(int userid)
        {
            lock (objLock)
            {
                List<ShowDownSDBFC> _tempshowdownlist = _judge.GetShowDownList();    //不同游戏处理算法 不同

                List<CommonPosValSD> _pos2Gold1 = _judge.GetCurrentPosGold();
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullColorUser tempUser = _DicPos2User[i];
                    //if (!tempUser._isBanker) return;//只给庄发通知消息
                    sc_showdown_bfc_n _showdown_n = new sc_showdown_bfc_n() { fn = "sc_showdown_bf_n", result = 1, _msgid = _TurnWaitTime };

                    ////_showdown_n.sdlist = _tempshowdownlist;
                    ////_showdown_n._pos2Gold = _pos2Gold1;

                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_showdown_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BFColorSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                //_bankerCanGetBonusPot 或_isContinueBigGamble 为true 本局先返是否 收取奖池且下庄 BankerGetBonusPot();  否则直接  End(); 


                _DicPos2User[_bankpos].SetTimeOutAction(1, "sc_showdown_bfc_n", 2);
            }
        }

        #endregion
         
        /// <summary>
        /// 如果是强制结束此桌，此桌不会记录与结处
        /// </summary>
        /// <param name="_forceOver"></param>
        private void DoExecuteAllEnd(bool _forceOver)
        {
            lock(objLock)
            {
                ForeashAllDo((i) =>
                {
                    int _bfRate = BullFight._dicbullfightRate[_DicPos2User[i]._bulltype];
                    //出现一次牛牛或五花牛加指定数据的钻石 疯狂牛牛只有五花牛加钻石
                    if (_bfRate >= 3)
                    {
                        int _addDiamond = (_bfRate / 3) * Math.Max(1, (int)(_judge.BaseAllMoney / 10000));
                        _DicPos2User[i]._tbUser.diamond += _addDiamond;
                    }
                    //更新数据                                  
                    tb_UserEx.UpdateData(_DicPos2User[i]._tbUser);
                });

                List<CommonPosValSD> _watchlist = new List<CommonPosValSD>();
                //写入此桌的金钱交易记录 
                if (!_forceOver)
                {
                    ForeashAllDo((i) =>
                    {
                        _pos2CardList.Add(i, _DicPos2User[i]._shouPaiArr);//记录手牌    
                        _pos2Money.Add(i, _DicPos2User[i]._CurrentGold);
                        _pos2Win.Add(i, _judge.CheckGameOverWin(_DicPos2User[i]._CurrentGold));
                        int _bfRate = BullFight._dicbullfightRate[_DicPos2User[i]._bulltype];
                        _pos2BullRate.Add(i, _bfRate);

                        //更新数据                                  
                        tb_UserEx.UpdateData(_DicPos2User[i]._tbUser);
                    });
                    ForeashAllDo((i) =>
                    {
                        //低于一定分数就当观众了
                        if (_judge.MinLimitMoney((int)_DicPos2User[i]._tbUser.UserMoney)) _DicPos2User[i]._isWatch = true;
                        else _DicPos2User[i]._isWatch = false;//金币模式中上局没开始成功的

                        _watchlist.Add(new CommonPosValSD() { pos = i, val = _DicPos2User[i]._isWatch ? 1 : 0 });
                        _pos2Watch.Add(i, _DicPos2User[i]._isWatch ? 1 : 0);
                    });
                }
                else _gameover = _forceOver;

                CheckResetTable();//  n-1家认输了。  
                _tablRecord.ActionList = "";     //写入此桌的录相 记录
                _tablRecord.EndTime = DateTime.Now;
                _tablRecord.gameid = _gameid;
                _tablRecord._isover = true;
                LogWriteToDB(_tablRecord);


                _gameover = true;
                tb_TableMoneyLogEx.SetRateDataByTableNum(_guid, _gameover, _numpertable);

                List<CommonPosValSD> _pos2Gold1 = _judge.GetCurrentPosGold();
                //向X家返回结果 通知 结束 
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullColorUser tempUser = _DicPos2User[i];

                    sc_end_bfc_n _end_n = new sc_end_bfc_n() { fn = "sc_end_bfc_n", result = 1, _msgid = _TurnWaitTime };// 显示结束面板就行

                    _end_n.gamemodel = _judge._gameCoin2Room1;
                    _end_n._OverTable = _gameover ? 1 : 0;
                    _end_n.createpos = _masterPos;
                    _end_n._pos2Watch = _watchlist;
                    _end_n._pos2Gold = _pos2Gold1;
                    _end_n._pos2Rate1 = tb_TableMoneyLogEx._pos2Rate1;
                    _end_n._pos2Rate2 = tb_TableMoneyLogEx._pos2Rate2;
                    _end_n._pos2Rate3 = tb_TableMoneyLogEx._pos2Rate3;
                    _end_n._pos2Rate4 = tb_TableMoneyLogEx._pos2Rate4;
                    _end_n.closefun = false;// 能收取奖池的时候   客户端不自动走准备方法，等下一个命令
                    if (_judge._gameCoin2Room1 == 2 && tempUser._isRobot) return;//结束不发给机器人
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_end_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BFColorSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                ForeashAllDo((i) => { _DicPos2User[i].SetTimeOutAction(1, "sc_end_bfc_n", 5 * 60); });//下把的自动开始功能        ReStart();      

                Reset(_gameover); //准备下局， 重置此桌信息    
            }
        } 

        #region 解散游戏相关，存在未开始游戏的解散，已开始游戏的解散        解散申请与超时是与其他行为并行的行为，不能使用通用超时方法
        /// <summary>
        /// 用户申请解散游戏 
        /// </summary>
        /// <param name="userID"></param>
        public void ApplyExitTable(int userid)
        {
            lock (objLock)
            {
                BullColorUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (myu.CheckisWatch()) return;     //爆分的人不能申请
                //======================数据统计
                //处理超时的动作      由_isAgreeExit状态自行处理
            }                                              
        }
        /// <summary>
        /// 执行延时处理逻辑
        /// </summary>
        private void ExcuteDelayedLogic()
        {
            lock (objLock)
            {
                //如果没人申请解散房间才执行
                if (!_applyExitTable)
                {
                    _timer = new Timer(TimerCallBack, this, -1, -1);
                    StartTimer(120000);
                }
            }
            
        }
        /// <summary>
        /// 定时器回调方法
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallBack(object state)
        {
            lock (objLock)
            {
                if (_DicPos2User != null && _judge != null)
                {
                    //解散房间
                    DoExecuteExitTable(true);
                }
            } 
        }
        /// <summary>
        /// 用户申请解散游戏      游戏已开始需要处理结算面板
        /// </summary>
        /// <param name="userID"></param>
        public void DealExitTable(int userid, bool _isagree)
        {
            lock (objLock)
            {
               
                BullColorUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (!_applyExitTable) return;//状态都不对
                if (myu._isAgreeExit != 2) return;//表示已处理了状态不能重复处理 同CheckFirstDeal
                myu._isAgreeExit = _isagree ? 1 : 0;
                //如果同意则重置定时器时间拒绝则关闭定时器
                if (_isagree)
                    ReStartTimer(120000);
                else StopTimer();
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullColorUser tempUser = _DicPos2User[i];

                    sc_dealexittable_n _gamble_n = new sc_dealexittable_n() { fn = "sc_dealexittable_n", result = 1, _msgid = _TurnWaitTime };

                    _gamble_n.pos = myu._Pos;
                    _gamble_n.agree = _isagree ? 1 : 0;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gamble_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BFColorSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                bool _allreply = true;
                ForeashAllDo((i) =>
                {
                    if (_DicPos2User[i]._isWatch) return;//输了的人没有资格回复
                    if (_DicPos2User[i]._isAgreeExit == 2) _allreply = false; //至少有一个人未回复
                });
                if (!_allreply) return;     //等所有人回复 同HaveSomeBodyUnDeal
                DoExecuteExitTable(true);
            }
        }

        /// <summary>
        /// 处理的所有人的处理一个人申请的解散游戏
        /// 规则 所有人同意才能解散。
        /// </summary>
        private void DoExecuteExitTable(bool _needDealEnd, bool _resettable=false)
        { 
            List<int> _posdisagree = new List<int>(); //有一个人不同意就不能解散
            ForeashAllDo((i) =>
            {
                if (0 == _DicPos2User[i]._isAgreeExit) _posdisagree.Add(i);
            });
            base.ExitTableall(_posdisagree, _needDealEnd);       //一定在前 因为DoExecuteAllEnd会清空基本用户数据 
            if (_posdisagree.Count > 0)
            { 
                _needDealEnd = false;
                _resettable = false;
            }
            if (_needDealEnd) DoExecuteAllEnd(true);
            if (_resettable)
            {
                //清理掉桌子则停止监听线程
                Reset(true);
                StopTimer();
            } 
        }
        #endregion

        #region 断线相关   掉线的通知，断线重连
        /// <summary>
        /// 掉线 状态通知
        /// </summary>
        /// <param name="userID"></param>
        public void NotifyDis(int userid)
        {   
            base.NotifyDisBase(userid, 0);   
        }
        /// <summary>
        /// 获取断线前的所有进入人的消息列表 
        /// </summary>
        /// <param name="userid"></param>
        public string GetEnterDisList(int userid)
        {
            BullColorUser _userbf = GetUserByID(userid);
            if (_userbf == null) return "";
            return base.GetEnterDisListBase(_userbf._Pos);  
        }
        /// <summary>
        /// 获取断线前的所有消息列表  一局的游戏操作逻辑
        /// </summary>
        /// <param name="userid"></param>
        public List<string> GetDisList(int userid)
        {
            return base.GetDisListBase(userid);        
        }
        /// <summary>
        /// 获取断线重连后的逻辑数据  如金币结算即，当前哪些人有多少金币，奖池数据 
        /// </summary>
        /// <param name="userid"></param>
        public List<string> GetDisLogicData(int userid)
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
        /// <summary>
        /// 断线重连     暂时不用
        /// </summary>     
        public void NotifyReConnect(int userid)
        {
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(userid);
            if (_us == null) return;
            _us.Status = UserStatusEnum.InTableDaiPai;
            BullColorUser myu = GetUserByID(userid);
            if (myu == null) return;
            myu._isDisconnet = false;
            //通知其他人有人上线了   
            base.NotifyDisBase(userid, 1);                    
        }
        #endregion      

        //===========================================================================================================================     
        /// <summary>
        /// 循环当前桌的每一个人   Write by jsw 201208011539
        /// </summary>
        public override void DealEveryUser()
        {
            //return;//测试专用 没和限制功能 
            lock (objLock)
            {
                if (_DicPos2User == null) return;
                ForeashAllDo((i) =>
                {
                    BullColorUser _bfuser = null;
                    if (!_DicPos2User.TryGetValue(i, out _bfuser))
                    {
                        ErrorRecord.Record(" 201611051939BF this is not enough  _numpertable" + _numpertable); return;
                    }
                    if (_bfuser._waitUserAction == "0") return;
                    switch (_bfuser._waitUserAction)
                    {
                        case "sc_entertable_n": //自动 准备
                            
                            break;
                        case "sc_ready_bfc_n":
                            Start();     // 一分钟后自己开始
                            break;
                        case "sc_tablestart_bfc_n":      //自动  不抢庄,已处理过的人不用再次处理，机器一定会提前处理的
                             break;
                       
                        case "sc_gambleover_bfc_n":
                            DoExecuteDealShowDown(_bfuser._userid);//默认都是最大类型 
                            break;
                        case "sc_setbulltype_bfc_n":
                            //TimeOutDoExecuteDealShowDown(_bfuser._userid);
                            break;
                        case "sc_showdown_bfc_n":
                            DoExecuteAllEnd(true);
                            break; 
                        case "sc_applyexittable_n": //暂时不处理超时功能
                            break;
                        case "sc_bankergetbonuspot_bf_n":
                            ////GetReady(_bfuser._userid);
                            break;
                        case "sc_exittable_n_delay":
                            base.ExitTableall(new List<int>(), true);
                            break;
                        default:  //没得状态 不处理
                            ErrorRecord.Record(" 201206171026BF _UserDic[i]._userAction:" + _DicPos2User[i]._waitUserAction);
                            break;
                    }
                    _bfuser.RecordTimeoutCount();         
                });
                base.DealAliveTime();
            }
        }

        /// <summary>
        /// 延时处理
        /// </summary>
        /// <param name="state"></param>
        private void DoTableTimer(object state)
        { 
            if (state is BaseTable)
            {

            }
        }

    }
} 