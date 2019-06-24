//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that builds LRS multiline.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    internal class ExtractGeometrySink : IGeometrySink110
    {
        private int _figureIndexIterator;
        private int _figureSubIndexIterator;
        private bool _figurePlotted, _figureEnded, _entryPoint, _isFigureStarted;
        private readonly int _elementIndex;
        private readonly int _elementSubIndex;
        private readonly SqlGeometryBuilder _target;
        private OpenGisGeometryType _lastType, _shapeType;
        public bool IsExtracted;

        public ExtractGeometrySink(SqlGeometryBuilder geomBuilder, int elementIndex, int elementSubIndex = 0)
        {
            _elementIndex = elementIndex;
            _elementSubIndex = elementSubIndex;
            _target = geomBuilder;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (!_entryPoint)
            {
                _shapeType = type;
                _entryPoint = true;
            }

            if (!_figurePlotted &&
                 (type == OpenGisGeometryType.Point
                 || type == OpenGisGeometryType.LineString
                 || type == OpenGisGeometryType.CircularString
                 || type == OpenGisGeometryType.Polygon
                 ))
            {
                _lastType = type;
            }
        }

        // Just add the points to the current line.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (_figurePlotted)
                return;

            _figureIndexIterator++;
            if (DoExtract())
            {
                if (!_isFigureStarted)
                {
                    IsExtracted = true;
                    _target.BeginGeometry(_lastType);
                    _isFigureStarted = true;
                }

                _target.BeginFigure(x, y, z, m);
            }
        }

        // Just add the points to the current line.
        public void AddLine(double x, double y, double? z, double? m)
        {
            if (DoExtract())
                _target.AddLine(x, y, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // Add the current line to the MULTILINESTRING collection
        public void EndFigure()
        {
            if (DoExtract())
            {
                // don't end
                if (GetCompareIndex() != 0)
                    _figureEnded = true;
                _target.EndFigure();
            }
        }

        // This is a NO-OP
        public void EndGeometry()
        {
            var isZeroIndex = GetCompareIndex() == 0;
            if (!_figureEnded)
            {
                // don't end
                if (!isZeroIndex)
                    _figureEnded = true;
                _target.EndGeometry();
            }
        }


        private bool DoExtract()
        {
            if (_figurePlotted)
                return false;

            var compareIndex = GetCompareIndex();
            // if index is zero then consider entire segment
            if (compareIndex == 0)
                return true;

            return _figureIndexIterator == compareIndex;
        }

        private int GetCompareIndex()
        {
            var compareIndex = _elementIndex;

            if (_shapeType == OpenGisGeometryType.Polygon)
                compareIndex = _elementSubIndex;

            return compareIndex;
        }

    }
}
