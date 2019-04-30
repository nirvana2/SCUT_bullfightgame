/* 
 * 
 * EntityTable：提供数据库连接串，表名，索引，以及存储方式和Cache结构类型等信息，只能放在类定义上面，EntityTable只能存在一个；
 * EntityField：提供数据库表的字段，主键，字段类型，长度，唯一键和定义对象序列化方式，只能放在成员属性上面，EntityField只能存在一个；
 * EntityFieldExtend：定义实体的字段是可以被子类继承的，只能放在成员属性上面，EntityFieldExtend只能存在一个；
 * https://github.com/ScutGame/Scut/wiki/Entity
 */

using System;
using ProtoBuf;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Model;

//namespace GameServer.Script.CsScript.Action
namespace GameServer.Script.Model
{
    [Serializable, ProtoContract]
    [EntityTable(DbConfig.ConnData)]
    public class GameUser : ZyGames.Framework.Game.Context.BaseUser
    {
        [ProtoMember(1)]
        [EntityField(true)]
        public int UserId { get; set; }

        [ProtoMember(2)]
        [EntityField]
        public String NickName
        {
            get;
            set;
        }

        [ProtoMember(3)]
        [EntityField]
        public String PassportId
        {
            get;
            set;
        }

        [ProtoMember(4)]
        [EntityField]
        public String RetailId
        {
            get;
            set;
        }

        [ProtoMember(5)]
        public int CurrRoleId { get; set; }

        public string SId { get; set; }

        protected override int GetIdentityId()
        {
            return UserId;
        }

        public override int GetUserId()
        {
            return UserId;
        }

        public override string GetNickName()
        {
            return NickName;
        }

        public override string GetPassportId()
        {
            return PassportId;
        }

        public override string GetRetailId()
        {
            return RetailId;
        }

        public override bool IsLock
        {
            get { return false; }
        }
         
    }

}