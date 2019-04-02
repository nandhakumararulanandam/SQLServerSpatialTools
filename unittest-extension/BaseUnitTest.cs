﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace SQLSpatialTools.UnitTests.Extension
{
    public class BaseUnitTest
    {
        public TestLogger Logger;
        public Stopwatch MSSQLTimer;
        public Stopwatch OracleTimer;

        [TestInitialize]
        public void Initialize()
        {
            Logger = new TestLogger(TestContext);
            MSSQLTimer = new Stopwatch();
            OracleTimer = new Stopwatch();
        }

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
    }
}