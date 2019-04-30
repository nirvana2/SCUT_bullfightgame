using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 玩家之不能相互确认的 让裁判来 确定
    /// 1.每两个人进行比牌 
    /// </summary>
    public class TCJudge
    {
        public   int _minLimit = 2;
        public  int _maxLimit = 6;
        private object obj = new object();
        /// <summary>
        ///   游戏类型，1正常的炸金花，2，欢乐三张
        /// </summary>
        public int _gametype;
        /// <summary>
        /// 基础分数：1w,3w,5w;升庄15 15*2, 30 30*2, 60 60*2
        /// </summary>
        private int _baseallmoney;
        public int BaseAllMoney
        {
            get { return _baseallmoney; }
        }
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
        private TCTable _myTable; 
        /// int 是用户的位置 按顺序判断 后面可以进行优先级抢断
        /// 这个只能在当前类，使用， 
        /// 且其他New的类不能使用 因为没有赋值
        /// </summary>
       private ConcurrentDictionary<int, int> DicPosToType;
       private ConcurrentDictionary<int, int> MingGangDicPosToType;
        /// <summary>
        /// 获取最大的  局数或者庄数
        /// </summary>
        public int GetTableorBankerMaxCount
        {
            get
            {
                if (_gametype == 2) return _maxTableOverCount;
                else return _maxTableOverCount;
            }
        }

        public TCJudge(TCTable myTable)
       {
           lock (obj)
           {
               _myTable = myTable;
               DicPosToType = new ConcurrentDictionary<int, int>();
               MingGangDicPosToType = new ConcurrentDictionary<int, int>();
                
           }
       }
        public void InitiArgs(cs_enterroom _data)
        {
            if (_data.rankertype < 1 || _data.rankertype > 2) return;
            if (_data.numpertable < 2 || _data.numpertable > 4) return;
            _gametype = _data.gametype;
            

            _roomcard = _data.roomcard;
            _minLimit = _data.numpertable;
            _maxLimit = _data.numpertable;
            _maxTableOverCount = _data.tableCount;
            _maxTableOverCount = 3;     //测试
            _baseallmoney = 200;//测试数据   
           
            _curTableOverCount = 0;     
        }

        public bool CheckDiamond(int pos)
        {
            if (_myTable._DicPos2User[pos]._tbUser.diamond >= _roomcard) return true;
            return false;
        }
        /// <summary>
        ///  //这一次打牌裁判动作结束
        /// </summary>
        private void Reset()
       {
           lock (obj)
           {               
               DicPosToType = new ConcurrentDictionary<int, int>();
               MingGangDicPosToType = new ConcurrentDictionary<int, int>();
             
           } 
       }
        /// <summary>
        /// 所有人进行比牌，
        /// </summary>
        public void CompareAll()
        {

        }
    }
}
