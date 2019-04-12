﻿// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that builds LRS multiline.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    class BuidLRSMultiLineSink : IGeometrySink110
    {
        private LRSLine currentLine;
        public LRSMultiLine Lines;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineMergeGeometrySink"/> class.
        /// </summary>
        public BuidLRSMultiLineSink()
        {
            Lines = new LRSMultiLine();
        }

        // This is a NO-OP
        public void SetSrid(int srid)
        {
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.LineString)
                currentLine = new LRSLine();
        }

        // Start the figure.  
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(new LRSPoint(x, y, z, m));
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            currentLine.AddPoint(new LRSPoint(x, y, z, m));
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NO-OP
        public void EndFigure()
        {
            Lines.AddLine(currentLine);
        }

        // This is a NO-OP
        public void EndGeometry()
        {
        }
    }
}
