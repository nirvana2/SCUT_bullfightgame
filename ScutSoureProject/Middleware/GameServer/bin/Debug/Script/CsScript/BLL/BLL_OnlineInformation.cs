
using GameServer.Script.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common.Configuration;
using ZyGames.Framework.Data;

namespace GameServer.Script.CsScript.Action
{

  
    public  class BLL_OnlineInformation:ConfigManger
    {
        public  bool Add(tb_OnlineInformation model)
        {
            if (string.IsNullOrEmpty(model.ID))
                model.ID = Guid.NewGuid().ToString();
            var command = Provider.CreateCommandStruct("tb_OnlineInformation", CommandMode.Insert);
            command.AddParameter("ID", model.ID);
            command.AddParameter("RoomId", model.RoomId);
            command.AddParameter("OnlineCount", model.OnlineCount);
            command.AddParameter("GameType", model.GameType);
            command.AddParameter("GameModel", model.GameModel);
            command.AddParameter("CreateTime", model.CreateTime);
            command.Parser();
            return Provider.ExecuteQuery(CommandType.Text, command.Sql, command.Parameters) > 0;
        }
        /// <summary>
        /// 增加多条数据
        /// </summary>
        /// <param name="dataList"></param>
        public void AddRange(List<tb_OnlineInformation> dataList)
        {
            if (dataList == null)
                return;
            foreach (var item in dataList)
            {
                Add(item);
            }
        }
    }
}
