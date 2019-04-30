using System.Collections.Generic;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Data;
using ZyGames.Framework.Model;
using ZyGames.Framework.Net;
using GameServer.Script.Model;
using ZyGames.Framework.Common;
using System;
using System.Data;
using System.Linq;
using ZyGames.Framework.Game.Cache;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Game.Contract;

/// <summary>
/// Scut 缓存获取不支持，?类型
/// </summary>
namespace GameServer.Script.CsScript.Action
{
    public class tb_UserEx : ConfigManger
    {
        public int UserID { get; set; }
        /// <summary>
        /// 
        /// </summary>     
        public int GameID { get; set; }


        public static string GetUserNameByUserID(int userid)
        {
            tb_User _tempuser = GetFromCachebyUserID(userid);
            if (_tempuser != null) return _tempuser.wechatName;
            return "";
        }

        public static tb_User GetFromCachebyUserID(int UserID)
        {
            var cacheSet = new PersonalCacheStruct<tb_User>();
            tb_User _tempuser = cacheSet.Find(UserID.ToString(), t => t.UserID == UserID);
            if (cacheSet.Count == 0 || _tempuser == null)
            {
                SchemaTable schema = EntitySchemaSet.Get<tb_User>();
                DbBaseProvider provider = DbConnectionProvider.CreateDbProvider(schema);
                DbDataFilter filter = new DbDataFilter(0);
                filter.Condition = provider.FormatFilterParam("UserId");
                filter.Parameters.Add("UserId", UserID);
                cacheSet.TryRecoverFromDb(filter);//从数据库中恢复数据    

            }
            _tempuser = cacheSet.Find(UserID.ToString(), t => t.UserID == UserID);

            return _tempuser;
        }
        public static void UpdateData(tb_User _user)
        {
            var cacheSet = new PersonalCacheStruct<tb_User>();
            ////tb_User _tempuser = cacheSet.Find((t_user) => { return t_user.UserID == _user.UserID; });
            ////_tempuser.ModifyLocked();
            cacheSet.AddOrUpdate(_user);
        }

        /// <summary>
        /// 示例
        /// </summary>
        /// <returns></returns>
        public static int GetMaxID()
        {
            ////var cacheSet = new PersonalCacheStruct<tb_User>();
            ////List<tb_User> _tempuserlist = cacheSet.FindAll("0", t => t.UserID != 0);
            ////_tempuserlist = MathUtils.QuickSort<tb_User>(_tempuserlist, (x, y) => { return x.id - y.id; });

            //////int pageCount=0;
            //////_tempuserlist.GetPaging<tb_User>(0, 20, out pageCount);

            ////if (_tempuserlist.Count != 0) return _tempuserlist[0].id;
            return 0;
        }
        /// <summary>
        /// 获取机器人ID
        /// </summary>
        /// <returns></returns>
        public static List<int> GetUserIdListByRobot(int value=0)
        {
            List<int> list = new List<int>();
            var command = Provider.CreateCommandStruct("tb_User", CommandMode.Inquiry);
            command.Columns = "UserId";
            command.Filter = ConfigManger.Provider.CreateCommandFilter();
            command.Filter.Condition = string.Format("{0}",
                command.Filter.FormatExpression("isRobot")
                );
            command.Filter.AddParam("isRobot", value);
            command.Parser();

            using (var reader = Provider.ExecuteReader(CommandType.Text, command.Sql, command.Parameters))
            {

                while (reader.Read())
                {
                    list.Add(reader["UserId"].ToInt());
                }
            }
            return list.OrderBy(w => w).ToList();
        }
        /// <summary>
        /// 从数据库恢复
        /// </summary>
        /// <param name="chart">条件</param>
        /// <param name="value">值</param>
        public static void RecoverFromDb(string chart, int value=-2)
        {
            var cacheSet = new GameDataCacheSet<tb_User>();
            if (cacheSet.Count == 0)
            {
                SchemaTable schema = EntitySchemaSet.Get<tb_User>();
                DbBaseProvider provider = DbConnectionProvider.CreateDbProvider(schema);
                DbDataFilter filter = new DbDataFilter(0);
                filter.Condition = provider.FormatFilterParam("isRobot", chart);
                filter.Parameters.Add("isRobot", value);
                cacheSet.TryRecoverFromDb(filter);//从数据库中恢复数据 
            }
        }

    }


