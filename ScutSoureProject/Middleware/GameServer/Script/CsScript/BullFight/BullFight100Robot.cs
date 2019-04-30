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
    public class BullFight100Robot
    { 
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
            BullFight100User myu = BullFight100Lobby.instance.GetUserByRoomIDandTableIDandUserID(_us.RoomID, _us.TableID, UserID);
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

            switch (_csdata.fn)
            {
                case "sc_entertable_n": //自动 准备
                    //Thread.Sleep(900); 
                    break; 
                case "sc_tablestart_bf100_n": 
                    break;
                case "sc_applybanker_bf100_n":  //如果 自己是庄，需要执行庄下注      
                    int _waittimeStart = ToolsEx.GetRandomSys(600, 3000);
                    Thread.Sleep(_waittimeStart);
                    sc_tablestart_bf100_n _tablestart = JsonUtils.Deserialize<sc_tablestart_bf100_n>(strMSG);
                    BullFight100Table myt = BullFight100Lobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                     
                    if (myt != null  )
                    {
                        lock (myt)
                        {
                            myt.GambleOne(myu._userid, 2, 10); //下注     
                        }
                    }
                    break;
                case "sc_getbankerone_bf_n":  //客户端显示 OK手势
                    break; 
                case "sc_gambleone_bf100_n":
                    break;  
                case "sc_end_bf100_n":
                    sc_end_bf100_n _showdown = JsonUtils.Deserialize<sc_end_bf100_n>(strMSG);
                    if (_showdown._OverTable == 1 || _showdown.gamemodel == 2) return;//OVer了
                    BullFight100Table _myt_showdown = BullFight100Lobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    BullFight100Table myt0014 = BullFight100Lobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);

                    if (myt0014 != null && myt0014._judge._gameCoin2Room1 == 1)
                    {
                    }
                    break;
                case "cs_bankergetbonuspot_bf100": //有人下庄了

                    break;    
                case "sc_applyexittable_n"://AI 都同意所有游戏解散               
                    Thread.Sleep(550);
                    sc_applyexittable_n _applyExit = JsonUtils.Deserialize<sc_applyexittable_n>(strMSG);
                    BullFight100Table _applyexitTable = BullFight100Lobby.instance.GetTableByRoomIDandTableID(myu._roomid, myu._tableID);
                    if (_applyexitTable != null)
                    {
                        lock (_applyexitTable)
                        {
                            _applyexitTable.DealExitTable(myu._userid, true);
                        }
                    }
                    break;
                case "sc_showdown_bf100_n": break;
                case "sc_dealexittable_n": break;
                case "sc_one_exittable_n": break;
                case "sc_exittable_n"://AI 在有人退出的情况下，全都退出
                   
                    break;
                case "sc_chat_n": break;
                case "sc_disconnect_n": break;
                case "sc_warning_n": break;
                
                default:
                    ErrorRecord.Record("201206190957BF AI 未处理，strSID：" + _csdata.fn);
                    break;
            }
        }
    }
}
