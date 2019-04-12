// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that reverses the input geometry.
    /// </summary>
    class ReverseLinearGeometrySink : IGeometrySink110
    {
        readonly SqlGeometryBuilder target;
        LRSMultiLine lines;
        LRSLine currentLine;
        bool isMultiLine;
        int lineCounter;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and reverse it accordingly.
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public ReverseLinearGeometrySink(SqlGeometryBuilder target)
        {
            this.target = target;
            lines = new LRSMultiLine();
            isMultiLine = false;
            lineCounter = 0;
        }

        // This is a NOP.
        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
        }

        // Check for types and begin geometry accordingly.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            // check if the type is of the supported types
            if (type == OpenGisGeometryType.MultiLineString)
            {
                isMultiLine = true;
                target.BeginGeometry(OpenGisGeometryType.MultiLineString);
            }
            if (type == OpenGisGeometryType.LineString)
            {
                lineCounter++;
            }
        }

        // Just add the points to the current line segment.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLine = new LRSLine();
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
                foreach (LRSLine line in lines.Lines)
                {
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
                if(isMultiLine)
                    target.EndGeometry();
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