    public class tb_TableMoneyLogEx
    {
        /// <summary>
        /// 1倍赔率的数据 
        /// </summary>
        public static List<CommonPosValSD> _pos2Rate1;
        /// <summary>
        /// 2倍赔率的数据 
        /// </summary>
        public static List<CommonPosValSD> _pos2Rate2;
        /// <summary>
        /// 3倍赔率的数据 
        /// </summary>
        public static List<CommonPosValSD> _pos2Rate3;
        /// <summary>
        /// 4倍赔率的数据 
        /// </summary>
        public static List<CommonPosValSD> _pos2Rate4;
        /// <summary>
        ///    一桌完统计数据
        /// </summary>
        /// <param name="guid">桌号</param>
        /// <param name="gameover">是否结束了</param>
        /// <param name="numpertable">一桌有多少人</param>
        public static void SetRateDataByTableNum(string _guid, bool gameover, int numpertable)
        {
            _pos2Rate1 = new List<CommonPosValSD>();
            _pos2Rate2 = new List<CommonPosValSD>();
            _pos2Rate3 = new List<CommonPosValSD>();
            _pos2Rate4 = new List<CommonPosValSD>();
            if (!gameover) return;
            var cacheSet = new ShareCacheStruct<tb_TableMoneyLog>();
            //if (cacheSet.Count == 0)
            //{
            DbDataFilter filter = new DbDataFilter();
            filter.Condition = "_guid = @guid";
            filter.Parameters.Add("guid", _guid);
            cacheSet.TryRecoverFromDb(filter);
            //}
            List<tb_TableMoneyLog> _tmList = cacheSet.FindAll();
            if (_tmList == null || !_tmList.Any()) return;
            for (int _pos = 1; _pos <= numpertable; _pos++)
            {
                var _moneylist1 = _tmList.FindAll((money) => { return money._pos == _pos && money._bullrate == 1; });
                if (_moneylist1.Count != 0) _pos2Rate1.Add(new CommonPosValSD() { pos = _pos, val = _moneylist1.Count });

                var _moneylist2 = _tmList.FindAll((money) => { return money._pos == _pos && money._bullrate == 2; });
                if (_moneylist2.Count != 0) _pos2Rate2.Add(new CommonPosValSD() { pos = _pos, val = _moneylist2.Count });

                var _moneylist3 = _tmList.FindAll((money) => { return money._pos == _pos && money._bullrate == 3; });
                if (_moneylist3.Count != 0) _pos2Rate3.Add(new CommonPosValSD() { pos = _pos, val = _moneylist3.Count });

                var _moneylist4 = _tmList.FindAll((money) => { return money._pos == _pos && money._bullrate == 4; });
                if (_moneylist4.Count != 0) _pos2Rate4.Add(new CommonPosValSD() { pos = _pos, val = _moneylist4.Count });
            }

            //写入排行信息
            _tmList = cacheSet.FindAll((t_user) => { return t_user._guid == _guid && t_user._isover; });
            foreach (tb_TableMoneyLog moneylog in _tmList)
            {
                var cacheSetRank = new GameDataCacheSet<tb_Rank>();
                DbDataFilter filter1 = new DbDataFilter();
                //filter1.Condition = "UserId=@UserId";
                //filter.Parameters.Add("UserId", moneylog.UserID);

                var userRank = new tb_Rank();
                userRank = cacheSetRank.FindKey(moneylog.UserID.ToString());
                if (userRank == null)
                {
                    cacheSetRank.TryRecoverFromDb(filter1);
                    tb_Rank rank = new tb_Rank() { UserID = moneylog.UserID };
                    if (moneylog._win)
                        rank.ScoreWin++;
                    else
                        rank.ScoreLost++;
                    rank.records.Add(new Record() { ItemId = cacheSetRank.GetNextNo(), MatchCode = moneylog.MatchCode, Money = moneylog.AddorReduceMoney });
                    cacheSetRank.Add(rank);
                }
                else
                {
                    userRank = cacheSetRank.FindKey(moneylog.UserID.ToString());
                    userRank.ModifyLocked(() => {
                        if (moneylog._win)
                            userRank.ScoreWin++;
                        else
                            userRank.ScoreLost++;
                        if (userRank.records == null)
                            userRank.records = new CacheList<Record>();
                        userRank.records.Add(new Record() { ItemId = cacheSetRank.GetNextNo(), MatchCode = moneylog.MatchCode, Money = moneylog.AddorReduceMoney });
                        cacheSetRank.Update();
                    });
                }
                // if(userRank.)
                //tb_Rank _temprank = cacheSetRank.Find((rank) => { return rank.UserID == moneylog.UserID; });
                //if (_temprank == null)
                //{
                //    _temprank = new tb_Rank();
                //    _temprank.UserID = moneylog.UserID;
                //    _temprank.ScoreLost = 0;
                //    _temprank.ScoreWin = 0;
                //}
                //_temprank.CreateDate = DateTime.Now;
                //if (moneylog._win) _temprank.ScoreWin++;
                //else _temprank.ScoreLost++;
                //cacheSetRank.AddOrUpdate(_temprank);
            }

        }
        /// <summary>
        /// 获取我的战绩列表
        /// </summary>
        /// <returns></returns>
        public static List<CombatGainInfoSD> GetCombatGainList(int _userid)
        {
            List<CombatGainInfoSD> _CombatGainList = new List<CombatGainInfoSD>();
            //取指定userid最近一天的一次房间结算OVER的记录
            var cacheSettable = new ShareCacheStruct<tb_TableMoneyLog>();
            var cacheRank = new GameDataCacheSet<tb_Rank>();
            DbDataFilter filter1 = new DbDataFilter();
            cacheRank.TryRecoverFromDb(filter1);
            tb_Rank rank = cacheRank.FindKey(_userid.ToString());
            if (rank == null || !rank.records.Any())
                return _CombatGainList;
            List<Record> records = rank.records.OrderByDescending(w => w.CreateDate).Take(10).ToList();
            foreach (var record in records)
            {
                CombatGainInfoSD _tempGain = new CombatGainInfoSD();
                _tempGain.tablenum = record.MatchCode;
                _tempGain._starttime = record.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                _tempGain._tableRecord = new List<CombatTableRecordSD>();
                //找到同桌的人
                var cacheSettable2 = new ShareCacheStruct<tb_TableMoneyLog>();
                DbDataFilter filter = new DbDataFilter();
                filter.Condition = "MatchCode=@MatchCode";
                filter.Parameters.Add("MatchCode", record.MatchCode);
                cacheSettable2.TryRecoverFromDb(filter);
                List<tb_TableMoneyLog> tablemoneylog2list = cacheSettable2.FindAll((r) => { return r.MatchCode == record.MatchCode && r._isover; });

                if (tablemoneylog2list.Any())
                {
                    foreach (var moneylog2 in tablemoneylog2list)
                    {
                        CombatTableRecordSD _combatTable = new CombatTableRecordSD();
                        _combatTable.userid = moneylog2.UserID;
                        _combatTable._username = tb_UserEx.GetUserNameByUserID(moneylog2.UserID);
                        _combatTable._winorlost = (int)moneylog2.AddorReduceMoney;
                        _tempGain._tableRecord.Add(_combatTable);
                    }
                }
                _CombatGainList.Add(_tempGain);
                //_lastCount++;
                //if (_lastCount >= 10) break;//暂时只取10条
            }

            return _CombatGainList;
        }

    }
    public class tb_RankEx
    {
        /// <summary>
        /// 每天统计一次排行榜内容
        /// </summary>
        public static void SetRankListEveryDay()
        {
            var cacheSettable = new ShareCacheStruct<tb_tablerecord>();
            List<tb_TableMoneyLog> _tmListall = new List<tb_TableMoneyLog>();//有最优的SQL语句或存储过程来执行的    还需要按UserID分组 按AddorReduceMoney从大到小排序 
            //取一天前的数据 
            List<tb_tablerecord> _tabList = cacheSettable.FindAll((t_user) => { return t_user.gameid == 4 && t_user._isover == true && t_user.EndTime > System.DateTime.Now.AddDays(-1); });
            foreach (tb_tablerecord t_tab in _tabList)
            {
                var cacheSet = new ShareCacheStruct<tb_TableMoneyLog>();
                _tmListall.AddRange(cacheSet.FindAll((t_user) => { return t_user._guid == t_tab._guid && t_user._isover == true; }));
            }
            //最前5名
            if (_tmListall.Count < 5) return;

        }
        public static List<RankInfoSD> GetRankList()
        {
            List<RankInfoSD> _rankinfolist = new List<RankInfoSD>();
            var userCache = new GameDataCacheSet<tb_User>();
            tb_UserEx.RecoverFromDb("==",0);
            var userIds = tb_UserEx.GetUserIdListByRobot(0);
            List<tb_User> userList = new List<tb_User>();
            foreach (var item in userIds)
            {
                tb_User user;
                userCache.TryFindKey(item.ToString(), out user);
                if (user != null) userList.Add(user);
            }
            if (userList.Any())
            {
                var tempList = userList.OrderByDescending(w => w.diamond).Take(20).ToList();
                _rankinfolist = tempList.Select(w => new RankInfoSD { userid = w.UserID, winScore = (int)w.diamond, uName = w.wechatName, rank = tempList.IndexOf(w) + 1,headurl= ToolsEx.IsHandlePhoto(w.isRobot,w.wechatHeadIcon) }).ToList();
            }
            return _rankinfolist;
        }
    }

