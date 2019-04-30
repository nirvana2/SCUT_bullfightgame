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
    public class TCUser   : BaseUser 
    {
        public TCUser()
        { }
       
        public bool _isBanker = false;//是否是庄
        /// <summary>
        /// 是否看了牌  关系到出钱的多少
        /// </summary>
        public bool _isShowCard;            

        /// <summary>
        /// 标识是否结束 自己弃牌与比牌失败后为ture
        /// </summary>
        public bool _isgiveup = false;

        /// <summary>
        /// 当前手牌 
        /// </summary>
        public List<int> _shouPaiArr = new List<int>();      
        /// <summary>
        /// 暂时不使用 如果是机器人， 该对象要赋值 不是，可以null
        /// </summary>
        public TCRobot _robot;

        public int _tempMoney;
        public int _myTurn;
        //属性                             
    }
}
