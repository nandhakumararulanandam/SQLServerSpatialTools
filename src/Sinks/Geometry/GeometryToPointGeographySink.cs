//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// Sink which extracts points from a geometry instance and forwards them to a geography sink.
    /// </summary>
    public sealed class GeometryToPointGeographySink : IGeometrySink110
	{
		private readonly IGeographySink110 sink;
		private int count;

		public GeometryToPointGeographySink(IGeographySink110 sink)
		{
			this.sink = sink;
			count = 0;
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (count == 0)
			{
				sink.BeginGeography(OpenGisGeographyType.MultiPoint);
			}
			count++;
		}

		public void EndGeometry()
		{
			count--;
			if (count == 0)
			{
				sink.EndGeography();
			}
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			sink.BeginGeography(OpenGisGeographyType.Point);
			sink.BeginFigure(y, x, z, m);
			sink.EndFigure();
			sink.EndGeography();
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			BeginFigure(x, y, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			// we ignore these calls
		}

		public void SetSrid(int srid)
		{
			sink.SetSrid(srid);
		}
	}
}