    public class tb_FeedbackEx : ConfigManger
    {
        /// <summary>
        /// 更新反馈信息到数据库  后面处理成不用Redis
        /// </summary>
        public static bool SetData(tb_FeedBack _fb)
        {
            //var cacheSettable = new ShareCacheStruct<tb_FeedBack>();
            //cacheSettable.Add(_fb);
            if (string.IsNullOrEmpty(_fb.id))
                _fb.id = Guid.NewGuid().ToString("N");
            var command = Provider.CreateCommandStruct("tb_FeedBack", CommandMode.Insert);
            command.AddParameter("id", _fb.id);
            command.AddParameter("UserName", _fb.UserName);
            command.AddParameter("CreateDate", _fb.CreateDate);
            command.AddParameter("feedbacktype", _fb.feedbacktype);
            command.AddParameter("tel", _fb.tel);
            command.AddParameter("content", _fb.content);
            command.Parser();
            return Provider.ExecuteQuery(CommandType.Text, command.Sql, command.Parameters) > 0;
        }

        public static List<tb_FeedBack> GetList()
        {
            List<tb_FeedBack> _rankinfolist = new List<tb_FeedBack>();
            return _rankinfolist;
        }
    }
    public class tb_NoticeEx
    {
        public static tb_Notice GetLastNotice()
        {
            var notice = new tb_Notice();
            var cacheSettable = new ShareCacheStruct<tb_Notice>();
            cacheSettable.ReLoad();

            notice = cacheSettable.Find(w => w.isStart == 1);
            if (notice == null)
            {
                SchemaTable schema = EntitySchemaSet.Get<tb_Notice>();
                DbBaseProvider provider = DbConnectionProvider.CreateDbProvider(schema);
                DbDataFilter filter = new DbDataFilter(0);
                filter.Condition = provider.FormatFilterParam("isStart");
                filter.Parameters.Add("isStart", 1);
                cacheSettable.TryRecoverFromDb(filter);//从数据库中恢复数据 
                notice = cacheSettable.Find(w => w.isStart == 1);
            }
            return notice;
        }
    }

