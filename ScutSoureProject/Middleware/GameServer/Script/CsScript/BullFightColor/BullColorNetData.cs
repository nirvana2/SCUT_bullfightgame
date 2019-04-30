using System;
using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 输入房号进入的玩家需再次申请 补丁
    /// </summary>
    public class cs_entertable_bfc : cs_base
    {
        public int gameid;
        public int levelid;
        public int tableid;   
    }
    /// <summary>
    ///  
    /// </summary>
    public class sc_entertable_bfc : sc_base
    {                                          
    }                     
        
    /// <summary>
    /// 申请准备  
    /// </summary>
    public class cs_ready_bfc : cs_base
    {
        public int levelid;
        public int tableid;
        public int pos;
    }
    public class sc_ready_bfc : sc_base
    {
       
    }
    /// <summary>
    /// 通知所有人谁准备了
    /// </summary>
    public class sc_ready_bfc_n : sc_base
    {                  
        public int pos;
    }

    /// <summary>
    /// 进入房间后开始推送每人的牌  _n表示是服务器推送的
    /// </summary>
    public class sc_tablestart_bfc_n : sc_base
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
    /// 下注  1次
    /// </summary>
    public class cs_gambleone_bfc : cs_base
    {
        public int levelid;
        public int tableid;             
        /// <summary>
        /// 下注的位置，如果 是下在自己的位置是，两pos上相同
        /// </summary>
        public int targetpos;
        public int rate;
        /// <summary>
        /// 升庄牛牛的筹码本地坐标
        /// </summary>
        public int lx;
        /// <summary>
        /// 升庄牛牛的筹码本地坐标
        /// </summary>
        public int ly;
    }
    public class sc_gambleone_bfc : sc_base
    {               
        /// <summary>
        /// 升庄牛牛的筹码本地坐标
        /// </summary>
        public int lx;
        /// <summary>
        /// 升庄牛牛的筹码本地坐标
        /// </summary>
        public int ly;

    }
    /// <summary>
    /// 完成下注1次操作
    /// </summary>
    public class sc_gambleone_bfc_n : sc_base
    {
        /// <summary>
        /// 谁下的注
        /// </summary>
        public int pos;
        /// <summary>
        /// 下注的位置，如果 是下在自己的位置是，两pos上相同
        /// </summary>
        public int targetpos;
        public int rate;
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
        public List<CommonPosValListSD> _pos2Gamble_order; 
        /// <summary>
        /// 升庄牛牛的筹码本地坐标
        /// </summary>
        public int lx;
        /// <summary>
        /// 升庄牛牛的筹码本地坐标
        /// </summary>
        public int ly;
    } 

    /// <summary>
    /// 下注完后，裁判按顺序摊牌 一个一个摊牌，通知所有人  客户端按列表的顺序处理开牌表现
    /// </summary>
    public class sc_showdown_bfc_n : sc_base
    {                                  
        /// <summary>
        /// 奖池中量
        /// </summary>
        public int _bonusPot;
        /// <summary>
        /// 5张牌
        /// </summary>
        public List<int> _cardlist;
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

    /// <summary>
    /// 摊牌结算
    /// </summary>
    public class ShowDownSDBFC
    {
        /// <summary>
        /// 位置 
        /// </summary>
        public int pos;
        /// <summary>
        /// 些位置的手牌
        /// </summary>
        public List<int> _cardlist;
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
  

    public class sc_end_bfc_n : sc_base
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
        /// <summary>
        /// 1倍赔率的数据 
        /// </summary>
        public List<CommonPosValSD> _pos2Rate1;
        /// <summary>
        /// 2倍赔率的数据 
        /// </summary>
        public List<CommonPosValSD> _pos2Rate2;
        /// <summary>
        /// 3倍赔率的数据 
        /// </summary>
        public List<CommonPosValSD> _pos2Rate3;
        /// <summary>
        /// 4倍赔率的数据 
        /// </summary>
        public List<CommonPosValSD> _pos2Rate4;
    }

    /// <summary>
    /// 请求在线列表
    /// </summary>
    public class cs_getonline_bfc : cs_base
    {
        public int levelid;
        public int tableid;
        public int Count;
    }

    public class sc_getonline_bfc : sc_base
    { 

    }
    /// <summary>
    /// 请求走势图
    /// </summary>
    public class cs_gethistorycolor_bfc : cs_base
    {
        public int levelid;
        public int tableid;
        public int Count;
    }
    public class sc_gethistorycolor_bfc : sc_base
    {

    }

    /// <summary>
    /// 请求个人下注记录
    /// </summary>
    public class cs_gethistorygamble_bfc : cs_base
    {
        public int levelid;
        public int tableid;
        public int Count;
    }
    public class sc_gethistorygamble_bfc : sc_base
    {

    }
}
