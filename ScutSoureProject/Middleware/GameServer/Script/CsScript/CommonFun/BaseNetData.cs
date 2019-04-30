using System; 
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common;
using ZyGames.Framework.Game.Contract.Action;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Common.Serialization;
using System.Collections.Generic;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Net;
using ZyGames.Framework.RPC.Sockets;
using GameServer.Script.Model;

namespace GameServer.Script.CsScript.Action
{
    /*
    * 基础的通信结构定义类 须放置这个文件中
    */
                   
    public class cs_base
    {
        /// <summary>
        /// 函数名
        /// </summary>
        public string fn = "";
        /// <summary>
        /// 1表示需要处理，为断线后重新绑定Session用        没用走cs_base的不会有断线重连状态处理 
        /// </summary>
        public int _userid;
        /// <summary>
        /// 
        /// </summary>
        public int cc = 0;
    }    
    public class sc_base
    {
        /// <summary>
        /// 函数名
        /// </summary>
        public string fn = "";       
        public int _msgid;
        public int cc = 0;
        /// <summary>
        /// 1.成功 0失败 -1具体原因。
        /// </summary>
        public int result;
        /// <summary>
        /// 有条件选择的，需要客户根据条件处理下一次请教用的 默认为 false
        /// </summary>
        public bool closefun;
    }
    public class sc_exit_rebindsession_n : sc_base
    {

    }
    public class cs_ping : cs_base
    {
        
    }
    public class sc_ping : sc_base
    {
        public int fps;   
    }

    /// <summary>
    /// 登录
    /// </summary>
    public class cs_login : cs_base
    {
        public string accountId;
        public string mName;
        public string mPwd;
        public float lat;
        public float lng;
    }
    /// <summary>
    /// 登录返回
    /// </summary>
    public class sc_login : sc_base
    {
        public int gameid;
        public PlayerInfoSD user;
    }

    public class cs_relogin : cs_base
    {
        public string accountId;
    }       
  
    /// <summary>
    /// 玩家的所有拥有数据
    /// </summary>
    public class PlayerInfoSD
    {
        /// <summary>
        /// Session的ID 非自增长的
        /// </summary>
        public int userid;
        public string uName;//玩家名字
        public string accountId;//玩家账号id
        public int SignInCount;//玩家连续登陆天数     
         
        public float Money;        //金币
        /// <summary>
        /// 钻石
        /// </summary>
        public float Diamond;
        public Int64 lastLoginTime;//上次发送消息时间
        public int vipLevel;//玩家vip等级 
        public int level;//玩家等级	 
        /// <summary>
        /// 玩家的游戏状态，默认为0   InLobby = 1, InRoom = 2, InTableDaiPai = 3, InTableDis = 4
        /// </summary>
        public int state;
        /// <summary>
        /// 1表示 一级代理 
        /// </summary>
        public int isagent;
        public WechatInfoSD _wechat;
    }
    public class WechatInfoSD
    {
        /// <summary>
        /// 微信昵称
        /// </summary>
        public string wechatName;
        /// <summary>
        /// 微信头像ICON
        /// </summary>
        public string HeadIconURL;
        /// <summary>
        /// 性别
        /// </summary>
        public int Sex;
    }          
      
    public class cs_getgamelist : cs_base
    {
        public string accountId;
    }
    public class sc_getgamelist : sc_base
    {
         
        public List<GameInfoSD> gamelist;
    }
    public class GameInfoSD
    {
        public int id;

        public string name;

        public string desc;

        public int currLimit;

        public float Money;   
        //1表示打开，               
        public int _isopen;
        public int level;
    }
    public class cs_getgamelevel : cs_base
    {
        public int gameid;
    }

    public class sc_getgamelevel : sc_base
    {
        
        public List<RoomInfoSD> levellist;
    }

