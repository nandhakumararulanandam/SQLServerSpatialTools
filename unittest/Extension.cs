using System;
using System.Data.SqlTypes;
using System.Text;
using MS = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.Tests
{
    public static class SqlAssert
    {
        public static void IsTrue(SqlBoolean sqlBoolean)
        {
            MS.Assert.IsTrue((bool)sqlBoolean);
        }

        public static void IsFalse(SqlBoolean sqlBoolean)
        {
            MS.Assert.IsFalse((bool)sqlBoolean);
        }

        public static void AreEqual(SqlDouble sqlDouble,  double targetValue)
        {
            MS.Assert.AreEqual(Math.Round((double)sqlDouble,4), Math.Round(targetValue, 4));
        }

        public static void Contains(string inputMessage, string searchString)
        {
            if (!inputMessage.Contains(searchString))
                MS.Assert.Fail(string.Format("Not expected exception, \"{0}\" not found in the message", searchString));
        }
    }

    public class TestLogger
    {
        private readonly MS.TestContext testContext;

        public TestLogger(MS.TestContext testContext)
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
            message.AppendFormat(msgFormat, args);
            testContext.WriteLine(message.ToString());
        }
    }
}
