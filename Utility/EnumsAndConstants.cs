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
        public const int DEFAULT_SRID = 4236;
    }

    public class ErrorMessage
    {
        public const string LineStringCompatible = "LINESTRING is currently the only spatial type supported";
        public const string SRIDCompatible = "SRID's of geometries doesn't match";
        public const string PointCompatible = "Start and End geometry must be a point.";
        public const string MeasureRange = "Measure not withing range.";
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
