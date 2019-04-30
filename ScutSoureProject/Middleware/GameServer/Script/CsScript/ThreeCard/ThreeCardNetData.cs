using System.Collections.Generic;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 有人进入一桌，推送给这一桌内的所人的
    /// </summary>
    public class sc_entertable_tc_n : sc_base
    {
        public List<OtherUserInfoSD> palyerlist;

    }
    /// <summary>
    /// 申请准备  
    /// </summary>
    public class cs_ready_tc : cs_base
    {
        public int levelid;
        public int tableid;
        public int pos;
    }
    public class sc_ready_tc : sc_base
    {

    }
    /// <summary>
    /// 通知所有人谁准备了
    /// </summary>
    public class sc_ready_tc_n : sc_base
    {
        public int pos;
    }
    /// <summary>
    /// 进入房间后开始推送每人的牌  _n表示是服务器推送的
    /// </summary>
    public class sc_tablestart_tc_n : sc_base
    {
        public int tableid;
        public int pos; 
        public int BankerPos;//表示哪个的庄        
        public List<CommonPosValSD> _user2pos;
        /// <summary>
        /// 表示此次的局号 时间格式编码
        /// </summary>
        public string MatchCode;  
    }
   
    /// <summary>
    /// 移一次token  用户可能有4个操作，，看牌，下注，放弃， 比牌【条件限制】 
    /// </summary>
    public class sc_token_tc_n : sc_base
    { 
        public int alltoken;
        public int allmoney;
        public int pos;
    }

    /// <summary>
    /// 看牌  
    /// </summary>
    public class cs_showcard_tc : cs_base
    {
        public int levelid;
        public int tableid;
    }
    public class sc_showcard_tc : sc_base
    { 
        public List<int> shoupai;
    }
    /// <summary>
    /// 通知书其他玩家，此人处于看的状态了，
    /// </summary>
    public class sc_showcard_tc_n : sc_base
    { 
        public int pos;
    }

    /// <summary>
    /// 下注
    /// </summary>
    public class cs_gamble_tc : cs_base
    {
        public int levelid;
        public int tableid;
        public int money;
        /// <summary>
        /// 加了倍没？
        /// </summary>
        public bool addrate;
    }
    public class sc_gamble_tc : sc_base
    {
        
    }
    /// <summary>
    /// 通知所有人下注成功
    /// </summary>
    public class sc_gamble_tc_n : sc_base
    {
         
        public int pos;
        public int money;
        /// <summary>
        /// 钱池
        /// </summary>
        public int allmoney;
        public bool addrate;
        /// <summary>
        /// 回合数
        /// </summary>
        public int allturn;
    }
    /// <summary>
    /// 比牌
    /// </summary>
    public class cs_compare_tc : cs_base
    {
        public int levelid;
        public int tableid;
        public int targetpos;
    }
    public class sc_compare_tc : sc_base
    {
        
        public bool win;
    }
    /// <summary>
    /// 比牌通知所有人
    /// </summary>
    public class sc_compare_tc_n : sc_base
    { 
        /// <summary>
        /// 比输了的人的位置 
        /// </summary>
        public int failpos;        
    }
    /// <summary>
    /// 弃牌
    /// </summary>
    public class cs_giveup_tc : cs_base
    {
        public int levelid;
        public int tableid;
        public int pos;
    }
    public class sc_giveup_tc : sc_base
    {
        public List<int> _shoupai;
    }
    /// <summary>
    /// 通知所有人，弃牌状态  
    /// </summary>
    public class sc_giveup_tc_n : sc_base
    {
        public int pos;
    }


    /// <summary>
    /// 结算 通知所有人
    /// </summary>
    public class sc_end_tc_n : sc_base
    {       
        /// <summary>
        /// 
        /// </summary>
        public List<CommonPosValSD> endMoneylist;
        /// <summary>
        /// 赢家的牌
        /// </summary>
        public List<int> winCard;
        /// <summary>
        /// 所有的钱
        /// </summary>
        public int allmoney;
        /// <summary>
        /// 喜钱， 顺金5倍，豹子10倍
        /// </summary>
        public int EasterEgg;
    }
}