    public class tb_gamelevelinfoEx
    {
        /// <summary>
        /// 从数据库恢复到缓存
        /// </summary>
        public static void TryRecoverFromDb()
        {
            var cache = new ShareCacheStruct<tb_gamelevelinfo>();
            var filter = new DbDataFilter();
            cache.TryRecoverFromDb(filter);
        }
    }

    public class HandleGoldOperation
    {
        public tb_User user { get; set; }
        public cs_askmoneytrading model { get; set; }
        public virtual string Operation()
        {
            return "";
        }
    }
    public class CreateHandleGoldFactory
    {

        public static HandleGoldOperation CreateHandleGoldOperation(HandelType type)
        {
            HandleGoldOperation sendMessage = null;
            switch (type)
            {
                case HandelType.normal:
                    sendMessage = new HandleGoldByNormal();
                    break;
                case HandelType.abnormal:
                    sendMessage = new HandleGoldByabnormal();
                    break;
            }
            return sendMessage;
        }
        /// <summary>
        /// 处理索取赠送逻辑
        /// </summary>
        /// <param name="user"></param>
        /// <param name="targetUser"></param>
        /// <param name="model"></param>
        public static void HandleLogic(tb_User user,tb_User targetUser, cs_askmoneytrading model)
        {
            sc_askmoneytrading_n _senddata = new sc_askmoneytrading_n() { fn = "sc_askmoneytrading_n" };
            _senddata.Money = model.Money;
            _senddata.objuserid = targetUser.UserID;
            _senddata.objusername = user.wechatName;
            //索取
            if (model.IsGet)
            {
                _senddata.IsGet = true;
                if (targetUser.UserMoney >= (decimal)model.Money)
                {
                    var rechange = new tb_UserRechangeLog() {fromuserid=model._userid,userid=user.UserID,money=(decimal)model.Money,cointype=1,fromtype=2,oldmoney=targetUser.UserMoney,remarks="索取请求",fromadminid=0 };
                    _senddata.result = 1;
                    BLL_UserRechangeLog.Add(rechange);
                    _senddata.objuserid = user.UserID;
                }
            }
            else
            {
                if (user.UserMoney >= (decimal)model.Money)
                {
                    _senddata.objuserid = user.UserID;
                    _senddata.objusername = user.wechatName;
                    _senddata.result = 1;
                }
                else
                {
                    _senddata.result = 2;
                }
            }
            if(_senddata.result==1)
                BullFight100Lobby.instance.SendTransferMsg(targetUser.UserID, _senddata);

        }
        /// <summary>
        /// 确认金钱逻辑
        /// </summary>
        /// <param name="user"></param>
        /// <param name="_targetUser"></param>
        /// <param name="data"></param>
        public static void EnsureMoneyLogic(tb_User user, tb_User _targetUser, cs_ensuremoneytrading data)
        {
            sc_ensuremoneytrading_n pushMsg = new sc_ensuremoneytrading_n { fn = "sc_ensuremoneytrading_n" };
            if (data.YesOrNo && _targetUser != null)
            {
                if (_targetUser.UserMoney >= (decimal)data.Money)
                {
                    _targetUser.UserMoney -= (decimal)data.Money;
                    user.UserMoney += (decimal)data.Money;
                    tb_UserEx.UpdateData(user);
                    tb_UserEx.UpdateData(_targetUser);
                    var rLog = new tb_UserRechangeLog();
                    rLog.cointype = 1;
                    rLog.createtime = DateTime.Now;
                    rLog.fromtype = 2;
                    rLog.money = (decimal)data.Money;
                    rLog.oldmoney = user.UserMoney;
                    rLog.remarks = "转账";
                    rLog.userid = user.UserID;
                    rLog.fromuserid = _targetUser.UserID;
                    BLL_UserRechangeLog.Add(rLog);
                    if (user.AgentId <= 0 && user.isagent == 0)
                    {
                        user.AgentId = _targetUser.UserID;
                        tb_UserEx.UpdateData(user);
                    }
                    pushMsg.result = 1;
                    pushMsg.Money = data.Money;
                    pushMsg.objuserid = user.UserID;
                    pushMsg.objusername = user.wechatName;
                    BullFight100Lobby.instance.SendMoneyTradinMsg(_targetUser.UserID, pushMsg);
                }
            }
            else
            {
                pushMsg.Money = data.Money;
                pushMsg.objuserid = user.UserID;
                pushMsg.objusername = user.wechatName;
                pushMsg.result = -4;
                BullFight100Lobby.instance.SendMoneyTradinMsg(_targetUser.UserID, pushMsg);
            }
        }
    }
    /// <summary>
    /// 正常账号
    /// </summary>
    public class HandleGoldByNormal : HandleGoldOperation
    {
        public override string Operation()
        {
            string result = string.Empty;
            var transferMsg = new sc_askmoneytrading() { fn = "sc_askmoneytrading",result=1 };
            //用户缓存
            var cacheUser = new PersonalCacheStruct<tb_User>();
            //取得目标用户信息
            try
            {
                var targetUser = tb_UserEx.GetFromCachebyUserID(model.objuserid);
                if (targetUser == null)
                {
                    transferMsg.result = -1;
                    return JsonUtils.Serialize(transferMsg);
                }
                if (targetUser.UserMoney <= 0)
                {
                    transferMsg.result = 2;
                    return JsonUtils.Serialize(transferMsg);
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
                    return JsonUtils.Serialize(transferMsg);
                }
                if (model.IsGet)
                {
                    if (targetUser.UserMoney < (decimal)model.Money)
                    {
                        transferMsg.result = 2;
                        return JsonUtils.Serialize(transferMsg);
                    }
                    if (targetUser.AgentId != user.UserID)
                    {
                        transferMsg.result = -3;
                        return JsonUtils.Serialize(transferMsg);
                    }
                }
                else
                {
                    if (user.UserMoney < (decimal)model.Money)
                    {
                        transferMsg.result = 2;
                        return JsonUtils.Serialize(transferMsg);
                    }
                }
                CreateHandleGoldFactory.HandleLogic(user, targetUser, model);
            }
            catch (Exception ex)
            {
                ErrorRecord.Record("转账赠送日志-----" + ex.Message);
                transferMsg.result = -3;
            }
            return JsonUtils.Serialize(transferMsg);
        }
    }
    /// <summary>
    /// 特殊账号
    /// </summary>
    public class HandleGoldByabnormal : HandleGoldOperation
    {
        public override string Operation()
        {
            sc_askmoneytrading reviceData = new sc_askmoneytrading { fn = "sc_askmoneytrading", result = 1 };
            if (user.UserID != model.objuserid)
            {
                reviceData.result = -3;
                return JsonUtils.Serialize(reviceData);
            }
            var tempUser = tb_UserEx.GetFromCachebyUserID(model.objuserid);
            if (tempUser == null || model == null)
            {
                reviceData.result = -1;
                return JsonUtils.Serialize(reviceData);
            }
            var rLog = new tb_UserRechangeLog() { cointype = 1, fromtype = 2, oldmoney = user.UserMoney, userid = user.UserID, money = (decimal)model.Money };
            if (model.IsGet)
            {
                user.UserMoney += (decimal)model.Money;
                rLog.remarks = "特殊账号增加金币";
            }
            else
            {
                user.UserMoney -= (decimal)model.Money;
                rLog.remarks = "特殊账号减少金币";
            }
            tb_UserEx.UpdateData(user);
            BLL_UserRechangeLog.Add(rLog);
            reviceData.result = 1;
            return JsonUtils.Serialize(reviceData);
        }
    }
   
    /// <summary>
    /// 处理类型
    /// </summary>
    public enum HandelType
    {
        /// <summary>
        /// 正常
        /// </summary>
        normal=0,
        /// <summary>
        /// 异常
        /// </summary>
        abnormal=1
    }

}

