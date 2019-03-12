using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;
using MST = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.UnitTests.Extension
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

        public static string GetResult(this bool result)
        {
            return result ? "Passed" : "Failed";
        }

        public static string GetResult(this SqlBoolean result)
        {
            return GetResult((bool)result);
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

        public void LogError(Exception ex, string errorMessage = "", params object[] args)
        {
            var message = new StringBuilder();
            var trace = new StackTrace(ex, true);
            var frame = trace.GetFrame(0);
            message.AppendLine();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                if (args != null && args.Length > 0)
                    message.AppendFormat(errorMessage, args);
                else
                    message.Append(errorMessage);
            }

            message.AppendLine();
            if (frame != null)
            {
                message.AppendFormat("Error module: {0}", frame.GetMethod().Name);
                message.AppendLine();
                message.AppendFormat("File Name: {0}", frame.GetFileName());
                message.AppendLine();
                message.AppendFormat("Line Number: {0}", frame.GetFileLineNumber());
                message.AppendLine();
            }
            message.AppendFormat("Exception: {0}", ex.Message);
            message.AppendLine();
            if (ex.StackTrace != null)
            {
                message.AppendFormat("Stack trace: {0}", ex.StackTrace);
                message.AppendLine();
            }

            if (ex.InnerException != null)
            {
                message.AppendFormat("Inner Exception: {0}", ex.InnerException.Message);
                message.AppendLine();
                if (ex.InnerException.StackTrace != null)
                    message.AppendFormat("Inner Stack trace: {0}", ex.InnerException.StackTrace);
            }

            testContext.WriteLine(message.ToString());
        }
    }
}
