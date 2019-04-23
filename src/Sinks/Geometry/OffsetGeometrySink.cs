// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;
using System.Linq;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that returns the offset geom segment from a clipped segment.
    /// </summary>
    public class OffsetGeometrySink : IGeometrySink110
    {
        readonly double offset;
        readonly SqlGeometryBuilder target;
        readonly LinearMeasureProgress progress;

        int srid;
        LRSLine line, parallelLine;

        public OffsetGeometrySink(SqlGeometryBuilder target, double offset, LinearMeasureProgress progress)
        {
            this.target = target;
            this.offset = offset;
            this.progress = progress;
        }

        // This is a NOP.
        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
            line = new LRSLine(srid);
            parallelLine = new LRSLine(srid);
            this.srid = srid;
        }

        /// <summary>
        /// Loop through each geometry types and collect the points
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException(ErrorMessage.LineStringCompatible);

            target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Just add the input points
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            line.AddPoint(x, y, z, m);
        }

        // Just add the input points
        public void AddLine(double x, double y, double? z, double? m)
        {
            line.AddPoint(x, y, z, m);
        }

        // This is a NOP.
        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
        }

        // This is where the real work is done.
        public void EndFigure()
        {
            parallelLine = line.ComputeParallelLine(offset, progress);
            var pointIterator = 1;

            if (parallelLine != null && parallelLine.Points.Any())
            {
                foreach (var point in parallelLine.Points)
                {
                    if (pointIterator == 1)
                        target.BeginFigure(point.X, point.Y, null, point.M);
                    else
                        target.AddLine(point.X, point.Y, null, point.M);
                    pointIterator++;
                }
                target.EndFigure();
            }
        }

        // This is a NOP.
        public void EndGeometry()
        {
            target.EndGeometry();
        }
    }
}
