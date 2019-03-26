//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class projects a geography segment based on specified project to a geometry segment.
    /// </summary>
	public sealed class Projector : IGeographySink110
	{
		private readonly SqlProjection projection;
		private readonly IGeometrySink110 sink;

		public Projector(SqlProjection projection, IGeometrySink110 sink)
		{
			this.projection = projection;
			this.sink = sink;
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			sink.BeginGeometry((OpenGisGeometryType)type);
		}

		public void EndGeography()
		{
			sink.EndGeometry();
		}

		public void BeginFigure(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
		{
            projection.ProjectPoint(latitude, longitude, out double x, out double y);
            sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
		{
            projection.ProjectPoint(latitude, longitude, out double x, out double y);
            sink.AddLine(x, y, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			sink.SetSrid(srid);
		}
	}
}