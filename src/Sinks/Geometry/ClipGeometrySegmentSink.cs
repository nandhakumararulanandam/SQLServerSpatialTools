// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that clips a line segment based on start and end point.
    /// </summary>
    class ClipGeometrySegmentSink : IGeometrySink110
    {
        SqlGeometry startPoint;
        SqlGeometry endPoint;
        int srid;                     // The _srid we are working in.
        SqlGeometryBuilder target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input line string we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public ClipGeometrySegmentSink(SqlGeometry startPoint, SqlGeometry endPoint, SqlGeometryBuilder target)
        {
            this.target = target;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            this.srid = srid;
            target.SetSrid(this.srid);
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the starting point.
            target.BeginFigure(startPoint.STX.Value, startPoint.STY.Value, null, startPoint.M.Value);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // If current measure is between start measure and end measure, we should add segment to the result line string
            if (m > startPoint.M.Value && m < endPoint.M.Value)
            {
                target.AddLine(x, y, z, m);
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            target.AddLine(endPoint.STX.Value, endPoint.STY.Value, null, endPoint.M.Value);
            target.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            target.EndGeometry();
        }

    }
}
