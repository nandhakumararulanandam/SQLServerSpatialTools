// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that resets M value to null.
    /// </summary>
    class ResetMGemetrySink : IGeometrySink110
    {
        SqlGeometryBuilder target;    // Where we place our result.

        public ResetMGemetrySink(SqlGeometryBuilder target)
        {
            this.target = target;
        }

        /// <summary>
        /// Save the SRID for later 
        /// </summary>
        /// <param name="srid">Spatial Reference Identifier</param>
        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
        }

        /// <summary>
        /// Start the geometry.  Throw if it isn't a LineString.
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");

            target.BeginGeometry(type);
        }

        /// <summary>
        /// Start the figure.  
        /// Note that since we only operate on LineStrings, this should only be executed once.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            target.BeginFigure(x, y, z, null);
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            target.AddLine(x, y, z, null);
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
        }
    }
}