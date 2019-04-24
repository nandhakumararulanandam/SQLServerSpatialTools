// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that finds a point along a geometry linestring instance and pipes
    /// it to another sink.
    /// </summary>
    class LocateAlongGeometrySink : IGeometrySink110
    {
    
        double distance;              // The running count of how much further we have to go.
        SqlGeometry lastPoint;        // The last point in the LineString we have passed.
        SqlGeometry foundPoint;       // This is the point we're looking for, assuming it isn't null, we're done.
        int srid;                     // The _srid we are working in.
        SqlGeometryBuilder target;   // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public LocateAlongGeometrySink(double distance, SqlGeometryBuilder target)
        {
            this.target = target;
            this.distance = distance;
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
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the point.
            lastPoint = SqlGeometry.Point(x, y, srid);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // If we've already found a point, then we're done.  We just need to keep ignoring these
            // pesky calls.
            if (foundPoint != null)
                return;

            // Make a point for our current position.
            SqlGeometry thisPoint = SqlGeometry.Point(x, y, srid);

            // is the found point between this point and the last, or past this point?
            double length = thisPoint.STDistance(lastPoint).Value;
            if (length < distance)
            {
                // it's past this point---just step along the line
                distance -= length;
                lastPoint = thisPoint;
            }
            else
            {
                // now we need to do the hard work and find the point in between these two
                foundPoint = Functions.General.Geometry.InterpolateBetweenGeom(lastPoint, thisPoint, distance);
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            if (foundPoint != null)
            {
                // We could use a simple point constructor, but by targeting another sink we can use this
                // class in a pipeline.
                target.SetSrid(srid);
                target.BeginGeometry(OpenGisGeometryType.Point);
                target.BeginFigure(foundPoint.STX.Value, foundPoint.STY.Value);
                target.EndFigure();
                target.EndGeometry();
            }
            else
            {
                throw new ArgumentException("Distance provided is greater than the length of the LineString.");
            }
        }

    }
}
