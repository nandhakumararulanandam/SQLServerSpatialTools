// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that split the geometry linestring into two segments based on point.
    /// </summary>
    class SplitGeometrySegmentSink : IGeometrySink110
    {
        SqlGeometry splitPoint;
        int srid;                      // The _srid we are working in.
        SqlGeometryBuilder target1;    // Where we place our result.
        SqlGeometryBuilder target2;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public SplitGeometrySegmentSink(SqlGeometry splitPoint, SqlGeometryBuilder target1, SqlGeometryBuilder target2)
        {
            this.target1 = target1;
            this.target2 = target2;
            this.splitPoint = splitPoint;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            this.srid = srid;
            target1.SetSrid(this.srid);
            target2.SetSrid(this.srid);
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            
            target1.BeginGeometry(OpenGisGeometryType.LineString);
            target2.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the starting point.
            target1.BeginFigure(x, y, z, m);
            target2.BeginFigure(splitPoint.STX.Value, splitPoint.STY.Value, splitPoint.Z.IsNull?(double?)null: splitPoint.Z.Value, splitPoint.M.Value);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // If current measure is between start measure and end measure, we should add segment to the first result linestring
            if (m < splitPoint.M.Value)
            {
                target1.AddLine(x, y, z, m);
            }

            if (m > splitPoint.M.Value)
            {
                target2.AddLine(x, y, z, m);
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            target1.AddLine(splitPoint.STX.Value, splitPoint.STY.Value, null, splitPoint.M.Value);
            target1.EndFigure();
            target2.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            target1.EndGeometry();
            target2.EndGeometry();
        }

    }
}
