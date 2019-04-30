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
    public class BullFight100Table : BaseColorTable
    {
        public BullFight100Table()
        { }

        //成员 变量    
        /// <summary>
        /// 此桌的玩家， 2~500开始固定的， 要求有顺序的  但可能 1, 3, 5在玩，
        /// 理论上不移出也不会影响 如果不是实时结算最后需要 用户信息数据 把int 表示 pos  因为人员进来不位置不是固定的，不能循环处理
        /// </summary>
        public ConcurrentDictionary<int, BullFight100User> _DicPos2User;

        //底分 暂时用不到        
        public int _baseMoney;        
        public int _bankpos; // 庄的位置         
        public BullFight100Judge _judge;   // 此桌对应的打牌裁判
        public BullFight100Room _room;
        /// <summary>
        /// 百人牛牛特有的四个牌数据
        /// </summary>
        public Dictionary<int, List<int>> _dicPokerList;
        /// <summary>
        /// 百人牛牛
        /// </summary>
        public Dictionary<int,  PokerBullFightType> _dicPokerBFType;
        /// <summary>
        /// 百人牛牛 三个区域的下注
        /// </summary>
        public Dictionary<int, int> _dicPokerGambleTotal;
        public int _allGambleTotal;

        /// <summary>
        /// 百人牛牛的下注列表
        /// </summary>
        public Dictionary<int, int> _pos2GambleTable;

        /// <summary>
        /// 申请上庄的列表
        /// </summary>
        public ConcurrentQueue<int> _quePosGetBanker;

        private int _timeScale = 30;//60
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
                   
        public void ForeashAllDoFirstPos(int firstpos, Action<int> match)
        {
            if (match == null) return;
            if (_DicPos2User == null)
            {
                ErrorRecord.Record("201701031156bf fetal error  _DicPos2User is null.............................");
                return;
            } 
            lock (objLock)
            {
                List<BullFight100User> _userOrderBypos = new List<BullFight100User>(); //后面只执行一次，每天都执行效率可能 有点小低==放外面会不支持嵌套==============
                foreach (int key in _DicPos2User.Keys)
                {
                    _userOrderBypos.Add(_DicPos2User[key]);
                }
                _userOrderBypos = _userOrderBypos.OrderBy(u => u._Pos).ToList<BullFight100User>();  //从小到大排序

                List<BullFight100User> OrderFromfirstpos = new List<BullFight100User>();

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
                    if (_DicPos2User == null) return;
                    match(OrderFromfirstpos[i]._Pos);
                }
            }
        }

        /// <summary>
        /// 
        /// 进入2~6人后处理的
        /// </summary>
        /// <param name="tablenum"></param>
        /// <param name="userList"></param>
        public BullFight100Table(int gameid, BaseRoom _room2, int tablenum, BullFight100User _firstUser, cs_enterroom _data)
        {
            _numpertable = 1;
            _roomid = _room2.mRoomID;
            _room = _room2 as BullFight100Room;
            _tableid = tablenum;
            _baseMoney = 1;
            _bankpos = 1;
            if (_DicPos2User == null) _DicPos2User = new ConcurrentDictionary<int, BullFight100User>(); 
            ConcurrentDictionary<int, BaseUser> _temppos2user = new ConcurrentDictionary<int, BaseUser>();
            
            _quePosGetBanker = new ConcurrentQueue<int>();
            _quePosGetBanker.Enqueue(_bankpos);//================测试数据 
            _firstUser._tableID = tablenum;        //赋值桌子号  
            
            _DicPos2User.TryAdd(1, _firstUser);//1~3表示位置 
            _temppos2user.TryAdd(1, _firstUser);
            _masterPos = 1;//房主，只做建房用

            _judge = new BullFight100Judge(this);
            _judge.InitiArgs(_data);
            base._gametype = _judge._gametype;
            _tableMaxCount = _judge.GetTableorBankerMaxCount;

            _TurnWaitTime = 15;

            base.Initi(_temppos2user, _judge._minLimit, _judge._maxLimit, gameid, BF100SendDataServer.instance, DoTableTimer);
            base.EnterTable();

            _strStatus = "BullFightTable...1";
        } 

        /// <summary>
        /// 非房主进入桌子 房间模式的
        /// </summary>
        /// <param name="userid"></param>
        public bool EnterTableAdditive(tb_User tbUser)
        {
            //不限制客户端的个数 可能 有破坏的外挂===========
            if (_numpertable >= _num_max) return false;
            BullFight100User myu = new BullFight100User();
            myu.Initi(tbUser.IP, _roomid, tbUser, false);// 当成客户端 的IP：Port用   
            AllocationtoTable(myu);
            return true;
        }

        /// <summary>
        /// 分配到 桌子上的空位，
        /// </summary>
        public void AllocationtoTable(BullFight100User _adduser)
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
            //金币模式1秒后自动准备 大于2人就开始游戏
            if (_tablestatus != TableStatusEnum.Playing)
            {
                _tablestatus = TableStatusEnum.Playing;

                _DicPos2User[_masterPos].SetTimeOutAction(1, "sc_tablestart_bf100_n", _judge._gameCoin2Room1 == 1 ? _TurnWaitTime : 1);
                //自动开始下一步 Start();   
                _strStatus = "BullFightTable...1.1 for auto ready...";
            }
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
                        BullFight100User _tempuser = _DicPos2User[i];
                        _tempuser._isBanker = false;
                        _tempuser._isGetBanker = false;
                        _tempuser._bulltype = PokerBullFightType.Bull_No;
                        _tempuser._pos2Gameble = new Dictionary<int, int>();
                        _tempuser._gambleTotal = 0;
                        _tempuser._showCardList = new List<int>();
                        _tempuser._SysDealTimeOutCount = 0;
                        _tempuser._gambletime = DateTime.Now.AddYears(100);
                        // _tempuser._WaitClientLimitCount = 0;       
                    });
                base.ResetBase(_no_again);
                ForeashAllDo((i) =>
                {
                    if (i <= 2) return;
                    BullFight100User _tempu;
                    _DicPos2User.TryRemove(i, out _tempu);
                });
                //不在的用户处理掉
                ForeashAllDo((i) =>
                {
                    if (!_pos2userbase.ContainsKey(i))
                    {
                        BullFight100User _tempu;
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
                    BullFight100Room myr = BullFight100Lobby.instance.GetRoomByRoomID(_roomid);
                    if (myr != null) myr.ResetTableByTableID(_tableid);
                    _tableid = 0;
                }
                else
                {//金币模式 才需要自动开始     
                    ForeashAllDo((i) => { _DicPos2User[i].SetTimeOutAction(1, "sc_tablestart_bf100_n", 1); });//自动开始下一步         Start();   
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
        public BullFight100User GetUserByID(int userID)
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
        public BullFight100User GetUserByPos(int pos)
        { 
            BullFight100User _tempUser;
            _DicPos2User.TryGetValue(pos, out _tempUser);
            if (_tempUser == null) ErrorRecord.Record("201708221538bf fetal Error 必需处理   pos:" + pos);
            return _tempUser;
        }
        #endregion

        #region  申请上庄
        /// <summary>
        ///  设置自己要抢庄，等judge处理
        /// </summary>
        /// <param name="userid"></param>
        public bool ApplyGetBanker(int userid, bool getbanker)
        {
            lock (objLock)
            {
                BullFight100User myu = GetUserByID(userid);
                if (myu == null) return false;
                if (myu.CheckisWatch()) return false;
                //if (!myu.CheckFirstDeal()) return false;
                if (myu._CurrentGold < 1000) return false; //1000才能申请上庄
                if (_quePosGetBanker.Contains(myu._Pos)) return false;//已在申请列表中的人不能再申请了

                _quePosGetBanker.Enqueue(myu._Pos); //放入抢庄列表设置抢庄标识 
                
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullFight100User tempUser = _DicPos2User[i];

                    sc_getbankerone_bf100_n _getbanker_n = new sc_getbankerone_bf100_n() { fn = "sc_getbankerone_bf100_n", result = 1, _msgid = _TurnWaitTime };
                    _getbanker_n.pos = myu._Pos;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_getbanker_n), tempUser._isRobot, tempUser._isDisconnet));
                });
                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

               // if (HaveSomeBodyUnDeal()) return false;
                _strStatus = "GetBanker...4 all getbanker  ";
                return true;
            }
        }

        /// <summary>
        /// 庄赢X局后奖池中还有钱，可以选择手动下庄，     
        /// </summary>
        public bool BankerGetBonusPot(int userid)
        {
            BullFight100User myu = GetUserByID(userid);
            if (myu == null) return false;
            if (myu.CheckisWatch()) return false;
            if (!_quePosGetBanker.Contains(myu._Pos)) return false;
            int _firstPos = 0;
            if (!_quePosGetBanker.TryPeek(out _firstPos)) return false;
            if (_firstPos != myu._Pos) return false;
            _quePosGetBanker.TryDequeue(out _firstPos);

            return true;
        }
        #endregion

        #region  进入桌子如果人够就可以下注  自动准备->开始发牌->下注->摊牌   

        /// <summary>
        /// 至少两人到位了进行 开局 ，开局就会扣掉配置钱===============================================
        /// </summary>
        private void Start(int userid)
        {
            lock (objLock)
            {
                BullFight100User myu = GetUserByID(userid);
                if (myu == null) return;
                if (myu.CheckisWatch()) return;
                if (!myu.CheckFirstDeal()) return;
                //if (HaveSomeBodyUnDeal()) return;
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
                var type = _judge._gametype;
                _dicPokerList = new Dictionary<int, List<int>>();
                _dicPokerBFType = new Dictionary<int, PokerBullFightType>();
                _dicPokerGambleTotal = new Dictionary<int, int>();
                _pos2GambleTable = new Dictionary<int, int>();

                _allGambleTotal = 0;
                _dicPokerList = BullFight.DistributePoker(out _tcardLeft, 4);
                foreach (int key in _dicPokerList.Keys)
                {
                    _dicPokerBFType.Add(key, BullFight.GetBullType(_dicPokerList[key]));
                    _dicPokerGambleTotal.Add(key, 0);
                } 
               
                ////Dictionary<int, int> _pos2RobotAI = new Dictionary<int, int>();
                ////ForeashAllDo((i) =>
                ////{
                ////    _pos2RobotAI.Add(i, _DicPos2User[i]._tbUser.winpercent); //_pos2RobotAI.Add(i, i == 1 ? 101 : 0);
                ////});
                ////_judge.ChangePokerByRobotAI(_pos2RobotAI, _dicPokerList); 

                List<int> _temppoker0 = new List<int>() { 0, 0, 0, 0, 0 };
                List<UserIDMSG> imList = new List<UserIDMSG>();
                List<CommonPosValSD> _uplist = new List<CommonPosValSD>();

                _judge.DealReduceDiamond();//执行扣房卡 已处理只扣一次
                _judge.SetCurTableorBankerCount();

                //同时把每位玩家的数据确认修改了 //发送消息到所有玩家   
                ForeashAllDo((i) =>
                {
                    BullFight100User tempUser = _DicPos2User[i];

                    tempUser._isPlaying = true;
                    //tempUser._shouPaiArr = _pokerList[i];
                    tempUser._pos2Gameble = new Dictionary<int, int>();
                    tempUser._showCardList = new List<int>() { 0, 0, 0, 0, 0 };
                    _DicPos2User[i] = tempUser;//必须要赋值回去 要求有3个返馈才处理    

                    _tablRecord.MatchCode = _tableMathCode;
                    _tablRecord._guid = _guid;
                    _tablRecord.StartTime = DateTime.Now;
                    _pos2UserID.Add(i, tempUser._userid);
                    _pos2IPPort.Add(i, tempUser._IPandPort);
                    //发送通知消息
                    sc_tablestart_bf100_n _start = new sc_tablestart_bf100_n() { result = 1, fn = "sc_tablestart_bf100_n", _msgid = _TurnWaitTime };
                    _start.tableid = _tableid;
                    _start.pos = i;

                    _start._canGetBanker = false;
                    _start._pos2userid = _uplist;//所有玩家及对应 的位置信息
                    _start._curTableCount = _judge._curTableOverCount;
                    _start._curBankerCount = 1;
                    _start.ShowCardList = _temppoker0;
                    _start.closefun = true;//需要抢庄，不关闭自动显示功能
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_start), tempUser._isRobot, tempUser._isDisconnet));
                });

                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                _strStatus = "Start...3  ";

                DoExecuteSetBanker();
            }
        }

     
        /// <summary>
        /// 处理谁的庄   通知每个人的下注列表
        /// </summary>
        private void DoExecuteSetBanker()
        {
            lock (objLock)
            {
                _judge.GetBankerPos(); //实际抢庄逻辑处理      
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullFight100User tempUser = _DicPos2User[i];
                    sc_applybanker_bf100_n _getbanker_n = new sc_applybanker_bf100_n() { fn = "sc_applybanker_bf100_n", result = 1 };
                    tempUser.gamblelist = _judge.GetGambleList(_bankpos, i);
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_getbanker_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);

                _DicPos2User[_bankpos].SetTimeOutAction(1, "sc_applybanker_bf100_n", _TurnWaitTime);      //处理超时的 DoExecuteDealShowDown();  
            }
        }

        /// <summary>
        /// 下注   1次    
        /// </summary>
        public bool GambleOne(int userid, int _targetpos,  int _gamble )
        {
            BullFight100User myu = GetUserByID(userid);
            if (myu == null) return false;
            if (_DicPos2User[myu._Pos]._isBanker) return false;     //庄不能下注
            if (myu.CheckisWatch()) return false;
            if (!_judge.CheckGambleMaxLimit()) return false;
            //if (!myu.CheckFirstDeal()) return; //可以下多次 
            if (_targetpos > 4 || _targetpos < 2) return false;
            if (_gamble < 1) return false;
            if (myu._CurrentGold < _gamble) return false;
            myu._CurrentGold -= _gamble;

            _dicPokerGambleTotal[_targetpos] +=  _gamble;
            _allGambleTotal += _gamble;
            if (_pos2GambleTable.ContainsKey(_targetpos)) _pos2GambleTable[_targetpos] += _gamble;
            else _pos2GambleTable.Add(_targetpos, _gamble);
              
            _DicPos2User[myu._Pos].AddorUpdateGamble(_targetpos, _gamble);
            _DicPos2User[myu._Pos]._gambletime = DateTime.Now;
            NotifyGambleOne(myu._Pos, myu._Pos, _gamble);
            _strStatus = "GambleOver...5    ";
            return true;
        }
      

        private void NotifyGambleOne(int _pos, int _targetpos, int _gamble)
        {
            lock (objLock)
            {
                List<UserIDMSG> imList = new List<UserIDMSG>();  
            
                List<CommonPosValSD> _pos2GambleTotal = new List<CommonPosValSD>(); 
                foreach (var _key in _pos2GambleTable.Keys)
                {
                    _pos2GambleTotal.Add(new CommonPosValSD() { pos = _key, val = _pos2GambleTable[_key] });
                }

                List<CommonPosValSD> _pos2Gamble = new List<CommonPosValSD>();
                //ForeashAllDo((i) => {
                BullFight100User tempUser = _DicPos2User[_pos];
                foreach (var _key in tempUser._pos2Gameble.Keys)
                {
                    _pos2Gamble.Add(new CommonPosValSD() { pos = _key, val = tempUser._pos2Gameble[_key] });
                }

                sc_gambleone_bf100_n _gambleone_n = new sc_gambleone_bf100_n() { fn = "sc_gambleone_bf100_n", result = 1, _msgid = _TurnWaitTime };
                _gambleone_n.pos = _pos;
                _gambleone_n.allrate = tempUser._gambleTotal;
                _gambleone_n._curGold = _DicPos2User[_pos]._CurrentGold;
                _gambleone_n._pos2Gamble = _pos2Gamble;
                _gambleone_n._pos2GambleTotal = _pos2GambleTotal;

                imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gambleone_n), tempUser._isRobot, tempUser._isDisconnet));
                //});
                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
            }
        }        
  
        /// <summary>
        /// 处理的所有人的摊牌与结算了，
        /// 结算规则，所有人与庄家进行比较牌的大小，然后 按倍率赔钱，
        /// </summary>
        private void DoExecuteDealShowDown()
        {
            lock (objLock)
            {
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullFight100User tempUser = _DicPos2User[i];
                    //if (!tempUser._isBanker) return;//只给庄发通知消息
                    sc_showdown_bf100_n _showdown_n = new sc_showdown_bf100_n() { fn = "sc_showdown_bf100_n", result = 1, _msgid = _TurnWaitTime };

                    _showdown_n.sdlist = _judge.GetShowDownList(i);
                    _showdown_n._curGold = tempUser._CurrentGold;

                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_showdown_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList); 

                _DicPos2User[_bankpos].SetTimeOutAction(1, "sc_showdown_bf100_n", 3);//处理超时的 UserEnd();
            }
        }

        #endregion 
             
        /// <summary>
        /// 此桌结束了，一次比完牌就结束
        /// </summary>
        private void UserEnd(int userid)
        {
            lock (objLock)
            {
                BullFight100User myu = GetUserByID(userid);
                if (myu == null) return;
                if (!myu.CheckFirstDeal()) return;
                //if (HaveSomeBodyUnDeal()) return;
                DoExecuteAllEnd(false);
            }
        }
        /// <summary>
        /// 如果是强制结束此桌，此桌不会记录与结处
        /// </summary>
        /// <param name="_forceOver"></param>
        private void DoExecuteAllEnd(bool _forceOver)
        {
            lock(objLock)
            {
                ForeashAllDo((i) =>
                {   //更新数据                                  
                    tb_UserEx.UpdateData(_DicPos2User[i]._tbUser);
                });
              
                List<CommonPosValSD> _watchlist = new List<CommonPosValSD>();
                //写入此桌的金钱交易记录 
                if (!_forceOver)
                {
                    _pos2CardList = new Dictionary<int, List<int>>(_dicPokerList);//记录手牌 
                    ForeashAllDo((i) =>
                    {
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
                
                _tablRecord.ActionList = "";     //写入此桌的录相 记录
                _tablRecord.EndTime = DateTime.Now;
                _tablRecord.gameid = _gameid;
                _tablRecord._isover = (_judge._gameCoin2Room1 == 2 ? true : _gameover);
                LogWriteToDB(_tablRecord);

                bool _bankerCanGetBonusPot = _judge.CheckCanBankerGetBonusPot();   //必须在结算了后面
                
                                  
                tb_TableMoneyLogEx.SetRateDataByTableNum(_guid, _gameover, _numpertable);

                List<CommonPosValSD> _pos2Gold1 = _judge.GetCurrentPosGold();
                //向X家返回结果 通知 结束 
                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullFight100User tempUser = _DicPos2User[i];

                    sc_end_bf100_n _end_n = new sc_end_bf100_n() { fn = "sc_end_bf100_n", result = 1, _msgid = _TurnWaitTime };// 显示结束面板就行

                    _end_n.gamemodel = _judge._gameCoin2Room1;
                    _end_n._OverTable = _gameover ? 1 : 0;
                    _end_n.createpos = _masterPos;
                    _end_n._pos2Watch = _watchlist;
                    _end_n._pos2Gold = _pos2Gold1;

                    _end_n.closefun = _bankerCanGetBonusPot ? true : false;// 能收取奖池的时候   客户端不自动走准备方法，等下一个命令
                    if (_judge._gameCoin2Room1 == 2 && tempUser._isRobot) return;//结束不发给机器人
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_end_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BF100SendDataServer.instance.SendDataDelay(imList);
                _tableSendData.Add(imList);
                ForeashAllDo((i) => { _DicPos2User[i].SetTimeOutAction(1, "sc_end_bf100_n", _bankerCanGetBonusPot  ? _TurnWaitTime : 6); });//下把的自动开始功能        ReStart();      

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
                BullFight100User myu = GetUserByID(userid);
                if (myu == null) return;
                if (myu.CheckisWatch()) return;     //爆分的人不能申请
                ExcuteDelayedLogic();
                if (_applyExitTable && myu._isAgreeExit != 2)
                { //有可能在有人申请解散后，其他人没有收到消息，然后就出现所有人卡死在房间的  myu._isAgreeExit != 2 补丁，允许自己申请当成同意解散。
                    return;//如果 有人申请解散了 就不处理了， 
                }
                //申请游戏解散想其它用户发送消息过后执行定时任务超过指定时间没人反应则强制解散房间
                ExcuteDelayedLogic();
                _applyExitTable = true;
              
                myu._isAgreeExit = 1;
                if (!_haveCheckRoomCard)
                {   //未开时扣房卡之前   1.如果是房主可以直接解散。2.非房主直接退出个人自己
                    if (_masterPos == myu._Pos) DoExecuteExitTable(false, true);
                    else
                    {
                        base.ExitTableOne(myu._Pos, userid);//仅发送了消息，未处理逻辑，需要移出  
                        BaseUser mybu;
                        base._pos2userbase.TryRemove(myu._Pos, out mybu);
                        _DicPos2User.TryRemove(myu._Pos, out myu);
                        _applyExitTable = false;
                        StopTimer();
                    }
                    return;
                }

                List<UserIDMSG> imList = new List<UserIDMSG>();
                ForeashAllDo((i) =>
                {
                    BullFight100User tempUser = _DicPos2User[i];

                    sc_applyexittable_n _gamble_n = new sc_applyexittable_n() { fn = "sc_applyexittable_n", result = 1, _msgid = _TurnWaitTime };
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
               
                BullFight100User myu = GetUserByID(userid);
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
                    BullFight100User tempUser = _DicPos2User[i];

                    sc_dealexittable_n _gamble_n = new sc_dealexittable_n() { fn = "sc_dealexittable_n", result = 1, _msgid = _TurnWaitTime };

                    _gamble_n.pos = myu._Pos;
                    _gamble_n.agree = _isagree ? 1 : 0;
                    imList.Add(new UserIDMSG(tempUser._userid, JsonUtils.Serialize(_gamble_n), tempUser._isRobot, tempUser._isDisconnet));
                });

                BF100SendDataServer.instance.SendDataDelay(imList);
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
            BullFight100User _userbf = GetUserByID(userid);
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
            BullFight100User myu = GetUserByID(userid);
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
                    BullFight100User _bfuser = null;
                    if (!_DicPos2User.TryGetValue(i, out _bfuser))
                    {
                        ErrorRecord.Record(" 201611051939BF this is not enough  _numpertable" + _numpertable); return;
                    }
                    if (_bfuser._waitUserAction == "0") return;
                    switch (_bfuser._waitUserAction)
                    {
                        case "sc_tablestart_bf100_n":
                            Start(_bfuser._userid);
                            break; 
                        case "sc_applybanker_bf100": //定庄后需要通知下注 
                         
                            break;
                        case "sc_applybanker_bf100_n": //定庄后需要通知下注  系统执行结算
                            DoExecuteDealShowDown();
                            break;
                        case "sc_showdown_bf100_n":
                            UserEnd(_bfuser._userid);
                            break;
                        case "sc_end_bf100_n"://自动下一局
                            ////GetReady(_bfuser._userid);
                            break;
                        case "sc_applyexittable_n": //暂时不处理超时功能
                            break;
                        case "sc_bankergetbonuspot_bf100_n":
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