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
    public class BLL_Record : ConfigManger
    {
        public bool Add(tb_tablerecord model)
        {
            if (string.IsNullOrEmpty(model.id))
                model.id = Guid.NewGuid().ToString();
            var command = Provider.CreateCommandStruct("tb_tablerecord", CommandMode.Insert);
            command.AddParameter("id", model.id);
            command.AddParameter("_guid", model._guid);
            command.AddParameter("_isover", model._isover);
            command.AddParameter("MatchCode", model.MatchCode);
            command.AddParameter("StartTime", model.StartTime);
            command.AddParameter("EndTime", model.EndTime);
            command.AddParameter("ActionList", model.ActionList);
            command.AddParameter("LookCount", model.LookCount);
            command.AddParameter("gameid", model.gameid);
            command.Parser();
            return Provider.ExecuteQuery(CommandType.Text, command.Sql, command.Parameters) > 0;
        }
    }
}
