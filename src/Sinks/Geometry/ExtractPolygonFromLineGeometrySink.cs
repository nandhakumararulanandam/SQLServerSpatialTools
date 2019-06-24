//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that extracts the polygon based upon the linestring.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    internal class ExtractPolygonFromLineGeometrySink : IGeometrySink110
    {
        private readonly SqlGeometryBuilder _target;

        public ExtractPolygonFromLineGeometrySink(SqlGeometryBuilder geomBuilder)
        {
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
            _target.BeginGeometry(OpenGisGeometryType.Polygon);
        }

        // Just add the points to the current line.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _target.BeginFigure(x, y, z, m);
        }

        // Just add the points to the current line.
        public void AddLine(double x, double y, double? z, double? m)
        {
            _target.AddLine(x, y, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // Add the current line to the MULTILINESTRING collection
        public void EndFigure()
        {
            _target.EndFigure();
        }

        // This is a NO-OP
        public void EndGeometry()
        {
            _target.EndGeometry();
        }
    }
}
