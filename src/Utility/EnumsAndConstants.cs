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

    public enum DimensionalInfo
    {
        None,
        [StringValue("2 Dimensional point, with x and y")]
        _2D,
        [StringValue("2 Dimensional point, with x, y and measure")]
        _2DM,
        [StringValue("3 Dimensional point, with x, y and z")]
        _3D,
        [StringValue("3 Dimensional point, with x, y, z with measure")]
        _3DM
    }

    public class Constants
    {
        public const int DefaultSRID = 4326;

        // 1 cm tolerance in most SRIDs
        public const double Threshold = 0.01;

        // Point sql char format.
        public const string PointSqlCharFormat = "POINT({0} {1} {2} {3})";
    }

    /// <summary>
    /// This attribute is used to represent a string value
    /// for a value in an enum.
    /// </summary>
    public sealed class StringValueAttribute : Attribute
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

    public enum LRSErrorCodes
    {
        [StringValue("Invalid LRS Segment")]
        Valid = 1,

        [StringValue("Invalid LRS Segment")]
        Invalid = 13331,

        [StringValue("Invalid LRS measure not defined")]
        MeasureNotDefined = 13331,

        [StringValue("Invalid LRS measure not in linear sequence")]
        MeasureNotLinear = 13333,
    }

    public enum LinearMeasureProgress : short
    {
        None = 0,
        Increasing = 1,
        Decreasing = 2
    }
}
