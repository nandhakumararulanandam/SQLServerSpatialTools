// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geography;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Types;
using SQLSpatialTools.Types.SQL;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Functions.General
{
    /// <summary>
    /// This class contains General Geometry type functions that can be registered in SQL Server.
    /// </summary>
    public class Geometry
    {
        /// <summary>
        /// Selectively filter unwanted artifacts in input object:
        ///	- empty shapes (if [filterEmptyShapes] is true)
        ///	- points (if [filterPoints] is true)
        ///	- line strings shorter than provided tolerance (if lineString.STLength < [lineStringTolerance])
        ///	- polygon rings thinner than provied tolerance (if ring.STArea < ring.STLength * [ringTolerance])
        ///	- general behaviour: Returned spatial objects will always to the simplest OGC construction
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="filterEmptyShapes"></param>
        /// <param name="filterPoints"></param>
        /// <param name="lineStringTolerance"></param>
        /// <param name="ringTolerance"></param>
        /// <returns></returns>
        public static SqlGeometry FilterArtifactsGeometry(SqlGeometry geometry, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)
        {
            if (geometry == null || geometry.IsNull)
                return geometry;

            var geomBuilder = new SqlGeometryBuilder();
            IGeometrySink110 filter = geomBuilder;

            if (filterEmptyShapes)
                filter = new GeometryEmptyShapeFilter(filter);
            if (ringTolerance > 0)
                filter = new GeometryThinRingFilter(filter, ringTolerance);
            if (lineStringTolerance > 0)
                filter = new GeometryShortLineStringFilter(filter, lineStringTolerance);
            if (filterPoints)
                filter = new GeometryPointFilter(filter);

            geometry.Populate(filter);
            geometry = geomBuilder.ConstructedGeometry;

            if (geometry == null || geometry.IsNull || !geometry.STIsValid().Value)
                return geometry;

            // Strip collections with single element
            while (geometry.STNumGeometries().Value == 1 && geometry.InstanceOf("GEOMETRYCOLLECTION").Value)
                geometry = geometry.STGeometryN(1);

            return geometry;
        }

        /// <summary>
        /// Convert Z co-ordinate of geom from XYZ to XYM
        /// </summary>
        /// <param name="wktXYM">Well Know Text with x,y,z representation</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static SqlGeometry GeomFromXYMText(string wktXYM, int srid)
        {
            var res = new ConvertXYZ2XYMGeometrySink();
            var geom = wktXYM.GetGeom(srid);
            geom.Populate(res);
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
            // We need to check a few prerequisites.
            // We only operate on points.
            if (!start.IsPoint() || !end.IsPoint())
            {
                throw new ArgumentException(ErrorMessage.PointCompatible);
            }

            // The SRIDs also have to match
            int srid = start.STSrid.Value;
            if (srid != end.STSrid.Value)
            {
                throw new ArgumentException(ErrorMessage.SRIDCompatible);
            }

            // Finally, the distance has to fall between these points.
            var length = start.STDistance(end).Value;
            if (distance > start.STDistance(end))
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBeBetweenTwoPoints);
            }
            else if (distance < 0)
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBePositive);
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

        /// <summary>
        /// Make the input geometry valid to use in geography manipulation
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <returns></returns>
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

        /// <summary>
        /// This implements a completely trivial conversion from geometry to geography, simply taking each
        /// point (x,y) --> (long, lat).  The result is assigned the given SRID.
        /// </summary>
        /// <param name="toConvert">Input Geometry to convert</param>
        /// <param name="targetSrid">Target SRID</param>
        /// <returns>Converted Geography</returns>
        public static SqlGeography VacuousGeometryToGeography(SqlGeometry toConvert, int targetSrid)
        {
            var geographyBuilder = new SqlGeographyBuilder();
            toConvert.Populate(new VacuousGeometryToGeographySink(targetSrid, geographyBuilder));
            return geographyBuilder.ConstructedGeography;
        }
    }

    /// <summary>
    /// This class contains General Geography type functions that can be registered in SQL Server.
    /// </summary>
    public class Geography
    {
        /// <summary>
        /// Make our LocateAlongGeographySink into a function call. 
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geography">Sql Geography</param>
        /// <param name="distance">Distance at which the point to be located</param>
        /// <returns></returns>
        public static SqlGeography LocatePointAlongGeog(SqlGeography geography, double distance)
        {
            var geogBuilder = new SqlGeographyBuilder();
            var geogSink = new LocateAlongGeographySink(distance, geogBuilder);
            geography.Populate(geogSink);
            return geogBuilder.ConstructedGeography;
        }

        /// <summary>
        /// Find the point that is the given distance from the start point in the direction of the end point.
        /// The distance must be less than the distance between these two points.
        /// </summary>
        /// <param name="start">Start Geography Point</param>
        /// <param name="end">End Geography Point</param>
        /// <param name="distance">Distance at which the point to be located</param>
        /// <returns></returns>
        public static SqlGeography InterpolateBetweenGeog(SqlGeography start, SqlGeography end, double distance)
        {
            // We need to check a few prerequisite.
            // We only operate on points.
            if (!start.IsPoint() || !end.IsPoint())
            {
                throw new ArgumentException(ErrorMessage.PointCompatible);
            }

            // The SRIDs also have to match
            int srid = start.STSrid.Value;
            if (srid != end.STSrid.Value)
            {
                throw new ArgumentException(ErrorMessage.SRIDCompatible);
            }

            // Finally, the distance has to fall between these points.
            var length = start.STDistance(end).Value;
            if (distance > start.STDistance(end))
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBeBetweenTwoPoints);
            }
            else if (distance < 0)
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBePositive);
            }

            // We'll just do this by binary search---surely this could be more efficient, 
            // but this is relatively easy.
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

                if (distance <= currentDistance)
                    endCart = currentCart;
                else
                    startCart = currentCart;
            } while (Math.Abs(currentDistance - distance) > Constants.Tolerance);

            return current;
        }

        /// <summary>
        /// This function is used for generating a new geography object where additional points are inserted
        /// along every line in such a way that the angle between two consecutive points does not
        /// exceed a prescribed angle. The points are generated between the unit vectors that correspond
        /// to the line's start and end along the great-circle arc on the unit sphere. This follows the
        /// definition of geodetic lines in SQL Server.
        /// </summary>
        /// <param name="geography">Input Sql geography</param>
        /// <param name="maxAngle">Max Angle</param>
        /// <returns></returns>
        public static SqlGeography DensifyGeography(SqlGeography geography, double maxAngle)
        {
            var geogBuilder = new SqlGeographyBuilder();
            geography.Populate(new DensifyGeographySink(geogBuilder, maxAngle));
            return geogBuilder.ConstructedGeography;
        }

        /// <summary>
        /// Performs a complete trivial conversion from geography to geometry, simply taking each
        ///point (lat,long) -> (y, x).  The result is assigned the given SRID.
        /// </summary>
        /// <param name="toConvert"></param>
        /// <param name="targetSrid"></param>
        /// <returns></returns>
        public static SqlGeometry VacuousGeographyToGeometry(SqlGeography toConvert, int targetSrid)
        {
            var geomBuilder = new SqlGeometryBuilder();
            toConvert.Populate(new VacuousGeographyToGeometrySink(targetSrid, geomBuilder));
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Computes ConvexHull of input geography and returns a polygon (unless all input points are collinear).
        /// </summary>
        /// <param name="geography">Input Sql Geography</param>
        /// <returns></returns>
        public static SqlGeography ConvexHullGeography(SqlGeography geography)
        {
            if (geography.IsNull || geography.STIsEmpty().Value) return geography;

            SqlGeography center = geography.EnvelopeCenter();
            SqlProjection gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);
            SqlGeometry geometry = gnomonicProjection.Project(geography);
            return gnomonicProjection.Unproject(geometry.MakeValid().STConvexHull());
        }

        /// <summary>
        /// Computes ConvexHull of input WKT and returns a polygon (unless all input points are collinear).
        /// This function does not require its input to be a valid geography. This function does require
        /// that the WKT coordinate values are longitude/latitude values, in that order and that a valid
        /// geography SRID value is supplied.
        /// </summary>
        /// <param name="inputWKT">Input Well Known Text</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static SqlGeography ConvexHullGeographyFromText(string inputWKT, int srid)
        {
            var geometry = SqlGeometry.STGeomFromText(new SqlChars(inputWKT), srid);
            var geographyBuilder = new SqlGeographyBuilder();
            geometry.Populate(new GeometryToPointGeographySink(geographyBuilder));
            return ConvexHullGeography(geographyBuilder.ConstructedGeography);
        }

        /// <summary>
        /// Check if an input geometry can represent a valid geography without throwing an exception.
        /// This function requires that the geometry be in longitude/latitude coordinates and that
        /// those coordinates are in correct order in the geometry instance (i.e. latitude/longitude
        /// not longitude/latitude). This function will return false (0) if the input geometry is not
        /// in the correct latitude/longitude format, including a valid geography SRID.
        /// </summary>
        /// <param name="geometry">Input Sql Geometry</param>
        /// <returns></returns>
        public static bool IsValidGeographyFromGeometry(SqlGeometry geometry)
        {
            if (geometry.IsNull) return false;

            try
            {
                var geogBuilder = new SqlGeographyBuilder();
                geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, geogBuilder));
                SqlGeography geography = geogBuilder.ConstructedGeography;
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

        /// <summary>
        /// Check if an input WKT can represent a valid geography. This function requires that
        /// the WTK coordinate values are longitude/latitude values, in that order and that a valid
        /// geography SRID value is supplied.  This function will not throw an exception even in
        /// edge conditions (i.e. longitude/latitude coordinates are reversed to latitude/longitude).
        /// </summary>
        /// <param name="inputWKT">Input Well Known Text</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static bool IsValidGeographyFromText(string inputWKT, int srid)
        {
            try
            {
                // If parse succeeds then our input is valid
                inputWKT.GetGeog(srid);
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

        /// <summary>
        /// Convert an input geometry instance to a valid geography instance.
        /// This function requires that the WKT coordinate values are longitude/latitude values,
        /// in that order and that a valid geography SRID value is supplied.
        /// </summary>
        /// <param name="geometry">Input Sql Geometry</param>
        /// <returns></returns>
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

        /// <summary>
        /// Constructs an empty Sql Geography
        /// </summary>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        private static SqlGeography CreateEmptyGeography(int srid)
        {
            var geogBuilder = new SqlGeographyBuilder();
            geogBuilder.SetSrid(srid);
            geogBuilder.BeginGeography(OpenGisGeographyType.GeometryCollection);
            geogBuilder.EndGeography();
            return geogBuilder.ConstructedGeography;
        }

        /// <summary>
        /// Convert an input WKT to a valid geography instance.
        /// This function requires that the WKT coordinate values are longitude/latitude values,
        /// in that order and that a valid geography SRID value is supplied.
        /// </summary>
        /// <param name="inputWKT">Input Well Know Text</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static SqlGeography MakeValidGeographyFromText(string inputWKT, int srid)
        {
            return MakeValidGeographyFromGeometry(inputWKT.GetGeom(srid));
        }

        // Selectively filter unwanted artifacts in input object:
        //	- empty shapes (if [filterEmptyShapes] is true)
        //	- points (if [filterPoints] is true)
        //	- line strings shorter than provided tolerance (if lineString.STLength < [lineStringTolerance])
        //	- polygon rings thinner than provided tolerance (if ring.STArea < ring.STLength * [ringTolerance])
        //	- general behavior: Returned spatial objects will always to the simplest OGC construction
        //
        public static SqlGeography FilterArtifactsGeography(SqlGeography geography, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)
        {
            if (geography == null || geography.IsNull)
                return geography;

            var geogBuilder = new SqlGeographyBuilder();
            IGeographySink110 filter = geogBuilder;

            if (filterEmptyShapes)
                filter = new GeographyEmptyShapeFilter(filter);
            if (ringTolerance > 0)
                filter = new GeographyThinRingFilter(filter, ringTolerance);
            if (lineStringTolerance > 0)
                filter = new GeographyShortLineStringFilter(filter, lineStringTolerance);
            if (filterPoints)
                filter = new GeographyPointFilter(filter);

            geography.Populate(filter);
            geography = geogBuilder.ConstructedGeography;

            if (geography == null || geography.IsNull)
                return geography;

            // Strip collections with single element
            while (geography.STNumGeometries().Value == 1 && geography.InstanceOf("GEOMETRYCOLLECTION").Value)
                geography = geography.STGeometryN(1);

            return geography;
        }

    }
}
