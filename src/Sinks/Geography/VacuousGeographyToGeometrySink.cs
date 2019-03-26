// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a completely trivial conversion from geography to geometry, simply taking each
    /// point(lat, long) --> (y, x).  The class takes a target geometry sink, as well as the target SRID to
    /// assign to the results.
    /// </summary>
	public class VacuousGeographyToGeometrySink : IGeographySink110
	{
		private readonly IGeometrySink110 target;
		private readonly int targetSrid;

		public VacuousGeographyToGeometrySink(int targetSrid, IGeometrySink110 target)
		{
			this.target = target;
			this.targetSrid = targetSrid;
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			target.AddLine(longitude, latitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			target.BeginFigure(longitude, latitude, z, m);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			// Convert geography to geometry types...
			target.BeginGeometry((OpenGisGeometryType) type);
		}

		public void EndFigure()
		{
			target.EndFigure();
		}

		public void EndGeography()
		{
			target.EndGeometry();
		}

		public void SetSrid(int srid)
		{
			target.SetSrid(targetSrid);
		}
	}
}
