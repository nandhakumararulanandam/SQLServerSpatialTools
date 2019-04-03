// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that checks whether the geometry collection is of supported types.
    /// </summary>
    class ValidateLinearMeasureGeometrySink : IGeometrySink110
    {
        readonly LinearMeasureProgress linearMeasureProgress;
        readonly SqlGeometryBuilder target;
        double lastM;
        bool isLinearMeasure = true;
        bool geomStarted = true;

        public ValidateLinearMeasureGeometrySink(SqlGeometryBuilder target, LinearMeasureProgress linearMeasureProgress)
        {
            this.target = target;
            this.linearMeasureProgress = linearMeasureProgress;
        }

        public bool IsLinearMeasure()
        {
            return isLinearMeasure;
        }

        // This is a NOP.
        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
        }

        /// <summary>
        /// Loop through each geometry types in geom collection and validate if the type is either POINT, LINESTRING, MULTILINESTRING
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.GeometryCollection)
                return;
            target.BeginGeometry(type);
        }

        // This is a NOP.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (geomStarted)
            {
                lastM = (double)m;
                geomStarted = false;
            }
            else
            {
                ValidateMeasure(m);
            }
            target.BeginFigure(x, y, z, m);
        }

        // This is a NOP.
        public void AddLine(double x, double y, double? z, double? m)
        {
            ValidateMeasure(m);
            target.AddLine(x, y, z, m);
        }


        // This is a NOP.
        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
        }

        // This is a NOP.
        public void EndFigure()
        {
            target.EndFigure();
        }

        // This is a NOP.
        public void EndGeometry()
        {
            target.EndGeometry();
        }

        private void ValidateMeasure(double? m)
        {
            var currentM = (double)m;
            if (isLinearMeasure && linearMeasureProgress == LinearMeasureProgress.Increasing)
            {
                if (currentM < lastM)
                    isLinearMeasure = false;
            }
            else if (isLinearMeasure && linearMeasureProgress == LinearMeasureProgress.Decreasing)
            {
                if (currentM > lastM)
                    isLinearMeasure = false;
            }
            lastM = currentM;
        }
    }
}
