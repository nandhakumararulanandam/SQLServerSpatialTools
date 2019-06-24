//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that extracts the polygon based upon the ring.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    internal class ExtractPolygonGeometrySink : IGeometrySink110
    {
        private int _figureIndexIterator;
        private readonly int _ringIndex;
        private readonly SqlGeometryBuilder _target;
        public bool IsExtracted;

        public ExtractPolygonGeometrySink(SqlGeometryBuilder geomBuilder, int ringIndex = 0)
        {
            _ringIndex = ringIndex;
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
            if (type != OpenGisGeometryType.Polygon)
            {
               SpatialExtensions.ThrowException("ExtractPolygonGeometrySink - Not a Polygon");
            }
            _target.BeginGeometry(type);
        }

        // Just add the points to the current line.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _figureIndexIterator++;
            if (_figureIndexIterator == _ringIndex)
            {
                IsExtracted = true;
                _target.BeginFigure(x, y, z, m);
            }
        }

        // Just add the points to the current line.
        public void AddLine(double x, double y, double? z, double? m)
        {
            if (_figureIndexIterator == _ringIndex)
                _target.AddLine(x, y, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // Add the current line to the MULTILINESTRING collection
        public void EndFigure()
        {
            if (_figureIndexIterator == _ringIndex)
                _target.EndFigure();
        }

        // This is a NO-OP
        public void EndGeometry()
        {
            if (_figureIndexIterator == _ringIndex)
                _target.EndGeometry();
        }
    }
}
