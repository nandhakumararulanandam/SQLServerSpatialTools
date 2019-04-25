// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;
using static SQLSpatialTools.Utility.SQLTypeConversions;

namespace SQLSpatialTools.Sinks.Geometry
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
        int lineCounter;
        int srid;                     // The _srid we are working in.
        double currentLength;
        double currentPointM;
        SqlGeometry target;    // Where we place our result.

        /// <summary>
        /// Gets the constructed geometry.
        /// </summary>
        /// <returns></returns>
        public SqlGeometry GetConstructedGeom()
        {
            return target;
        }

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public PopulateGeometryMeasuresSink(double startMeasure, DecimalValue endMeasure, double length)
        {
            this.startMeasure = startMeasure;
            this.endMeasure = endMeasure;
            totalLength = length;
            isMultiLine = false;
            lineCounter = 0;
            currentPointM = startMeasure;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            lines = new LRSMultiLine(srid);
            this.srid = srid;
        }

        // Start geometry and check if the type is of the supported types
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.MultiLineString)
                isMultiLine = true;
            else if (type == OpenGisGeometryType.LineString)
                lineCounter++;
        }


        // This operates on LineStrings, multi linestring
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLine = new LRSLine(srid);
            currentLine.AddPoint(x, y, null, currentPointM);

            // Memorize the starting point.
            lastPoint = SqlGeometry.Point(x, y, srid);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            thisPoint = SqlGeometry.Point(x, y, srid);
            currentLength += lastPoint.STDistance(thisPoint).Value;
            currentLine.AddPoint(x, y, null, GetCurrentMeasure());

            // reset the last point with the current point.
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
                target = lines.ToSqlGeometry();
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
            currentPointM = startMeasure + (currentLength / totalLength) * (endMeasure - startMeasure);
            return currentPointM;
        }
    }
}
