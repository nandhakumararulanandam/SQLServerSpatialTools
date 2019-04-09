using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.UnitTests.Extension;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class UtilityFunctionsTest : BaseUnitTest
    {
        [TestMethod]
        public void ConvertTo3DCoordinatesTest()
        {
            var obj = OracleConnector.GetInstance();
            var segment = "MULTILINE ((1 1 1, 2 2 2), (1 1 1, 2 2 2))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "MULTILINE ((1 1 1 5, 2 2 2 10), (1 1 1 15, 2 2 2 20))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "MULTILINE ((1 1 NULL 5, 2 2 2 10), (1 1 NULL 15, 2 2 NULL 20))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "LINESTRING (1 1 1, 2 2 2, 3 3 3, 4 4 4)";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "LINESTRING (1 1 1 NULL, 2 2 2, 3 3 3, 4 4 NULL NULL)";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "LINESTRING (1 1, 2 2, 3 3 NULL 3, 4 4 10 NULL)";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output : {0}", obj.ConvertTo3DCoordinates(segment));
        }

        [TestMethod]
        public void TrimDecimalPointsTest()
        {
            var input = "1335.0 45)";
            var expected = "1335 45)";
            var result= input.TrimDecimalPoints();
            Assert.AreEqual(expected, result);
        }
    }
}
