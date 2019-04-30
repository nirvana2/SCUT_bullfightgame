using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameServer.Script.CsScript.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using ZyGames.Framework.Game.Runtime;

namespace GameServer.Script.CsScript.Action.Tests
{
    [TestClass()]
    public class CommonLogicTests
    {
        [TestMethod()]
        public void EnterGameTest()
        {
            new ConsoleRuntimeHost().Start();
            CommonLogic _templogic = new CommonLogic();
            //_templogic.EnterGame(new Model.tb_User() { UserID = 1380065 });
            //Assert.Fail();
        }
    }
}