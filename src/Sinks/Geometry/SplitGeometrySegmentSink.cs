// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that split the geometry LINESTRING and MULTILINESTRING into two segments based on split point.
    /// </summary>
    class SplitGeometrySegmentSink : IGeometrySink110
    {
        readonly double splitPointMeasure;
        readonly SqlGeometry splitPoint;

        public SqlGeometry Segment1;    // Where we place our result.
        public SqlGeometry Segment2;    // Where we place our result.

        LRSMultiLine segment1;
        LRSMultiLine segment2;
        LRSLine currentLineForSegment1;
        LRSLine currentLineForSegment2;

        int srid, lineCounter;
        bool isMultiLine;
        bool splitPointReached;
        double lastM;

        // Initialize Split Geom Sink with split point
        public SplitGeometrySegmentSink(SqlGeometry splitPoint)
        {
            this.splitPoint = splitPoint;
            isMultiLine = false;
            lineCounter = 0;
            splitPointMeasure = splitPoint.HasM ? splitPoint.M.Value : 0;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            segment1 = new LRSMultiLine(srid);
            segment2 = new LRSMultiLine(srid);
            this.srid = srid;
        }

        private bool IsEqualToSplitMeasure(double? currentMeasure)
        {
            return (double)currentMeasure == splitPoint.M.Value;
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.MultiLineString)
                isMultiLine = true;
            if (type == OpenGisGeometryType.LineString)
                lineCounter++;
        }

        // Start the figure.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLineForSegment1 = new LRSLine(srid);
            currentLineForSegment2 = new LRSLine(srid);

            // just add it to the second segment once split point is reached
            if (splitPointReached && m != splitPointMeasure)
            {
                currentLineForSegment2.AddPoint(x, y, z, m);
                return;
            }

            if (m < splitPointMeasure)
                currentLineForSegment1.AddPoint(x, y, null, m);
            else if (m > splitPointMeasure || IsEqualToSplitMeasure(m))
            {
                currentLineForSegment2.AddPoint(x, y, null, m);
                splitPointReached = IsEqualToSplitMeasure(m);
            }
            lastM = (double)m;
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // just add it to the second segment once split point is reached
            if (splitPointReached && m != splitPointMeasure)
            {
                currentLineForSegment2.AddPoint(x, y, z, m);
                return;
            }

            // If current measure is less than split measure; then add it to the first segment.
            if (m < splitPointMeasure)
            {
                currentLineForSegment1.AddPoint(x, y, z, m);
            }

            // split measure in between last point measure and current point measure.
            else if (splitPointMeasure > lastM && splitPointMeasure < m)
            {
                currentLineForSegment1.AddPoint(splitPoint);
                currentLineForSegment2.AddPoint(splitPoint);
                currentLineForSegment2.AddPoint(x, y, z, m);
                splitPointReached = true;
            }

            // if current measure is equal to split measure; then it is a shape point
            else if (IsEqualToSplitMeasure(m) && lastM != m)
            {
                currentLineForSegment1.AddPoint(x, y, z, m);
                currentLineForSegment2.AddPoint(x, y, z, m);
                splitPointReached = true;
            }

            // If current measure is greater than split measure; then add it to the second segment.
            else if (m > splitPointMeasure)
            {
                currentLineForSegment2.AddPoint(x, y, z, m);
            }

            // reassign current measure to last measure
            lastM = (double)m;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        // add segments to target
        public void EndGeometry()
        {
            // if not multi line then add the current line to the collection.
            if (!isMultiLine)
            {
                segment1.AddLine(currentLineForSegment1);
                segment2.AddLine(currentLineForSegment2);
            }

            // if line counter is 0 then it is multiline
            // if 1 then it is linestring 
            if (lineCounter == 0 || !isMultiLine)
            {
                Segment1 = segment1.ToSqlGeometry();

                // TODO:: Messy logic to be meet as per Oracle; need to be re-factored
                // if second is a multi line
                // end measure of first line > end measure of last line
                // then consider only first line 
                // by locating the point
                if (!segment1.IsEmpty && segment2.IsMultiLine)
                {
                    var endSegmentEndM = segment2.GetLastLine().GetEndPointM();
                    var startSegmentStartM = segment2.GetFirstLine().GetEndPointM();

                    if (startSegmentStartM > endSegmentEndM)
                    {
                        var trimmedLine = segment2.GetFirstLine();
                        var newLS = new LRSLine(srid);

                        // add points up to end segment measure
                        foreach (var point in trimmedLine)
                        {
                            if (point.M < endSegmentEndM)
                                newLS.AddPoint(point);
                        }

                        // add the end point
                        if (endSegmentEndM == splitPointMeasure)
                            newLS.AddPoint(splitPoint);
                        else
                            newLS.AddPoint(trimmedLine.LocatePoint(endSegmentEndM, newLS.GetEndPoint()));

                        Segment2 = newLS.ToSqlGeometry();
                    }
                    // if end segment measure is equal to split measure; then return the split alone for second segment
                    else if (endSegmentEndM == splitPointMeasure)
                        Segment2 = splitPoint;
                    else
                        Segment2 = segment2.ToSqlGeometry();
                }
                else
                    Segment2 = segment2.ToSqlGeometry();
            }
            else
            {
                if (currentLineForSegment1.IsLine)
                    segment1.AddLine(currentLineForSegment1);

                if (currentLineForSegment2.IsLine)
                    segment2.AddLine(currentLineForSegment2);

                // reset the line counter so that the child line strings chaining is done and return to base multiline type
                lineCounter--;
            }
        }
    }
}
