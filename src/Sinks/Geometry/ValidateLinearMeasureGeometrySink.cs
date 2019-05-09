//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that checks whether the geometry collection is of supported types.
    /// </summary>
    internal class ValidateLinearMeasureGeometrySink : IGeometrySink110
    {
        private readonly LinearMeasureProgress _linearMeasureProgress;
        private readonly SqlGeometryBuilder _target;
        private double _lastM;
        private bool _isLinearMeasure = true;
        private bool _geomStarted = true;

        public ValidateLinearMeasureGeometrySink(SqlGeometryBuilder target, LinearMeasureProgress linearMeasureProgress)
        {
            _target = target;
            _linearMeasureProgress = linearMeasureProgress;
        }

        public bool IsLinearMeasure()
        {
            return _isLinearMeasure;
        }

        // This is a NOP.
        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        /// <summary>
        /// Loop through each geometry types in geom collection and validate if the type is either POINT, LINESTRING, MULTILINESTRING
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.GeometryCollection)
                return;
            _target.BeginGeometry(type);
        }

        // This is a NOP.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (_geomStarted)
            {
                if (m != null) _lastM = (double) m;
                _geomStarted = false;
            }
            else
            {
                ValidateMeasure(m);
            }
            _target.BeginFigure(x, y, z, m);
        }

        // This is a NOP.
        public void AddLine(double x, double y, double? z, double? m)
        {
            ValidateMeasure(m);
            _target.AddLine(x, y, z, m);
        }


        // This is a NOP.
        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
        }

        // This is a NOP.
        public void EndFigure()
        {
            _target.EndFigure();
        }

        // This is a NOP.
        public void EndGeometry()
        {
            _target.EndGeometry();
        }

        private void ValidateMeasure(double? m)
        {
            if (m == null) return;
            var currentM = (double)m;
            if (_isLinearMeasure && _linearMeasureProgress == LinearMeasureProgress.Increasing)
            {
                if (currentM < _lastM)
                    _isLinearMeasure = false;
            }
            else if (_isLinearMeasure && _linearMeasureProgress == LinearMeasureProgress.Decreasing)
            {
                if (currentM > _lastM)
                    _isLinearMeasure = false;
            }
            _lastM = currentM;
        }
    }
}
