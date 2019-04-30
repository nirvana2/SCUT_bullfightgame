using System;
using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 输入房号进入的玩家需再次申请 补丁
    /// </summary>
    public class cs_entertable_bf100 : cs_base
    {
        public int gameid;
        public int levelid;
        public int tableid;   
    }
    /// <summary>
    ///  
    /// </summary>
    public class sc_entertable_bf100 : sc_base
    {                                          
    }              
     

    /// <summary>
    /// 进入房间后开始推送每人的牌  _n表示是服务器推送的
    /// </summary>
    public class sc_tablestart_bf100_n : sc_base
    {
        public int tableid;
        public int pos;
        /// <summary>
        /// 当前第几局
        /// </summary>
        public int _curTableCount;
        /// <summary>
        /// 当前第几庄
        /// </summary>
        public int _curBankerCount;
        /// <summary>
        /// 上局的奖池金额
        /// </summary>
        public int _lastbonusPot;
        /// <summary>
        /// 表示这个       pos能否抢庄不
        /// </summary>
        public bool _canGetBanker;
        public List<CommonPosValSD> _pos2userid;        
        public List<int> ShowCardList;//需要明的牌， 
    }


    /// <summary>
    /// 申请抢庄  
    /// </summary>
    public class cs_applybanker_bf100 : cs_base
    {
        public int levelid;
        public int tableid;
        public int pos; 
    }
    public class sc_applybanker_bf100 : sc_base
    {

    }
    /// <summary>
    /// 有可能钱不够，申请失败
    /// </summary>
    public class sc_applybanker_bf100_n : sc_base
    {                  
        
    }

    /// <summary>
    /// 完成抢庄操作
    /// </summary>
    public class sc_getbankerone_bf100_n : sc_base
    {
        public int pos;
        public int bonuspot;
        public string name;
        public string UserID;
    }   

    /// <summary>
    /// 下注  1次
    /// </summary>
    public class cs_gambleone_bf100 : cs_base
    {
        public int levelid;
        public int tableid;             
        /// <summary>
        /// 下注的位置 .2.3.4
        /// </summary>
        public int targetpos;
        public int gamble; 
    }
    public class sc_gambleone_bf100 : sc_base
    {
        
    }
    /// <summary>
    /// 完成下注1次操作
    /// </summary>
    public class sc_gambleone_bf100_n : sc_base
    {
        /// <summary>
        /// 谁下的注
        /// </summary>
        public int pos;
        /// <summary>
        /// 下的总注返回 
        /// </summary>
        public int allrate;
        /// <summary>
        /// 当前用户的金币
        /// </summary>
        public int _curGold;
        /// <summary>
        /// 上面POS在其位置上的下注的列表
        /// </summary>
        public List<CommonPosValSD> _pos2Gamble;
        /// <summary>
        /// 当前所有位置下注总金额
        /// </summary>
        public List<CommonPosValSD> _pos2GambleTotal;

    }


    [Serializable]
    public class GambleinfoSD
    {
        public int pos;
        public int rate;
    } 

    /// <summary>
    /// 下注完后，裁判按顺序摊牌 一个一个摊牌，通知所有人  客户端按列表的顺序处理开牌表现
    /// </summary>
    public class sc_showdown_bf100_n : sc_base
    {                                  
        /// <summary>
        /// 奖池中量
        /// </summary>
        public int _bonusPot;
        public ShowDownSD100 sdlist;
        public int _curGold;
    }

    /// <summary>
    /// 摊牌结算
    /// </summary>
    public class ShowDownSD100
    {
        /// <summary>
        /// 位置 
        /// </summary>
        public int pos;
        public List<CommonPosValListSD> _pos2CardList; 
        /// <summary>
        ///  对应 PokerBullFightType的值
        /// </summary>
        public int bulltype;
        /// <summary>
        /// 赢或输的钱，
        /// </summary>
        public int money;

        public int gamble;
    }            
  

    public class sc_end_bf100_n : sc_base
    { 
        /// <summary>
        /// 游戏类型
        /// </summary>
        public int gamemodel; 
        /// <summary>
        /// 1表示结算了，整个房局结算
        /// </summary>
        public int _OverTable;

        /// <summary>
        /// 房主
        /// </summary>
        public int createpos;

        /// <summary>
        /// 结算金币 
        /// </summary>
        public List<CommonPosValSD> _pos2Gold;
        /// <summary>
        /// 观众状态数据 
        /// </summary>
        public List<CommonPosValSD> _pos2Watch; 
    }
    /// <summary>
    /// 庄家赢了X次了，选择下庄了
    /// </summary>
    public class cs_bankergetbonuspot_bf100 : cs_base
    {
        public int levelid;
        public int tableid;
        /// <summary>
        /// 是否全下庄
        /// </summary>
        public bool _isgetall; 
    }

    /// <summary>
    /// 庄家赢了三次了，选择下庄了
    /// </summary>
    public class sc_bankergetbonuspot_bf100 : sc_base
    {

    }
    /// <summary>
    /// 庄家赢了三次了，选择下庄成功通知所有人
    /// </summary>
    public class sc_bankergetbonuspot_bf100_n : sc_base
    { 
        /// <summary>
        /// 上一次庄中的奖池
        /// </summary>
        public int bonus_pot; 
        /// <summary>
        /// true 表示收庄 收钱
        /// </summary>
        public bool _isGetBonuspot;
    }
}
