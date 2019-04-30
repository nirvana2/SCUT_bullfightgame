using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Model;
using ZyGames.Framework.Data;
using ZyGames.Framework.Net;
using GameServer.Script.Model;
using ZyGames.Framework.Common.Timing;
using ZyGames.Framework.Game.Contract;
using System.Net;
using ZyGames.Framework.Game.Cache;
using System.IO;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// 逻辑消息进来 的接口处理
    /// </summary>
    public class CommonLogic
    {
        private BullFight100Logic _bf100logic;
        private ThreeCardLogic _tclogic;
        private object _lockObj = new object();
        private string _strIPandPort = "";
        private static ConcurrentDictionary<string, DateTime> dicHeartBeatDT = new ConcurrentDictionary<string, DateTime>();
        
        public CommonLogic()
        {
            _bf100logic = new BullFight100Logic();//4     
            _tclogic = new ThreeCardLogic();
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="clientcommand"></param>
        /// <returns></returns>
        public bool DealDataEx(string _data, string _ipport, int SessionUserID, out string senddata)
        {
            senddata = "";
            if (SessionUserID == 0)
            {
                sc_exit_rebindsession_n _rebind = new sc_exit_rebindsession_n() { result = 1, fn = "sc_exit_rebindsession_n" };
                senddata = JsonUtils.Serialize(_rebind);
                return true;
            }
            _strIPandPort = _ipport;
            cs_base _basedata = JsonUtils.Deserialize<cs_base>(_data);
            var cacheSet = new PersonalCacheStruct<tb_User>();
            tb_User _tempuser = cacheSet.FindKey(SessionUserID.ToString());
            if (cacheSet.Count == 0 || _tempuser == null)
            {
                SchemaTable schema = EntitySchemaSet.Get<tb_User>();
                DbBaseProvider provider = DbConnectionProvider.CreateDbProvider(schema);
                DbDataFilter filter = new DbDataFilter(0);
                filter.Condition = provider.FormatFilterParam("UserId");
                filter.Parameters.Add("UserId", SessionUserID);
                cacheSet.TryRecoverFromDb(filter);//从数据库中恢复数据    
                ////cacheSet.TryRecoverFromDb(new DbDataFilter(0));//all
                _tempuser = cacheSet.FindKey(SessionUserID.ToString());//
            }
            if (_tempuser == null)
            {
                ErrorRecord.Record("CommonLogic 201611051736 User数据找不到SessionUserID：" + SessionUserID);
                return false;
            }
            lock (_lockObj)
            {
                try
                {
                    switch (_basedata.fn)
                    {
                        case "cs_base": break;
                        case "cs_login"://登录
                            cs_login _login = JsonUtils.Deserialize<cs_login>(_data);
                            senddata = Login(_tempuser, _login.accountId);
                            break;
                        case "cs_getgamelist": //获取游戏列表
                            cs_getgamelist _gamelist = JsonUtils.Deserialize<cs_getgamelist>(_data);
                            senddata = GetGameList(_tempuser);
                            break;
                        case "cs_getgamelevel": //获取游戏列表   也是房间列表 
                            cs_getgamelevel _levellist = JsonUtils.Deserialize<cs_getgamelevel>(_data);
                            senddata = GetLevelList(_tempuser, _levellist.gameid);
                            break;
                        case "cs_freshplayerInfoSD": //获取玩家信息 
                            senddata = GetCurrentPlayerInfoSD(_tempuser);
                            break;
                        case "cs_enterroom": //进入指定房间
                            cs_enterroom _enterroom = JsonUtils.Deserialize<cs_enterroom>(_data);
                            senddata = EnterRoom(_tempuser, _enterroom);
                            break;
                        case "cs_enterroomtable": //进入指定房间
                            cs_enterroomtable _enterroomtable = JsonUtils.Deserialize<cs_enterroomtable>(_data);
                            senddata = EnterRoomTable(_tempuser, _enterroomtable);
                            break;
                        case "cs_exitroom": //退出指定房间
                            cs_exitroom _exitroom = JsonUtils.Deserialize<cs_exitroom>(_data);
                            senddata = ExitRoom(_tempuser, _exitroom);
                            break;
                        case "cs_applyexittable":
                            cs_applyexittable _exittable = JsonUtils.Deserialize<cs_applyexittable>(_data);
                            senddata = ApplyExitTable(_tempuser, _exittable);
                            break;
                        case "cs_dealexittable":
                            cs_dealexittable _dealexittable = JsonUtils.Deserialize<cs_dealexittable>(_data);
                            senddata = DealExitTable(_tempuser, _dealexittable);
                            break;
                        case "cs_getnotice":// 请求当前公告
                            cs_getnotice _notice = JsonUtils.Deserialize<cs_getnotice>(_data);
                            senddata = GetNotice("");
                            break;
                        case "cs_chat":// 发送聊天信息
                            cs_chat _chat = JsonUtils.Deserialize<cs_chat>(_data);
                            senddata = NotifyChat(_tempuser, _chat);
                            break;
                        case "cs_gm_chesscard":    //GM 操作        
                            break;
                        case "cs_reenterroom": //断线重连第一版
                            cs_reenterroom _reroom = JsonUtils.Deserialize<cs_reenterroom>(_data);
                            senddata = ReEnterRoom(_tempuser, _reroom);
                            break;
                        case "cs_ping"://GetPing  //给请求方发送服务器时间
                            senddata = GetPing();
                            break;
                        case "cs_getranklist": //获取排行榜列表  
                            cs_getranklist _getrank = JsonUtils.Deserialize<cs_getranklist>(_data);
                            senddata = GetRankList(_tempuser, _getrank);
                            break;
                        case "cs_getcombatgainlist": //获取排行榜列表  
                            cs_getcombatgainlist _getcombatgain = JsonUtils.Deserialize<cs_getcombatgainlist>(_data);
                            senddata = GetCombatGainList(_tempuser, _getcombatgain);
                            break;
                        case "cs_feedback":
                            cs_feedback _feedback = JsonUtils.Deserialize<cs_feedback>(_data);
                            senddata = PostFeedback(_tempuser, _feedback);
                            break;
                        case "cs_askmoneytrading"://索要赠送请求
                            cs_askmoneytrading data = JsonUtils.Deserialize<cs_askmoneytrading>(_data);
                            //如果是作弊账号则走单独处理流程
                            HandelType type;
                            type = _tempuser.winpercent > 0 ? HandelType.abnormal : HandelType.normal;
                            var handleGoldOper = CreateHandleGoldFactory.CreateHandleGoldOperation(type);
                            handleGoldOper.model = data;
                            handleGoldOper.user = _tempuser;
                            senddata = handleGoldOper.Operation();
                            break;
                        case "cs_ensuremoneytrading"://确认接收赠送金币
                            cs_ensuremoneytrading data1 = JsonUtils.Deserialize<cs_ensuremoneytrading>(_data);
                            senddata = EnsureMoneyTrading(_tempuser, data1);
                            break;
                        default:
                            if (_basedata.fn.EndsWith("_bf100"))
                            {
                                senddata = _bf100logic.DealDataEx(_data, _ipport, _tempuser);
                            }
                            else if (_basedata.fn.EndsWith("_bfc"))
                            {
                                senddata = _tclogic.DealDataEx(_data, _ipport, _tempuser);
                            }
                            //else if (_basedata.fn.EndsWith("_tc"))
                            //{ senddata = _tclogic.DealDataEx(_data, _ipport, _tempuser); }
                            break;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorRecord.Record(ex, " 201206091508 ");
                    return false;
                }
            }
        }


        public static void AutoHeartBeat()
        {
            foreach (var tempDT in dicHeartBeatDT)
            {
                DateTime dt;
                if (DateTime.Now.AddSeconds(-60) > tempDT.Value)
                { //60秒没心跳移出 
                    dicHeartBeatDT.TryRemove(tempDT.Key, out dt);
                }
            }
        }

        /// <summary>
        /// 给请求方发送服务器时间
        /// </summary>
        /// <returns></returns>
        public string GetPing()
        {
            sc_ping _senddata = new sc_ping() { result = 1, fn = "sc_ping" };
            _senddata.fps = 45;
            return JsonUtils.Serialize(_senddata);
        }

        /// <summary>
        /// 返回用户信息，只有当前玩家需要处理   同时返回是否为断线重连，后续处理
        /// </summary>
        /// <returns></returns>
        public string Login(tb_User _tempuser, string accountid)
        {
            sc_login _senddata = new sc_login() { result = 1, fn = "sc_login", user = new PlayerInfoSD() };

            var cacheSet = new PersonalCacheStruct<tb_User>();

            _senddata.user.userid = _tempuser.UserID;
            _senddata.user.uName = _tempuser.wechatName;
            _senddata.user.Money = (float)_tempuser.UserMoney;
            _senddata.user.accountId = accountid;
            _senddata.user.isagent = _tempuser.isagent;
            _senddata.user._wechat = new WechatInfoSD() { HeadIconURL = ToolsEx.IsHandlePhoto(_tempuser.isRobot, _tempuser.wechatHeadIcon), Sex = _tempuser.Sex, wechatName = _tempuser.wechatName };
            _tempuser.LastLotinTime2 = _tempuser.LastLotinTime1;
            _tempuser.LastLotinTime1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
            _tempuser.IP = _strIPandPort;
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(_tempuser.UserID);//获取是否需要断线重连   
            if (_us != null)
            {
                _senddata.user.state = (int)_us.Status;
                _senddata.gameid = _us.Gameid;
            }
            tb_UserEx.UpdateData(_tempuser);  //更新登录时间与IP 
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 通知所有在线玩家，如公告信息
        /// </summary>
        /// <returns></returns>
        public string DealGM(cs_gm_chesscard _user)
        {
            //处理公告        
            sc_getnotice_n _getnotice = new sc_getnotice_n() { result = 1, fn = "_getnotice", noticelist = new List<string>() };
            _getnotice.noticelist.Add("玩家1524879,获得了以排行榜一等级奖！");
            BaseSendDataServer.AutoNotifySendData(JsonUtils.Serialize((_getnotice)));

            sc_base _senddata = new sc_base() { result = 1, fn = "sc_base" };
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 返回 断线重连消息
        /// </summary>
        /// <returns></returns>
        public string ReEnterRoom(tb_User _tempuser, cs_reenterroom _data)
        {
            //自动找到gameid,levelid,tableid;
            UserStatus _curStatus = BaseLobby.instanceBase.GetUserStatusbyUserID(_tempuser.UserID);
            sc_reenterroom _senddata = new sc_reenterroom() { result = 0, fn = "sc_reenterroom" };
            if (_curStatus != null)
            {
                if (_curStatus.Gameid == 42)
                {
                    BullFight100Table _bftable = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_curStatus.RoomID, _curStatus.TableID);
                    if (_bftable != null)
                    {
                        lock (_bftable)
                        {
                            _senddata.result = 1;
                            _senddata.gameid = _curStatus.Gameid;
                            _senddata.levelid = _curStatus.RoomID;
                            _senddata.tableid = _curStatus.TableID;
                            _senddata._tableEnterSendData = _bftable.GetEnterDisList(_tempuser.UserID);
                            _senddata._tableSendData = _bftable.GetDisList(_tempuser.UserID);
                            _senddata._pos2Gold = _bftable._judge.GetCurrentPosGold();
                            _senddata._numpertable = _bftable._numpertable;
                            _senddata._isStarted = _bftable._haveCheckRoomCard;
                            _senddata.gametype = _bftable._judge._gametype;
                            _senddata.gameModel = _bftable._judge._gameCoin2Room1;
                            _bftable.NotifyReConnect(_tempuser.UserID);
                        }
                    }
                    else
                    {
                        _curStatus.Status = UserStatusEnum.InLobby;
                        BaseLobby.instanceBase.AddorUpdateUserStatus(_curStatus);
                    }
                }
            }

            return JsonUtils.Serialize(_senddata);
        }

        public void SetNotifyReConnect(int userid)
        {
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(userid);
            if (_us == null) return;
            if (_us.TableID == 0) return;

            if (_us.Gameid == 42)
            {   //如果 在房间里，打牌需要处理状态
                BullFight100Table _bftable = BullFight100Lobby.instance.GetTableByRoomIDandTableID(_us.RoomID, _us.TableID);
                if (_bftable == null) return;
                _bftable.NotifyReConnect(userid);
            }
        }
        /// <summary>
        /// 返回游戏列表，只有当前玩家需要处理
        /// </summary>
        /// <returns></returns>
        public string GetRankList(tb_User _user, cs_getranklist _data)
        {
            sc_getranklist _senddata = new sc_getranklist() { result = 1, fn = "sc_getranklist", _ranklist = new List<RankInfoSD>() };
            _senddata._onlymine = _data._onlymine;
            _senddata._ranklist = tb_RankEx.GetRankList();
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 返回我的战绩列表，
        /// </summary>
        /// <returns></returns>
        public string GetCombatGainList(tb_User _user, cs_getcombatgainlist _data)
        {
            sc_getcombatgainlist _senddata = new sc_getcombatgainlist() { result = 1, fn = "sc_getcombatgainlist", _ranklist = new List<CombatGainInfoSD>() };

            _senddata._ranklist = tb_TableMoneyLogEx.GetCombatGainList(_user.UserID);

            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 返回游戏列表，只有当前玩家需要处理
        /// </summary>
        /// <returns></returns>
        public string GetGameList(tb_User _user)
        {
            sc_getgamelist _senddata = new sc_getgamelist() { result = 1, fn = "sc_getgamelist", gamelist = new List<GameInfoSD>() };

            _senddata.gamelist.Add(new GameInfoSD() { id = 42, name = "牛牛", desc = "百人牛牛DESC", currLimit = 1, level = 1, Money = 50, _isopen = 1 });
            _senddata.gamelist.Add(new GameInfoSD() { id = 31, name = "炸金花", desc = "炸金花DESC", currLimit = 1, level = 1, Money = 50, _isopen = 1 });
            _senddata.gamelist.Add(new GameInfoSD() { id = 28, name = "二八杠", desc = "二八杠DESC", currLimit = 1, level = 1, Money = 50, _isopen = 1 });
            _senddata.gamelist.Add(new GameInfoSD() { id = 13, name = "十三水", desc = "十三水DESC", currLimit = 1, level = 1, Money = 50, _isopen = 1 });
            _senddata.gamelist.Add(new GameInfoSD() { id = 51, name = "德州扑克", desc = "德州扑克DESC", currLimit = 1, level = 1, Money = 50, _isopen = 1 });
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 返回游戏列表，只有当前玩家需要处理
        /// </summary>
        /// <returns></returns>
        public string GetLevelList(tb_User _user, int gameid)
        {
            sc_getgamelevel _senddata = new sc_getgamelevel() { result = 1, fn = "sc_getgamelevel", levellist = new List<RoomInfoSD>() };
            switch (gameid)
            {
                case 42:  
                        var _roomlist = BullFight100Room.roomCache.FindAll();
                        var tempInfo = _roomlist;
                        List<RoomInfoSD> roomInfoList = new List<RoomInfoSD>();
                        foreach (var level in tempInfo)
                        { 
                            RoomInfoSD infoSd = new RoomInfoSD()
                            {
                                baserate = level._currRoomInfo.Baserate,
                                gameid = level._currRoomInfo.gameid,
                                gametype = level._currRoomInfo.gametype,
                                id = level.mRoomID,
                                name = level._currRoomInfo.Name,
                                _max = level._currRoomInfo._max,
                                _min = level._currRoomInfo._min
                            };

                            var onlineCount = level._curNumberInRoom + (level.DicTable.Count * 4 + level.DicUser.Count + ToolsEx.GetRandomSys(1, 20));
                            infoSd.onlineCount = onlineCount;
                            roomInfoList.Add(infoSd);
                        }
                        _senddata.levellist = roomInfoList; 
                    break;
                case 41:
                    {
                        ////var _roomlist =  BullColorRoom.roomCache.FindAll();
                        ////var tempInfo = _roomlist;
                        ////List<RoomInfoSD> roomInfoList = new List<RoomInfoSD>();
                        ////foreach (var level in tempInfo)
                        ////{

                        ////    RoomInfoSD infoSd = new RoomInfoSD()
                        ////    {
                        ////        baserate = level._currRoomInfo.Baserate,
                        ////        gameid = level._currRoomInfo.gameid,
                        ////        gametype = level._currRoomInfo.gametype,
                        ////        id = level.RoomId,
                        ////        name = level._currRoomInfo.Name,
                        ////        _max = level._currRoomInfo._max,
                        ////        _min = level._currRoomInfo._min
                        ////    };
                             
                        ////    var onlineCount = level._curNumberInRoom + (level.DicTable.Count * 4 + level.DicUser.Count + ToolsEx.GetRandomSys(1, 20));
                        ////    infoSd.onlineCount = onlineCount;
                        ////    roomInfoList.Add(infoSd);
                        ////}
                        ////_senddata.levellist = roomInfoList;
                    }
                    break;
                default:
                    break;
            }
            return JsonUtils.Serialize(_senddata);
        }

        /// <summary>
        /// 返回游戏列表，只有当前玩家需要处理
        /// </summary>
        /// <returns></returns>
        public string PostFeedback(tb_User _user, cs_feedback _data)
        {
            sc_feedback _senddata = new sc_feedback() { result = 1, fn = "sc_feedback" };
            tb_FeedBack _fbacktable = new tb_FeedBack();
            _fbacktable.content = _data._content;
            _fbacktable.tel = _data._tel;
            _fbacktable.UserID = _user.UserID;
            _fbacktable.UserName = _user.wechatName;
            _fbacktable.feedbacktype = _data._type;
            tb_FeedbackEx.SetData(_fbacktable);
            return JsonUtils.Serialize(_senddata);
        }
        public string GetCurrentPlayerInfoSD(tb_User tbUser)
        {

            var scSd = new sc_freshplayerInfoSD { result = 1, fn = "sc_freshplayerInfoSD", user = new PlayerInfoSD() };
            scSd.user.userid = tbUser.UserID;
            scSd.user.uName = tbUser.wechatName;
            scSd.user.Money = (float)tbUser.UserMoney;
            scSd.user.isagent = tbUser.isagent;
            scSd.user.Diamond = tbUser.diamond;
            scSd.user._wechat = new WechatInfoSD() { HeadIconURL = tbUser.wechatHeadIcon, Sex = tbUser.Sex, wechatName = tbUser.wechatName };
            if (tbUser.AgentId > 0)
            {
                var agentUser = tb_UserEx.GetFromCachebyUserID(tbUser.AgentId);
                if (agentUser != null)
                {
                    scSd.AgentId = agentUser.UserID;
                    scSd.AgentName = agentUser.wechatName;
                }
            }
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(tbUser.UserID);//获取是否需要断线重连  
            if (_us != null)
                scSd.user.state = (int)_us.Status;
            return JsonUtils.Serialize(scSd);
        }
        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        public string EnterRoom(tb_User _user, cs_enterroom _data)
        {
            if (_data.levelid == 0)
            {
                ErrorRecord.Record("cs_enterroom _data.levelid == 0...");
                return "";
            }
            switch (_data.gameid)
            { 
                case 42:
                    return _bf100logic.EnterRoom(_user, _data);
                case 41:
                    return _tclogic.EnterRoom(_user, _data);
                default:
                    break;
            }
            return "";
        }
        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        public string EnterRoomTable(tb_User _user, cs_enterroomtable _data)
        {
            switch (_data.gameid)
            {
                case 42:
                    return _bf100logic.EnterRoomTable(_user, _data);
                default:
                    break;
            }

            sc_enterroomtable _senddata = new sc_enterroomtable() { result = 0, fn = "sc_enterroomtable", cc = 0 };
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        private string ExitRoom(tb_User _user, cs_exitroom _data)
        {
            sc_exitroom _senddata = new sc_exitroom() { result = 0, fn = "sc_exitroom", cc = 0 };
            switch (_data.gameid)
            {
                case 42:
                    BullFight100Room _bfroom = BullFight100Lobby.instance.GetRoomByRoomID(_data.levelid);
                    if (_bfroom != null)
                    {

                        if (_bfroom.ExitRoom(_user.UserID))
                        {
                            _senddata.result = 1;
                        }
                    }
                    break;
                case 41:
                    ////BullColorRoom _bfcroom = BullColorLobby.instance.GetRoomByRoomID(_data.levelid);
                    ////if (_bfcroom != null)
                    ////{ 
                    ////    if (_bfcroom.ExitRoom(_user.UserID))
                    ////    {
                    ////        _senddata.result = 1;
                    ////    }
                    ////}
                    break;
                default:
                    break;
            }

            return JsonUtils.Serialize(_senddata);
        }

        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        public static void ExitRoomByDisConnect(int userid)
        {
            UserStatus _us = BaseLobby.instanceBase.GetUserStatusbyUserID(userid);
            if (_us == null) return;
            if (_us.Status == UserStatusEnum.InTableDaiPai || _us.Status == UserStatusEnum.InTableWaiting)
            {
                _us.Status = UserStatusEnum.InTableDaiPaiDis;
                switch (_us.Gameid)
                {                        
                    case 42:
                        _us.Status = UserStatusEnum.InLobby;
                        BullFight100Room _bfroom = BullFight100Lobby.instance.GetRoomByRoomID(_us.RoomID);
                        if (_bfroom == null) return;
                        BullFight100Table _bftable = _bfroom.GetTableByTableID(_us.TableID);
                        if (_bftable == null) return;
                        lock (_bftable)
                        {
                            _bftable.NotifyDis(userid);
                        }
                        break;
                    case 20:
                        ////LandLordRoom _llroom = LandLordLobby.instance.GetRoomByRoomID(_us.RoomID);
                        ////if (_llroom == null) return;
                        ////LandLordTable _lltable = _llroom.GetTableByTableID(_us.TableID);
                        ////if (_lltable == null) return;
                        ////_lltable.NotifyDis(userid);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        private string ApplyExitTable(tb_User _user, cs_applyexittable _data)
        {
            sc_applyexittable _senddata = new sc_applyexittable() { result = 1, fn = "sc_applyexittable", cc = 0 };
            switch (_data.gameid)
            {
                case 42:
                    BullFight100Room _bfroom = BullFight100Lobby.instance.GetRoomByRoomID(_data.levelid);
                    if (_bfroom != null)
                    {
                        BullFight100Table _bftable = _bfroom.GetTableByTableID(_data.tableid);
                        if (_bftable != null)
                        {
                            lock (_bftable)
                            {
                                _bftable.ApplyExitTable(_user.UserID);
                            }
                        }
                    }
                    break;
                case 41:
                    ////BullColorRoom _bfcroom = BullColorLobby.instance.GetRoomByRoomID(_data.levelid);
                    ////if (_bfcroom != null)
                    ////{
                    ////    BullColorTable _bftable = _bfcroom.GetTableByTableID(_data.tableid);
                    ////    if (_bftable != null)
                    ////    {
                    ////        lock (_bftable)
                    ////        {
                    ////            _bftable.ApplyExitTable(_user.UserID);
                    ////        }
                    ////    }
                    ////}
                    break;
                default:
                    break;
            }

            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 进入房间 返回现在等待用户数 
        /// </summary>                          
        /// <returns></returns>
        private string DealExitTable(tb_User _user, cs_dealexittable _data)
        {
            sc_dealexittable _senddata = new sc_dealexittable() { result = 1, fn = "sc_dealexittable", cc = 0 };
            switch (_data.gameid)
            {
                case 42:
                    BullFight100Room _bfroom = BullFight100Lobby.instance.GetRoomByRoomID(_data.levelid);
                    if (_bfroom != null)
                    {
                        BullFight100Table _bftable = _bfroom.GetTableByTableID(_data.tableid);
                        if (_bftable != null)
                        {
                            _bftable.DealExitTable(_user.UserID, _data.agree == 1);
                        }
                    }
                    break;
                case 41:
                    ////BullColorRoom _bfcroom = BullColorLobby.instance.GetRoomByRoomID(_data.levelid);
                    ////if (_bfcroom != null)
                    ////{
                    ////    BullColorTable _bfctable = _bfcroom.GetTableByTableID(_data.tableid);
                    ////    if (_bfctable != null)
                    ////    {
                    ////        _bfctable.DealExitTable(_user.UserID, _data.agree == 1);
                    ////    }
                    ////}
                    break;
                default:
                    break;
            }

            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 给请求方发送服务器公告
        /// </summary>
        /// <returns></returns>
        public string GetNotice(string content)
        {
            sc_getnotice _senddata = new sc_getnotice() { result = 1, fn = "sc_getnotice", cc = 0, noticelist = new List<string>() };
            //=================================================================
            tb_Notice _notice = tb_NoticeEx.GetLastNotice();
            if (_notice != null)
            {
                _senddata.noticelist.Add(_notice.content);
            }
            else
                _senddata.noticelist.Add(content == "" ? "本游戏用于比赛，请勿用于赌博，发现立即举报" : content);
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 给同桌玩家发送语音
        /// </summary>
        /// <returns></returns>
        public string NotifyChat(tb_User _user, cs_chat chat)
        {
            sc_chat _senddata = new sc_chat() { result = 1, fn = "sc_chat", cc = 0 };

            //通知同桌的玩家
            bool _sendok = false;

            if (BullFight100Lobby.instance.Gameid == chat.gameid)
            {
                _sendok = BullFight100Lobby.instance.SendChat(_user.UserID, chat);
            }

            _senddata.result = _sendok ? 1 : 0;
            return JsonUtils.Serialize(_senddata);
        }
        /// <summary>
        /// 执行GM命令
        /// </summary>
        /// <returns></returns>
        public string DealGM(tb_User _user, cs_gm_chesscard gm)
        {
            sc_gm_chesscard _senddata = new sc_gm_chesscard() { result = 1, fn = "sc_gm_chesscard", cc = 0 };

            bool _sendok = false;
            //if (BullFightLobby.Gameid == gm.gameid)
            //{
            //    _sendok = BullFightLobby.SendChat(_user.UserID, gm);
            //}    
            _senddata.result = _sendok ? 1 : 0;
            return JsonUtils.Serialize(_senddata);
        }

        public static List<tb_User> _robotUserList;
        /// <summary>
        /// 初始化机哭人队列，MySQLDAL.Model.tb_User 数据 
        /// </summary> 
        public static void InitiRobotList()
        {
            _robotUserList = new List<tb_User>();

            var cacheSet = new GameDataCacheSet<tb_User>();
            cacheSet.ReLoad();
            if (cacheSet.Count == 0)
            {
                SchemaTable schema = EntitySchemaSet.Get<tb_User>();
                DbBaseProvider provider = DbConnectionProvider.CreateDbProvider(schema);
                DbDataFilter filter = new DbDataFilter(0);
                filter.Condition = provider.FormatFilterParam("isRobot", "=");
                filter.Parameters.Add("isRobot", 1);

                cacheSet.TryRecoverFromDb(filter);//从数据库中恢复数据   
            }
            var robotIdList = tb_UserEx.GetUserIdListByRobot(1);
            List<tb_User> _userList = new List<tb_User>();     // List<tb_User> _userList = cacheSet.FindAll();
            if (robotIdList.Any())
            {
                robotIdList.ForEach(t =>
                {
                    tb_User user;
                    cacheSet.TryFindKey(t.ToString(), out user);
                    if (user != null) _userList.Add(user);
                });
            }

            //// List<tb_User> _userList = cacheSet.FindAll();
            if (_userList == null || _userList.Count == 0)
            {
                ErrorRecord.Record(" tb_user 中没有机器人，201610231608");
                return;
            }
            //SetWebChartName();
            // var setName = SetWebChartName(_userList);
            // var temp = SetRobotWebChartImg(_userList);
            // ModifyFileName();
            // cacheSet.AddOrUpdate(setName);
            //cacheSet.Update();
            _robotUserList.AddRange(_userList);

        }
        /// <summary>
        /// 批量设置头像图片
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private static List<tb_User> SetRobotWebChartImg(List<tb_User> users, string path = @"C:\Users\Administrator\Desktop\微信头像")
        {
            List<tb_User> data = new List<tb_User>();
            DirectoryInfo folder = new DirectoryInfo(path);
            FileInfo[] fileinfos = folder.GetFiles("*.jpg");
            for (int i = 0; i <= fileinfos.Length; i++)
            {
                if (i < users.Count)
                {
                    //fileinfos[i].sa
                    users[i].wechatHeadIcon = fileinfos[i].Name;
                    data.Add(users[i]);
                }
            }
            return data;
        }
        /// <summary>
        /// 批量修改文件名称
        /// </summary>
        /// <param name="path"></param>
        private static void ModifyFileName(string path = @"C:\Users\Administrator\Desktop\wechat")
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            FileInfo[] fileinfos = folder.GetFiles("*.jpg");
            string srcPath = "";
            int fileCount = 277;
            foreach (var item in fileinfos)
            {
                fileCount++;
                srcPath = item.DirectoryName + "/" + fileCount + ".jpg";
                item.MoveTo(srcPath);
            }
        }
        private static List<tb_User> InitiUserTable()
        {
            List<tb_User> ulist = new List<tb_User>();

            for (int i = 21; i <= 301; i++)
            {
                tb_User _tempuser = new tb_User()
                {
                    Desc = "desc",
                    diamond = 0, 
                    IP = "114.114.114.114",
                    isRobot = 1,
                    LastLotinTime1 = "",
                    LastLotinTime2 = "",
                    RegTime = "",
                    RobotLevel = 1,
                    Status = 0,
                    UserID = 2000000 + i,
                    UserMaxMoney = 20 * 10000 * 10000,
                    UserMoney = 5000,
                    UserName = string.Format("r1000{0}", i),
                    wechatName = string.Format("r1000{0}", i),
                    UserPassword = ""
                };
                ulist.Add(_tempuser);
            }

            return ulist;
        }
        /// <summary>
        /// 处理索取赠送信息
        /// </summary>
        /// <returns></returns>
        private string HandleRecharge(tb_User user, cs_askmoneytrading data)
        {
            var model = new tb_UserRechangeLog();
            string result = string.Empty;
            sc_askmoneytrading_n _senddata2 = new sc_askmoneytrading_n() { fn = "sc_askmoneytrading_n" };
            var transferMsg = new sc_askmoneytrading() { fn = "sc_askmoneytrading" };
            //用户缓存
            var cacheUser = new PersonalCacheStruct<tb_User>();
            //取得目标用户信息
            try
            {
                var targetUser = cacheUser.FindKey(data.objuserid.ToString());
                if (targetUser == null)
                {
                    transferMsg.result = -1;
                    result = JsonUtils.Serialize(transferMsg);
                    return result;
                }
                if (data.Money <= 0)
                {
                    transferMsg.result = 2;
                    result = JsonUtils.Serialize(transferMsg);
                    return result;
                }
                //如果用户是特殊用户设置了胜率的用户则不能提现E:\project\BullFightHeDan_Server\ScutSoureProject\Middleware\GameServer\Script\CsScript\Tools\
                if (user.winpercent > 0)
                {
                    transferMsg.result = -3;
                    return JsonUtils.Serialize(transferMsg);
                }
                var sessionUser = GameSession.Get(targetUser.UserID);
                if (sessionUser == null || !sessionUser.Connected)
                {
                    transferMsg.result = -5;
                    return JsonUtils.Serialize(transferMsg);
                }

                if (targetUser.UserID == user.UserID)
                {
                    transferMsg.result = -2;
                    result = JsonUtils.Serialize(targetUser);
                    return result;
                }
                _senddata2.Money = data.Money;
                _senddata2.objuserid = targetUser.UserID;
                _senddata2.objusername = targetUser.wechatName;
                //索取
                if (data.IsGet)
                {
                    _senddata2.IsGet = true;
                    if (targetUser.UserMoney >= (decimal)data.Money)
                    {
                        _senddata2.result = 1;
                        model.fromuserid = data._userid;
                        model.userid = user.UserID;
                        model.money = (decimal)data.Money;
                        model.cointype = 1;
                        model.createtime = DateTime.Now;
                        model.fromtype = 2;
                        model.oldmoney = targetUser.UserMoney;
                        model.remarks = "索取";
                        model.fromadminid = 0;
                        BLL_UserRechangeLog.Add(model);
                        _senddata2.objuserid = user.UserID;
                        transferMsg.result = 1;
                        BullFight100Lobby.instance.SendTransferMsg(targetUser.UserID, _senddata2);
                    }
                    else
                    {
                        transferMsg.result = 2;
                    }
                    result = JsonUtils.Serialize(transferMsg);
                }
                else
                {
                    if (user.UserMoney >= (decimal)data.Money)
                    {
                        _senddata2.objuserid = user.UserID;
                        _senddata2.objusername = user.wechatName;
                        _senddata2.result = 1;
                        transferMsg.result = 1;
                        BullFight100Lobby.instance.SendTransferMsg(targetUser.UserID, _senddata2);
                    }
                    else
                    {
                        _senddata2.result = 2;
                        transferMsg.result = 2;
                    }
                    result = JsonUtils.Serialize(transferMsg);
                }
            }
            catch (Exception ex)
            {
                //ErrorRecord.Record("转账赠送日志-----" + ex.Message);
                transferMsg.result = -3;
                return JsonUtils.Serialize(transferMsg);
            }
            return result;
        }
        /// <summary>
        /// 赠送索取发送消息逻辑
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        private void HandleRechargeLogic(tb_User user, cs_askmoneytrading data,bool isEnsure=false)
        {
            
        }
        /// <summary>
        /// 处理特殊账号转账赠送
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string HandleSpecial(tb_User user, cs_askmoneytrading data)
        {
            sc_askmoneytrading reviceData = new sc_askmoneytrading { fn = "sc_askmoneytrading", result = 1 };
            if (user.UserID != data.objuserid)
            {
                reviceData.result = -3;
                return JsonUtils.Serialize(reviceData);
            }
            var tempUser = tb_UserEx.GetFromCachebyUserID(data.objuserid);
            if (tempUser == null||data==null)
            {
                reviceData.result = -1;
                return JsonUtils.Serialize(reviceData);
            }
            var rLog = new tb_UserRechangeLog() { cointype=1,fromtype=2,oldmoney=user.UserMoney,userid=user.UserID,money=(decimal)data.Money};
            if (data.IsGet)
            {
                user.UserMoney += (decimal)data.Money;
                rLog.remarks = "特殊账号增加金币";
            }
            else
            {
                user.UserMoney -= (decimal)data.Money;
                rLog.remarks = "特殊账号减少金币";
            }
            tb_UserEx.UpdateData(user);
            BLL_UserRechangeLog.Add(rLog);
            reviceData.result = 1;
            return JsonUtils.Serialize(reviceData);
        }
        /// <summary>
        /// 确认赠送金币
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string EnsureMoneyTrading(tb_User user, cs_ensuremoneytrading data)
        {
            // result 1:可以扣款，2 余额不足不能扣款,-1 用户账号不合法或者钱小于等于0,-2用户未在线或者给自己赠送钱，-3操作失败账号状态异常,-4用户拒绝            
            sc_ensuremoneytrading sendData = new sc_ensuremoneytrading { fn = "sc_ensuremoneytrading", result = 1, Msg = "赠送成功" };
            tb_User _targetUser = tb_UserEx.GetFromCachebyUserID(data.objuserid);// userCache.FindKey(data.objuserid.ToString());
            if (_targetUser == null)
            {
                ErrorRecord.Record(" fetal error ...data.objuserid:" + data.objuserid);
                sendData.result = -1;
                sendData.Msg = "赠送用户不存在";
                return JsonUtils.Serialize(sendData);
            }
            CreateHandleGoldFactory.EnsureMoneyLogic(user, _targetUser, data);
            return JsonUtils.Serialize(sendData);
        }
    }
}