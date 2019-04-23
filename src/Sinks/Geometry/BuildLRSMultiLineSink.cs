// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that builds LRS multiline.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    class BuildLRSMultiLineSink : IGeometrySink110
    {
        private int srid;
        private LRSLine currentLine;
        public LRSMultiLine MultiLine;

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            MultiLine = new LRSMultiLine(srid);
            this.srid = srid;
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.LineString)
                currentLine = new LRSLine(srid);
        }

        // Just add the points to the current line.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(x, y, z, m);
        }

        // Just add the points to the current line.
        public void AddLine(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(x, y, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // Add the current line to the MULTILINESTRING collection
        public void EndFigure()
        {
            MultiLine.AddLine(currentLine);
        }

        // This is a NO-OP
        public void EndGeometry()
        {
        }
    }
}
