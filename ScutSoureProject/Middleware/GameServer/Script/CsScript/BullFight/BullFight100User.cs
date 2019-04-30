using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
 
using System.Threading;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 虚拟 用户/角色/玩家
    /// </summary>
    public class BullFight100User : BaseUser
    {
        public BullFight100User()
        {
            _enterTime = DateTime.Now;
            _gambletime = DateTime.Now.AddYears(100);                 
            _isPlaying = false;
            _isRobot = false;
            _gambleTotal = 0;
        }
     
        /// <summary>
        /// 默认不抢庄
        /// </summary>
        public bool _isGetBanker = false;
        /// <summary>
        /// 是否庄家
        /// </summary>
        public bool _isBanker = false;

        /// <summary>
        ///  当前用户的所有下注值 ，用于显示 
        /// </summary>
        public int _gambleTotal;

        /// <summary>
        /// 百人牛牛的下注列表
        /// </summary>
        public Dictionary<int, int> _pos2Gameble;
    
        /// <summary>
        /// 用于中间显示过程中的牌
        /// </summary>
        public List<int> _showCardList = new List<int>();
        /// <summary>
        /// 暂时不使用 如果是机器人， 该对象要赋值 不是，可以null
        /// </summary>
        public BullFight100Robot _robot;
        /// <summary>
        /// 牛的类型
        /// </summary>
        public PokerBullFightType _bulltype;
        /// <summary>
        /// 仅用于外围下注，先后赔付。
        /// </summary>
        public DateTime _gambletime;
        /// <summary>
        /// 可以的下注列表
        /// </summary>
        public List<int> gamblelist;

        public override void ResetBase()
        {
            _isGetBanker = false;   
            _isBanker = false;       
            _showCardList = new List<int>();
            _bulltype = PokerBullFightType.Bull_No;
            _pos2Gameble = new Dictionary<int, int>();
            _gambletime = DateTime.Now.AddYears(100);
            base.ResetBase();
        }
        public void AddorUpdateGamble(int _targetpos, int _gamble)
        {
            _gambleTotal += _gamble;
            if (_pos2Gameble.ContainsKey(_targetpos))  _pos2Gameble[_targetpos] += _gamble;
            else _pos2Gameble.Add(_targetpos, _gamble);
        }
         
        /// <summary>
        /// 获取指定pos 的下注值 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int GetGambleByPos(int pos)
        {
            if (_pos2Gameble == null) return 0;

            foreach (var key in _pos2Gameble.Keys)
            {
                if (key == pos) return _pos2Gameble[key];
            }
            return 0;
        }
    }
}
