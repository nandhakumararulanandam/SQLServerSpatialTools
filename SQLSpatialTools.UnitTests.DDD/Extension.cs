using System;
using System.Data.SqlTypes;
using System.Text;
using MST = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.UnitTests.DDD
{
    public static class SqlAssert
    {
        public static void IsTrue(SqlBoolean sqlBoolean)
        {
            MST.Assert.IsTrue((bool)sqlBoolean);
        }

        public static void IsFalse(SqlBoolean sqlBoolean)
        {
            MST.Assert.IsFalse((bool)sqlBoolean);
        }

        public static void AreEqual(SqlDouble sqlDouble, double targetValue)
        {
            MST.Assert.AreEqual(Math.Round((double)sqlDouble, 4), Math.Round(targetValue, 4));
        }

        public static void Contains(string inputMessage, string searchString)
        {
            if (!inputMessage.Contains(searchString))
                MST.Assert.Fail(string.Format("Not expected exception, \"{0}\" not found in the message", searchString));
        }
    }

    public class TestLogger
    {
        private readonly MST.TestContext testContext;

        public TestLogger(MST.TestContext testContext)
        {
            this.testContext = testContext;
        }

        public void Log(string msgFormat, params object[] args)
        {
            testContext.WriteLine(string.Format(msgFormat, args));
        }

        public void LogLine(string msgFormat, params object[] args)
        {
            var message = new StringBuilder();
            message.AppendLine();
            if (args != null && args.Length > 0)
                message.AppendFormat(msgFormat, args);
            else
                message.Append(msgFormat);

            testContext.WriteLine(message.ToString());
        }
    }
}
