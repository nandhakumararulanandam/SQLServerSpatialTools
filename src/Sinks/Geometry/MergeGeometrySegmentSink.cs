// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that merges two geometry linestrings.
    /// </summary>
    class MergeGeometrySegmentSink : IGeometrySink110
    {
        int srid;                     // The _srid we are working in.
        readonly bool isFirst;        // We begin geometry/figure for the first one only 
        readonly bool isLast;         // We end geometry/figure for the last one only
        double? mOffset;              // If measures at the merge point are not equal, measures for second geometry will be adjusted
        SqlGeometryBuilder target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public MergeGeometrySegmentSink(SqlGeometryBuilder target, bool isFirst, bool isLast, double? mOffset)
        {
            this.target = target;
            this.isFirst = isFirst;
            this.isLast = isLast;
            this.mOffset = mOffset;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            if (isFirst)
            {
                this.srid = srid;
                target.SetSrid(this.srid);
            }
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            if (isFirst)
                target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the starting point.
            if (isFirst)
                target.BeginFigure(x, y, z, m);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            if (isFirst)
                target.AddLine(x, y, z, m);
            else
                target.AddLine(x, y, z, m + mOffset);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            if (isLast)
                target.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            if (isLast)
                target.EndGeometry();
        }

    }
}
