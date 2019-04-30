using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 玩家之不能相互确认的 让裁判来 确定
    /// 1.亮牌后，庄家与每一个闲家比牌 
    /// 比大小，计算倍率,支持500人，都是外围
    /// </summary>
    public class BullFight100Judge
    {
        private object obj = new object();

        #region 变量
        public  int _minLimit = 2;
        public  int _maxLimit = 500;
        private BullFight100Table _myTable;
        /// <summary>
        /// int 是用户的位置 按顺序判断 后面可以进行优先级抢断
        /// 这个只能在当前类，使用， 
        /// 且其他New的类不能使用 因为没有赋值
        /// </summary>
        private ConcurrentDictionary<int, int> DicPosToType;

        /// <summary>
        /// 游戏类型 1.通比牛牛 2.疯狂牛牛
        /// </summary>
        public int _gametype;
        /// <summary>
        /// 游戏模式，1房卡模式，2金币模式
        /// </summary>
        public int _gameCoin2Room1;
        /// <summary>
        /// 基础分数：1w,3w,5w 
        /// </summary>
        private int _baseMoney;
        public int BaseAllMoney
        {
            get { return _baseMoney; }
        }
        /// <summary>
        /// 抢庄类型BFRankerEnum
        /// </summary>
        public BFRankerEnum _rankertype;
        /// <summary>
        /// 消耗一张房卡，还是两张
        /// </summary>
        private int _roomcard;
        /// <summary>
        /// 当前限制的局数 
        /// </summary>
        private int _maxTableOverCount;
        /// <summary>
        /// 当前 的局数。
        /// </summary>
        public int _curTableOverCount;
          
        /// <summary>
        /// 上局当庄的位置，这局需要判断是否要强制继续还是手动选择，或大家来抢一次
        /// </summary>
        private int _lastbankerpos;
        /// <summary>
        /// 
        /// </summary>
        private int _FirstFaPaiPos;
        
        /// <summary>
        /// 获取最大的  局数或者庄数
        /// </summary>
        public int GetTableorBankerMaxCount
        {
            get
            {
                return _maxTableOverCount;
            }
        }

        /// <summary>
        /// 小于等于一定的分数就当观众了 
        /// </summary>
        public bool MinLimitMoney(int currentGold)
        {
            if (currentGold <= 500) return true;
            return false; 
        }
        #endregion

        public BullFight100Judge(BullFight100Table myTable)
        {
            _myTable = myTable;
            DicPosToType = new ConcurrentDictionary<int, int>();
        }

        public void InitiArgs(cs_enterroom _data)
        {
            if (_data.rankertype < 1 || _data.rankertype > 2) return;
            if (_data.numpertable < 2 || _data.numpertable > 500) return;
            _gametype = _data.gametype;
            _gameCoin2Room1 = _data.gamemodel;
            _baseMoney = _data.baserate;
            _rankertype = BFRankerEnum.TurnFixed;

            _roomcard = 0;// _data.roomcard;
            _minLimit = 2;
            _maxLimit = _data.numpertable;
            _maxTableOverCount = 100000;// _data.tableCount;
          
            //_baseallmoney = 200;//测试数据    
            _curTableOverCount = 0; 
        }
        public void SetCurTableorBankerCount()
        {
            _curTableOverCount++; 
        }
        public bool CheckDiamond(int pos)
        {
            BullFight100User _tempUser = _myTable.GetUserByPos(pos);
            if (_tempUser == null) return false;
            if (_tempUser._tbUser.diamond >= _roomcard) return true;
            return false;
        }

        public void DealReduceDiamond()
        {
            if (_myTable._haveCheckRoomCard) return;
            _myTable._haveCheckRoomCard = true;
            _myTable._DicPos2User[_myTable._masterPos]._tbUser.diamond -= _roomcard;
        }

        /// <summary>
        /// 给机器配置胜率后，机器人拿大牌，有多个机器时，以早大POS的机器人为准
        /// 如果有多个设置胜率帐号在同一桌，则会只有一个机率有效。
        /// </summary>
        /// <param name="pos2RobotAI"></param>
        /// <param name="_pos2Poker"></param>
        /// <returns></returns>
        public Dictionary<int, List<int>> ChangePokerByRobotAI(Dictionary<int, int> pos2RobotAI, Dictionary<int, List<int>> _pos2Poker)
        { 
            Dictionary<int, PokerBullFightType> _pbftype = new Dictionary<int, PokerBullFightType>();
            _myTable.ForeashAllDo((i) =>
            {
                _pbftype.Add(i, BullFight.GetBullType(_pos2Poker[i]));
            });
            int _maxpos = _myTable._masterPos;
            PokerBullFightType maxvalue = PokerBullFightType.Bull_No;
            _myTable.ForeashAllDo((i) =>
            {
                if (maxvalue < _pbftype[i])
                {
                    maxvalue = _pbftype[i];
                    _maxpos = i;
                }
            });
            int _RobotRatePos = _maxpos;
            _myTable.ForeashAllDo((i) =>
            {
                int _AIRate = pos2RobotAI[i];
                if (_AIRate == 0) return;
                int _tempRate = ToolsEx.GetRandomSys(0, 100);
                if (_tempRate <= _AIRate)
                {
                    _RobotRatePos = i;
                }
            });
            List<int> _tempChange = new List<int>();//把最大的牌换给机器人最高胜率的人
            _tempChange = _pos2Poker[_RobotRatePos];
            _pos2Poker[_RobotRatePos] = _pos2Poker[_maxpos];
            _pos2Poker[_maxpos] = _tempChange; 

            return _pos2Poker;
        }
        /// <summary>
        /// 根据裁判来处理是谁的庄，不同游戏类型，抢庄玩法处理不一样
        /// </summary>
        /// <returns></returns>
        public void GetBankerPos()
        {
            int bankpos = 0;
            if (!_myTable._quePosGetBanker.TryPeek(out bankpos))
            {
                ErrorRecord.Record(" banker get error! must deal...");
                bankpos = _myTable._bankpos;

                _myTable._DicPos2User[bankpos]._isBanker = true;
            }
            
            _lastbankerpos = bankpos;//记录一次局的下局可能用到，升庄牛牛的
            _myTable._bankpos = bankpos;   //多存一次位置 写起方便
        }

        /// <summary>
        /// 包括 升庄牛牛中，庄的下注列表
        /// </summary>
        /// <param name="bankerpos"></param>
        /// <param name="pos"></param>ShowDownSD
        /// <returns></returns>
        public List<int> GetGambleList(int bankerpos, int pos)
        {
            List<int> _gambleist = new List<int>() { 1, 5, 10, 20, 50, 100 }; 

            return _gambleist;
        }


        /// <summary>
        /// 处理庄的最大赔率
        /// </summary> 
        /// <returns></returns>
        public bool CheckGambleMaxLimit( )
        {
            if (_myTable._DicPos2User[_myTable._bankpos]._CurrentGold < _myTable._allGambleTotal * 3) return false;//最大三倍，限制不能出现赔不起的情况
            return true;
        }
         
        /// <summary>
        ///  不同游戏类型结算方法不一样
        /// </summary>
        /// <returns></returns>
        public ShowDownSD100 GetShowDownList(int _pos)
        {
            int _bankerPos = _myTable._bankpos;
            List<CommonPosValListSD> _pos2CardList = new List<CommonPosValListSD>();
            foreach (var pokerlist in _myTable._dicPokerList)
            {
                _pos2CardList.Add(new CommonPosValListSD() { pos = pokerlist.Key, vallist = pokerlist.Value });
            }

            ShowDownSD100 _tempdicpos2SD = new ShowDownSD100()
            {
                bulltype = (int)_myTable._DicPos2User[_pos]._bulltype,
                gamble = _myTable._DicPos2User[_pos]._gambleTotal,
                money = 0,
                pos = _pos,
                _pos2CardList = _pos2CardList
            };

            BullFight100User _tempBankerUser = _myTable._DicPos2User[_bankerPos];
            List<int> _bankerCardList = _myTable._dicPokerList[1];
            PokerBullFightType _bankderType = _myTable._dicPokerBFType[1];
            foreach (int key in _myTable._dicPokerList.Keys)
            {
                if (key == 1) continue;//固定1号位为庄
                bool _tempBankerWin = BullFight.ComparePoker(_bankerCardList, _bankderType, _myTable._dicPokerList[key], _myTable._dicPokerBFType[key]);
                int _gambleRate = 1;
                if (_tempBankerWin)
                {
                    _gambleRate = BullFight._dicbullfightRate[_bankderType];
                }
                else
                {
                    _gambleRate = BullFight._dicbullfightRate[_myTable._dicPokerBFType[key]];
                }

                _myTable.ForeashAllDo((i) =>
                {
                    if (_myTable._masterPos == i) return;//房主不计算
                    if (_myTable._bankpos == i) return;//庄家不计算
                    if (_tempBankerWin)
                    {
                        int _tempvar = _myTable._baseMoney * _myTable._DicPos2User[i].GetGambleByPos(key) * _gambleRate;
                        _myTable._DicPos2User[_bankerPos]._CurrentGold += _tempvar;       //扣出5%费用 只要是赢了的人
                        _myTable._DicPos2User[i]._CurrentGold -= _tempvar;
                    }
                    else
                    {
                        int _tempvar = _myTable._baseMoney * _myTable._DicPos2User[i].GetGambleByPos(key) * _gambleRate;
                        _myTable._DicPos2User[_bankerPos]._CurrentGold -= _tempvar;
                        _myTable._DicPos2User[i]._CurrentGold += _tempvar;//扣出5%费用 只要是赢了的人
                    }
                });
            }

            return _tempdicpos2SD;
        }

       
        /// <summary>
        /// 升庄牛牛，庄是否可以收完所有奖池
        /// </summary>
        /// <returns></returns>
        public bool CheckCanBankerGetBonusPot()
        {
            return false; 
        }
     
      
        public int GetOterGambleOrder(int pos, int targetpos)
        {
            if (pos == targetpos) return 1;
            Dictionary<int, int> _dic = _myTable._DicPos2User[pos]._pos2Gameble;
            if (_dic.Count == 0) return 4;
            int _tempOrder = 4;
            DateTime _currentTime = DateTime.Now;
            TimeSpan _ts1 = new TimeSpan(0);
            _myTable.ForeashAllDo((i) =>
                {
                    if (targetpos == i) return;//本家是固定为1的顺序的
                    TimeSpan _tstemp= _currentTime - _myTable._DicPos2User[i]._gambletime;
                    if (_tstemp.CompareTo(_ts1) > 0)
                    {//表示ts1时间早，_tempPos1Order为2。 
                        _ts1 = _tstemp;
                        if (pos == i)   _tempOrder = 2;
                    }
                }); 
            return _tempOrder;
        }
        /// <summary>
        /// 判断一个人是否赢了这局
        /// </summary>
        /// <param name="leftmoney"></param>
        /// <returns></returns>
        public bool CheckGameOverWin(int leftmoney)
        {
            if (leftmoney >= _baseMoney) return true;
            return false;
        }

        public List<CommonPosValSD> GetCurrentPosGold()
        {
            List<CommonPosValSD> _pos2Gold = new List<CommonPosValSD>();
            _myTable.ForeashAllDo((j) =>
            {
                _pos2Gold.Add( new CommonPosValSD() { pos = j, val = _myTable._DicPos2User[j]._CurrentGold });
            });
            return _pos2Gold;
        }
    }
    /// <summary>
    /// 庄的类型
    /// </summary>
    public enum BFRankerEnum
    {
        /// <summary>
        /// 轮流抢庄 统一一起抢。判定规是位置从小到大。      所有人都放弃，是第一个
        /// </summary>
        TurnSelect=1,             
        /// <summary>
        /// 房主包庄，房主爆分后按上面的规则 找下一个庄
        /// </summary>
        TurnFixed = 2,
        /// <summary>
        /// 四人升庄牛牛
        /// </summary>
        FourBF = 3,    
    }
}
