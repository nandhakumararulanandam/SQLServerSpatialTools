//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Types;
using SQLSpatialTools.Utility;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools.Functions.Util
{
    /// <summary>
    /// Utility class to manipulate planar Geometry data type.
    /// </summary>
    public static class Geometry
    {
        public static SqlGeometry ExtractGeometry(SqlGeometry sqlGeometry, int elementIndex, int ringIndex = 0)
        {
            var isSimpleType = sqlGeometry.IsPoint() || sqlGeometry.IsLineString() || sqlGeometry.IsCircularString();

            // if simple type then return input geometry when index is 1 or 0
            if (isSimpleType)
            {
                if (elementIndex != 1)
                    Ext.ThrowInvalidElementIndex();

                if (ringIndex > 1)
                    Ext.ThrowInvalidSubElementIndex();

                if (isSimpleType && ringIndex <= 1)
                    return sqlGeometry;
            }

            // Is Multi point or line
            var isMultiPointOrLine = sqlGeometry.IsMultiPoint() || sqlGeometry.IsMultiLineString();

            if (isMultiPointOrLine)
            {
                var obtainedGeom = sqlGeometry.STGeometryN(elementIndex);
                if (obtainedGeom == null)
                    Ext.ThrowInvalidElementIndex();

                return obtainedGeom;
            }

            // Is Polygon type
            var isPolygonType = sqlGeometry.IsPolygon() || sqlGeometry.IsCurvePolygon() || sqlGeometry.IsMultiPolygon();


            // Ring Index is applicable only for Polygon; for other types throw error
            if (!isPolygonType && ringIndex > 0)
                Ext.ThrowInvalidSubElementIndex();

            // IF NOT POLYGON or MULTIPOLYGON or CURVEPOLYGON and element index is 1 then return the input geometry as is
            if (!isPolygonType && elementIndex == 1)
                return sqlGeometry;

            if (sqlGeometry.IsPolygon())
            {
                // if sub element index is zero then return the input geometry
                if (ringIndex == 0)
                    return sqlGeometry;

                if (ringIndex > sqlGeometry.STNumInteriorRing() + 1)
                    Ext.ThrowInvalidSubElementIndex();

                var polygonBuilder = new SqlGeometryBuilder();
                var polygonSink = new ExtractPolygonGeometrySink(polygonBuilder, ringIndex);
                sqlGeometry.Populate(polygonSink);
                if (!polygonSink.IsExtracted)
                    Ext.ThrowInvalidSubElementIndex();
                return polygonBuilder.ConstructedGeometry;
            }

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new ExtractGeometrySink(geomBuilder, elementIndex, ringIndex);
            sqlGeometry.Populate(geomSink);

            if (!geomSink.IsExtracted)
            {
                if (sqlGeometry.IsPolygon())
                    Ext.ThrowInvalidSubElementIndex();
                Ext.ThrowInvalidSubElementIndex();
            }
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Utility method for converting Polygon types to LineString types.
        /// </summary>
        /// <param name="geometry">The Input SqlGeometry</param>
        /// <returns>SqlGeometry</returns>
        public static SqlGeometry PolygonToLine(SqlGeometry geometry)
        {
            // Do manipulation only if it a polygon type
            // else return the input geometry as is
            if (geometry.IsPolygon(false))
                return geometry.GetLineWKTFromPolygon().GetGeom(geometry.STSrid);
            else if (geometry.IsMultiPolygon(false))
                return geometry.GetLineWKTFromMultiPolygon().GetGeom(geometry.STSrid);
            else if (geometry.IsCurvePolygon(false))
                return geometry.GetLineWKTFromCurvePolygon().GetGeom(geometry.STSrid);
            else
                return geometry;
        }
    }
}
