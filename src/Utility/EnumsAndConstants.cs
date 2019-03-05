using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLSpatialTools.Utility
{
    /// <summary>
    /// Enum for Open Geospatial Consortium Type Names
    /// </summary>
    public enum OGCType
    {
        [StringValue("Point")]
        Point,

        [StringValue("LineString")]
        LineString,

        [StringValue("CircularString")]
        CircularString,

        [StringValue("CompoundCurve")]
        CompoundCurve,

        [StringValue("Polygon")]
        Polygon,

        [StringValue("CurvePolygon")]
        CurvePolygon,

        [StringValue("GeometryCollection")]
        GeometryCollection,

        [StringValue("MultiPoint")]
        MultiPoint,

        [StringValue("MultiLineString")]
        MultiLineString,

        [StringValue("MultiPolygon")]
        MultiPolygon
    }

    public class Constants
    {
        public const int DEFAULT_SRID = 4326;

        // 1 cm tolerance in most SRIDs
        public const double THRESHOLD = 0.01;
    }

    public class ErrorMessage
    {
        public const string LineStringCompatible = "LINESTRING is currently the only spatial type supported";
        public const string SRIDCompatible = "SRID's of geography\\geometry objects doesn't match";
        public const string PointCompatible = "Start and End geometry must be a point.";
        public const string MeasureRange = "Measure not withing range.";
        public const string WKT3DOnly = "Input WKT should only have three dimensions!";
        public const string DistanceMustBeBetweenTwoPoints = "The distance value provided exceeds the distance between the two points.";
        public const string DistanceMustBePositive = "The distance must be positive.";
    }

    /// <summary>
    /// This attribute is used to represent a string value
    /// for a value in an enum.
    /// </summary>
    public class StringValueAttribute : Attribute
    {

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }
}
