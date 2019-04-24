// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geography
{
    /// <summary>
    /// This class implements a geography sink that finds a point along a geography linestring instance and pipes
    /// it to another sink.
    /// </summary>
    class LocateAlongGeographySink : IGeographySink110
    {

        double distance;               // The running count of how much further we have to go.
        SqlGeography lastPoint;        // The last point in the LineString we have passed.
        SqlGeography foundPoint;       // This is the point we're looking for, assuming it isn't null, we're done.
        int srid;                      // The _srid we are working in.
        SqlGeographyBuilder target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public LocateAlongGeographySink(double distance, SqlGeographyBuilder target)
        {
            this.target = target;
            this.distance = distance;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            this.srid = srid;
        }

        // Start the geography.  Throw if it isn't a LineString.
        public void BeginGeography(OpenGisGeographyType type)
        {
            if (type != OpenGisGeographyType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Memorize the point.
            lastPoint = SqlGeography.Point(latitude, longitude, srid);
        }

        // This is where the real work is done.
        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            // If we've already found a point, then we're done.  We just need to keep ignoring these
            // pesky calls.
            if (foundPoint != null) return;

            // Make a point for our current position.
            SqlGeography thisPoint = SqlGeography.Point(latitude, longitude, srid);

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
                foundPoint = Functions.General.Geography.InterpolateBetweenGeog(lastPoint, thisPoint, distance);
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
        public void EndGeography()
        {
            if (foundPoint != null)
            {
                // We could use a simple point constructor, but by targeting another sink we can use this
                // class in a pipeline.
                target.SetSrid(srid);
                target.BeginGeography(OpenGisGeographyType.Point);
                target.BeginFigure(foundPoint.Lat.Value, foundPoint.Long.Value);
                target.EndFigure();
                target.EndGeography();
            }
            else
            {
                throw new ArgumentException("Distance provided is greater then the length of the LineString.");
            }
        }
    }
}
