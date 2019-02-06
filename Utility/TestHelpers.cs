// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace SQLSpatialTools
{
    public static class TestHelpers
    {
        /// <summary>
        /// Convert geometric string to SqlGeometry object.
        /// </summary>
        /// <param name="geomString">geometry in string representation</param>
        /// <param name="srid">spatial reference identifier; default for SQL Server 4236</param>
        /// <returns>SqlGeometry</returns>
        public static SqlGeometry GetGeom(this string geomString, int srid = 4236 )
        {
            return SqlGeometry.STGeomFromText(new SqlChars(geomString), srid);
        }
    }
}