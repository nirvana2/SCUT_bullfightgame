using GameServer.Script.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Data;

namespace GameServer.Script.CsScript.Action
{
    public class BLL_MoneyLog : ConfigManger
    {
        public bool Add(tb_TableMoneyLog model)
        {
            bool result = false;
            if (string.IsNullOrEmpty(model.id))
                model.id = Guid.NewGuid().ToString();
            var command = Provider.CreateCommandStruct("tb_TableMoneyLog", CommandMode.Insert);
            command.AddParameter("AddorReduceMoney", model.AddorReduceMoney);
            command.AddParameter("_win", model._win);
            command.AddParameter("gameid", model.gameid);
            command.AddParameter("MatchCode", model.MatchCode);
            command.AddParameter("_guid", model._guid);
            command.AddParameter("TableRecordID", model.TableRecordID);
            command.AddParameter("UserID", model.UserID);
            command.AddParameter("_ipport", model._ipport);
            string cardStr = string.Empty;
            if (model._cardList!=null)
            {
                cardStr = "[" + string.Join(",", model._cardList) + "]";
            }
            command.AddParameter("_cardList", cardStr);
            command.AddParameter("_isover", model._isover);
            command.AddParameter("_bullrate", model._bullrate);
            command.AddParameter("_isWatch", model._isWatch);
            command.AddParameter("CreateDate", DateTime.Now);
            command.AddParameter("id", model.id);
            command.AddParameter("_pos", model._pos);

            try
            {
                command.Parser();
                 result = Provider.ExecuteQuery(CommandType.Text, command.Sql, command.Parameters) > 0;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}
