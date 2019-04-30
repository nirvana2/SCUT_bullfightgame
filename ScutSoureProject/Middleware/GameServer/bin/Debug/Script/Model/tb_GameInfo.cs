using System;
using System.Collections.Generic;
using ZyGames.Framework.Model;
using GameServer.Script.Model;
using ProtoBuf;
using ZyGames.Framework.Game.Context;

namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Entity, "ConnData", "tb_gameinfo", StorageType = StorageType.ReadWriteDB)]
    public class tb_GameInfo : ShareEntity  // BaseEntity
    {
        public tb_GameInfo()
        {
        }
        [ProtoMember(1)]
        [EntityField(true)]
        public int id { get; set; }
        [ProtoMember(2)]
        [EntityField]
        public string name { get; set; }
        [ProtoMember(3)]
        [EntityField]
        public string gameIntroduce { get; set; }
        [ProtoMember(4)]
        [EntityField]
        public string gameRule { get; set; }
        [ProtoMember(5)]
        [EntityField]
        public bool isEnable { get; set; }
        [ProtoMember(6)]
        [EntityField]
        public string isEnableDesc { get; set; }
        [ProtoMember(7)]
        [EntityField]
        public string modifyUser { get; set; }
        [ProtoMember(8)]
        [EntityField]
        public DateTime modifyTime { get; set; }
        [ProtoMember(9)]
        [EntityField]
        public bool isDelete { get; set; }
        ///// <summary>
        ///// tb_GameInfo        的id 
        ///// </summary>
        //[ProtoMember(2)]
        //[EntityField]
        //public int gameid { get; set; }

        ///// <summary>
        ///// 此游戏的在线人线
        ///// </summary>
        //[ProtoMember(3)]
        //[EntityField]
        //public int onlineCount { get; set; }
        ///// <summary>
        ///// 初，中，高级场
        ///// </summary>
        //[ProtoMember(4)]
        //[EntityField]
        //public string name { get; set; }
        ///// <summary>
        /////       底分
        ///// </summary>
        //[ProtoMember(5)]
        //[EntityField]
        //public int baserate { get; set; }

        ///// <summary>
        ///// 最低与最高限制 
        ///// </summary>
        //[ProtoMember(6)]
        //[EntityField]
        //public string minmax { get; set; }
        ///// <summary>
        ///// 每局扣门票 
        ///// </summary>
        //[ProtoMember(6)]
        //[EntityField]
        //public int firstTick { get; set; }
    }
}
