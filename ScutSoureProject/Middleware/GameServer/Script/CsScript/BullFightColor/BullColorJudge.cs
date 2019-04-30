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
    /// 比大小，计算倍率
    /// 顺序为离庄家近的，
    /// 如果庄家钱不够的情况，后面的闲家就赔不钱了。
    /// </summary>
    public class BullColorJudge
    {
        private object obj = new object();

        #region 变量
        public  int _minLimit = 4;
        public  int _maxLimit = 4;
        private BullColorTable _myTable;
        /// <summary>
        /// int 是用户的位置 按顺序判断 后面可以进行优先级抢断
        /// 这个只能在当前类，使用， 
        /// 且其他New的类不能使用 因为没有赋值
        /// </summary>
        private ConcurrentDictionary<int, int> DicPosToType;

        /// <summary>
        /// 游戏类型 1牛牛时时彩
        /// </summary>
        public int _gametype;
        /// <summary>
        /// 游戏模式，2金币模式
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
        /// 下注次数限制
        /// </summary>
        public int _gambleCountLimit;
        /// <summary>
        /// 抢庄类型BFRankerEnum
        /// </summary>
        public BFRankerEnum _rankertype;
        /// <summary>
        /// 当前限制的局数 
        /// </summary>
        private int _maxTableOverCount;
        /// <summary>
        /// 当前 的局数。
        /// </summary>
        public int _curTableOverCount;
          
        
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
            int mixGold = _baseMoney / 10 / 12 / 3;
            if (mixGold < 500) mixGold = 500;//确定最低分数为500，先强制写死的
            if (currentGold <= mixGold) return true;
            return false; 
        }
        #endregion

        public BullColorJudge(BullColorTable myTable)
        {
            _myTable = myTable;
            DicPosToType = new ConcurrentDictionary<int, int>();
        }
        /// <summary>
        /// 一桌最大支持500个人。
        /// </summary>
        /// <param name="_data"></param>
        public void InitiArgs(cs_enterroom _data)
        {
            if (_data.rankertype < 1 || _data.rankertype > 2) return; 
            _gametype = 2;
            _gameCoin2Room1 = 2;
            _baseMoney = _data.baserate;
            _rankertype = BFRankerEnum.TurnFixed; 
            _minLimit = 1;
            _maxLimit = 500;

            _maxTableOverCount = 1;// _data.tableCount;
            //_baseallmoney = 200;//测试数据    
            _curTableOverCount = 1;
            _gambleCountLimit = 50;//限制50次单次最大的下注次数
        } 
 

        /// <summary>
        /// 包括 升庄牛牛中，庄的下注列表
        /// </summary>
        /// <param name="bankerpos"></param>
        /// <param name="pos"></param>ShowDownSD
        /// <returns></returns>
        public List<int> GetGambleList(int pos)
        {
            List<int> _gambleist = new List<int>() {1000};//一次下注1000，以后服务器可以调

            int bankermoney = (int)_myTable._DicPos2User[0]._tbUser.UserMoney;//就是奖池用户
            
              
            return _gambleist;
        }
       
        /// <summary>
        ///  不同游戏类型结算方法不一样
        /// </summary>
        /// <returns></returns>
        public List<ShowDownSDBFC> GetShowDownList()
        {
            Dictionary<int, ShowDownSDBFC> _tempdicpos2SD = new Dictionary<int, ShowDownSDBFC>();
            int _bankerPos = _myTable._bankpos;
            _myTable.ForeashAllDo((i) =>
            {
                _tempdicpos2SD.Add(i, new ShowDownSDBFC()
                {
                    bulltype = (int)_myTable._DicPos2User[i]._bulltype,
                    gamble = _myTable._DicPos2User[i]._gambleTotal,
                    money = 0,
                    pos = i,
                    _cardlist = _myTable._DicPos2User[i]._shouPaiArr
                });
            });

            BullColorUser _tempBankerUser = _myTable._DicPos2User[_bankerPos];

            _myTable.ForeashAllDo((i) =>
            {
                if (_myTable._DicPos2User[i]._isBanker) return;

                bool _tempBankerWin = BullFight.ComparePoker(_tempBankerUser._shouPaiArr, _tempBankerUser._bulltype, _myTable._DicPos2User[i]._shouPaiArr, _myTable._DicPos2User[i]._bulltype);
                if (_tempBankerWin)
                {
                    int _tempvar = _myTable._baseMoney * _myTable._DicPos2User[i]._gambleTotal * BullFight._dicbullfightRate[_tempBankerUser._bulltype];
                    _tempdicpos2SD[_bankerPos].money += _tempvar;
                    _tempdicpos2SD[i].money -= _tempvar;
                }
                else
                {
                    int _tempvar = _myTable._baseMoney * _myTable._DicPos2User[i]._gambleTotal * BullFight._dicbullfightRate[_myTable._DicPos2User[i]._bulltype];
                    _tempdicpos2SD[_bankerPos].money -= _tempvar;
                    _tempdicpos2SD[i].money += _tempvar;
                }
            });

            //算完后，统一数据持久化       
            foreach (int key in _tempdicpos2SD.Keys)
            {
                if (_tempdicpos2SD[key].money > 0)//扣出5%费用 只要是赢了的人
                    _tempdicpos2SD[key].money = (int) (_tempdicpos2SD[key].money * 0.95f);
                _myTable._DicPos2User[key]._CurrentGold += _tempdicpos2SD[key].money;
            }
            return _tempdicpos2SD.Values.ToList<ShowDownSDBFC>();
        }

        /// <summary>
        /// 通用结算
        /// </summary>
        /// <param name="_tempBankerWin"></param>
        /// <param name="_thisposGamble"></param>
        /// <param name="pos"></param>
        /// <param name="_pointRate"></param>
        /// <param name="_tempdicpos2SD"></param>
        private void Balance(bool _tempBankerWin, int _thisposGamble, int pos, int _pointRate, Dictionary<int, ShowDownSDBFC> _tempdicpos2SD)
        {
            int _bankerPos = _myTable._bankpos;
            int _bankerRate = BullFight._dicbullfightRate[_myTable._DicPos2User[_bankerPos]._bulltype];
            if (_tempBankerWin)
            {
                int _tempvar = _myTable._baseMoney * _thisposGamble * _bankerRate;
                _tempdicpos2SD[_bankerPos].money += _tempvar;
                _tempdicpos2SD[pos].money -= _tempvar;                                              
            }
            else
            {
                int _tempvar = _myTable._baseMoney * _thisposGamble * _pointRate;
            }
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
     
}
