using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZyGames.Framework.Common.Serialization;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 机器人：
    /// 做简单的AI判断            
    /// 按客户端流程要求处理
    /// </summary>
    public class BullColorRobot
    {
        private readonly  object _objLock = new object();
        

        public static void RobotDealMSG(object UserIDandStrMSG)
        {
            object[] objArr = new object[2];
            objArr = (object[])UserIDandStrMSG;
            int UserID = (int)objArr[0];
            string strMSG = (string)objArr[1];

            try
            {
                RobotDealMSG(UserID, strMSG);
            }
            catch (Exception ex)
            { ErrorRecord.Record(ex, "201611122210bf"); }
        }
        /// <summary>
        /// 摹仿客户端 消息处理  不加锁
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="strMSG"></param>
        private static void RobotDealMSG(int UserID, string strMSG)
        { 
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(UserID);
            if (_us == null)
            {
                ErrorRecord.Record(" 201611301728BF " + UserID);
                return;
            }
            if (_us.Status == UserStatusEnum.InLobby) return;//一局结算了，收到的消息需要处理，也处理不了，Table已释放 
            BullColorUser myu = BullColorLobby.instance.GetUserByRoomIDandTableIDandUserID(_us.RoomID, _us.TableID, UserID);
            if (myu == null)
            {
                ErrorRecord.Record(" 201611301729BF " + UserID + ",_us.Status:" + _us.Status + " strMSG:" + strMSG);
                return;
            }
            sc_base _csdata = JsonUtils.Deserialize<sc_base>(strMSG);
            if (_csdata == null)
            {
                ErrorRecord.Record(" 201206062216BF " + UserID);
                return;
            }
            int _waittime3000 = 4000;
            switch (_csdata.fn)
            {
                case "sc_entertable_n": //自动 准备
                    //Thread.Sleep(900);
                    sc_entertable_n _entertable = JsonUtils.Deserialize<sc_entertable_n>(strMSG);
                    BullColorTable myt001 = BullColorLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    if (myt001 != null && myu._Pos == _entertable.pos)
                    {
                        //// myt001.GetReady(myu._userid); //  自己的进房间通知才准备           
                    }
                    break;
                case "sc_ready_bf_n": //    
                    break;
                case "sc_tablestart_bf_n":
                    int _waittimeStart = ToolsEx.GetRandomSys(600, 3000);
                    Thread.Sleep(_waittimeStart);
                    sc_tablestart_bfc_n _tablestart = JsonUtils.Deserialize<sc_tablestart_bfc_n>(strMSG);
                    BullColorTable myt = BullColorLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);

                    //根据需求判断 是否抢庄            
                    if (myt != null && !_tablestart.closefun && _tablestart._canGetBanker)
                    { 
                    }
                    break; 
                case "sc_gambleone_bfc_n":
                    break; 
                case "sc_setbulltype_bfc_n":     //无AI处理 直接等待摊牌

                    break;
                case "sc_showone_bfc_n": break;
                case "sc_showdown_bfc_n":
                    //Thread.Sleep(410);
                    break;
                case "sc_end_bfc_n":
                    break;
               
                case "sc_applyexittable_n"://AI 都同意所有游戏解散               
                    Thread.Sleep(550);
                    sc_applyexittable_n _applyExit = JsonUtils.Deserialize<sc_applyexittable_n>(strMSG);
                    BullColorTable _applyexitTable = BullColorLobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    if (_applyexitTable != null)
                    {
                        lock (_applyexitTable)
                        {
                            _applyexitTable.DealExitTable(myu._userid, true);
                        }
                    }
                    break;
                case "sc_dealexittable_n": break;
                case "sc_one_exittable_n": break;
                case "sc_exittable_n"://AI 在有人退出的情况下，全都退出
                    ////sc_exittable  _exittable = JsonUtils.Deserialize<sc_exittable>(strMSG); 
                    ////Thread.Sleep(10);
                    ////BullFightUser myu_exit;
                    ////DicIDtoUser.TryGetValue(UserID, out myu_exit);
                    ////if (_exittable.pos != myu_exit._Pos)
                    ////{   //自己的退出消息不再处理  但是只能处理一次
                    ////    ////BullColorTable mytexit = BullColorLobby.GetTableByRoomIDandTableID(myu_exit._roomid, _exittable.tableid);
                    ////    ////if (mytexit != null) mytexit.ExitTable(myu_exit._userid); //    
                    ////}
                    break;
                case "sc_chat_n": break;
                case "sc_disconnect_n": break;
                case "sc_warning_n": break;
                case "020":  //此桌结束了，正常结束
                    break;
                default:
                    ErrorRecord.Record("201206190957BF AI 未处理，strSID：" + _csdata.fn);
                    break;
            }
        }
    }
}
