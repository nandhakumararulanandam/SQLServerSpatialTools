using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.LRS;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;

namespace SQLSpatialTools.UniTests.SQL.Tests
{
    [TestClass]
    public class SqlRegisterAndValidateTests
    {
        private static string connectionString;
        private static string targetDir;
        private static SqlConnection dbConnection;
        private static Server dbServer;

        private static string registerScriptFilePath;
        private static string unregisterScriptFilePath;
        private static string lrsExampleScriptFilePath;

        private const string registerScriptFileName = "Register.sql";
        private const string unregisterScriptFileName = "Unregister.sql";
        private const string lrsExampleScriptFileName = "lrs_geometry_example.sql";

        [ClassInitialize()]
        public static void Intialize(TestContext testContext)
        {
            connectionString = ConfigurationManager.AppSettings.Get("sql_connection");
            dbConnection = new SqlConnection(connectionString);
            dbServer = new Server(new ServerConnection(dbConnection));

            targetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            targetDir = targetDir.Substring(0, targetDir.LastIndexOf('\\'));
            targetDir = Path.Combine(targetDir, "lib\\SQL Scripts");
            if (!Directory.Exists(targetDir))
                throw new Exception("Target Directory not found : " + targetDir);

            // register script
            registerScriptFilePath = Path.Combine(targetDir, registerScriptFileName);
            if (!File.Exists(registerScriptFilePath))
                throw new Exception("Register Script file not found : " + registerScriptFilePath);

            // unregister script
            unregisterScriptFilePath = Path.Combine(targetDir, unregisterScriptFileName);
            if (!File.Exists(unregisterScriptFilePath))
                throw new Exception("Unregister Script file not found : " + unregisterScriptFilePath);

            // LRS examples script
            lrsExampleScriptFilePath = Path.Combine(targetDir, lrsExampleScriptFileName);
            if (!File.Exists(lrsExampleScriptFilePath))
                throw new Exception("Unregister Script file not found : " + lrsExampleScriptFilePath);

            // call unregister script as part of initialize
            Unregister();
        }

        private static void Unregister()
        {
            var scriptContent = File.ReadAllText(unregisterScriptFilePath);
            dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [TestMethod]
        [Priority(1)]
        public void UnregisterOSSLibraryTest()
        {
            Unregister();
        }

        [TestMethod]
        [Priority(2)]
        public void RegisterOSSLibraryTest()
        {
            UnregisterOSSLibraryTest();
            var scriptContent = File.ReadAllText(registerScriptFilePath);
            dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [TestMethod]
        [Priority(3)]
        public void RunLRSExamplesTest()
        {
            RegisterOSSLibraryTest();
            var scriptContent = File.ReadAllText(lrsExampleScriptFilePath);
            dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [ClassCleanup()]
        public static void Cleanup()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }
    }
}
