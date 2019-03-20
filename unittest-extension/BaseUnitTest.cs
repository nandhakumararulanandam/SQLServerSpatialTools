using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace SQLSpatialTools.UnitTests.Extension
{
    public class BaseUnitTest
    {
        public TestLogger Logger;
        public Stopwatch Timer;

        [TestInitialize]
        public void Initialize()
        {
            Logger = new TestLogger(TestContext);
            Timer = new Stopwatch();            
        }

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
    }
}