// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;
using static SQLSpatialTools.Utility.SQLTypeConversions;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that populate measures for each point in a geometry .
    /// </summary>
    class PopulateGeometryMeasuresSink : IGeometrySink110
    {
        SqlGeometry lastPoint;
        SqlGeometry thisPoint;

        LRSMultiLine lines;
        LRSLine currentLine;
        
        readonly double startMeasure;
        readonly double endMeasure;
        readonly double totalLength;

        bool isMultiLine;
        bool isLinestring;
        int lineCounter;
        int srid;                     // The _srid we are working in.
        double currentLength;
        bool ispoint = false;
        SqlGeometryBuilder target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public PopulateGeometryMeasuresSink(double startMeasure, DecimalValue endMeasure, double length, SqlGeometryBuilder target)
        {
            this.target = target;
            this.startMeasure = startMeasure;
            this.endMeasure = endMeasure;
            totalLength = length;
            lines = new LRSMultiLine();
            isMultiLine = false;
            isLinestring = false;
            lineCounter = 0;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            this.srid = srid;
            target.SetSrid(srid);
        }

        // Start geometry and check if the type is of the supported types
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.MultiLineString)
            {
                isMultiLine = true;
                target.BeginGeometry(OpenGisGeometryType.MultiLineString);
            }
            else if (type == OpenGisGeometryType.LineString)
            {
                lineCounter++;
            }
            else if (type == OpenGisGeometryType.Point)
            {
                ispoint = true;
            }
        }


        // This operates on LineStrings, multi linestring and point these should only be executed
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Memorize the starting point.
            lastPoint = SqlGeometry.Point(x, y, srid);
            currentLine = new LRSLine();
            if (ispoint)
            {
                currentLine.AddPoint(x, y, null, endMeasure);
            }
            else if (!isLinestring)
            {
                currentLine.AddPoint(x, y, null, startMeasure);
                isLinestring = true;
            }
            else
            {
                currentLine.AddPoint(x, y, null, GetCurrentMeasure());
            }
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            thisPoint = SqlGeometry.Point(x, y, srid);
            currentLength += lastPoint.STDistance(thisPoint).Value;
            currentLine.AddPoint(x, y, null, GetCurrentMeasure());
            lastPoint = thisPoint;
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
        public void EndGeometry()
        {
            // if not multi line then add the current line to the collection.
            if (!isMultiLine)
                lines.AddLine(currentLine);

            // if line counter is 0 then it is multiline
            // if 1 then it is linestring 
            if (lineCounter == 0 || !isMultiLine)
            {
                foreach (LRSLine line in lines.Lines)
                {
                    // Point Check
                    if (line.Points.Count == 1)
                        target.BeginGeometry(OpenGisGeometryType.Point);
                    else
                        target.BeginGeometry(OpenGisGeometryType.LineString);

                    var pointIterator = 1;
                    foreach (LRSPoint point in line.Points)
                    {
                        if (pointIterator == 1)
                            target.BeginFigure(point.x, point.y, point.z, point.m);
                        else
                            target.AddLine(point.x, point.y, point.z, point.m);
                        pointIterator++;
                    }
                    target.EndFigure();
                    target.EndGeometry();
                }
                if (isMultiLine)
                    target.EndGeometry();
            }
            else
            {
                lines.AddLine(currentLine);
                // reset the line counter so that the child line strings chaining is done and return to base multiline type
                lineCounter--;
            }
        }

        private double GetCurrentMeasure()
        {
           return startMeasure + (currentLength / totalLength) * (endMeasure - startMeasure);
        }
    }
}
