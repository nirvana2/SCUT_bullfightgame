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
   public class BLL_UserRechangeLog:ConfigManger
    {
        public static bool Add(tb_UserRechangeLog model)
        {
            var command = Provider.CreateCommandStruct("tb_userrechargelog", CommandMode.Insert);
            command.AddParameter("cointype", model.cointype);
            command.AddParameter("createtime", model.createtime);
            command.AddParameter("fromtype", model.fromtype);
            command.AddParameter("fromadminid", model.fromadminid);
            command.AddParameter("fromuserid", model.fromuserid);
            command.AddParameter("money", model.money);
            command.AddParameter("remarks", model.remarks);
            command.AddParameter("oldmoney", model.oldmoney);
            command.AddParameter("userid", model.userid);
            command.AddParameter("userid", model.userid);
            command.AddParameter("userid", model.userid);
            command.AddParameter("userid", model.userid);
            command.Parser();
            return Provider.ExecuteQuery(CommandType.Text, command.Sql, command.Parameters) > 0;
        }
    }
}
