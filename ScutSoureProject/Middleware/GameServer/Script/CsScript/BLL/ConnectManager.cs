/****************************************************************************
Copyright (c) 2013-2015 scutgame.com

http://www.scutgame.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using GameServer.Script.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using ZyGames.Framework.Common.Configuration;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Common.Security;
using ZyGames.Framework.Data;
using ZyGames.Framework.Game.Runtime;

namespace GameServer.Script.CsScript.Action
{
    /// <summary>
    /// Config.
    /// </summary>
    internal class ConnectManager
    {
        private static readonly DbBaseProvider _dataDbBaseProvider;

        static ConnectManager()
        {
            _dataDbBaseProvider = DbConnectionProvider.CreateDbProvider(DbConfig.ConnData);
        }

        public static DbBaseProvider DataProvider
        {
            get { return _dataDbBaseProvider; }
        }
    }

    internal class GameServerManager
    {
        public static List<tb_ActiveCode> GetActiveCode()
        {
            try
            {
                var command = ConnectManager.DataProvider.CreateCommandStruct("tb_ActiveCode", CommandMode.Inquiry);
                command.Columns = "Activecode,GenerateUserId,UseUserId,CreateDate";
                command.Parser();

                using (var reader = ConnectManager.DataProvider.ExecuteReader(CommandType.Text, command.Sql, command.Parameters))
                {
                    List<tb_ActiveCode> olist = new List<tb_ActiveCode>();
                    while (reader.Read())
                    {
                        tb_ActiveCode ordermode = new tb_ActiveCode
                        {
                            Activecode = reader["Activecode"].ToStringEmpty(),
                            GenerateUserId = reader["GenerateUserId"].ToInt32(),
                            UseUserId = reader["UseUserId"].ToInt32(),
                            CreateDate = reader["CreateDate"].ToDateTime()
                        };
                        olist.Add(ordermode);
                    }
                    return olist;
                }
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("ChangeGameServerStatus error:{0}", ex);
            }
            return new List<tb_ActiveCode>();
        }

        public static bool AddActiveCode(tb_ActiveCode model)
        {
            var command = ConnectManager.DataProvider.CreateCommandStruct("tb_ActiveCode", CommandMode.Insert);

            command.AddParameter("Activecode", model.Activecode);
            command.AddParameter("GenerateUserId", model.GenerateUserId);
            command.AddParameter("UseUserId", model.UseUserId);
            command.AddParameter("CreateDate", model.CreateDate);
            command.Parser();

            return ConfigManger.Provider.ExecuteQuery(CommandType.Text, command.Sql, command.Parameters) > 0;
        }

        /// <summary>
        /// 获取好友数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetFriendNum(int userId)
        {
            var command = ConnectManager.DataProvider.CreateCommandStruct("tb_ActiveCode", CommandMode.Inquiry);
            command.Columns = "Count(1)";
            command.Filter = ConnectManager.DataProvider.CreateCommandFilter();
            command.Filter.Condition = command.Filter.FormatExpression("GenerateUserId");
            command.Filter.Condition = command.Filter.FormatExpression("UseUserId", ">");
            command.Filter.AddParam("GenerateUserId", userId);
            command.Filter.AddParam("UseUserId", 0);
            command.Parser();

            return ConnectManager.DataProvider.ExecuteScalar(CommandType.Text, command.Sql, command.Parameters).ToInt32();
        }

        /// <summary>
        /// 根据激活码获得信息
        /// </summary>
        /// <param name="useUserId"></param>
        /// <returns></returns>
        public static int GetGenerateUserId(int useUserId)
        {
            var command = ConnectManager.DataProvider.CreateCommandStruct("tb_ActiveCode", CommandMode.Inquiry);
            command.Columns = "GenerateUserId";
            command.Filter = ConnectManager.DataProvider.CreateCommandFilter();
            command.Filter.Condition = command.Filter.FormatExpression("UseUserId");
            command.Filter.AddParam("UseUserId", useUserId);
            command.Parser();
            return ConnectManager.DataProvider.ExecuteScalar(CommandType.Text, command.Sql, command.Parameters).ToInt32();
            //using (var reader = ConnectManager.DataProvider.ExecuteReader(CommandType.Text, command.Sql, command.Parameters))
            //{
            //    return new tb_ActiveCode
            //    {
            //        Activecode = reader["Activecode"].ToStringEmpty(),
            //        GenerateUserId = reader["GenerateUserId"].ToInt32(),
            //        UseUserId = reader["UseUserId"].ToInt32(),
            //        CreateDate = reader["CreateDate"].ToDateTime()
            //    };
            //}
        }
    }
}