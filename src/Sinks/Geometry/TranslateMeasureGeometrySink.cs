// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that translates the measure values of input geometry.
    /// </summary>
    class TranslateMeasureGeometrySink : IGeometrySink110
    {
        readonly SqlGeometryBuilder target;
        readonly double translateMeasure;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and translates its measure.
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public TranslateMeasureGeometrySink(SqlGeometryBuilder target, double translateMeasure)
        {
            this.target = target;
            this.translateMeasure = translateMeasure;
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

        private double GetUpdatedMeasure(double? m)
        {
           return m.HasValue ? (double)m + translateMeasure : translateMeasure;
        }
    }
}
