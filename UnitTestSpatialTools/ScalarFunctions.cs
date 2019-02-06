using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools;

namespace SpatialTools.UnitTest
{
    [TestClass]
    public class ScalarFunctions
    {
        [TestMethod]
        public void LocateMAlongGeom_Test()
        {
            var geom = "LINESTRING (0 0 0 0, 10 0 0 10)".GetGeom();
            var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
            var distance = 5;

            var sqlgeom = Functions.LocateMAlongGeom(geom, distance);
            Assert.IsTrue((bool)sqlgeom.STEquals(returnPoint));
        }

        [TestMethod]
        public void InterpolateBetweenGeom_Test()
        {
            var geom1 = SqlGeometry.STPointFromText(new SqlChars("POINT(0 0 0 0)"), Functions.DEFAULT_SRID);
            var geom2 = SqlGeometry.STPointFromText(new SqlChars("POINT(10 0 0 10)"), Functions.DEFAULT_SRID);
            var returnPoint = SqlGeometry.STPointFromText(new SqlChars("POINT (5 0 NULL 5)"), Functions.DEFAULT_SRID);
            var distance = 5;

            var sqlgeom = Functions.InterpolateBetweenGeom(geom1, geom2, distance);
            Assert.IsTrue((bool)sqlgeom.STEquals(returnPoint));
        }
    }
}
