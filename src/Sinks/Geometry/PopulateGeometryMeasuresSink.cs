// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that populate measures for each point in a geometry line string.
    /// </summary>
    class PopulateGeometryMeasuresSink : IGeometrySink110
    {
        SqlGeometry lastPoint;
        SqlGeometry thisPoint;
        readonly double startMeasure;
        readonly double endMeasure;
        readonly double totalLength;
        double currentLength = 0;
        int srid;                     // The _srid we are working in.
        SqlGeometryBuilder target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public PopulateGeometryMeasuresSink(double startMeasure, double endMeasure, double length, SqlGeometryBuilder target)
        {
            this.target = target;
            this.startMeasure = startMeasure;
            this.endMeasure = endMeasure;
            totalLength = length;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            this.srid = srid;
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            target.SetSrid(srid);
            target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the starting point.
            target.BeginFigure(x, y, null, startMeasure);
            lastPoint = SqlGeometry.Point(x, y, srid);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            thisPoint = SqlGeometry.Point(x, y, srid);
            currentLength += lastPoint.STDistance(thisPoint).Value;
            double currentM = startMeasure + (currentLength / totalLength) * (endMeasure - startMeasure);
            target.AddLine(x, y, null, currentM);
            lastPoint = thisPoint;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
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
