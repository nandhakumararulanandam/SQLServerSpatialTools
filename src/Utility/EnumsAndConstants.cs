using System;

namespace SQLSpatialTools.Utility
{
    /// <summary>
    /// Enum for Open Geo spatial Consortium Type Names
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
        public const double Tolerance = 0.5;

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
        /// Holds the string value for a value in an enum.
        /// </summary>
        public string StringValue { get; private set; }

        /// <summary>
        /// Constructor used to initialize a StringValue Attribute
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

    /// <summary>
    /// Progression of Geom Segment based on measure.
    /// </summary>
    public enum LinearMeasureProgress : short
    {
        None = 0,
        Increasing = 1,
        Decreasing = 2
    }

    /// <summary>
    /// Merging position of Geom Segments
    /// </summary>
    public enum MergePosition
    {
        None = 0,
        StartStart = 1,
        StartEnd = 2,
        EndStart = 3,
        EndEnd = 4,
        BothEnds = 5,
        CrossEnds = 6
    }

    /// <summary>
    /// Merge Input Segments Geometry type.
    /// </summary>
    public enum MergeInputType
    {
        None = 0,
        LSLS = 1,
        LSMLS = 2,
        MLSLS = 3,
        MLSMLS = 4
    }
}