    public class RoomInfoSD
    {
        public int id;
        /// <summary>
        /// ChessGameModel的id
        /// </summary>
        public int gameid;
        /// <summary>
        /// 此游戏的在线人线
        /// </summary>
        public int onlineCount;
        /// <summary>
        /// 游戏类型，1经典牛牛，2疯狂牛牛
        /// </summary>
        public int gametype;
        /// <summary>
        /// 初，中，高级场
        /// </summary>
        public string name;
        /// <summary>
        /// 底分
        /// </summary>
        public int baserate;

        /// <summary>
        /// 最低
        /// </summary>
        public int _min;
        /// <summary>
        /// 最高限制 
        /// </summary>
        public int _max;
    }
    public class cs_freshplayerInfoSD : cs_base
    {
    }
    public class sc_freshplayerInfoSD : sc_base
    {
        /// <summary>
        /// 代理ID
        /// </summary>
        public int AgentId { get; set; }
        /// <summary>
        /// 代理名称
        /// </summary>
        public string  AgentName { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public PlayerInfoSD user;
    }
    //开房模式的结构
    public class cs_createtable : cs_base
    {                                
        public int gameid;
        /// <summary>
        /// 1能比牛牛，2四人地方牛牛
        /// </summary>
        public int gametype;
        /// <summary>
        /// 15~30 或 30~60 60~90
        /// 开的房间的游戏模式，如成都麻将 可以选择不没有封顶方式 
        /// </summary>
        public int gameModel;
    }

    public class sc_createtable : sc_base
    {
        public int playerCount;
        public int leftroomcard;
        public int levelid;
        public int tableid;
        /// <summary>
        /// 房号
        /// </summary>
        public int roomNum;
    }

    /// <summary>
    /// 输入房号进入房间
    /// </summary>
    public class cs_enterroomtable : cs_base
    {
        public int gameid;
        public int levelid;
        public string tablenum;
    }
    /// <summary>
    /// 只获得桌子号，需要再申请协议处理，，，协议顺序不对的一个补丁
    /// </summary>
    public class sc_enterroomtable : sc_base
    {
        public int tableid; 
        /// <summary>
        /// 2,3,4;升庄固定为4人。
        /// </summary>
        public int numpertable;
        /// <summary>
        /// 1通比牛牛，2，升庄四人牛牛
        /// </summary>
        public int gametype;
        /// <summary>
        /// 进入桌子添加房间模式 1 为房间模式，2为金币模式
        /// </summary>
        public int gameModel;
        /// <summary>
        /// 房间等级
        /// </summary>
        public int levelid;
        /// <summary>
        /// 排队个数
        /// </summary>
        public int waitcount;
    }
    /// <summary>
    /// 进入房间，四个游戏走同样的接口
    /// </summary>
    public class cs_enterroom:cs_base
    {                             
        public int gameid;
        public int levelid; 
        /// <summary>
        /// 游戏模式，1房卡模式，2金币模式
        /// </summary>
        public int gamemodel;
        /// <summary>
        /// 游戏类型，1经典牛牛，2疯狂牛牛
        /// </summary>
        public int gametype;
        /// <summary>
        /// 2,3,4;升庄固定为4人。
        /// </summary>
        public int numpertable; 
        /// <summary>        
        /// 轮流抢庄        TurnSelect=1,
        /// 随机抢庄        RandomSelect = 2,
        /// 固定轮庄        Turn = 3,
        /// 轮庄，可放弃    TurnGiveUp = 4, 
        /// </summary>
        public int rankertype;

        /// <summary>
        /// 消耗一张房卡，还是两张
        /// </summary>
        public int roomcard;

