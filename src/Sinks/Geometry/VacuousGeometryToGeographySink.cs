﻿// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a completely trivial conversion from geometry to geography, simply taking each
    /// point(x, y) --> (long, lat).  The class takes a target geography sink, as well as the target SRID to
    /// assign to the results.
    /// </summary>
    public class VacuousGeometryToGeographySink : IGeometrySink110
	{
		private readonly IGeographySink110 target;
		private readonly int targetSrid;

		public VacuousGeometryToGeographySink(int targetSrid, IGeographySink110 target)
		{
			this.target = target;
			this.targetSrid = targetSrid;
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			target.AddLine(y, x, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void BeginFigure(double x, double y, double? z, double? m)
		{
			target.BeginFigure(y, x, z, m);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			// Convert geography to geometry types...
			target.BeginGeography((OpenGisGeographyType) type);
		}

		public void EndFigure()
		{
			target.EndFigure();
		}

		public void EndGeometry()
		{
			target.EndGeography();
		}

		public void SetSrid(int srid)
		{
			target.SetSrid(targetSrid);
		}
	}
}
