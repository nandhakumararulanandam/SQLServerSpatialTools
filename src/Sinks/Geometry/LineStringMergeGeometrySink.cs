// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using static SQLSpatialTools.Utility.SQLTypeConversions;

namespace SQLSpatialTools
{
    /// <summary>
    /// Class implements a geometry sink that builds a line string by reducing from the _geometry in the range of points represented by indexes
    /// </summary>
    class LineStringMergeGeometrySink : IGeometrySink110
    {
        private SqlGeometryBuilder target;     /* builder reference to store the reduced range of points */
        private readonly bool isFirstSegment;         /* represent the _geometry would be the first part of the resultant geometry */
        private readonly int numPoints;          /* counter indicates the index of the coordinates present in the _geometry */
        private int indexCounter = 0;          /* counter indicates the index of the coordinates present in the _geometry */

        public LineStringMergeGeometrySink(SqlGeometryBuilder target, bool isFirstSegment, Numeric numPoints)
        {
            this.target = target;
            this.isFirstSegment = isFirstSegment;
            this.numPoints = numPoints;
        }

        /// <summary>
        /// Persisting the srid of the first geometry and using the 
        /// </summary>
        /// <param name="srid"></param>
        public void SetSrid(int srid)
        {
            if (isFirstSegment)
                target.SetSrid(srid);
        }

        /// <summary>
        /// Loop through each geometry types in geom collection and validate if the type is LINESTRING
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.LineString)
            {
                if (isFirstSegment)
                    target.BeginGeometry(type);
            }
            else
                throw new System.Exception("Line string is the only supported type");
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            ++indexCounter;
            if (isFirstSegment)
                target.BeginFigure(x, y, z, m);
            else
                target.AddLine(x, y, z, m);

        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            ++indexCounter;
            // skip merging point for fist segment
            if (!(isFirstSegment && numPoints == indexCounter))
                target.AddLine(x, y, z, m);

        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("Circular arc not yet implemented");

        }

        /// <summary>
        /// End the figure, if its last point
        /// </summary>
        public void EndFigure()
        {
            if (!isFirstSegment)
                target.EndFigure();
        }

        /// <summary>
        /// End geometry construction
        /// </summary>
        public void EndGeometry()
        {
            if (!isFirstSegment)
                target.EndGeometry();
        }
    }
}
