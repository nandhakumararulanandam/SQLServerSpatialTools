//------------------------------------------------------------------------------
// Copyright (c) 2010 Microsoft Corporation.
//------------------------------------------------------------------------------
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Types
{
    struct Vertex
	{
        readonly double x;
        readonly double y;
		double? z;
		double? m;

		public Vertex(double x, double y, double? z, double? m)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.m = m;
		}

		public void BeginFigure(IGeometrySink110 sink) { sink.BeginFigure(x, y, z, m); }
		public void AddLine(IGeometrySink110 sink) { sink.AddLine(x, y, z, m); }

		public void BeginFigure(IGeographySink110 sink) { sink.BeginFigure(x, y, z, m); }
		public void AddLine(IGeographySink110 sink) { sink.AddLine(x, y, z, m); }
	}
}