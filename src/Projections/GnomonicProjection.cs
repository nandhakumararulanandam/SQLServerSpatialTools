﻿//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// References: http://mathworld.wolfram.com/GnomonicProjection.html
//
// Note: The gnomonic projection is the only projection that maps SqlGeography
//       Polygons and LineString exactly to their SqlGeometry counterparts.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SQLSpatialTools.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Projections
{
	// EPSG Code:    None
	// OGC WKT Name: Gnomonic
	internal sealed class GnomonicProjection : Projection
	{
		// Input argument: the center of the projection
		public GnomonicProjection(Dictionary<String, double> parameters)
			: base(parameters)
		{
			_center = Util.SphericalRadToCartesian(InputLatitude("latitude1", 90), InputLongitude("longitude1", 360));

			// This projection is designed for numerical computations rather than cartography.
			// The choice of coordinate basis for the tangent plane - which affects the 
			// orientation of the projection in the xy plane - is optimized for accuracy rather
			// than good looks. The first basis vector is obtained by dropping the smallest coordinate,
			// switching the other two, and flipping the sign of one of them. The second one is
			// obtained by cross product.

			double[] center = { _center.X, _center.Y, _center.Z };
			var vector = new double[3];

			var k = GetMinEntry(center);
			var j = (k + 2) % 3;
			var i = (j + 2) % 3;

			vector[i] = -center[j];
			vector[j] = center[i];
			vector[k] = 0;

			_xAxis = new Vector3(vector[0], vector[1], vector[2]).Unitize();

			_yAxis = _center.CrossProduct(_xAxis);
		}

		private static int GetMinEntry(IList<double> values)
		{
			var i = 0;
			if (Math.Abs(values[1]) < Math.Abs(values[0]))
				i = 1;
			if (Math.Abs(values[2]) < Math.Abs(values[i]))
				i = 2;
			return i;
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			var vector = Util.SphericalRadToCartesian(latitude, longitude);
			var r = vector * _center;

			if (r < _tolerance)
			{
				throw new ArgumentOutOfRangeException(nameof(latitude), "Input point is too far away from the center of projection.");
			}
			vector = vector / r;

			x = vector * _xAxis;
			y = vector * _yAxis;
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			var vector = _center + _xAxis * x + _yAxis * y;
			latitude = Util.Latitude(vector);
			longitude = Util.Longitude(vector);
		}

		private readonly Vector3 _center;
		private readonly Vector3 _xAxis;
		private readonly Vector3 _yAxis;
		private static readonly double _tolerance = 1e-8;
	}
}