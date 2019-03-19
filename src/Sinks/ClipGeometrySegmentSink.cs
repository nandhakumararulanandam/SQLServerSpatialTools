// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that finds a point along a geography linestring instance and pipes
     * it to another sink.
     */
    class ClipGeometrySegmentSink : IGeometrySink110
    {
        SqlGeometry _startPoint;
        SqlGeometry _endPoint;
        int _srid;                     // The _srid we are working in.
        SqlGeometryBuilder _target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public ClipGeometrySegmentSink(SqlGeometry startPoint, SqlGeometry endPoint, SqlGeometryBuilder target)
        {
            _target = target;
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            _srid = srid;
            _target.SetSrid(_srid);
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            _target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Memorize the starting point.
             _target.BeginFigure(_startPoint.STX.Value, _startPoint.STY.Value, null, _startPoint.M.Value);
        }

        // This is where the real work is done.
        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            // If current measure is betweem start measure and end measure, we should add segment to the result linestring
            if (m > _startPoint.M.Value && m < _endPoint.M.Value)
            {
                _target.AddLine(latitude, longitude, z, m);
            }
                
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            _target.AddLine(_endPoint.STX.Value, _endPoint.STY.Value, null, _endPoint.M.Value);
            _target.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            _target.EndGeometry();
        }

    }

    class ClipGeometrySegmentSink2 : IGeometrySink110
    {
        // Where we place our result
        readonly SqlGeometryBuilder target;
        readonly double startMeasure;
        readonly double endMeasure;

        double lastX;
        double lastY;
        double lastM;
        bool started;
        bool finished;

        // The _srid we are working in.
        int srid;

        /// <summary>
        /// We target another builder, to which we will send a point representing the point we find.
        /// We also take a distance, which is the point along the input linestring we will travel.
        /// Note that we only operate on LineString instances: anything else will throw an exception.
        /// </summary>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <param name="target"></param>
        public ClipGeometrySegmentSink2(double startMeasure, double endMeasure, SqlGeometryBuilder target)
        {
            this.target = target;
            this.startMeasure = startMeasure;
            this.endMeasure = endMeasure;
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
            if (startMeasure == endMeasure)
                target.BeginGeometry(OpenGisGeometryType.Point);
            else
                target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the starting point.
            if (m == startMeasure || m == endMeasure)
            {
                target.BeginFigure(x, y, z, m);
                started = true;
            }
            lastX = x;
            lastY = y;
            lastM = (double)m;
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            if (started && startMeasure == endMeasure)//There's nothing more for us here if point is already created 
                return;
            double startEndMeasure;//To unify code for ascending and descending measures

            // If current measure is between start measure and end measure, we should add segment to the result linestring
            if ((m >= startMeasure && m <= endMeasure) || (m <= startMeasure && m >= endMeasure))
            {
                if (started)
                {
                    target.AddLine(x, y, z, m);
                }
                else //We'll need to begin figure here first
                {
                    if (lastM < m)
                        startEndMeasure = Math.Min(startMeasure, endMeasure);
                    else
                        startEndMeasure = Math.Max(startMeasure, endMeasure);
                    double f = (startEndMeasure - lastM) / ((double)m - lastM);  // The fraction of the way from start to end.
                    double newX = (lastX * (1 - f)) + (x * f);
                    double newY = (lastY * (1 - f)) + (y * f);
                     target.BeginFigure(newX, newY, null, startEndMeasure);
                    started = true;
                    if (startMeasure == endMeasure)
                        return;
                    target.AddLine(x, y, z, m);

                    lastX = x;
                    lastY = y;
                    lastM = (double)m;
                }
            }
            else //We may still need to add last segment, if current point is the first one after we passed range of interest
            {
                if (!started)
                {
                    if (lastM < m)
                        startEndMeasure = Math.Min(startMeasure, endMeasure);
                    else
                        startEndMeasure = Math.Max(startMeasure, endMeasure);
                    if ((startEndMeasure < m && startEndMeasure > lastM) || (startEndMeasure > m && startEndMeasure < lastM))
                    {
                        double f = (startEndMeasure - lastM) / ((double)m - lastM);  // The fraction of the way from start to end.
                        double newX = (lastX * (1 - f)) + (x * f);
                        double newY = (lastY * (1 - f)) + (y * f);
                        target.BeginFigure(newX, newY, null, startEndMeasure);
                        started = true;
                        if (startMeasure == endMeasure)
                            return;
                    }
                }
                if (started && !finished)
                {
                    if (lastM < m)
                        startEndMeasure = Math.Max(startMeasure, endMeasure);
                    else
                        startEndMeasure = Math.Min(startMeasure, endMeasure);
                    if((startEndMeasure < m && startEndMeasure > lastM) || (startEndMeasure > m && startEndMeasure < lastM))
                    {
                        double f = (startEndMeasure - lastM) / ((double)m - lastM);  // The fraction of the way from start to end.
                        double newX = (lastX * (1 - f)) + (x * f);
                        double newY = (lastY * (1 - f)) + (y * f);
                        target.AddLine(newX, newY, null, startEndMeasure);
                        finished = true;
                    }
                }
            }
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
