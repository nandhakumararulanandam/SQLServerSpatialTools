// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that clips a line segment based on measure.
    /// </summary>
    class ClipMultiLineSegmentSink : IGeometrySink110
    {
        readonly double clipStartMeasure;
        readonly double clipEndMeasure;
        readonly double tolerance;
        readonly bool retainClipMeasure;
        readonly SqlGeometryBuilder target;
        readonly LRSPoint clipStartPoint;
        readonly LRSPoint clipEndPoint;

        int srid;
        int lineCounter;
        bool started;
        ClipMGeometrySegmentSink mSink;
        LRSMultiLine multiLines;
        LRSLine currentLine;
        
        /// <summary>
        /// We target another builder, to which we will send a point representing the point we find.
        /// We also take a distance, which is the point along the input linestring we will travel.
        /// Note that we only operate on LineString instances: anything else will throw an exception.
        /// </summary>
        /// <param name="startMeasure">Start Measure to be clipped</param>
        /// <param name="endMeasure">End Measure to be clipped</param>
        /// <param name="target">SqlGeometry builder</param>
        /// <param name="tolerance">tolerance value</param>
        /// <param name="retainClipMeasure">Flag to retain ClipMeasure values</param>
        public ClipMultiLineSegmentSink(SqlGeometryBuilder target, SqlGeometry clipStartPoint, SqlGeometry clipEndPoint, double tolerance, bool retainClipMeasure = false)
        {
            this.target = target;
            this.tolerance = tolerance;
            this.retainClipMeasure = retainClipMeasure;
            this.clipStartPoint = new LRSPoint(clipStartPoint);
            this.clipEndPoint = new LRSPoint(clipEndPoint);
            clipStartMeasure = (double)this.clipStartPoint.m;
            clipEndMeasure = (double)this.clipEndPoint.m;
            started = false;
            lineCounter = 0;
        }

        public void SetSrid(int srid)
        {
            multiLines = new LRSMultiLine(srid);
            this.srid = srid;
        }

        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (!started && type != OpenGisGeometryType.MultiLineString)
                throw new Exception("The geometry must be MULTILINESTRING for this sink.");

            if (type == OpenGisGeometryType.MultiLineString)
                started = true;

            if (type == OpenGisGeometryType.LineString)
                currentLine = new LRSLine(srid);
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            lineCounter++;
            currentLine.AddPoint(x, y, z, m);
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(x, y, z, m);
        }


        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        public void EndGeometry()
        {
            if (lineCounter == 0)
            {
                var clippedLineCount = 0;

                // filter out the line segments falling in the clip start and end measure
                foreach (LRSLine line in multiLines)
                {
                    if (line.IsWithinRange(clipStartMeasure, clipEndMeasure, clipStartPoint, clipEndPoint))
                        clippedLineCount++;
                }

                // MultiLineString
                var isMultiLineResult = clippedLineCount > 1;

                if (isMultiLineResult)
                {
                    mSink = new ClipMGeometrySegmentSink(clipStartMeasure, clipEndMeasure, target, tolerance, retainClipMeasure);
                    mSink.SetSrid(srid);
                    target.BeginGeometry(OpenGisGeometryType.MultiLineString);
                }

                foreach (LRSLine line in multiLines)
                {
                    if (line.IsInRange)
                    {
                        // if result is a single line segment; then the line segments start and end point measure becomes the clip start and end measure
                        // Or update clip start and end measure if the line is completely within range
                        if (!isMultiLineResult || line.IsCompletelyInRange)
                        {
                            mSink = new ClipMGeometrySegmentSink(line.GetStartPointM(), line.GetEndPointM(), target, tolerance, retainClipMeasure);
                            mSink.SetSrid(srid);
                        }

                        if (isMultiLineResult)
                        {
                            mSink.UpdateClipMeasures(
                                 clipStartMeasure: line.GetStartPointM() > clipStartMeasure ? line.GetStartPointM() : clipStartMeasure,
                                 clipEndMeasure: line.GetEndPointM() < clipEndMeasure ? line.GetEndPointM() : clipEndMeasure
                                );
                        }

                        mSink.BeginGeometry(OpenGisGeometryType.LineString);
                        var isFirst = true;
                        foreach (LRSPoint point in line)
                        {
                            if (isFirst)
                            {
                                mSink.BeginFigure(point.x, point.y, point.z, point.m);
                                isFirst = false;
                            }
                            else
                                mSink.AddLine(point.x, point.y, point.z, point.m);
                        }
                        mSink.EndFigure();
                        mSink.EndGeometry();
                    }
                }

                if (isMultiLineResult)
                    target.EndGeometry();
            }
            else
            {
                multiLines.AddLine(currentLine);
            }
            lineCounter--;
        }
    }
}