        /// <summary>
        /// 当前限制的局数，升庄是庄数。
        /// </summary>
        public int tableCount;
        /// <summary>
        /// 最低的底注
        /// </summary>
        public int baserate;

    }
    public class sc_enterroom : sc_base
    {
        public int gameid;
        public int levelid;
        /// <summary>
        /// 排队个数
        /// </summary>
        public int waitcount;
        public int gamemodel;
        public int numpertable;
    }
    /// <summary>
    /// <summary>
    /// 断线后居上进入房间 
    /// </summary>
    public class cs_reenterroom : cs_base
    {

    }
    /// <summary>
    /// 反回数据最大容器，
    /// </summary>
    public class sc_reenterroom : sc_base
    {
        public int gameid;
        public int levelid;
        public int tableid;
        /// <summary>
        /// 初始允许的最大人数
        /// </summary>
        public int _numpertable;
        /// <summary>
        /// 进入人消息，优先处理，并且大结算前不会清理 
        /// </summary>
        public string _tableEnterSendData;
        public List<string> _tableSendData;
        /// <summary>
        /// 当前所有人的金币，前面协议的是最初始的了
        /// </summary>
        public List<CommonPosValSD> _pos2Gold;
        /// <summary>
        /// 升庄牛牛的奖池 庄比牛牛为0
        /// </summary>
        public int _bonusPot;
        /// <summary>
        ///       是否已开始了游戏
        /// </summary>
        public bool _isStarted;
        /// <summary>
        /// 1.2牛牛类型
        /// </summary>
        public int gametype;
        /// <summary>
        /// 房间模式 1 是房间模式 2 是金币模式
        /// </summary>
        public int gameModel;
    }

    public class OtherUserInfoSD
    {         
        public PlayerInfoSD otherpalyer;
        public int pos;
        /// <summary>
        /// 是否掉线了       1    已掉线
        /// </summary>
        public int _isDisconnet;
        /// <summary>
        /// 是否已准备      1     已准备
        /// </summary>
        public int _isReady;
    }
    /// <summary>
    /// 退出房间，四个游戏走同样的接口
    /// </summary>
    public class cs_exitroom : cs_base
    {
        public int gameid;
        public int levelid;
    }
    public class sc_exitroom : sc_base
    {
          
    }
    public class sc_exitroom_n : sc_base
    {

    }

    public class cs_getnotice : cs_base
    {
        /// <summary>
        /// 获取公告的最新条数
        /// </summary>
        public int Count;
    }
    public class sc_getnotice : sc_base
    {          
        public List<string> noticelist;
    }
    public class sc_getnotice_n : sc_base
    {           
        public List<string> noticelist;
    }
    /// <summary>
    /// 聊天功能             只有在游戏中，的具体一桌才能发消息
    /// </summary>
    public class cs_chat : cs_base
    {
        public int gameid;
        public int levelid;
        public int tableid; 
        /// <summary>
        /// 要发送的聊天内容
        /// </summary>
        public string content;
        /// <summary>
        /// 1语音，2表情，3文本
        /// </summary>
        public int type;
    }
    public class sc_chat : sc_base
    {
       
    }
    public class sc_chat_n : sc_base
    {       
        public int pos;
        /// <summary>
        /// 要发送的聊天内容
        /// </summary>
        public string content;
        /// <summary>
        /// 1语音，2表情，3文本
        /// </summary>
        public int type;
        public int gameid;
    }       
  
    /// <summary>
    /// 通用结构 ，pos 对应一个值 （rate, que, ready ）
    /// </summary>
    public class CommonPosValSD
    {
        public int pos;
        public int val;
    }
    /// <summary>
    /// 通用结构 ，pos 对应一个值 shoupai
    /// </summary>
    public class CommonPosValListSD
    {
        public int pos;
        public List<int> vallist;
    }
    /// <summary>
    /// 通知进入桌 可以进行准备操作了，8秒不操作自动准备
    /// </summary>
    public class sc_entertable_n : sc_base
    {
        public int gameid;
        public int tableid;
        public int levelid;
        public int pos;
        /// <summary>
        /// 1通比牛牛，2，升庄四人牛牛
        /// </summary>
        public int gametype;             
        /// <summary>
        /// 表示此次的局号 时间格式编码
        /// </summary>
        public string MatchCode;
        /// <summary>
        /// 房卡限制的最大数，局数或庄数
        /// </summary>
        public int maxCount;
        public List<OtherUserInfoSD> palyerlist;
    }          

