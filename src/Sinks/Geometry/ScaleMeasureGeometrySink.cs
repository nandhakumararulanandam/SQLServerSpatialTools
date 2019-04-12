// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that scales the measure values of input geometry.
    /// </summary>
    class ScaleMeasureGeometrySink : IGeometrySink110
    {
        readonly SqlGeometryBuilder target;
        readonly double scaleMeasure;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and scales the measure by given magnitude.
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public ScaleMeasureGeometrySink(SqlGeometryBuilder target, double scaleMeasure)
        {
            this.target = target;
            this.scaleMeasure = scaleMeasure;
        }

        // Just pass through target.
        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
        }

        // Just pass through target.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            target.BeginGeometry(type);
        }

        // Just add the points with updated measure.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            target.BeginFigure(x, y, z, GetUpdatedMeasure(m));
        }

        // Just add the points with updated measure.
        public void AddLine(double x, double y, double? z, double? m)
        {
            target.AddLine(x, y, z, GetUpdatedMeasure(m));
        }

        // Just pass through target.
        public void EndFigure()
        {
            target.EndFigure();
        }

        // Just pass through target.
        public void EndGeometry()
        {
            target.EndGeometry();
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        private double? GetUpdatedMeasure(double? m)
        {
            double? measure = null;
            if (m.HasValue)
                measure = (double)m * scaleMeasure;
            return measure;
        }
    }
}
