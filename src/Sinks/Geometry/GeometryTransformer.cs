//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types.SQL;

namespace SQLSpatialTools.Sinks.Geometry
{
	public sealed class GeometryTransformer : IGeometrySink110
	{
		readonly IGeometrySink110 sink;
		readonly AffineTransform transform;

		public GeometryTransformer(IGeometrySink110 sink, AffineTransform transform)
		{
			this.sink = sink;
			this.transform = transform;
		}

		public void SetSrid(int srid)
		{
			sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			sink.BeginGeometry(type);
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			sink.BeginFigure(transform.GetX(x, y), transform.GetY(x, y), z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			sink.AddLine(transform.GetX(x, y), transform.GetY(x, y), z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			sink.EndFigure();
		}

		public void EndGeometry()
		{
			sink.EndGeometry();
		}
	}
}