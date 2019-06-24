﻿//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geometry;
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
            if (sqlGeometry.IsNullOrEmpty())
                return sqlGeometry;

            // GEOMETRYCOLLECTION
            if(sqlGeometry.IsGeometryCollection())
            {
                if (elementIndex == 0 || elementIndex > sqlGeometry.STNumGeometries())
                    Ext.ThrowInvalidElementIndex();

                // reset geometry and element index and pass through
                sqlGeometry = sqlGeometry.STGeometryN(elementIndex);
                elementIndex = 1; 
            }

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
                if (elementIndex != 1)
                    Ext.ThrowInvalidElementIndex();

                if (ringIndex == 0)
                    return sqlGeometry;

                var obtainedGeom = sqlGeometry.STGeometryN(ringIndex);
                if (obtainedGeom == null || obtainedGeom.IsNull)
                    Ext.ThrowInvalidSubElementIndex();

                return obtainedGeom;
            }

            // COMPOUND CURVE
            if (sqlGeometry.IsCompoundCurve())
            {
                if (elementIndex != 1)
                    Ext.ThrowInvalidElementIndex();

                if (ringIndex > 1)
                    Ext.ThrowInvalidSubElementIndex();

                return sqlGeometry;
            }

            // POLYGON
            if (sqlGeometry.IsPolygon())
            {
                if (elementIndex != 1)
                    Ext.ThrowInvalidElementIndex();

                // if sub element index is zero then return the input geometry
                if (ringIndex == 0)
                    return sqlGeometry;

                if (ringIndex > sqlGeometry.STNumInteriorRing() + 1)
                    Ext.ThrowInvalidSubElementIndex();

                return GetPolygon(sqlGeometry, ringIndex);
            }

            // MULTIPOLYGON
            if (sqlGeometry.IsMultiPolygon())
            {
                if (elementIndex == 0 || elementIndex > sqlGeometry.STNumGeometries())
                    Ext.ThrowInvalidElementIndex();

                sqlGeometry = sqlGeometry.STGeometryN(elementIndex);

                // if sub element index is zero then return the input geometry
                if (ringIndex == 0)
                    return sqlGeometry;

                if (ringIndex > sqlGeometry.STNumInteriorRing() + 1)
                    Ext.ThrowInvalidSubElementIndex();

                return GetPolygon(sqlGeometry, ringIndex);
            }

            return sqlGeometry;
        }

        /// <summary>
        /// Build Polygon from Linestring
        /// </summary>
        /// <param name="sqlGeometry">Input Sql Geometry</param>
        /// <param name="ringIndex">Polygon Ring Index</param>
        /// <returns></returns>
        private static SqlGeometry GetPolygon(SqlGeometry sqlGeometry, int ringIndex)
        {
            if (ringIndex == 1)
                sqlGeometry = sqlGeometry.STExteriorRing();
            else
                sqlGeometry = sqlGeometry.STInteriorRingN(ringIndex - 1);

            var polygonBuilder = new SqlGeometryBuilder();
            var polygonSink = new ExtractPolygonFromLineGeometrySink(polygonBuilder);
            sqlGeometry.Populate(polygonSink);
            return polygonBuilder.ConstructedGeometry;
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