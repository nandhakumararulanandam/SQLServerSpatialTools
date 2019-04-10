// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System.Collections.Generic;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that checks whether the geometry collection is of supported types.
    /// </summary>
    class ReverseLinearGeometrySink : IGeometrySink110
    {
        #region DS

        class Line
        {
            public List<Point> Points;

            public Line()
            {
                Points = new List<Point>();
            }

            public void AddPoint(Point point)
            {
                Points.Add(point);
            }

        }
        struct Point
        {
            public double x;
            public double y;
            public double? z;
            public double? m;

            public Point(double x, double y, double? z, double? m)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.m = m;
            }
        }

        #endregion

        readonly SqlGeometryBuilder target;
        List<Line> lines;
        Line currentLine;
        int srid;
        bool isMultiLine = false;

        int lineCounter = 0;
        /// <summary>
        /// Loop through each geometry types in geom collection and validate if the type is either POINT, LINESTRING, MULTILINESTRING
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public ReverseLinearGeometrySink(SqlGeometryBuilder target)
        {
            this.target = target;
            lines = new List<Line>();
        }
        /// 
        public void SetSrid(int srid)
        {
            this.srid = srid;
            target.SetSrid(this.srid);
        }
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

        // This is a NOP.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLine = new Line();
            currentLine.AddPoint(new Point(x, y, z, m));
        }

        // This is a NOP.
        public void AddLine(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(new Point(x, y, z, m));
        }

        // This is a NOP.
        public void EndFigure()
        {
            currentLine.Points.Reverse();
        }

        // This is a NOP.
        public void EndGeometry()
        {
            if (!isMultiLine)
                lines.Add(currentLine);

            if (lineCounter == 0 || !isMultiLine)
            {
                lines.Reverse();
                foreach (Line line in lines)
                {
                    target.BeginGeometry(OpenGisGeometryType.LineString);

                    var pointIterator = 1;
                    foreach (Point point in line.Points)
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
                lines.Add(currentLine);
                lineCounter--;
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.NotImplementedException();
        }
    }
}
