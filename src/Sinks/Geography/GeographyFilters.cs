//------------------------------------------------------------------------------
// Copyright (c) 2010 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geography
{
	public class GeographyEmptyShapeFilter : IGeographySink110
	{
		private IGeographySink110 sink;
		private Queue<OpenGisGeographyType> types = new Queue<OpenGisGeographyType>();
		private bool root = true;

		public GeographyEmptyShapeFilter(IGeographySink110 sink)
		{
			this.sink = sink;
		}

		public void SetSrid(int srid)
		{
			sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (root)
			{
				root = false;
				sink.BeginGeography(type);
			}
			else
			{
				types.Enqueue(type);
			}
		}

		public void EndGeography()
		{
			if (types.Count > 0)
				types.Dequeue();
			else
				sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			while (types.Count > 0)
				sink.BeginGeography(types.Dequeue());
			sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			sink.EndFigure();
		}
	}

	public class GeographyPointFilter : IGeographySink110
	{
		private IGeographySink110 sink;
		private int depth;
		private bool root = true;

		public GeographyPointFilter(IGeographySink110 sink)
		{
			this.sink = sink;
		}

		public void SetSrid(int srid)
		{
			sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (type == OpenGisGeographyType.Point || type == OpenGisGeographyType.MultiPoint)
			{
				if (root)
				{
					root = false;
					sink.BeginGeography(OpenGisGeographyType.GeometryCollection);
					sink.EndGeography();
				}
				depth++;
			}
			else
			{
				root = false;
				sink.BeginGeography(type);
			}
		}

		public void EndGeography()
		{
			if (depth > 0)
				depth--;
			else
				sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (depth == 0)
				sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (depth == 0)
				sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (depth == 0)
				sink.EndFigure();
		}
	}

	public class GeographyShortLineStringFilter : IGeographySink110
	{
		private IGeographySink110 sink;
		private readonly double tolerance;
		private int srid;
		private bool insideLineString;
		private List<Vertex> figure = new List<Vertex>();

		public GeographyShortLineStringFilter(IGeographySink110 sink, double tolerance)
		{
			this.sink = sink;
			this.tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			this.srid = srid;
			sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			sink.BeginGeography(type);
			insideLineString = type == OpenGisGeographyType.LineString;
		}

		public void EndGeography()
		{
			sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (insideLineString)
			{
				figure.Clear();
				figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (insideLineString)
			{
				figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				sink.AddLine(latitude, longitude, z, m);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (insideLineString)
			{
				if (!IsShortLineString())
				{
					PopulateFigure(sink);
				}
			}
			else
			{
				sink.EndFigure();
			}
		}

		private bool IsShortLineString()
		{
			try
			{
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(srid);
				b.BeginGeography(OpenGisGeographyType.LineString);
				PopulateFigure(b);
				b.EndGeography();
				SqlGeography g = b.ConstructedGeography;
				return g.STLength().Value < tolerance;
			}
			catch (FormatException) { }
			catch (ArgumentException) { }
			return false;
		}

		private void PopulateFigure(IGeographySink110 sink)
		{
			figure[0].BeginFigure(sink);
			for (int i = 1; i < figure.Count; i++)
				figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}

	public class GeographyThinRingFilter : IGeographySink110
	{
		private IGeographySink110 sink;
		private readonly double tolerance;
		private bool insidePolygon;
		private int srid;
		private List<Vertex> figure = new List<Vertex>();

		public GeographyThinRingFilter(IGeographySink110 sink, double tolerance)
		{
			this.sink = sink;
			this.tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			this.srid = srid;
			sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			sink.BeginGeography(type);
			insidePolygon = type == OpenGisGeographyType.Polygon;
		}

		public void EndGeography()
		{
			sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (insidePolygon)
			{
				figure.Clear();
				figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (insidePolygon)
			{
				figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				sink.AddLine(latitude, longitude, z, m);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (insidePolygon)
			{
				if (!IsThinRing())
				{
					PopulateFigure(sink, false);
				}
			}
			else
			{
				sink.EndFigure();
			}
		}

		private bool IsThinRing()
		{
			SqlGeography poly = RingToPolygon(true);
			if (poly == null)
			{
				// ring was not valid, try with different orientation
				poly = RingToPolygon(false);
				if (poly == null)
				{
					// if both orientations are invalid, we are dealing with very thin ring
					// so just return true
					return true;
				}
			}
			return poly.STArea().Value < tolerance * poly.STLength().Value;
		}

		private SqlGeography RingToPolygon(bool reverse)
		{
			try
			{
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(srid);
				b.BeginGeography(OpenGisGeographyType.Polygon);
				PopulateFigure(b, reverse);
				b.EndGeography();
				return b.ConstructedGeography;
			}
			catch (FormatException) { }
			catch (ArgumentException) { }
			return null;
		}

		private void PopulateFigure(IGeographySink110 sink, bool reverse)
		{
			if (reverse)
			{
				figure[figure.Count - 1].BeginFigure(sink);
				for (int i = figure.Count - 2; i >= 0; i--)
					figure[i].AddLine(sink);
			}
			else
			{
				figure[0].BeginFigure(sink);
				for (int i = 1; i < figure.Count; i++)
					figure[i].AddLine(sink);
			}
			sink.EndFigure();
		}
	}
}