    /// <summary>
    /// 请求离开桌子，申请解散游戏
    /// </summary>
    public class cs_applyexittable : cs_base
    {
        public int gameid;
        public int levelid;
        public int tableid;
        /// <summary>
        /// 申请位置
        /// </summary>
        public int pos;         
    }
    public class sc_applyexittable : sc_base
    {                        
    }
    /// <summary>
    /// 通知所有人申请离开桌子
    /// </summary>
    public class sc_applyexittable_n : sc_base
    {
        public int gameid;
        /// <summary>
        /// 申请位置
        /// </summary>
        public int pos;
    }
    /// <summary>
    /// 处理请求离开桌子，  处理别人的申请解散，可同意与不同意
    /// </summary>
    public class cs_dealexittable : cs_base
    {
        public int gameid;
        public int levelid;
        public int tableid;        
        /// <summary>
        /// 1，表示同意解散。 0，表示不同意
        /// </summary>
        public int agree;
    }
    public class sc_dealexittable : sc_base
    {
        public int gameid;
        public int pos;
        /// <summary>
        /// 1，表示同意解散。 0，表示不同意
        /// </summary>
        public int agree;
    }
    /// <summary>
    /// 通知其他人，处理人的结果 
    /// </summary>
    public class sc_dealexittable_n : sc_base
    {
        public int pos;
        /// <summary>
        /// 1，表示同意解散。 0，表示不同意
        /// </summary>
        public int agree;
    }

    /// <summary>
    /// 通知有人离开桌子了，可能是自己人离开，可能是被服务器规则T出
    /// </summary>
    public class sc_exittable_n : sc_base
    {
        public int gameid;        
        /// <summary>
        ///  不同意的列表 
        /// </summary>
        public List<int> disagree;
        /// <summary>
        /// 还没有开始的话，没有扣出房卡的时候就直接退出，开始后需要显示结算面板
        /// </summary>
        public bool _showResult;
    }
    /// <summary>
    /// 通知有人离开桌子了，可能是自己人离开，可能是被服务器规则T出
    /// </summary>
    public class sc_one_exittable_n : sc_base
    {
        public int gameid;              
        public int userid;
        /// <summary>
        /// 客户处理 如果是自己就退出到大厅
        /// </summary>
        public int pos;
    }
    /// <summary>
    /// 警告 同IP，或同位置，GPS计算
    /// </summary> 
    public class sc_warning_n : sc_base
    {
        /// <summary>
        /// 1同IP，2同位置 
        /// </summary>
        public int type;
        public int gameid;

        /// <summary>
        /// 要发送的内容
        /// </summary>
        public string content;
    }
    /// <summary>
    /// 游戏中掉线通知，
    /// </summary>
    public class sc_disconnect_n : sc_base
    {
        public int gameid;
        public int levelid;
        public int tableid;
        public int pos;
        /// <summary>
        /// 1表示 又重新连接上了
        /// </summary>
        public int reconnect;
    }


    public class cs_gm_chesscard : cs_base
    {
        public int gmcode;
        /// <summary>
        /// 指定用户
        /// </summary>
        public int userid;
        /// <summary>
        /// 设置指定的钱
        /// </summary>
        public int money;
    }

    public class sc_gm_chesscard : sc_base
    {
        
    }
    #region    大厅相关功能，排行榜，战绩， 反馈

    public class cs_getranklist : cs_base
    {
        public int gameid;
        /// <summary>
        /// 暂时不用了
        /// </summary>
        public int _onlymine;
    }
    public class sc_getranklist : sc_base
    {
        public List<RankInfoSD> _ranklist;
        /// <summary>
        /// 暂时不用了
        /// </summary>
        public int _onlymine;
    }
    /// <summary>
    ///  
    /// </summary>
    public class RankInfoSD
    {                       
        public int userid;
        public string uName;  
        public int winScore;
        public int failScore;     
        public int rank;   //  名次   
        public string headurl;
    }

