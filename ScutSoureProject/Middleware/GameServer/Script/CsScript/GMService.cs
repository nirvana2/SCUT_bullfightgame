using GameServer.Script.CsScript.Action;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Common.Serialization;
using GameServer.Script.Model;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Model;
using ZyGames.Framework.Data;
using ZyGames.Framework.Net;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Common.Timing;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Cache;

namespace GameServer.Script.CsScript.Action
{
    public  class GMService
    {
        #region 单例
        public static GMService Current;
        static GMService()
        {
            Current = new GMService();
        }
        SyncTimer _syncTimer;
        private GMService()
        {
            httpListener = new HttpListener();
            _syncTimer = new SyncTimer(StopServer, 1, 6000, 60 * 1000);
        }
        public static bool isStop = false;
        #endregion
        public object tablelock = new object();
        private HttpListener httpListener;

        /// <summary>
        /// 请求方式:http://127.0.0.1:8080/Service/?CMD=1001|1,2,3
        /// </summary>
        /// <param name="address">http://127.0.0.1</param>
        /// <param name="port">8080</param>
        /// <param name="httpName">Service</param>
        public void Start(string address, int port, string httpName)
        {
            try
            {
                string url = string.Format("{0}:{1}/{2}/", address, port, httpName);
                httpListener.Prefixes.Add(url);
                httpListener.Start();
                httpListener.BeginGetContext(OnHttpRequest, httpListener);
                TraceLog.WriteInfo(address + " GM服务启动成功!");
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("GM服务器启动失败,\n{0}\n{1}", ex.Message.ToString(), ex.StackTrace.ToString());
            }
        }

        #region http server

        private void OnHttpRequest(IAsyncResult result)
        {
            string ErrorCode = "1";
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            listener.BeginGetContext(OnHttpRequest, listener);
            try
            {
                string address = context.Request.RemoteEndPoint.Address.ToString();
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                int index = context.Request.RawUrl.IndexOf("?data=", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    string _baseData = context.Request.RawUrl.Substring(index + 6);
                    string _json = HttpUtility.UrlDecode(_baseData);
                    ErrorCode = DoExecCmd(address, _json);
                }

            }
            catch (Exception ex)
            {
                TraceLog.WriteError("OnHttpRequest error:{0}", ex);
            }
            finally
            {
                context.Response.ContentType = "application/json";// "text/plain";// "application/octet-stream";
                StreamWriter output = new StreamWriter(context.Response.OutputStream);
                output.Write(ErrorCode);
                output.Close();
                context.Response.Close();
            }
        }

        #endregion


