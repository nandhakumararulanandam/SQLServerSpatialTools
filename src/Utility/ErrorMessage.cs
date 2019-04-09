﻿namespace SQLSpatialTools.Utility
{
    public class ErrorMessage
    {
        public const string LineStringCompatible = "LINESTRING is currently the only spatial type supported";
        public const string LRSCompatible = "POINT, LINESTRING or MULTILINE STRING is currently the only spatial type supported";
        public const string LineOrMultiLineStringCompatible = "LINESTRING or MULTILINE STRING is currently the only spatial type supported";
        public const string PointCompatible = "Start and End geometry must be a point.";
        public const string SRIDCompatible = "SRID's of geography\\geometry objects doesn't match";
        public const string MeasureRange = "Measure not withing range.";
        public const string WKT3DOnly = "Input WKT should only have three dimensions!";
        public const string LinearGeometryMeasureMustBeInRange = "{0} is not within the measure range {1} : {2} of the linear geometry."; 
        public const string DistanceMustBeBetweenTwoPoints = "The distance value provided exceeds the distance between the two points.";
        public const string DistanceMustBePositive = "The distance must be positive."; 
    }
}