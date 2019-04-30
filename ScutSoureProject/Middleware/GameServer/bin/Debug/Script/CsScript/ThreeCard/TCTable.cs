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
    public class TCTable : BaseTable
    {
        public TCTable()
        { }

        // 成员 变量     
        /// <summary>
        /// 1.2.3,4,5,6 key
        /// 此桌的玩家， 固定3个 开始固定的， 要求有顺序的  但可能 1, 3, 5在玩，
        /// 理论上不移出也不会影响 如果不是实时结算最后需要 用户信息数据
        /// 把int 表示 pos  因为人员进来不位置不是固定的，不能循环处理
        /// </summary>
        public ConcurrentDictionary<int, TCUser> _DicPos2User;
        //底分 暂时用不到        
        public int _baseMoney;
        public int _bankpos; // 庄的位置 
        /// <summary>
        /// 此桌对应的打牌裁判
        /// </summary>
        private TCJudge _judge; 

        /// <summary>
        /// 总回合数  === 最大回合数后，就直接比牌，   暂时行于人数*20
        /// </summary>
        public int _allTurnCount;
        private int _currentTurnCount;
        public int _allMoney;         
     
        public void ForeashAllDo(Action<int> match)
        {
            if (_DicPos2User == null)
            {
                ErrorRecord.Record("201611151415TC fetal error  _DicPos2User is null.............................");
                return;
            }
            if (match == null) return;
            foreach (int key in _DicPos2User.Keys)
            {
                match(key);
            }
        }

        public void ForeashAllDoFirstPos(int firstpos, Action<int> match)
        {
            if (_DicPos2User == null)
            {
                ErrorRecord.Record("201701031156TC fetal error  _DicPos2User is null.............................");
                return;
            }
            if (match == null) return;
            List<TCUser> _userOrderBypos = new List<TCUser>(); //后面只执行一次，每天都执行效率可能 有点小低==放外面会不支持嵌套==============
            foreach (int key in _DicPos2User.Keys)
            {
                _userOrderBypos.Add(_DicPos2User[key]);
            }
            _userOrderBypos = _userOrderBypos.OrderBy(u => u._Pos).ToList<TCUser>();  //从小到大排序

            List<TCUser> OrderFromfirstpos = new List<TCUser>();

            foreach (var tempuser in _userOrderBypos)
            {   //先执行大于等于pos的
                if (tempuser._Pos >= firstpos) OrderFromfirstpos.Add(tempuser);
            }

            foreach (var tempuser in _userOrderBypos)
            {  //再执行大于等于pos的
                if (tempuser._Pos < firstpos) OrderFromfirstpos.Add(tempuser);
            }
            for (int i = 0; i < OrderFromfirstpos.Count; i++)
            {
                match(OrderFromfirstpos[i]._Pos);
            }
        }
        /// <summary>
        /// 进入三个人后处理的
        /// </summary>
        /// <param name="tablenum"></param>
        /// <param name="userList"></param>
        public TCTable(int gameid, int roomid, int tablenum, List<TCUser> userList, cs_enterroom _data)
        {
            lock (objLock)
            {
                _numpertable = userList.Count;
                _roomid = roomid;
                _tableid = tablenum;
                _baseMoney = 1;
                _bankpos = 1;

                _allTurnCount = _numpertable * 20;
                _currentTurnCount = 0;   
                if (_DicPos2User == null) _DicPos2User = new ConcurrentDictionary<int, TCUser>();

                List<TCUser> _fuserlist = new List<TCUser>(userList);
                ConcurrentDictionary<int, BaseUser> _temppos2user = new ConcurrentDictionary<int, BaseUser>();
                for (int i = 0; i < _fuserlist.Count; i++)
                {
                    _fuserlist[i]._tableID = tablenum;        //赋值桌子号 

                    _DicPos2User.TryAdd(i + 1, _fuserlist[i]);//1~3表示位置 
                    _temppos2user.Add(i + 1, _fuserlist[i]);
                }
                _judge = new TCJudge(this);
                _judge.InitiArgs(_data);
                base._gametype = _judge._gametype;
                _tableMaxCount = _judge.GetTableorBankerMaxCount;

                ForeashAllDo((i) =>
                {
                    _DicPos2User[i]._CurrentGold = _judge.BaseAllMoney;   //初始化底分。     
                });

                base.Initi(_temppos2user, _judge._minLimit, _judge._maxLimit, gameid, TCSendDataServer.instance, DoTableTimer);
                base.EnterTable();
            }
        }
        public int CheckRoomCard(int userid)
        {
            TCUser myu = GetUserByID(userid);
            if (myu == null) return -1;
            _masterPos = myu._Pos;
            if (_judge.CheckDiamond(myu._Pos)) return 1;
            else return -10;
        }
        #region 非功能 成员 方法
        /// <summary>
        /// 重置此桌信息
        /// </summary>
        private void Reset(bool _no_again)
        {
            lock (objLock)
            {
                ForeashAllDo((i) =>
                {
                    TCUser _tempuser = _DicPos2User[i];
                    _tempuser._isBanker = false;       
                    _tempuser._shouPaiArr = new List<int>();     
                    _tempuser._SysDealTimeOutCount = 0;
                    // _tempuser._WaitClientLimitCount = 0;       
                });
                base.ResetBase(_no_again);
                //不在的用户处理掉
                ForeashAllDo((i) =>
                {
                    if (!_pos2userbase.ContainsKey(i))
                    {
                        TCUser _tempu;
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
                    TCRoom myr = TCLobby.instance.GetRoomByRoomID(_roomid);
                    if (myr != null) myr.ResetTableByTableID(_tableid);
                    _tableid = 0;
                }
            } 
        }
      
        /// <summary>
        /// 获取此桌中指定用户ID的对象
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public TCUser GetUserByID(int userID)
        {
            lock (objLock)
            {
                if (_DicPos2User == null)
                {
                    ErrorRecord.Record("201208221537tc _DicUser == null fetal Error 必需处理     运行正常后去掉 userID:" + userID);
                    return null;
                }
                foreach (int key in _DicPos2User.Keys)         
                {
                    if (_DicPos2User[key]._userid == userID) return _DicPos2User[key];
                }
                ErrorRecord.Record("201208221538tc fetal Error 必需处理   没找到userID:" + userID);
                return null;
            }
        }   
       
        // 获取本桌已经弃牌的人数 
        private int GetGiveUp()
        {
            lock (objLock)
            {
                int giveup = 0;
                ForeashAllDo((i) =>
                {                                                  
                    if (_DicPos2User[i]._isgiveup) giveup++;
                });
                return giveup;
            }
        }

        /// <summary>
        /// 所有人进行比牌， 按顺序自动进行比牌
        /// </summary>
        private TCUser CompareAll()
        {                 
            List<TCUser> _tempall = new List<TCUser>();
            ForeashAllDo((i) =>
            {                                                   
                if (_DicPos2User[i]._isgiveup) return;
                _tempall.Add(_DicPos2User[i]);
            });
            if (_tempall.Count == 0)
            {
                ErrorRecord.Record(" 201610281559TC  到达最大TOKEN，比牌user个数为0 ");
                return null;
            }

            TCUser _tempfirstUser = _tempall[0];
            if (_tempall.Count >1)
            {             
                for (int i = 1; i < _tempall.Count; i++)
                {
                    if (ThreeCard.ComparePoker(_tempfirstUser._shouPaiArr, _tempall[i]._shouPaiArr))
                    {                        
                        _tempall[i]._isgiveup = true;
                    }
                    else
                    {
                        _tempfirstUser = _tempall[i];
                        _tempfirstUser._isgiveup = true;
                    }
                }
            }
            return _tempfirstUser;
        }

        private void MoveNextToken()
        {
            do
            {
                SetNextDicUserIndex();
            }
            while (_DicPos2User[_userTokenPos]._isgiveup);
        }
        #endregion

        #region 进入桌子->准备->发牌，  下注(包括是否加倍下注)， 看牌， 弃牌， 比牌 结算

        /// <summary>
        /// 准备后，自动发牌     不处理后续超时行为
        /// </summary>
        /// <param name="userid"></param>
        public void GetReady(int userid)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;
                _DicPos2User[myu._Pos]._isReady = true; //设置准备标识

                //通知所有人，有人准备了      

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    sc_ready_tc_n _ready = new sc_ready_tc_n() { fn = "sc_ready_tc_n", result = 1, _msgid = _bsDataServer.TurnWaitTime };
                    _ready.pos = myu._Pos;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_ready), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                if (HaveSomeBodyUnDeal()) return;
                // 处理所有人准备完了
                ForeashAllDo((i) => { _DicPos2User[i].SetTimeOutAction(1, "sc_ready_tc_n"); });//处理超时的动作   sc_ready_tc_n    Start();  
            }
        }

        /// <summary>
        /// 至少两人到位了进行 开局，开局就会扣掉配置钱===============================================
        /// </summary>
        public void Start(int userid)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (myu.CheckisWatch()) return;
                if (!myu.CheckFirstDeal()) return;
                if (HaveSomeBodyUnDeal()) return;
                if (_numpertable < _num_min) return;

                base.StartBase(60 * 10);
                //第一次随机庄
                _bankpos = new Random().Next(1, _numpertable + 1); //BankerPos = 1; 

                Queue<int> _tcardLeft = new Queue<int>();
                Dictionary<int, List<int>> _pokerList = ThreeCard.DistributePoker(out _tcardLeft, _numpertable);   //分牌

                 
                List<UserIDMSG> imList = new List<UserIDMSG>();
                List<CommonPosValSD> _uplist = new List<CommonPosValSD>();
                ForeashAllDo((i) =>
                {
                    _uplist.Add(new CommonPosValSD() { pos = i, val = _DicPos2User[i]._userid });
                });
                // 同时把每位玩家的数据确认修改了 //发送消息到所有玩家
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    tempUser._isPlaying = true;
                    tempUser._Pos = i;
                    tempUser._shouPaiArr = _pokerList[i];
                                                       
                    _DicPos2User[i] = tempUser;//必须要赋值回去 要求有3个返馈才处理     
                     
                    _tablRecord.MatchCode = _tableMathCode;
                    _tablRecord._guid = _guid;
                    _tablRecord.StartTime = DateTime.Now;  

                    //发送通知消息
                    sc_tablestart_tc_n _start = new sc_tablestart_tc_n() { result = 1, fn = "sc_tablestart_tc_n" };
                    _start.tableid = _tableid;
                    _start.pos = i;
                    _start.BankerPos = _bankpos;
                    _start._user2pos = _uplist;//所有玩家及对应 的位置信息                                       
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_start), tempUser._isRobot, tempUser._isDisconnet));

                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                AutoBaseGamble();                                                   
                
                // ，只有庄的下一个位置有超时处理
                MoveNextToken();
                _DicPos2User[_userTokenPos].SetTimeOutAction(1, "sc_tablestart_tc_n");//下一位的处理 令牌功能           MoveTableToken();   
            }
        }
        /// <summary>
        /// 自动打底，，，基础下注
        /// </summary>
        private void AutoBaseGamble()
        {
            ForeashAllDo((i) =>
            {
                TCUser tempUser = _DicPos2User[i];
                tempUser._tempMoney -= _baseMoney;
                _allMoney += _baseMoney;
            });
        }
      
        /// <summary>
        /// 正常轮换令牌。   通知所有人现在哪个说话
        /// </summary> 
        private void NotifyNextUser(int userid)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;
                if (HaveSomeBodyUnDeal()) return; //其实只有一个等待操作
                if (myu._Pos != _userTokenPos)
                {
                    ErrorRecord.Record("201611071850tc  不是你说话呀，，，，，，，bug....................");
                }

                 
                _currentTurnCount++;
                if (_currentTurnCount >= _allTurnCount)
                {//所有人进行比牌，强制结束
                    CompareAll(); 
                    return;
                } 
                if (GetGiveUp() >= _numpertable - 1)
                {
                   // End();   //==================================================================================
                    return; 
                }
           
                //通知所有人
                List <UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];
                    if (i == _userTokenPos)
                    {                                      
                        _DicPos2User[_userTokenPos].SetTimeOutAction(1, "sc_token_tc_n");//处理超时的动作
                    }

                    sc_token_tc_n _token = new sc_token_tc_n() { fn = "sc_token_tc_n", result = 1 };
                    _token.pos = _userTokenPos;
                    _token.allmoney = _allMoney;
                    _token.alltoken = _allTurnCount;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_token), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
            }
        }

        /// <summary>
        /// 看牌 只是中间操作，，，不会做为超时中断处理，，
        /// </summary>
        /// <param name="userID"></param>
        public List<int> ShowCard(int userID)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userID);
                if (myu == null) return null;
                // if (!myu.IsHaveLimit()) return;  //========
                 
                int showpos = myu._Pos;
                myu._isShowCard = true;         

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    //发送通知消息
                    sc_showcard_tc_n _show = new sc_showcard_tc_n() { fn = "sc_showcard_tc_n", result = 1 };
                    _show.pos = showpos;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_show), tempUser._isRobot, tempUser._isDisconnet));
                });
                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                //返回3张手牌  
                return _DicPos2User[showpos]._shouPaiArr;
            }
        }
            
        /// <summary>
        /// 处理的所有人的下注倍数
        /// </summary>
        public void Gamble(int userID, int money, bool addrate=false)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userID);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;


                //money 有效规则检测 
                if (money == 0)
                {
                    money = _baseMoney;//超时处理
                }
                else
                {
                    if (money % _baseMoney != 0)
                    {
                        ErrorRecord.Record(money + "<-:money   _baseMoney->" + _baseMoney); return;
                    }
                }
                int showpos = myu._Pos;
                _allTurnCount++;

                _allMoney += money;  
                _DicPos2User[showpos]._tempMoney -= money;

                _DicPos2User[showpos]._myTurn++;

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    sc_gamble_tc_n _gamble_n = new sc_gamble_tc_n() { fn = "sc_gamble_tc_n", result = 1 };
                    _gamble_n.money = money;
                    _gamble_n.pos = showpos;
                    _gamble_n.addrate = false;
                    _gamble_n.allmoney = _allMoney;
                    _gamble_n.allturn = _allTurnCount;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gamble_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                MoveNextToken(); _DicPos2User[_userTokenPos].SetTimeOutAction(1, "sc_gamble_tc_n");//下把的自动开始功能           MoveTableToken(); 
            }
        }


        /// <summary>
        /// 请求比牌  结算条件在此处理 
        /// </summary>
        public void Compare(int userID, int targetpos)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userID);
                if (myu == null) return ;
                if (!myu.CheckFirstDeal()) return ;

                _allMoney += _baseMoney*2;          //比牌暂时扣2倍基础的钱
                myu._tempMoney -= _baseMoney * 2;   //比牌暂时扣2倍基础的钱  ====================

                int applypos = myu._Pos;
                int fialpos;
                if (ThreeCard.ComparePoker(_DicPos2User[applypos]._shouPaiArr, _DicPos2User[targetpos]._shouPaiArr)) fialpos = targetpos;
                else fialpos = applypos;
                _DicPos2User[fialpos]._isgiveup = true;

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    sc_compare_tc_n _compare_n = new sc_compare_tc_n() { fn = "sc_compare_tc_n", result = 1 };
                    _compare_n.failpos = fialpos;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_compare_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                MoveNextToken(); _DicPos2User[_userTokenPos].SetTimeOutAction(1, "sc_compare_tc_n");//下把的自动开始功能           MoveTableToken(); 
            }
        }

        /// <summary>
        /// 弃牌
        /// </summary>
        /// <param name="userID"></param>
        public void GiveUp(int userID)
        {
            lock (objLock)
            { 
                TCUser myu = GetUserByID(userID);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;
                  

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];
                    if (myu._Pos == i) myu._isgiveup = true;    
                    sc_giveup_tc_n _compare_n = new sc_giveup_tc_n() { fn = "sc_giveup_tc_n", result = 1 };
                    _compare_n.pos = myu._Pos;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_compare_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                MoveNextToken(); _DicPos2User[_userTokenPos].SetTimeOutAction(1, "sc_giveup_tc_n");//下把的自动开始功能           MoveTableToken(); 
            }
        }
        #endregion


        #region 一桌结束 相关操作
        //一桌结束 相关操作      
        /// <summary>
        /// 此桌结束了，一次比完牌就结束
        /// </summary>
        private void UserEnd(int userid)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;
                if (HaveSomeBodyUnDeal()) return;
                DoExecuteAllEnd(false);
            }
        }
        /// <summary>
        /// 此桌结束了，所有人  比完了， ===============喜钱，，，两种喜钱==============================
        /// </summary>
        private void DoExecuteAllEnd(bool _forceOver)
        {
            lock (objLock)
            {
                bool isEnd = false;//必须提前处理 
                if (GetGiveUp() >= _numpertable - 1) isEnd = true;//一桌结束，  
                if (!isEnd) return;
                List<int> _wincard = new List<int>();
                List<CommonPosValSD> _moneylist = new List<CommonPosValSD>();//缺的清单信息 
                ForeashAllDo((i) =>
                {
                    _moneylist.Add(new CommonPosValSD() { pos = i, val = _DicPos2User[i]._tempMoney });
                    if (!_DicPos2User[i]._isgiveup)
                    {
                        _wincard = _DicPos2User[i]._shouPaiArr;
                    }
                });
                //只有最后一家才有收喜钱，，，还有一种规则是直接看牌的不给喜钱，
                ForeashAllDo((i) =>
                {

                });

                    //向X家返回结果 通知 结束 
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    sc_end_tc_n _end_n = new sc_end_tc_n() { fn = "sc_end_tc_n", result = 1 };// 显示结束面板就行
                    _end_n.allmoney = _allMoney;
                    _end_n.winCard = _wincard;
                    _end_n.endMoneylist = _moneylist;
                    _end_n.EasterEgg = 0;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_end_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                ForeashAllDo((i) => { _DicPos2User[i].SetTimeOutAction(1, "sc_end_tc_n"); });//下把的自动开始功能        ReStart(); 

                //写入此桌的录相 记录
                _tablRecord.ActionList = "";
                _tablRecord.EndTime = DateTime.Now;   
                try
                {

                    tb_tablerecord tr = new tb_tablerecord()
                    {
                        MatchCode = _tablRecord.MatchCode,
                        StartTime = _tablRecord.StartTime,
                        EndTime = _tablRecord.EndTime,
                        //PostoIPArr = _tablRecord.PostoIPArr,
                        //posarr = _tablRecord.posarr,
                        ActionList = _tablRecord.ActionList,
                        //ActionCount = _tablRecord.ActionCount,
                        LookCount = _tablRecord.LookCount
                    };
                   //// LogWriteToDB(0, tr);
                }
                catch (Exception ex)
                {
                    ErrorRecord.Record(ex, " 201207231051TC");
                }
                //重置此桌信息
               ///// Reset(CheckResetTable());
            }
        }

        private void ReStart(int userid)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;
                if (HaveSomeBodyUnDeal()) return;

                if (_numpertable < base._num_min)
                {
                    ErrorRecord.Record(" 201611061137tc again game      _numpertable < base._num_min...... 需要系统再分配，，，暂时未处理，，，，，============================");
                    return;
                }
                ForeashAllDo((i) =>
                {
                    _DicPos2User[i]._WaitClientLimitCount = 1;   //必须初始化，否则下局会报错
                    GetReady(_DicPos2User[i]._userid);
                });
            }
        }
        #endregion

        #region 解散游戏相关，存在未开始游戏的解散，已开始游戏的解散        解散申请与超时是与其他行为并行的行为，不能使用通用超时方法
        /// <summary>
        /// 用户申请解散游戏 
        /// </summary>
        /// <param name="userID"></param>
        public void ApplyExitTable(int userid)
        {
            lock (objLock)
            {
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (myu.CheckisWatch()) return;     //爆分的人不能申请
                if (_applyExitTable) return;//如果 有人申请解散了 就不处理了， 

                _applyExitTable = true;
                myu._isAgreeExit = 1;
                if (!_haveCheckRoomCard)
                {   //未开时扣房卡之前   1.如果是房主可以直接解散。2.非房主直接退出个人自己
                    if (_masterPos == myu._Pos) DoExecuteExitTable(false);
                    else
                    {
                        base.ExitTableOne(myu._Pos, userid);//仅发送了消息，未处理逻辑，需要移出  
                        base._pos2userbase.Remove(myu._Pos);
                        _DicPos2User.TryRemove(myu._Pos, out myu);
                    }
                    return;
                }

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    sc_applyexittable_n _gamble_n = new sc_applyexittable_n() { fn = "sc_applyexittable_n", result = 1, _msgid = _bsDataServer.TurnWaitTime };
                    _gamble_n.gameid = _gameid;
                    _gamble_n.pos = myu._Pos;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gamble_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                //处理超时的动作      由_isAgreeExit状态自行处理                                              
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
                TCUser myu = GetUserByID(userid);
                if (myu == null) return;
                if (!_applyExitTable) return;//状态都不对
                if (myu._isAgreeExit != 2) return;//表示已处理了状态不能重复处理 同CheckFirstDeal
                myu._isAgreeExit = _isagree ? 1 : 0;

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    TCUser tempUser = _DicPos2User[i];

                    sc_dealexittable_n _gamble_n = new sc_dealexittable_n() { fn = "sc_dealexittable_n", result = 1, _msgid = _bsDataServer.TurnWaitTime };

                    _gamble_n.pos = myu._Pos;
                    _gamble_n.agree = _isagree ? 1 : 0;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gamble_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                TCSendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                bool _allreply = true;
                ForeashAllDo((i) =>
                {
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
        private void DoExecuteExitTable(bool _needDealEnd)
        {
            lock (objLock)
            {
                List<int> _posdisagree = new List<int>(); //有一个人不同意就不能解散
                ForeashAllDo((i) =>
                {
                    if (0 == _DicPos2User[i]._isAgreeExit) _posdisagree.Add(i);
                });

                base.ExitTableall(_posdisagree, _needDealEnd);       //一定在前 因为DoExecuteAllEnd会清空基本用户数据 
                if (_needDealEnd) DoExecuteAllEnd(true);
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
            TCUser _userbf = GetUserByID(userid);
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
        /// 断线重连     暂时
        /// </summary>     
        public void NotifyReConnect(int userid)
        {
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(userid);
            if (_us == null) return;
            _us.Status = UserStatusEnum.InTableDaiPai;
            TCUser myu = GetUserByID(userid);
            if (myu == null) return;
            myu._isDisconnet = false;
            //通知其他人有人上线了   
            base.NotifyDisBase(userid, 1);
        }
        #endregion      


        //===========================================================================================================================
        /// <summary>
        /// 循环当前桌的每一个人 不加锁 Write by jsw 201208011539
        /// </summary>
        public override void DealEveryUser()
        {
            //return;//测试专用 没和限制功能 
            lock (objLock)
            {
                if (_DicPos2User == null) return;
                ForeashAllDo((i) =>
                {
                    TCUser _bfuser = null;
                    if (!_DicPos2User.TryGetValue(i, out _bfuser))
                    {
                        ErrorRecord.Record(" 201611051939TC this is not enough  _numpertable" + _numpertable); return;
                    }
                    if (_bfuser._waitUserAction == "" || _bfuser._waitUserAction == "0") return;
                    switch (_bfuser._waitUserAction)
                    {
                        case "sc_entertable_n":                          //自动 准备
                            GetReady(_bfuser._userid);
                            break;
                        case "sc_ready_tc_n":
                            Start(_bfuser._userid);              // 处理所有人准备完了  
                            break;
                        case "sc_tablestart_tc_n":
                        case "sc_gamble_tc_n":
                        case "sc_compare_tc_n":
                        case "sc_giveup_tc_n":
                            NotifyNextUser(_bfuser._userid);
                            break;
                        case "sc_token_tc_n":  //自动 最低倍跟注 弃牌       
                            //GiveUp(_bfuser._userid); //弃牌
                            Gamble(_bfuser._userid, 0);
                            break;
                        case "sc_end_tc_n":
                            ReStart(_bfuser._userid);
                            break;
                        default:  //没得状态 不处理
                            ErrorRecord.Record(" 201206171026 _UserDic[i]._userAction:" + _bfuser._waitUserAction);
                            break;
                    }
                    _bfuser.RecordTimeoutCount();
                    //_bfuser._waitUserAction = "0";//==========================================看后面是否需要执行一次就不需要执行了
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