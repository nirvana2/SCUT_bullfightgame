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
    public class BullColorUser : BaseUser
    {
        public BullColorUser()
        {
            _enterTime = DateTime.Now;
            _gambletime = DateTime.Now.AddYears(100);                 
            _isPlaying = false;
            _isRobot = false;
            _gambleTotal = 0;
        }
     
        /// <summary>
        /// 是否庄家
        /// </summary>
        public bool _isBanker = false;

        /// <summary>
        ///  牛牛的下注
        /// </summary>
        public int _gambleTotal;
         

        /// <summary>
        /// 当前手牌 5张 前3张自动翻开 
        /// </summary>
        public List<int> _shouPaiArr = new List<int>(); 
        /// <summary>
        /// 暂时不使用 如果是机器人， 该对象要赋值 不是，可以null
        /// </summary>
        public BullColorRobot _robot;
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
        /// <summary>
        /// 下注次数
        /// </summary>
        public int _gambleCount;

        public override void ResetBase()
        {   
            _isBanker = false;      
            _shouPaiArr = new List<int>(); 
            _bulltype = PokerBullFightType.Bull_No; 
            _gambletime = DateTime.Now.AddYears(100);
            base.ResetBase();
        }
      
       
    }
}
