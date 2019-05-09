﻿//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Aggregates
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[SqlUserDefinedAggregate(
		Format.Native,
		IsInvariantToDuplicates = true,
		IsInvariantToNulls = true,
		IsInvariantToOrder = true,
		IsNullIfEmpty = true)]
	public class GeometryEnvelopeAggregate : IGeometrySink110
	{
		private double minX, maxX, minY, maxY;
		private int lastSrid;
		private bool failed;

		public void SetSrid(int srid)
		{
			if (lastSrid != -1 && lastSrid != srid) failed = true;
			lastSrid = srid;
		}

		public void BeginGeometry(OpenGisGeometryType type) { }

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			IncludePoint(x, y);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			IncludePoint(x, y);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure() { }

		public void EndGeometry() {	}

		public void Init()
		{
			minX = minY = double.PositiveInfinity;
			maxX = maxY = double.NegativeInfinity;
			lastSrid = -1;
			failed = false;
		}

		public void Accumulate(SqlGeometry geometry)
        {
            geometry?.Populate(this);
        }

		public void Merge(GeometryEnvelopeAggregate group)
		{
			minX = Math.Min(minX, group.minX);
			maxX = Math.Max(maxX, group.maxX);
			minY = Math.Min(minY, group.minY);
			maxY = Math.Max(maxY, group.maxY);

			if (group.lastSrid != -1)
			{
				if (lastSrid != -1 && lastSrid != group.lastSrid) failed = true;
				lastSrid = group.lastSrid;
			}
			if (group.failed) failed = true;
		}

		public SqlGeometry Terminate()
		{
			if (failed) return SqlGeometry.Null;

			var builder = new SqlGeometryBuilder();
			builder.SetSrid(lastSrid);
			builder.BeginGeometry(OpenGisGeometryType.Polygon);
			builder.BeginFigure(minX, minY);
			builder.AddLine(maxX, minY);
			builder.AddLine(maxX, maxY);
			builder.AddLine(minX, maxY);
			builder.AddLine(minX, minY);
			builder.EndFigure();
			builder.EndGeometry();
			return builder.ConstructedGeometry;
		}

		private void IncludePoint(double x, double y)
		{
			minX = Math.Min(minX, x);
			maxX = Math.Max(maxX, x);
			minY = Math.Min(minY, y);
			maxY = Math.Max(maxY, y);
		}
	}
}