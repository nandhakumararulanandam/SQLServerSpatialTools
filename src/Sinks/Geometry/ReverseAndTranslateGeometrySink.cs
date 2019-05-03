// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that reverses the input geometry and translate the measure
    /// </summary>
    class ReverseAndTranslateGeometrySink : IGeometrySink110
    {
        SqlGeometryBuilder target;
        readonly double translateMeasure;
        LRSMultiLine lines;
        LRSLine currentLine;
        bool isMultiLine;
        int lineCounter;
        int srid;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and reverse and translate measure it accordingly.
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public ReverseAndTranslateGeometrySink(SqlGeometryBuilder target, double translateMeasure)
        {
            this.target = target;
            this.translateMeasure = translateMeasure;
            isMultiLine = false;
            lineCounter = 0;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            lines = new LRSMultiLine(srid);
            this.srid = srid;
        }

        // Check for types and begin geometry accordingly.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.MultiLineString)
                isMultiLine = true;
            if (type == OpenGisGeometryType.LineString)
                lineCounter++;
        }

        // Just add the points to the current line segment.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLine = new LRSLine(srid);
            currentLine.AddPoint(x, y, z, m);
        }

        // Just add the points to the current line segment.
        public void AddLine(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(x, y, z, m);
        }

        // Reverse the points at the end of figure.
        public void EndFigure()
        {
            currentLine.ReversePoints();
        }

        // This is where real work is done.
        public void EndGeometry()
        {
            // if not multi line then add the current line to the collection.
            if (!isMultiLine)
                lines.AddLine(currentLine);

            // if line counter is 0 then it is multiline
            // if 1 then it is linestring 
            if (lineCounter == 0 || !isMultiLine)
            {
                // reverse the line before constructing the geometry
                lines.ReversLines();
                lines.TranslateMeasure(translateMeasure);
                lines.ToSqlGeometry(ref target);
            }
            else
            {
                lines.AddLine(currentLine);
                // reset the line counter so that the child line strings chaining is done and return to base multiline type
                lineCounter--;
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }
    }
}
