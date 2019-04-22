// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that builds Multiline from Line String
    /// </summary>
    class BuildMultiLineFromLinesSink : IGeometrySink110
    {
        SqlGeometryBuilder target;
        bool isFirstPoint;
        int linesCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildMultiLineFromLinesSink"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="linesCount">The lines count.</param>
        public BuildMultiLineFromLinesSink(SqlGeometryBuilder target, int linesCount)
        {
            this.target = target;
            this.linesCount = linesCount;
            isFirstPoint = true;
        }

        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.Point)
                return;

            if (type != OpenGisGeometryType.LineString)
                SpatialExtensions.ThrowException(ErrorMessage.LineStringCompatible);

            if (isFirstPoint)
            {
                isFirstPoint = false;
                target.BeginGeometry(OpenGisGeometryType.MultiLineString);
            }
            target.BeginGeometry(type);
            linesCount--;
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            target.BeginFigure(x, y, z, m);
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            target.AddLine(x, y, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
        {
            target.EndFigure();
        }

        public void EndGeometry()
        {
            target.EndGeometry();

            // end of multi line
            if (linesCount == 0)
                target.EndGeometry();
        }
    }
}
