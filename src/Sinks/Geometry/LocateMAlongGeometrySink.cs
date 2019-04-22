// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;
using SQLSpatialTools.Functions.LRS;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that finds a point along a geometry linestring instance and pipes
    /// it to another sink.
    /// </summary>
    class LocateMAlongGeometrySink : IGeometrySink110
    {
        readonly double measure;      // The running count of how much further we have to go.
        readonly double tolerance;      // The tolerance.
        int srid;                     // The srid we are working in.
        SqlGeometry lastPoint;        // The last point in the LineString we have passed.
        SqlGeometry foundPoint;       // This is the point we're looking for, assuming it isn't null, we're done.
        SqlGeometryBuilder target;    // Where we place our result.
        public bool IsPointDerived;

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a measure, which is the point along the input linestring we will travel to.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public LocateMAlongGeometrySink(double measure, SqlGeometryBuilder target, double tolerance = Constants.Tolerance)
        {
            this.target = target;
            this.measure = measure;
            this.tolerance = tolerance;
            IsPointDerived = false;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            this.srid = srid;
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (foundPoint != null)
                return;
            // Memorize the point.
            lastPoint = Ext.GetPoint(x, y, z, m, srid);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // If we've already found a point, then we're done.  We just need to keep ignoring these
            // pesky calls.
            if (foundPoint != null)
                return;

            // Make a point for our current position.
            var thisPoint = Ext.GetPoint(x, y, z, m, srid);

            // is the found point between this point and the last, or past this point?
            if (measure.IsWithinRange(lastPoint.M.Value, (double)m))
            {
                // now we need to do the hard work and find the point in between these two
                foundPoint = Geometry.InterpolateBetweenGeom(lastPoint, thisPoint, measure);
                if (lastPoint.IsWithinTolerance(foundPoint, tolerance))
                    foundPoint = lastPoint;
                else if (thisPoint.IsWithinTolerance(foundPoint, tolerance))
                    foundPoint = thisPoint;
            }
            else
            {
                // it's past this point---just step along the line
                lastPoint = thisPoint;
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
            if (foundPoint != null && !IsPointDerived)
            {
                // We could use a simple point constructor, but by targeting another sink we can use this
                // class in a pipeline.
                target.SetSrid(srid);
                target.BeginGeometry(OpenGisGeometryType.Point);
                target.BeginFigure(foundPoint.STX.Value, foundPoint.STY.Value, foundPoint.Z.IsNull ? (double?)null : foundPoint.Z.Value, foundPoint.M.Value);
                target.EndFigure();
                target.EndGeometry();
                IsPointDerived = true;
            }
        }
    }
}
