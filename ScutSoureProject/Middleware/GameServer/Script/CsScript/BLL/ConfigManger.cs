using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Data;

namespace GameServer.Script.CsScript.Action
{
    public class ConfigManger
    {
        private static readonly DbBaseProvider _dbBaseProvider;
        internal const string ConnectKey = "ConnData";

        static ConfigManger()
        {
            _dbBaseProvider = DbConnectionProvider.CreateDbProvider(ConnectKey);

        }

        public static DbBaseProvider Provider
        {
            get { return _dbBaseProvider; }
        }
    }
}
