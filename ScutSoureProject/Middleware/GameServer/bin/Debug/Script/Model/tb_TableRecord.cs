using System;
using System.Collections.Generic; 
using ZyGames.Framework.Model;
using ProtoBuf;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Cache.Generic;

namespace GameServer.Script.Model
{
    /// <summary>
    /// 
    /// </summary>                        
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Entity, strFixed.strConnectstring, "tb_tablerecord")]    //, StorageType = StorageType.ReadWriteDB
    public class tb_tablerecord : ShareEntity
    {
        //Guid.NewGuid().ToString("M");         IsIdentity = true 写入数据可能有问题，在Redis里数据好像不能自增，短时间内多条数据后不能产生
        [ProtoMember(1)]
        [EntityField("id", IsKey = true)]
        public string id { get; set; }

        /// <summary>
        /// 生成的一个房间消耗内的唯一编号
        /// </summary>
        [ProtoMember(2)]
        [EntityField]
        public string _guid { get; set; }
        /// <summary>
        /// 6位
        /// </summary>
        [ProtoMember(3)]
        [EntityField]
        public int MatchCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(4)]
        [EntityField]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(5)]
        [EntityField]
        public DateTime EndTime { get; set; }


        /// <summary>
        ///  所有的通信行为
        /// </summary>
        [ProtoMember(6)]
        [EntityField(false, ColumnDbType.LongText)]
        public string ActionList { get; set; }
 
        /// <summary>
        /// 查看这条记录的次数，暂时作限制使用
        /// </summary>
        [ProtoMember(7)]
        [EntityField]
        public int LookCount { get; set; }
        /// <summary>
        /// 游戏id
        /// </summary>
        [ProtoMember(8)]
        [EntityField]
        public int gameid { get; set; }

        /// <summary>
        /// 此次是否为最后一把了
        /// </summary>
        [ProtoMember(9)]
        [EntityField]
        public bool _isover { get; set; }
      

        public tb_tablerecord()
        {
            id = Guid.NewGuid().ToString("N");
        }
    }        

}
