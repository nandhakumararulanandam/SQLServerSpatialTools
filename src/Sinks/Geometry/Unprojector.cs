//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public sealed class Unprojector : IGeometrySink110
	{
		private readonly SqlProjection projection;
		private readonly IGeographySink110 sink;

		public Unprojector(SqlProjection projection, IGeographySink110 sink, int newSrid)
		{
			this.projection = projection;
			this.sink = sink;
			this.sink.SetSrid(newSrid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			sink.BeginGeography((OpenGisGeographyType)type);
		}

		public void EndGeometry()
		{
			sink.EndGeography();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
            projection.UnprojectPoint(x, y, out double latitude, out double longitude);
            sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
            projection.UnprojectPoint(x, y, out double latitude, out double longitude);
            sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			// Input argument not used since a new srid is defined in the constructor.
		}
	}
}