    /// <summary>
    /// 我的战绩
    /// </summary>     
    public class cs_getcombatgainlist : cs_base
    {
        public int gameid;
        /// <summary>
        /// 1表示仅仅我的战绩
        /// </summary>
        public int _onlymine;
    }
        
    public class sc_getcombatgainlist : sc_base
    {
        public int gameid;
        /// <summary>
        /// 按时间倒序排序
        /// </summary>
        public List<CombatGainInfoSD> _ranklist;
    }

    /// <summary>
    ///   我的战绩结构 
    /// </summary>      
    public class CombatGainInfoSD
    {
        public int tablenum;
        public string _starttime;
        public List<CombatTableRecordSD> _tableRecord;
    }
    /// <summary>
    ///   战绩 中参战人员结构 
    /// </summary>       
    public class CombatTableRecordSD
    {
        public int userid;
        public string _username;
        public int _winorlost;
    }

    /// <summary>
    /// 反馈
    /// </summary>     
    public class cs_feedback : cs_base
    {
        public int gameid;
        public int _type;
        public string _tel;
        public string _content;
    }
    /// <summary>
    /// 反馈
    /// </summary>   
    public class sc_feedback : sc_base
    {

    }
    #endregion
    #region
    /// <summary>
    ///  索要赠送请求
    /// </summary>
    [Serializable]
    public class cs_askmoneytrading : cs_base
    {
        public int objuserid;      //目标用户
        public float Money;        //交易金币
        public bool IsGet;  //true为索要，false为增送
    }
    /// <summary>
    /// 索要赠送请求返回 /result 1:可以扣款，2 余额不足不能扣款,-1 用户账号不存在，-2给自己赠送钱，-3账号异常（作弊号不能提现）,-4用户拒绝接收金币,-5用户不在线
    /// </summary>
    [Serializable]
    public class sc_askmoneytrading : sc_base
    {
    }
    /// <summary>
    /// 索要赠送服务器主动推送消息//result 1:可以扣款，2 余额不足不能扣款,-1 用户账号不存在，-2给自己赠送钱，-3账号异常（作弊号不能提现）,-4用户拒绝接收金币,-5用户不在线
    /// </summary>
    [Serializable]
    public class sc_askmoneytrading_n : sc_base
    {
        public int objuserid;      //目标用户
        public string objusername;  //目标用户昵称
        public float Money;        //交易金币
        public bool IsGet;  //true为索要，false为增送   
    }
    /// <summary>
    /// 确认接收赠送金币
    /// </summary>
    [Serializable]
    public class cs_ensuremoneytrading : cs_base
    {
        public int objuserid;      //目标用户
        public float Money;        //交易金币
        /// <summary>
        /// 接收或者拒绝
        /// </summary>
        public bool YesOrNo; 
    }

    /// <summary>
    /// 确认接收赠送金币返回 //result 1:可以扣款，2 余额不足不能扣款,-1 用户账号不存在，-2给自己赠送钱，-3账号异常（作弊号不能提现）,-4用户拒绝接收金币,-5用户不在线
    /// </summary>
    [Serializable]
    public class sc_ensuremoneytrading : sc_base
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        public string Msg { get; set; }
    }
    /// <summary>
    /// 确认接收赠送金币推送返回 //result 1:可以扣款，2 余额不足不能扣款,-1 用户账号不存在，-2给自己赠送钱，-3账号异常（作弊号不能提现）,-4用户拒绝接收金币,-5用户不在线
    /// </summary>
    [Serializable]
    public class sc_ensuremoneytrading_n : sc_base
    {
        public int objuserid;      //对方用户
        public string objusername;  //对方用户昵称
        public float Money;        //交易金币
    }
    #endregion

}