        /// <summary>
        /// 执行指令 格式 "001|1,2,3"
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string DoExecCmd(string address, string json)
        {
            TraceLog.WriteInfo("{0}执行指令:{1}", address, json);
            cs_base_gm _basegm = null;
            try
            {
                _basegm = JsonUtils.Deserialize<cs_base_gm>(json);
            }
            catch (Exception ex)
            {
                return "0";
            }
            string errorCode = "0";
            switch (_basegm.fn)
            {
                case "0001"://通知所有玩家 ?CMD=0001|玩家1524879,获得了以排行榜一等级奖！  
                    //sc_getnotice_n _getnotice = new sc_getnotice_n() { result = 1, fn = "_getnotice", noticelist = new List<string>() };
                    //_getnotice.noticelist.Add(cmds[1]);
                    //BaseSendDataServer.AutoNotifySendData(JsonUtils.Serialize((_getnotice)));
                    errorCode = "1";
                    break;
                case "0002"://踢出游戏 ?CMD=0002|1524879

                    errorCode = "1";
                    break;
                case "0003"://充值 ?CMD=0002|1524879|5000


                    //int userId = 0;
                    //int diamond = 0;
                    //if(int.TryParse(cmds[1],out userId) && int.TryParse(cmds[2],out diamond))
                    //{
                    //    var user = tb_UserEx.GetFromCachebyUserID(userId);
                    //    if(user != null)
                    //    {
                    //        user.ModifyLocked(() =>
                    //            {
                    //                user.diamond += diamond;
                    //            });
                    //    }
                    //    errorCode = "1";
                    //}
                    break;
                case "0004"://设置指定玩家的当前分数     ?CMD=0004|1380162|99     http://127.0.0.1:8080/Service/?CMD=0004|2000001|-9999|4
                    //if (cmds.Length != 4) break;
                    //int _userId004 = 0;
                    //int _money = 0;
                    //int _gameid = 0;
                    //if (int.TryParse(cmds[1], out _userId004) && int.TryParse(cmds[2], out _money) && int.TryParse(cmds[3], out _gameid))
                    //{
                    //    if (_gameid == BullFightLobby.instance.Gameid)
                    //    {
                    //        //只修改内存数据，不做持久化
                    //        UserStatus _us = BullFightLobby.instance.GetUserStatusbyUserID(_userId004);
                    //        if (_us == null) break;
                    //        BullFightTable _bftable = BullFightLobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                    //        if (_bftable == null) break;


                    //        BullFightUser _bfuser = _bftable.GetUserByID(_userId004);
                    //        if (_bfuser == null) break;
                    //        _bfuser._moneyaddorreduce = _money;
                    //        //_bfuser.UpdateMoney(99);            被覆盖了，无效
                    //        errorCode = "1";
                    //    }
                    //    else if (_gameid == LandLordLobby.instance.Gameid)
                    //    {   //只修改内存数据，不做持久化
                    //        UserStatus _us = LandLordLobby.instance.GetUserStatusbyUserID(_userId004);
                    //        if (_us == null) break;
                    //        LandLordTable _bftable = LandLordLobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                    //        if (_bftable == null) break;


                    //        LandLordUser _bfuser = _bftable.GetUserByID(_userId004);
                    //        if (_bfuser == null) break;
                    //        _bfuser._moneyaddorreduce = _money;
                    //        //_bfuser.UpdateMoney(99);            被覆盖了，无效
                    //        errorCode = "1";
                    //    }
                    //}
                    break;
                case "0005"://设置指定玩家申请解散游戏     ?CMD=0005|1380162     http://127.0.0.1:8080/Service/?CMD=0004|2000001|4
                    //if (cmds.Length != 3) break;
                    //int _userId005 = 0;
                    //int _gamedi5 = 0;   
                    //if (int.TryParse(cmds[1], out _userId005) && int.TryParse(cmds[2], out _gamedi5))
                    //{
                    //    if (_gamedi5 == BullFightLobby.instance.Gameid)
                    //    {
                    //        //只修改内存数据，不做持久化
                    //        UserStatus _us = BullFightLobby.instance.GetUserStatusbyUserID(_userId005);
                    //        if (_us == null) break;
                    //        BullFightTable _bftable = BullFightLobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                    //        if (_bftable == null) break;
                    //        _bftable.ApplyExitTable(_userId005);

                    //        //_bfuser.UpdateMoney(99);            被覆盖了，无效
                    //        errorCode = "1";
                    //    }
                    //    else if (_gamedi5 == LandLordLobby.instance.Gameid)
                    //    {   
                    //        //只修改内存数据，不做持久化
                    //        UserStatus _us = LandLordLobby.instance.GetUserStatusbyUserID(_userId005);
                    //        if (_us == null) break;
                    //        LandLordTable _bftable = LandLordLobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                    //        if (_bftable == null) break;
                    //        _bftable.ApplyExitTable(_userId005);

                    //        //_bfuser.UpdateMoney(99);            被覆盖了，无效
                    //        errorCode = "1";
                    //    }
                    //    }
                    break;
                case "cs_setcard_ll_gm"://设置指定玩家牌型最小    ?CMD=1001|1380162     http://127.0.0.1:8080/Service/?CMD=cs_setcard_ll_gm|1380162|4
                    cs_setcard_ll_gm _setcard = JsonUtils.Deserialize<cs_setcard_ll_gm>(json);
                    if (_setcard != null)
                    {
                        int _userId1001 = _setcard.userid;
                        int _gameid1001 = _setcard.gameid;
                        sc_setcard_ll_gm _scSetcard = new sc_setcard_ll_gm() { fn = "sc_setcard_ll_gm", _good = true, _info = "", _ret = 1 };
                        if (_gameid1001 == BullFight100Lobby.instance.Gameid)
                        {
                            //只修改内存数据，不做持久化
                            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(_userId1001);
                            if (_us != null)
                            {
                                BullFight100Table _bftable = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                                if (_bftable != null)
                                {
                                    ////_bftable.ForeashAllDo((i) =>
                                    ////{
                                    ////    if (_bftable._DicPos2User[i]._userid == _userId1001)
                                    ////    {
                                    ////        _bftable._DicPos2User[i]._shouPaiArr = new List<int>() { 103, 103, 202, 203, 201 };
                                    ////    }
                                    ////});
                                    _scSetcard._ret = 0;
                                }
                            }
                        }
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_settb_user_gm":
                    cs_settb_user_gm _settb_user = JsonUtils.Deserialize<cs_settb_user_gm>(json);
                    if (_settb_user != null)
                    {
                        tb_User _user = JsonUtils.Deserialize<tb_User>(_settb_user._userjson);
                        tb_UserEx.UpdateData(_user);
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_settb_user_gm", _info = "", _ret = 0 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_settb_user_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;

                case "cs_setuserdes_gm":
                    cs_setuserdes_gm _setuserinfo = JsonUtils.Deserialize<cs_setuserdes_gm>(json);
                    if (_setuserinfo != null)
                    {
                        int _userId1001 = _setuserinfo.userid;
                        sc_base_gm _scResult = new sc_base_gm() { fn = "cs_setuserdes_gm", _info = "", _ret = 0 };
                        tb_User _user = tb_UserEx.GetFromCachebyUserID(_userId1001);
                        if (_user != null)
                        {
                            if (_user.wechatName != _setuserinfo.webname ||
                                _user.wechatHeadIcon != _setuserinfo.headinfo || _user.AgentId != Convert.ToInt32(_setuserinfo.AgentId))
                            {
                                _user.wechatName = _setuserinfo.webname;
                                _user.wechatHeadIcon = _setuserinfo.headinfo;
                                _user.AgentId = Convert.ToInt32(_setuserinfo.AgentId);
                                tb_UserEx.UpdateData(_user);
                            }
                            else
                            {
                                _scResult._ret = 1;
                                _scResult._info = "无需重新设置";
                            }
                        }
                        else
                        {
                            _scResult._ret = 1;
                            _scResult._info = "会员不存在";
                        }
                        errorCode = JsonUtils.Serialize(_scResult);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_setuserdes_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_setagent_ll_gm"://设置代理
                    cs_setagent_ll_gm _setagent = JsonUtils.Deserialize<cs_setagent_ll_gm>(json);
                    if (_setagent != null)
                    {
                        int _userId1001 = _setagent.userid;
                        sc_base_gm _scResult = new sc_base_gm() { fn = "sc_setagent_ll_gm", _info = "", _ret = 0 };
                        tb_User _user = tb_UserEx.GetFromCachebyUserID(_userId1001);
                        if (_user != null)
                        {
                            if (_user.isagent != _setagent.agentid)
                            {
                                _user.isagent = _setagent.agentid;
                                tb_UserEx.UpdateData(_user);
                            }
                            else
                            {
                                _scResult._ret = 1;
                                _scResult._info = "无需重新设置";
                            }
                        }
                        else
                        {
                            _scResult._ret = 1;
                            _scResult._info = "会员不存在";
                        }
                        errorCode = JsonUtils.Serialize(_scResult);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_setagent_ll_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_setrobot_gm"://设置机器人
                    cs_setrobot_gm _setrobot = JsonUtils.Deserialize<cs_setrobot_gm>(json);
                    if (_setrobot != null)
                    {
                        int _userId1001 = _setrobot.userid;
                        sc_base_gm _scResult = new sc_base_gm() { fn = "sc_setagent_ll_gm", _info = "", _ret = 0 };
                        tb_User _user = tb_UserEx.GetFromCachebyUserID(_userId1001);
                        if (_user != null)
                        {
                            if (_user.isRobot != _setrobot.isrobot ||
                                _user.winpercent != _setrobot.winpercent ||
                                _user.RobotLevel != _setrobot.robotlevel)
                            {
                                _user.isRobot = _setrobot.isrobot;
                                _user.RobotLevel = _setrobot.robotlevel;
                                _user.winpercent = _setrobot.winpercent;
                                tb_UserEx.UpdateData(_user);
                            }
                            else
                            {
                                _scResult._ret = 1;
                                _scResult._info = "无需重新设置";
                            }
                        }
                        else
                        {
                            _scResult._ret = 1;
                            _scResult._info = "会员不存在";
                        }
                        errorCode = JsonUtils.Serialize(_scResult);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_setagent_ll_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_setlocktime_gm"://锁定账号
                    cs_setlocktime_gm _setlocktime = JsonUtils.Deserialize<cs_setlocktime_gm>(json);
                    if (_setlocktime != null)
                    {
                        int _userId1001 = _setlocktime.userid;
                        sc_base_gm _scResult = new sc_base_gm() { fn = "sc_setagent_ll_gm", _info = "", _ret = 0 };
                        tb_User _user = tb_UserEx.GetFromCachebyUserID(_userId1001);
                        if (_user != null)
                        {
                            _user.lockTime = _setlocktime.locktime;
                            tb_UserEx.UpdateData(_user);
                        }
                        else
                        {
                            _scResult._ret = 1;
                            _scResult._info = "会员不存在";
                        }
                        errorCode = JsonUtils.Serialize(_scResult);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_setlocktime_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_charge_gm"://修改充值金额或钻石
                    cs_charge_gm _charge = JsonUtils.Deserialize<cs_charge_gm>(json);
                    if (_charge != null)
                    {
                        sc_charge_gm _scResult = new sc_charge_gm() { fn = "sc_charge_gm", _info = "", _ret = 0, UserMoney = 0 };
                        tb_User _user = tb_UserEx.GetFromCachebyUserID(_charge.userid);
                        if (_user != null)
                        {
                            if (_charge.type == 1)
                            {
                                _scResult.UserMoney = _user.UserMoney;
                                _user.UserMoney += (decimal)_charge.money;
                                _user.TotalMoney += (decimal)_charge.money;
                            }
                            else
                            {
                                _scResult.UserMoney = (decimal)_user.diamond;
                                _user.diamond += _charge.money;
                                _user.totaldiamond += (decimal)_charge.money;
                            }
                            tb_UserEx.UpdateData(_user);
                        }
                        else
                        {
                            _scResult._ret = 1;
                            _scResult._info = "会员不存在";
                        }
                        errorCode = JsonUtils.Serialize(_scResult);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "sc_charge_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_setNotice_gm":
                    cs_setnotice_gm _setnotice = JsonUtils.Deserialize<cs_setnotice_gm>(json);
                    if (_setnotice != null)
                    {
                        //播放消息还没做好  BullFightLobby.SendChat(_user.UserID, gm);
                    }
                    break;
                case "cs_applyexittable_gm":    //tick somebody
                    cs_applyexittable_gm _applyexit = JsonUtils.Deserialize<cs_applyexittable_gm>(json);
                    if (_applyexit != null)
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "sc_base_gm", _info = "", _ret = 0 };

                        if (_applyexit.gameid == BullFight100Lobby.instance.Gameid)
                        {
                            //只修改内存数据，不做持久化
                            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(_applyexit.userid);
                            if (_us != null)
                            {
                                BullFight100Table _bftable = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                                if (_bftable != null) _bftable.ApplyExitTable(_applyexit.userid);
                                _scSetcard._ret = 1;
                            }
                        }

                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    else
                    {
                        sc_base_gm _scSetcard = new sc_base_gm() { fn = "cs_applyexittable_gm", _info = "参数错误", _ret = 1 };
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_createtable_gm":
                    ////cs_createtable_gm _createtable = JsonUtils.Deserialize<cs_createtable_gm>(json);
                    ////if (_createtable != null)
                    ////{
                    ////    sc_base_gm _scSetcard = new sc_base_gm() { fn = "sc_base_gm", _info = "", _ret = 0 };

                    ////    if (_createtable.gameid == BullFightLobby.instance.Gameid)
                    ////    {
                    ////        cs_enterroom _enterroomdata = new cs_enterroom();
                    ////        _enterroomdata.gameid = _createtable.gameid;   //暂时写固定
                    ////        _enterroomdata.levelid = 1;   //暂时写固定    
                    ////        _enterroomdata.gametype = 1;
                    ////        _enterroomdata.baseallmoney = 10000;
                    ////        _enterroomdata.numpertable = 4;
                    ////        _enterroomdata.roomcard = 2;
                    ////        _enterroomdata.tableCount = 3;
                    ////        _enterroomdata.rankertype = 2;

                    ////    }
                    ////    errorCode = JsonUtils.Serialize(_scSetcard);
                    ////}
                    break;
                //case "cs_getonlinecount_gm":
                //    ////cs_getonlinecount_gm _getonline = JsonUtils.Deserialize<cs_getonlinecount_gm>(json);
                //    ////if (_getonline != null)
                //    ////{
                //    ////    sc_getonlinecount_gm _scSetcard = new sc_getonlinecount_gm() { fn = "sc_getonlinecount_gm", _info = "", _ret = 0 };

                //    ////    if (_getonline.gameid == BullFightLobby.instance.Gameid)
                //    ////    {
                //    ////        //只修改内存数据，不做持久化
                //    ////        UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(_applyexit.userid);
                //    ////        if (_us != null)
                //    ////        {
                //    ////            BullFightTable _bftable = BullFightLobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                //    ////            if (_bftable != null) _bftable.ApplyExitTable(_applyexit.userid);
                //    ////            _scSetcard._ret = 1;
                //    ////        }
                //    ////    }
                //    ////    else if (_applyexit.gameid == LandLordLobby.instance.Gameid)
                //    ////    {

                //    ////    }
                //    ////    errorCode = JsonUtils.Serialize(_scSetcard);
                //    ////}
                //    break;
                case "cs_enterroom_gm":
                    cs_enterroom_gm _createroomtable = JsonUtils.Deserialize<cs_enterroom_gm>(json);
                    if (_createroomtable != null)
                    {
                        sc_enterroom_gm _scSetcard = new sc_enterroom_gm() { fn = "sc_enterroom_gm", _info = "", _ret = 0 };
                        //只修改内存数据，不做持久化
                        UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(_createroomtable.userid);
                        if (_us == null)
                        {
                            var cacheSet = new PersonalCacheStruct<tb_User>();
                            tb_User _tempuser = cacheSet.FindKey(_createroomtable.userid.ToString());
                            if (cacheSet.Count == 0 || _tempuser == null)
                            {
                                SchemaTable schema = EntitySchemaSet.Get<tb_User>();
                                DbBaseProvider provider = DbConnectionProvider.CreateDbProvider(schema);
                                DbDataFilter filter = new DbDataFilter(0);
                                filter.Condition = provider.FormatFilterParam("UserId");
                                filter.Parameters.Add("UserId", _createroomtable.userid);
                                cacheSet.TryRecoverFromDb(filter);//从数据库中恢复数据    
                                                                  ////cacheSet.TryRecoverFromDb(new DbDataFilter(0));//all
                                _tempuser = cacheSet.FindKey(_createroomtable.userid.ToString());//
                            }
                            if (_tempuser == null)
                            {
                                ErrorRecord.Record("CommonLogic 201611051736 User数据找不到SessionUserID：" + _createroomtable.userid);
                                return "";
                            }
                            cs_enterroom _enterData = new cs_enterroom()
                            {
                                cc = 0,
                                fn = "cs_enterroom",
                                gameid = _createroomtable.gameid,
                                levelid = _createroomtable.levelid,
                                gametype = _createroomtable.gametype,
                                numpertable = _createroomtable.numpertable,
                                rankertype = _createroomtable.rankertype,
                                roomcard = _createroomtable.roomcard,
                                tableCount = _createroomtable.tableCount,
                                _userid = _createroomtable.userid
                            };
                            CommonLogic _commonLogic = new CommonLogic();
                            _scSetcard.tablenum = _commonLogic.EnterRoom(_tempuser, _enterData);
                        }
                        errorCode = JsonUtils.Serialize(_scSetcard);
                    }
                    break;
                case "cs_maintain_operation":
                    cs_maintain_operation data = JsonUtils.Deserialize<cs_maintain_operation>(json);
                    _syncTimer.Start();
                    sc_maintain_operation sedata = new sc_maintain_operation { fn = "sc_maintain_operation", _ret = 1, _info = "操作成功" };
                    int tableCount = 0;
                    var brlist = BullFight100Room.roomCache.FindAll();
                    foreach (var bullfightroom in brlist)
                    {
                        tableCount += bullfightroom.DicTable.Count;
                    }
                    sedata.tableCount = tableCount;
                    errorCode = JsonUtils.Serialize(sedata);
                    break;
                case "cs_getonlinecount_gm":
                    sc_getonlinecount senddata = new sc_getonlinecount { fn = "sc_getonlinecount", _ret = 1, _info = "获取成功" };
                    senddata.userCount = GameSession.Count;
                    errorCode = JsonUtils.Serialize(senddata);
                    break;
                case "cs_closetable":
                    cs_closetable receiveData = JsonUtils.Deserialize<cs_closetable>(json);
                    // receiveData.
                    sc_closetable sendData = new sc_closetable { fn = "sc_closetable", _ret = 1, _info = "操作成功" };
                    if (receiveData.userNo <= 0)
                    {
                        sendData._ret = -1;
                        sendData._info = "用户名错误";
                        errorCode = JsonUtils.Serialize(sendData);
                        break;
                    }
                    var userStatus = BullFight100Lobby.instanceBase.GetUserStatusbyUserID(receiveData.userNo);
                    if (userStatus == null)
                    {
                        sendData._ret = -1;
                        sendData._info = "用户不存在";
                        errorCode = JsonUtils.Serialize(sendData);
                        break;
                    }
                    lock (tablelock)
                    {
                        var table = BullFight100Lobby.instance.GetTableByRoomIDandTableID(userStatus.RoomID, userStatus.TableID);
                        table._gameover = true;
                    }
                    errorCode = JsonUtils.Serialize(sendData);
                    break;
                case "cs_updatePro"://更新机器人获胜几率
                    cs_updatePro data1 = JsonUtils.Deserialize<cs_updatePro>(json);
                    sc_updatePro sendUpdatePro = new sc_updatePro { fn = "", _ret = 1,_info="成功" };
                    var cacheUser =new GameDataCacheSet<tb_User>();
                    cacheUser.ReLoad();
                    if (cacheUser.Count == 0)
                    {
                        DbDataFilter filter = new DbDataFilter();
                        filter.Condition = "isRobot=@isRobot";
                        filter.Parameters.Add("@isRobot", 1);
                        cacheUser.TryRecoverFromDb(filter);
                    }
                    List<tb_User> userList = new List<tb_User>();
                    var robotId = tb_UserEx.GetUserIdListByRobot(1);
                    robotId.ForEach(d => 
                    {
                        tb_User user;
                        cacheUser.TryFindKey(d.ToString(), out user);
                        if (user != null)
                        {
                            user.winpercent = data1.probability;
                            userList.Add(user);
                        } 
                    });
                    cacheUser.AddOrUpdate(userList);
                    errorCode = JsonUtils.Serialize(sendUpdatePro);
                    break;
                case "cs_updateRobot"://更新机器人头像名称信息
                    cs_updateRobot robotData = JsonUtils.Deserialize<cs_updateRobot>(json);
                   var result=  UpdateRobotImgAndName(robotData);
                    sc_updateRobot sendUpdate = new sc_updateRobot { fn = "", _ret = 1 };
                    sendUpdate._ret = result ? 1 : -1;
                    errorCode = JsonUtils.Serialize(sendUpdate);
                    break;
                case "cs_gameinfo":
                    
                    sc_gameInfo sendgameinfo = new sc_gameInfo { _ret=1 };
                    try
                    {
                        tb_gamelevelinfo gameinfo = JsonUtils.Deserialize<tb_gamelevelinfo>(json);
                        var gameCache = new ShareCacheStruct<tb_gamelevelinfo>();
                        gameCache.AddOrUpdate(gameinfo);
                    }
                    catch (Exception)
                    {
                        sendgameinfo._ret = -1;
                    }
                    errorCode = JsonUtils.Serialize(sendgameinfo);
                    break;
            }
            return errorCode;
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        /// <param name="state"></param>
        private void StopServer(object state)
        {
            var brlist = BullFight100Room.roomCache.FindAll(); 
            if (brlist.Count > 0) isStop = true;
            else
            {
                //停服  
                var runtime = new ConsoleRuntimeHost(); ;
                runtime.Stop();
                 isStop = false;
                _syncTimer.Stop();
            }

        }
        /// <summary>
        /// 更新机器人信息
        /// </summary>
        /// <returns></returns>
        private bool UpdateRobotImgAndName(cs_updateRobot data)
        {
            List<tb_User> users = new List<tb_User>();
            try
            {
                int num = data.UpdateNum == 0 ? 100 : data.UpdateNum;
                var robotIDs = tb_UserEx.GetUserIdListByRobot(1).Where(w => w > data.RobotId).Take(num).ToList();
                var cache = new GameDataCacheSet<tb_User>();
                DbDataFilter filter = new DbDataFilter();
                filter.Condition = "isRobot=@isRobot";
                filter.Parameters.Add("@isRobot", 1);
                cache.TryRecoverFromDb(filter);
                robotIDs.ForEach(i =>
                {
                    tb_User user;
                    cache.TryFindKey(i.ToString(), out user);
                    if (user != null) users.Add(user);
                });
                SetWebChartName(users, data.FilePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 批量设置名称
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private void  SetWebChartName(List<tb_User> users = null, string path = @"C:\Users\Administrator\Desktop\nameorimg.txt")
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            int index = users.Count;
            while ((line = sr.ReadLine().Trim()) != null && index > 0)
            {

                if (index > 0 && !string.IsNullOrEmpty(line))
                {
                    var strs = line.Split(',');
                    if (strs.Any())
                    {
                        users[index - 1].wechatName = strs[1];
                        users[index - 1].wechatHeadIcon = strs[0];
                        tb_UserEx.UpdateData(users[index - 1]);
                        index--;

                    }
                }
            }
            sr.Close();
        }
    }

    public class cs_getonlinecount : cs_base_gm {

    }
    public class sc_getonlinecount : sc_base_gm {

        public int userCount { get; set; }
    }
    public class cs_base_gm
    {
        public string fn = "";
    }
    /// <summary>
    /// 输入房号进入的玩家需再次申请 补丁
    /// </summary>
    public class cs_setcard_ll_gm : cs_base_gm
    {
        public int gameid;
        public int userid;
    }
    public class sc_base_gm
    {
        public string fn = "";
        /// <summary>
        /// 0表示 成功 1以上表示 失败
        /// </summary>
        public int _ret;
        /// <summary>
        /// 如果有错误的描述信息
        /// </summary>
        public string _info;
    }
    /// <summary>
    /// 输入房号进入的玩家需再次申请 补丁
    /// </summary>
    public class sc_setcard_ll_gm : sc_base_gm
    {
        public bool _good;
    }
    public class cs_setagent_ll_gm : cs_base_gm
    {
        public int gameid;
        public int userid;
        public int agentid;
    }
    public class sc_setagent_ll_gm : sc_base_gm
    {
        public bool _ok;
    }
    public class cs_setrobot_gm : cs_base_gm
    {
        public int gameid;
        public int userid;
        public int isrobot;
        public int winpercent;
        public int robotlevel;
    }
    /// <summary>
    /// 设置用户表数据
    /// </summary>
    public class cs_settb_user_gm : cs_base_gm
    {
        public string _userjson;
    }
    /// <summary>
    /// 设置用户静态描述字段
    /// </summary>
    public class cs_setuserdes_gm : cs_base_gm
    {
        public int gameid;
        public int userid;
        /// <summary>
        /// 微信用户名称
        /// </summary>
        public string webname;
        /// <summary>
        /// 头像地址
        /// </summary>
        public string headinfo;
        public string AgentId { get; set; }
    }
    /// <summary>
    /// 发送公告内容
    /// </summary>
    public class cs_setnotice_gm : cs_base_gm
    {
        public string _content;
    }
    /// <summary>
    /// 充值接口
    /// </summary>
    public class cs_charge_gm : cs_base_gm
    {
        /// <summary>
        /// 1、充值金币 2、砖石
        /// </summary>
        public int type;
        /// <summary>
        /// 固定传4
        /// </summary>
        public int gameid;
        /// <summary>
        /// 唯一值
        /// </summary>
        public int userid;
        /// <summary>
        /// 数量
        /// </summary>
        public float money;
    }
    public class sc_charge_gm : sc_base_gm
    {
        /// <summary>
        /// 原金币数或钻石数
        /// </summary>
        public decimal UserMoney;
    }

    /// <summary>
    /// 设置用户表数据
    /// </summary>
    public class cs_setlocktime_gm : cs_base_gm
    {
        public int userid;
        public string locktime;
    }
    /// <summary>
    /// 请求离开桌子，申请解散游 pu戏
    /// </summary>
    public class cs_applyexittable_gm : cs_base_gm
    {
        public int gameid;
        public int userid;
    }
    /// <summary>
    /// 请求离开桌子，申请解散游戏
    /// </summary>
    public class cs_createtable_gm : cs_base_gm
    {
        public int gameid;
        public int userid;
    }

    /// <summary>
    /// 获取在线人数
    /// </summary>
    public class cs_getonlinecount_gm : cs_base_gm
    {
        public int gameid;
    }
    public class sc_getonlinecount_gm : sc_base_gm
    {
        public int onlinecount;
    }

    public class cs_enterroom_gm : cs_base_gm
    {
        public int gameid;
        public int levelid;
        public int userid;
        /// <summary>
        /// 游戏类型，1通比牛牛，2，升庄四人牛牛
        /// </summary>
        public int gametype;
        /// <summary>
        /// 2,3,4;升庄固定为4人。
        /// </summary>
        public int numpertable;
        /// <summary>
        /// 基础分数：1w,3w,5w;升庄15 15*2, 30 30*2, 60 60*2,
        /// </summary>
        public int baseallmoney;
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

    }

    public class sc_enterroom_gm : sc_base_gm
    {
        public string tablenum;
    }
    /// <summary>
    /// 执行维护操作服务端返回
    /// </summary>
    public class sc_maintain_operation : sc_base_gm
    {
        public int tableCount { get; set; }
    }
    /// <summary>
    /// 执行维护操作客户端发送
    /// </summary>
    public class cs_maintain_operation : cs_base_gm
    {
        public string userName { get; set; }
    }
    public class cs_closetable : cs_base_gm
    {
        public int userNo { get; set; }
    }
    public class sc_closetable : sc_base_gm
    {

    }
    /// <summary>
    /// 修改机器人几率服务器返回
    /// </summary>
    public class sc_updatePro : sc_base_gm
    {

    }
    /// <summary>
    /// 修改机器人获胜几率后台发送
    /// </summary>
    public class cs_updatePro : cs_base_gm
    {
        public int probability { get; set; }
    }
    /// <summary>
    /// 更新机器人信息发送
    /// </summary>
    public class cs_updateRobot : cs_base_gm
    {
        /// <summary>
        /// 机器人ID
        /// </summary>
        public int RobotId { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 更新数量
        /// </summary>
        public int UpdateNum { get; set; }
    }
    /// <summary>
    /// 更新机器人信息返回
    /// </summary>
    public class sc_updateRobot : sc_base_gm
    {

    }
    /// <summary>
    /// 游戏场次信息
    /// </summary>
    public class sc_gameInfo : sc_base_gm {
        
    }
    /// <summary>
    /// 游戏场次信息
    /// </summary>
    public class cs_gameInfo : cs_base_gm {

        /// <summary>
        /// 场次ID
        /// </summary>
        public int id;

        /// <summary>
        /// 房间名称
        /// </summary>
        public string name = "";

        /// <summary>
        /// 底分
        /// </summary>
        public int baserate;

        /// <summary>
        /// 游戏ID
        /// </summary>
        public int gameid;

        /// <summary>
        /// 最低分
        /// </summary>
        public int _min;

        /// <summary>
        /// 最高分
        /// </summary>
        public int _max;

        /// <summary>
        /// 在线人数
        /// </summary>
        public int onlineCount;

        /// <summary>
        /// 桌子数量
        /// </summary>
        public int openTableCount;

        /// <summary>
        /// 游戏类型
        /// </summary>
        public int gametype;

        /// <summary>
        /// 游戏类型说明
        /// </summary>
        public string gametypeDesc = "";

        /// <summary>
        /// 是否启用
        /// </summary>
        public int isEnable;

        /// <summary>
        /// 是否启用说明
        /// </summary>
        public string isEnableDesc = "";

        /// <summary>
        /// 是否删除
        /// </summary>
        public int isDelete;

        /// <summary>
        /// 修改人
        /// </summary>
        public string modifyUser = "";

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime modifyTime;
    }
}
