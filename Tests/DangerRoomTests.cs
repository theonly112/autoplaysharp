using autoplaysharp.Tests;
using NUnit.Framework;
using System;

namespace Tests
{
    public class DangerRoomTests : SingleFrameTest
    {
        [TestCase("TestImages\\DangerRoom_0_5_Daily_Entry.PNG", "0/5")]
        [TestCase("TestImages\\DangerRoom_1_5_Daily_Entry.PNG", "1/5")]
        public void Test1(string fileName, string expectedText)
        {
            Setup(fileName);
            var foundText = Game.GetText(UIds.DANGER_ROOM_DAILY_ENTRY_REWARD_COUNT);
            Assert.AreEqual(expectedText, foundText);
        }
    }
}