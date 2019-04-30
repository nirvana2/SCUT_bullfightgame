using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using ZyGames.Framework.Common.Serialization;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 机器人：
    /// 做简单的AI判断 
    /// 下注，一直下注。确定AI值 
    /// </summary>
    public class TCRobot
    { 
        /// <summary>
        /// 机器人类型  
        /// </summary>
        public static int Type;
        /// <summary>
        /// 对应 Type的值 如 1->10 表示吃10%的钱
        /// 3->10 表示 输10%的钱
        /// </summary>
        public static int AIValue;
      
        /// <summary>
        /// 设置的机器 人列表
        /// </summary>
        //private static List<int> robotIDlist = new List<int>();
        private static ConcurrentQueue<int> robotIDlist = new ConcurrentQueue<int>();

        /// <summary>
        /// 全局集合 存放所有参加的机器人 
        /// 必须注意 移出
        /// </summary>
        private static ConcurrentDictionary<int, TCUser> DicIDtoUser;
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserID"></param>
        public static void DelDicIDtoUser(int UserID)
        {
            TCUser tempu = null;
            if (!DicIDtoUser.TryRemove(UserID, out tempu))
            {                                               
                ErrorRecord.Record("201208011747TC 清除出局数据失败");
            }
        }
        /// <summary>
        /// 添加机器人到列表
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="u"></param>
        public static void AddtoDicIDtoUser(int UserID, TCUser u)
        {

            if (DicIDtoUser == null)
            {
                DicIDtoUser = new ConcurrentDictionary<int, TCUser>();
            }
            if (!DicIDtoUser.TryAdd(UserID, u))
            {
                ErrorRecord.Record("201208011742TC 上次没有清空");
            }
        }

        public static void RobotDealMSG(object UserIDandStrMSG)
        { 
            try
            {
                if (DicIDtoUser == null)
                {
                    ErrorRecord.Record(" 201610271644TC ");
                    return;
                }
                object[] objArr = new object[2];
                objArr = (object[])UserIDandStrMSG;
                int UserID = (int)objArr[0];
                string strMSG = (string)objArr[1];
                RobotDealMSG(UserID, strMSG);
            }
            catch (Exception ex)
            { ErrorRecord.Record(ex, "201611122210tc"); }
        }
        /// <summary>
        /// 摹仿客户端 消息处理  不加锁
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="strMSG"></param>
        private static void RobotDealMSG(int UserID, string strMSG)
        { 
            if (!DicIDtoUser.ContainsKey(UserID))
            {   //同在BUG容易出现机器先清空了，没有收到结算的方法 需要延时处理      -------------==================================   什么器结算退出后需要休息一定的时间
               // ErrorRecord.Record(" 201206062215TC ");
                return;
            }
            sc_base _csdata = JsonUtils.Deserialize<sc_base>(strMSG);
            if (_csdata  == null)
            {
                ErrorRecord.Record(" 201206062216TC ");
                return;
            }
            if (_csdata.fn == "")
            {
                ErrorRecord.Record(" 201206071117TC 没找到 _csdata.fn");
                return;
            }

            switch (_csdata.fn)
            {
                case "sc_entertable_n": //自动 准备
                    sc_entertable_n _entertable = JsonUtils.Deserialize<sc_entertable_n>(strMSG);
                    Thread.Sleep(100);
                    TCUser myuentertable;
                    DicIDtoUser.TryGetValue(UserID, out myuentertable);

                    TCTable myt001 = TCLobby.instance.GetTableByRoomIDandTableID(myuentertable._roomid, myuentertable._tableID);
                    if (myt001 != null) myt001.GetReady(myuentertable._userid); //                    
                    break;
                case "sc_ready_tc_n":
                    break;
                case "sc_entertable_tc_n"://默认就是准备状态不处理的其他
                    break;
                case "sc_tablestart_tc_n":
                    break;
                case "sc_token_tc_n": //判断是不是自己的token  AI只弃牌，如果有两个以前的机器人一直下注会卡，，，
                    sc_token_tc_n _tabletoken = JsonUtils.Deserialize<sc_token_tc_n>(strMSG);
                    Thread.Sleep(100);
                    TCUser _myUserGiveUp;
                    DicIDtoUser.TryGetValue(UserID, out _myUserGiveUp);
                    if (_myUserGiveUp._Pos == _tabletoken.pos)
                    {
                        TCTable _myt_token = TCLobby.instance.GetTableByRoomIDandTableID(_myUserGiveUp._roomid, _myUserGiveUp._tableID);
                        //if (_myt_token != null) _myt_token.GiveUp(_myUserGiveUp._userid); //弃牌
                        if (_myt_token != null) _myt_token.Gamble(_myUserGiveUp._userid, 0);
                    }
                    break;
                case "sc_showcard_tc_n":
                    break;
                case "sc_gamble_tc_n":
                    break;
                case "sc_compare_tc_n":
                    break;
                case "sc_giveup_tc_n":
                    break;
                case "sc_end_tc_n":
                    break;
                case "sc_exittable_n":
                    ////sc_exittable_n _exittable = JsonUtils.Deserialize<sc_exittable_n>(strMSG);

                    ////Thread.Sleep(10);
                    ////TCUser myu_exit;
                    ////DicIDtoUser.TryGetValue(UserID, out myu_exit);
                    ////if (_exittable. != myu_exit._Pos)
                    ////{   //自己的退出消息不再处理 
                    ////    TCTable mytexit = TCLobby.GetTableByRoomIDandTableID(myu_exit._roomid, _exittable.tableid);
                    ////    if (mytexit != null) mytexit.ExitTable(myu_exit._userid); //    
                    ////}
                    break;
                case "020":  //此桌结束了，正常结束
                    break;
                default:
                    ErrorRecord.Record("201206190957 strSID：" + _csdata.fn);
                    break;
            }
        }
      

         
    }
}
