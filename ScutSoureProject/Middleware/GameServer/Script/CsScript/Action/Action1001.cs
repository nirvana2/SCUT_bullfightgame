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
using System.Collections.Generic; 
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Service;
using GameServer.Script.Model;
using ZyGames.Framework.Common.Serialization;
using ZyGames.Framework.Game.Sns;

namespace GameServer.Script.CsScript.Action
{
    public class Action1001 : BaseStruct
    {
        private string _openid;
        private string _senddata = "";// 
        public Action1001(HttpGet httpGet)
            : base(1001, httpGet)
        {

        }
        public override bool GetUrlElement()
        {
            string _dataEx = "";
            if (actionGetter.GetString("_dataEx", ref _dataEx))
            {
                cs_getexiste_openid _temp = JsonUtils.Deserialize<cs_getexiste_openid>(_dataEx);
                if (_temp._istrueWeiXin) _openid = _temp.openid;
                else
                {
                    _openid = _temp.openid;
                    if(_openid.Length > 32) _openid = _openid.Substring(0, 32);
                }
                return true;
            }
            else return false;
        }
        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
            try
            {
                string _pid = "";        
                bool _isExiste = false;
                SnsUser _snsuser = SnsManager.LoginByWeixin(_openid);
                if (string.IsNullOrEmpty(_snsuser.WeixinCode))
                {//注册绑定                                    

                    ////var q = SnsManager.Register(_openid, "123456", "", true);
                    ////var s = SnsManager.RegisterWeixin(_openid, "123456", "", _openid);
                    ////SnsUser _tempu = SnsManager.LoginByWeixin(_openid);
                    ////_pid = _tempu.PassportId;
                    ////_userid = _tempu.UserId;
                }
                else
                {
                    _isExiste = true;
                    _pid = _snsuser.PassportId;
                    ////_userid = _snsuser.UserId;
                }
                sc_getexiste_openid _scd = new sc_getexiste_openid() { fn = "sc_getexiste_openid", result = 1 };
                _scd._existe = _isExiste;  //------------------------------ 
                _scd._pid = _pid;            
                _senddata = JsonUtils.Serialize(_scd);
                return true;
            }
            catch (Exception ex)
            {
                ErrorRecord.Record("20170216 验证帐号时就不对了");
                this.SaveLog(ex);
                this.ErrorCode = 10086;
                return false;
            }
        }
        public override void BuildPacket()
        {
            if (_senddata != "") this.PushIntoStack(_senddata); 
        }  
    }
    public class cs_getexiste_openid : cs_base
    {
        public string openid;
        public bool _istrueWeiXin;  
    }    

    public class sc_getexiste_openid : sc_base
    {
        public bool _existe;
        public string _pid;      
    }
}
