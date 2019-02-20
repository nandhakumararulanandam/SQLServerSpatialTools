using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools;
using SQLSpatialTools.Function;

namespace SQLSpatialTools.Tests
{
    [TestClass]
    public class GeneralFunctionTests : UnitTest
    {
        [TestMethod]
        public void InterpolateBetweenGeomTest()
        {
            var geom1 = "POINT(0 0 0 0)".GetGeom();
            var geom2 = "POINT(10 0 0 10)".GetGeom();
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;
            Logger.LogLine("Input Point 1:{0} Point 2:{1}", geom1, geom2);
            Logger.Log("Interpolating at a distance of {0}", geom1, geom2, distance);
            Logger.LogLine("Expected Point: {0}", returnPoint.ToString());
            var sqlgeom = General.InterpolateBetweenGeom(geom1, geom2, distance);
            Logger.Log("Obtained Point: {0}", sqlgeom.ToString());
            SqlAssert.IsTrue(sqlgeom.STEquals(returnPoint));
        }

       
    }
}