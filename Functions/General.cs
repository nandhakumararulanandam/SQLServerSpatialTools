// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;
using System.Collections.Generic;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Function
{
    // This class contains functions that can be registered in SQL Server.
    public class General
    {
        // 1 cm tolerance in most SRIDs
        public const double THRESHOLD = 0.01;

        public class Geometry
        {
            // Selectively filter unwanted artifacts in input object:
            //	- empty shapes (if [filterEmptyShapes] is true)
            //	- points (if [filterPoints] is true)
            //	- linestrings shorter than provided tolerance (if lineString.STLength < [lineStringTolerance])
            //	- polygon rings thinner than provied tolerance (if ring.STArea < ring.STLength * [ringTolerance])
            //	- general behaviour: Returned spatial objects will always to the simplest OGC construction
            //
            public static SqlGeometry FilterArtifactsGeometry(SqlGeometry g, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)
            {
                if (g == null || g.IsNull)
                    return g;

                SqlGeometryBuilder b = new SqlGeometryBuilder();
                IGeometrySink110 filter = b;

                if (filterEmptyShapes)
                    filter = new GeometryEmptyShapeFilter(filter);
                if (ringTolerance > 0)
                    filter = new GeometryThinRingFilter(filter, ringTolerance);
                if (lineStringTolerance > 0)
                    filter = new GeometryShortLineStringFilter(filter, lineStringTolerance);
                if (filterPoints)
                    filter = new GeometryPointFilter(filter);

                g.Populate(filter);
                g = b.ConstructedGeometry;

                if (g == null || g.IsNull || !g.STIsValid().Value)
                    return g;

                // Strip collections with single element
                while (g.STNumGeometries().Value == 1 && g.InstanceOf("GEOMETRYCOLLECTION").Value)
                    g = g.STGeometryN(1);

                return g;
            }

            public static SqlGeometry GeomFromXYMText(string wktXYM, int srid)
            {
                ConvertXVZ2XYM res = new ConvertXVZ2XYM();

                SqlGeometry.STGeomFromText(new SqlChars(wktXYM), srid)
                           .Populate(res);

                return res.ConstructedGeometry;
            }

            /// <summary>
            /// Find the point that is the given distance from the start point in the direction of the end point.
            /// The distance must be less than the distance between these two points.
            /// </summary>
            /// <param name="start">Starting Geometry Point</param>
            /// <param name="end">End Geometry Point</param>
            /// <param name="distance">Distance measure of the point to locate</param>
            /// <returns></returns>
            public static SqlGeometry InterpolateBetweenGeom(SqlGeometry start, SqlGeometry end, double distance)
            {
                // We need to check a few prequisites.
                // We only operate on points.
                if (!start.IsPoint() || !end.IsPoint())
                {
                    throw new ArgumentException("Start and end value must be a point.");
                }

                // The SRIDs also have to match
                int srid = start.STSrid.Value;
                if (srid != end.STSrid.Value)
                {
                    throw new ArgumentException("The start and end SRIDs must match.");
                }

                // Finally, the distance has to fall between these points.
                var length = start.STDistance(end).Value;
                if (distance > start.STDistance(end))
                {
                    throw new ArgumentException("The distance value provided exceeds the distance between the two points.");
                }
                else if (distance < 0)
                {
                    throw new ArgumentException("The distance must be positive.");
                }

                // Since we're working on a Cartesian plane, this is now pretty simple.
                // The fraction of the way from start to end.
                double fraction = distance / length;
                double newX = (start.STX.Value * (1 - fraction)) + (end.STX.Value * fraction);
                double newY = (start.STY.Value * (1 - fraction)) + (end.STY.Value * fraction);
                return SqlGeometry.Point(newX, newY, srid);
            }

            // Make our LocateAlongGeometrySink into a function call.  This function just hooks up
            // and runs a pipeline using the sink.
            public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double distance)
            {
                if (!geometry.IsLineString())
                    throw new ArgumentException(ErrorMessage.LineStringCompatible);

                var geometryBuilder = new SqlGeometryBuilder();
                var geometrySink = new LocateAlongGeometrySink(distance, geometryBuilder);
                geometry.Populate(geometrySink);
                return geometryBuilder.ConstructedGeometry;
            }

            public static SqlGeometry MakeValidForGeography(SqlGeometry geometry)
            {
                // Note: This function relies on an undocumented feature of the planar Union and MakeValid
                // that polygon rings in their result will always be oriented using the same rule that
                // is used in geography. But, it is not good practice to rely on such fact in production code.

                if (geometry.STIsValid().Value && !geometry.STIsEmpty().Value)
                    return geometry.STUnion(geometry.STPointN(1));

                return geometry.MakeValid();
            }

            /// <summary>
            /// Reverse the Line Segment.
            /// </summary>
            /// <param name="geometry">Input SqlGeometry</param>
            /// <returns></returns>
            public static SqlGeometry ReverseLinestring(SqlGeometry geometry)
            {
                if (!geometry.IsLineString())
                    throw new ArgumentException(ErrorMessage.LineStringCompatible);

                var geomBuilder = new SqlGeometryBuilder();
                
                geomBuilder.SetSrid((int)geometry.STSrid);
                geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
                geomBuilder.BeginFigure(geometry.STEndPoint().STX.Value, geometry.STEndPoint().STY.Value);
                for (int i = (int)geometry.STNumPoints() - 1; i >= 1; i--)
                {
                    geomBuilder.AddLine(
                        geometry.STPointN(i).STX.Value,
                        geometry.STPointN(i).STY.Value);
                }
                geomBuilder.EndFigure();
                geomBuilder.EndGeometry();
                return geomBuilder.ConstructedGeometry;
            }

            /// <summary>
            /// Shift the input Geometry x and y co-ordinate by specified amount
            /// Make our ShiftGeometrySink into a function call by hooking it into a simple pipeline.
            /// </summary>
            /// <param name="geometry">Input Geometry</param>
            /// <param name="xShift">X value to shift</param>
            /// <param name="yShift">Y value to shift</param>
            /// <returns>Shifted Geometry</returns>
            public static SqlGeometry ShiftGeometry(SqlGeometry geometry, double xShift, double yShift)
            {
                // create a sink that will create a geometry instance
                var geometryBuilder = new SqlGeometryBuilder();

                // create a sink to do the shift and plug it in to the builder
                var geomSink = new ShiftGeometrySink(xShift, yShift, geometryBuilder);

                // plug our sink into the geometry instance and run the pipeline
                geometry.Populate(geomSink);

                // the end of our pipeline is now populated with the shifted geometry instance
                return geometryBuilder.ConstructedGeometry;
            }
        }


        public class Geography
        {
            // Make our LocateAlongGeographySink into a function call.  This function just hooks up
            // and runs a pipeline using the sink.
            public static SqlGeography LocateAlongGeog(SqlGeography g, double distance)
            {
                SqlGeographyBuilder b = new SqlGeographyBuilder();
                LocateAlongGeographySink p = new LocateAlongGeographySink(distance, b);
                g.Populate(p);
                return b.ConstructedGeography;
            }

            // Find the point that is the given distance from the start point in the direction of the end point.
            // The distance must be less than the distance between these two points.
            public static SqlGeography InterpolateBetweenGeog(SqlGeography start, SqlGeography end, double distance)
            {
                // We need to check a few prequisites.

                // We only operate on points.

                if (start.STGeometryType().Value != "Point")
                {
                    throw new ArgumentException("Start value must be a point.");
                }

                if (end.STGeometryType().Value != "Point")
                {
                    throw new ArgumentException("Start value must be a point.");
                }

                // The SRIDs also have to match
                int srid = start.STSrid.Value;
                if (srid != end.STSrid.Value)
                {
                    throw new ArgumentException("The start and end SRIDs must match.");
                }

                // Finally, the distance has to fall between these points.
                if (distance > start.STDistance(end))
                {
                    throw new ArgumentException("The distance value provided exceeds the distance between the two points.");
                }
                else if (distance < 0)
                {
                    throw new ArgumentException("The distance must be positive.");
                }

                // We'll just do this by binary search---surely this could be more efficient, but this is 
                // relatively easy.
                //
                // Note that we can't just take the take the linear combination of end vectors because we
                // aren't working on a sphere.

                // We are going to do our binary search using 3D Cartesian values, however
                Vector3 startCart = Util.GeographicToCartesian(start);
                Vector3 endCart = Util.GeographicToCartesian(end);
                Vector3 currentCart;

                SqlGeography current;
                double currentDistance;

                // Keep refining until we slip below the THRESHOLD value.
                do
                {
                    currentCart = (startCart + endCart) / 2;
                    current = Util.CartesianToGeographic(currentCart, srid);
                    currentDistance = start.STDistance(current).Value;
                    if (distance <= currentDistance) endCart = currentCart;
                    else startCart = currentCart;
                } while (Math.Abs(currentDistance - distance) > THRESHOLD);

                return current;
            }

            // This function is used for generating a new geography object where additional points are inserted
            // along every line in such a way that the angle between two consecutive points does not
            // exceed a prescribed angle. The points are generated between the unit vectors that correspond
            // to the line's start and end along the great-circle arc on the unit sphere. This follows the
            // definition of geodetic lines in SQL Server.
            public static SqlGeography DensifyGeography(SqlGeography g, double maxAngle)
            {
                SqlGeographyBuilder b = new SqlGeographyBuilder();
                g.Populate(new DensifyGeographySink(b, maxAngle));
                return b.ConstructedGeography;
            }

            // This implements a completely trivial conversion from geometry to geography, simply taking each
            // point (x,y) --> (long, lat).  The result is assigned the given SRID.
            public static SqlGeography VacuousGeometryToGeography(SqlGeometry toConvert, int targetSrid)
            {
                SqlGeographyBuilder b = new SqlGeographyBuilder();
                toConvert.Populate(new VacuousGeometryToGeographySink(targetSrid, b));
                return b.ConstructedGeography;
            }

            // This implements a completely trivial conversion from geography to geometry, simply taking each
            // point (lat,long) --> (y, x).  The result is assigned the given SRID.
            public static SqlGeometry VacuousGeographyToGeometry(SqlGeography toConvert, int targetSrid)
            {
                SqlGeometryBuilder b = new SqlGeometryBuilder();
                toConvert.Populate(new VacuousGeographyToGeometrySink(targetSrid, b));
                return b.ConstructedGeometry;
            }

            // Computes ConvexHull of input geography and returns a polygon (unless all input points are colinear).
            //
            public static SqlGeography ConvexHullGeography(SqlGeography geography)
            {
                if (geography.IsNull || geography.STIsEmpty().Value) return geography;

                SqlGeography center = geography.EnvelopeCenter();
                SqlProjection gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);
                SqlGeometry geometry = gnomonicProjection.Project(geography);
                return gnomonicProjection.Unproject(geometry.MakeValid().STConvexHull());
            }

            // Computes ConvexHull of input WKT and returns a polygon (unless all input points are colinear).
            // This function does not require its input to be a valid geography. This function does require
            // that the WKT coordinate values are longitude/latitude values, in that order and that a valid
            // geography SRID value is supplied.
            //
            public static SqlGeography ConvexHullGeographyFromText(string inputWKT, int srid)
            {
                SqlGeometry geometry = SqlGeometry.STGeomFromText(new SqlChars(inputWKT), srid);
                SqlGeographyBuilder geographyBuilder = new SqlGeographyBuilder();
                geometry.Populate(new GeometryToPointGeographySink(geographyBuilder));
                return ConvexHullGeography(geographyBuilder.ConstructedGeography);
            }

            // Check if an input geometry can represent a valid geography without throwing an exception.
            // This function requires that the geometry be in longitude/latitude coordinates and that
            // those coordinates are in correct order in the geometry instance (i.e. latitude/longitude
            // not longitude/latitude). This function will return false (0) if the input geometry is not
            // in the correct latitude/longitude format, including a valid geography SRID.
            //
            public static bool IsValidGeographyFromGeometry(SqlGeometry geometry)
            {
                if (geometry.IsNull) return false;

                try
                {
                    SqlGeographyBuilder builder = new SqlGeographyBuilder();
                    geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, builder));
                    SqlGeography geography = builder.ConstructedGeography;
                    return true;
                }
                catch (FormatException)
                {
                    // Syntax error
                    return false;
                }
                catch (ArgumentException)
                {
                    // Semantical (Geometrical) error
                    return false;
                }
            }

            // Check if an input WKT can represent a valid geography. This function requires that
            // the WTK coordinate values are longitude/latitude values, in that order and that a valid
            // geography SRID value is supplied.  This function will not throw an exception even in
            // edge conditions (i.e. longitude/latitude coordinates are reversed to latitude/longitude).
            //
            public static bool IsValidGeographyFromText(string inputWKT, int srid)
            {
                try
                {
                    // If parse succeeds then our input is valid
                    SqlGeography.STGeomFromText(new SqlChars(inputWKT), srid);
                    return true;
                }
                catch (FormatException)
                {
                    // Syntax error
                    return false;
                }
                catch (ArgumentException)
                {
                    // Semantical (Geometrical) error
                    return false;
                }
            }

            // Convert an input geometry instance to a valid geography instance.
            // This function requires that the WKT coordinate values are longitude/latitude values,
            // in that order and that a valid geography SRID value is supplied.
            //
            public static SqlGeography MakeValidGeographyFromGeometry(SqlGeometry geometry)
            {
                if (geometry.IsNull) return SqlGeography.Null;
                if (geometry.STIsEmpty().Value) return CreateEmptyGeography(geometry.STSrid.Value);

                // Extract vertices from our input to be able to compute geography EnvelopeCenter
                SqlGeographyBuilder pointSetBuilder = new SqlGeographyBuilder();
                geometry.Populate(new GeometryToPointGeographySink(pointSetBuilder));
                SqlGeography center;
                try
                {
                    center = pointSetBuilder.ConstructedGeography.EnvelopeCenter();
                }
                catch (ArgumentException)
                {
                    // Input is larger than a hemisphere.
                    return SqlGeography.Null;
                }

                // Construct Gnomonic projection centered on input geography
                SqlProjection gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);

                // Project, run geometry MakeValid and unproject
                SqlGeometryBuilder geometryBuilder = new SqlGeometryBuilder();
                geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, new Projector(gnomonicProjection, geometryBuilder)));
                SqlGeometry outGeometry = Geometry.MakeValidForGeography(geometryBuilder.ConstructedGeometry);

                try
                {
                    return gnomonicProjection.Unproject(outGeometry);
                }
                catch (ArgumentException)
                {
                    // Try iteratively to reduce the object to remove very close vertices.
                    for (double tollerance = 1e-4; tollerance <= 1e6; tollerance *= 2)
                    {
                        try
                        {
                            return gnomonicProjection.Unproject(outGeometry.Reduce(tollerance));
                        }
                        catch (ArgumentException)
                        {
                            // keep trying
                        }
                    }
                    return SqlGeography.Null;
                }
            }

            private static SqlGeography CreateEmptyGeography(int srid)
            {
                SqlGeographyBuilder b = new SqlGeographyBuilder();
                b.SetSrid(srid);
                b.BeginGeography(OpenGisGeographyType.GeometryCollection);
                b.EndGeography();
                return b.ConstructedGeography;
            }

            // Convert an input WKT to a valid geography instance.
            // This function requires that the WKT coordinate values are longitude/latitude values,
            // in that order and that a valid geography SRID value is supplied.
            //
            public static SqlGeography MakeValidGeographyFromText(string inputWKT, int srid)
            {
                return MakeValidGeographyFromGeometry(SqlGeometry.STGeomFromText(new SqlChars(inputWKT), srid));
            }           

            // Selectively filter unwanted artifacts in input object:
            //	- empty shapes (if [filterEmptyShapes] is true)
            //	- points (if [filterPoints] is true)
            //	- linestrings shorter than provided tolerance (if lineString.STLength < [lineStringTolerance])
            //	- polygon rings thinner than provied tolerance (if ring.STArea < ring.STLength * [ringTolerance])
            //	- general behaviour: Returned spatial objects will always to the simplest OGC construction
            //
            public static SqlGeography FilterArtifactsGeography(SqlGeography g, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)
            {
                if (g == null || g.IsNull)
                    return g;

                SqlGeographyBuilder b = new SqlGeographyBuilder();
                IGeographySink110 filter = b;

                if (filterEmptyShapes)
                    filter = new GeographyEmptyShapeFilter(filter);
                if (ringTolerance > 0)
                    filter = new GeographyThinRingFilter(filter, ringTolerance);
                if (lineStringTolerance > 0)
                    filter = new GeographyShortLineStringFilter(filter, lineStringTolerance);
                if (filterPoints)
                    filter = new GeographyPointFilter(filter);

                g.Populate(filter);
                g = b.ConstructedGeography;

                if (g == null || g.IsNull)
                    return g;

                // Strip collections with single element
                while (g.STNumGeometries().Value == 1 && g.InstanceOf("GEOMETRYCOLLECTION").Value)
                    g = g.STGeometryN(1);

                return g;
            }

        }

    }
}