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
using System;
using ProtoBuf;
using ZyGames.Framework.Event;
using ZyGames.Framework.Model;
using ZyGames.Framework.Cache.Generic;

namespace GameServer.Script.Model
{
    /// <summary>
    /// 排行榜的规则      一天排一次   每天12点处理一次写入数据库
    /// </summary>
    [Serializable, ProtoContract]
    [EntityTable(CacheType.Dictionary, strFixed.strConnectstring, "tb_rank")]
    public class tb_Rank : BaseEntity
    {   
               

        [ProtoMember(1)]
        [EntityField(true)]
        public int UserID { get; set; }

        [ProtoMember(2)]
        [EntityField]
        public string UserName { get; set; }

        /// <summary>
        /// 胜的次数
        /// </summary>
        [ProtoMember(3)]
        [EntityField]
        public int ScoreWin { get; set; }

        [ProtoMember(4)]
        [EntityField]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 输的次数
        /// </summary>
        [ProtoMember(5)]
        [EntityField]
        public int ScoreLost { get; set; }
        /// <summary>
        /// 成绩
        /// </summary>
        [ProtoMember(6)]
        [EntityField(true,ColumnDbType.LongText)]
        public CacheList<Record> records { get; set; }

        protected override int GetIdentityId()
        {
            return UserID;
        }
        public tb_Rank() : base(false)
        {
            records = new CacheList<Record>();
            CreateDate = DateTime.Now;
        }
    }
    [ProtoContract,Serializable]
    public class Record : EntityChangeEvent
    {
        public Record()
        {
            CreateDate = DateTime.Now;
        }
        /// <summary>
        /// 唯一编号
        /// </summary>
        [ProtoMember(1)]
        public long ItemId { get; set; }
        /// <summary>
        /// 输赢（钱）
        /// </summary>
        [ProtoMember(2)]
        public decimal Money { get; set; }
        /// <summary>
        /// 桌子号
        /// </summary>
        [ProtoMember(3)]
        public int MatchCode { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [ProtoMember(4)]
        public DateTime CreateDate { get; set; }
    }


}