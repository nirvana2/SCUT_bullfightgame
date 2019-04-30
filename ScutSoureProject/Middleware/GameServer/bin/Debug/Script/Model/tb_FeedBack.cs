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
    [EntityTable(CacheType.Dictionary, strFixed.strConnectstring, "tb_feedback")]
    public class tb_FeedBack : BaseEntity
    {        

        [ProtoMember(1)]
        [EntityField("id", IsKey = true)]
        public string id { get; set; }

        [ProtoMember(2)]       
        public int UserID { get; set; }

        [ProtoMember(3)]
        [EntityField]
        public string UserName { get; set; }

        [ProtoMember(4)]
        [EntityField]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 反馈的类型      1.BUG；2.代理加盟；3.举报作弊；4.充值反馈
        /// </summary>
        [ProtoMember(5)]
        [EntityField]
        public int feedbacktype { get; set; }

        /// <summary>
        /// 反馈人的手机号
        /// </summary>
        [ProtoMember(6)]
        [EntityField]
        public string tel { get; set; }

        /// <summary>
        /// 反馈内容
        /// </summary>
        [ProtoMember(7)]
        [EntityField]
        public string content { get; set; }
         

        public tb_FeedBack() : base(false)
        {
            id = Guid.NewGuid().ToString("N");
            CreateDate = DateTime.Now;
        }

        protected override int GetIdentityId()
        {
            return UserID;
        }
    }
          
}