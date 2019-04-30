﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using ZyGames.Framework.Model;

namespace GameServer.Script.Model
{
    /// <summary>
    /// 配置一个账号多个角色
    /// </summary>
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Dictionary, "ConnData")]
    public class UserRole : BaseEntity
    {
        [ProtoMember(1)]
        [EntityField(true)]
        public int RoleId { get; set; }

        [ProtoMember(2)]
        [EntityField(true)]
        public int UserId { get; set; }

        [ProtoMember(3)]
        [EntityField]
        public string RoleName { get; set; }

        [ProtoMember(4)]
        [EntityField]
        public string HeadImg { get; set; }

        [ProtoMember(5)]
        [EntityField]
        public bool Sex { get; set; }

        [ProtoMember(6)]
        [EntityField]
        public int LvNum { get; set; }

        [ProtoMember(7)]
        [EntityField]
        public int ExperienceNum { get; set; }

        [ProtoMember(8)]
        [EntityField]
        public int LifeNum { get; set; }

        [ProtoMember(9)]
        [EntityField]
        public int LifeMaxNum { get; set; }


        [ProtoMember(10)]
        [EntityField]
        public int pos_x { get; set; }

        [ProtoMember(11)]
        [EntityField]
        public int pos_y { get; set; }

        [ProtoMember(12)]
        [EntityField]
        public int pos_z { get; set; }

        [ProtoMember(13)]
        [EntityField]
        public string scene_name { get; set; }

        protected override int GetIdentityId()
        {
            return UserId;
        }
    }
}
