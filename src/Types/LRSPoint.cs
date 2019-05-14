//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Data structure to capture POINT geometry type.
    /// </summary>
    internal class LRSPoint
    {
        // Fields.
        private string _wkt;
        internal readonly double X;
        internal readonly double Y;
        internal double? Z, M;
        private readonly int _srid;
        internal double? Slope;
        private double _angle;
        internal SlopeValue SlopeType;

        internal double? OffsetBearing;
        private double _offsetAngle;
        internal double OffsetDistance;
        internal int Id { private get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        /// <param name="srid">The srid.</param>
        internal LRSPoint(double x, double y, double? z, double? m, int srid)
        {
            X = x;
            Y = y;
            Z = m.HasValue ? z : null;
            M = m ?? z;
            _srid = srid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        internal LRSPoint(SqlGeometry sqlGeometry)
        {
            if (sqlGeometry.IsNullOrEmpty() || !sqlGeometry.IsPoint())
                return;

            if (sqlGeometry == null) return;
            X = sqlGeometry.STX.Value;
            Y = sqlGeometry.STY.Value;
            Z = sqlGeometry.HasZ ? sqlGeometry.Z.Value : (double?)null;
            M = sqlGeometry.HasM ? sqlGeometry.M.Value : (double?)null;
            _srid = (int)sqlGeometry.STSrid;
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Translating measure of LRSPoint
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void TranslateMeasure(double offsetMeasure)
        {
            M += offsetMeasure;
        }

        /// <summary>
        /// Scaling Measure of LRSPoint
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void ScaleMeasure(double offsetMeasure)
        {
            M *= offsetMeasure;
        }

        /// <summary>
        /// Gets the distance from point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        internal double GetDistance(LRSPoint nextPoint)
        {
            return SpatialExtensions.GetDistance(X, Y, nextPoint.X, nextPoint.Y);
        }

        /// <summary>
        /// Determines whether X, Y co-ordinates of current and second point is within tolerance
        /// </summary>
        /// <param name="secondPoint">The second point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if X, Y co-ordinates of current and second point is within tolerance; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsXYWithinTolerance(LRSPoint secondPoint, double tolerance)
        {
            return (Math.Abs(X - secondPoint.X) <= tolerance && Math.Abs(Y - secondPoint.Y) <= tolerance);
        }

        /// <summary>
        /// Re calculate the measure.
        /// </summary>
        /// <param name="previousPoint">The previous point.</param>
        /// <param name="currentLength"></param>
        /// <param name="totalLength">The total length.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        internal void ReCalculateMeasure(LRSPoint previousPoint, ref double currentLength, double totalLength, double startMeasure, double endMeasure)
        {
            currentLength += GetDistance(previousPoint);
            M = startMeasure + (currentLength / totalLength) * (endMeasure - startMeasure);
        }

        /// <summary>
        /// Gets the previous point.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="currentPoint">The current point.</param>
        /// <returns></returns>
        private static LRSPoint GetPreviousPoint(ref List<LRSPoint> points, LRSPoint currentPoint)
        {
            var index = points.FindIndex(e => e.Id == currentPoint.Id);
            if (index > 0 && index < points.Count)
                return points[index - 1];
            return currentPoint;
        }

        /// <summary>
        /// Gets the next point.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="currentPoint">The current point.</param>
        /// <returns></returns>
        private static LRSPoint GetNextPoint(ref List<LRSPoint> points, LRSPoint currentPoint)
        {
            var index = points.FindIndex(e => e.Id == currentPoint.Id);
            if (index > 0 && index < points.Count)
                return points[index + 1];
            return currentPoint;
        }

        #endregion

        #region Operator Overloading

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static LRSPoint operator -(LRSPoint a, LRSPoint b)
        {
            return new LRSPoint(b.X - a.X, b.Y - a.Y, null, null, a._srid);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>

        public static bool operator ==(LRSPoint a, LRSPoint b)
        {
            if (ReferenceEquals(b, null) && ReferenceEquals(a, null))
                return true;
            if (ReferenceEquals(b, null) || ReferenceEquals(a, null))
                return false;
            return a.X.EqualsTo(b.X) && a.Y.EqualsTo(b.Y) && EqualityComparer<double?>.Default.Equals(a.M, b.M);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(LRSPoint a, LRSPoint b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var point = obj as LRSPoint;
            return point != null &&
                   X.EqualsTo(point.X) &&
                   Y.EqualsTo(point.Y) &&
                   EqualityComparer<double?>.Default.Equals(M, point.M);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            var hashCode = -1911090832;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<double?>.Default.GetHashCode(Z);
            hashCode = hashCode * -1521134295 + EqualityComparer<double?>.Default.GetHashCode(M);
            return hashCode;
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode

        #endregion

        #region Data Structure Conversions

        /// <summary>
        /// Converts to WKT format.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_wkt))
                return _wkt;

            _wkt = $"POINT ({X} {Y} {M})";

            return _wkt;
        }

        /// <summary>
        /// Converts to SqlGeometry.
        /// </summary>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry()
        {
            var geomBuilder = new SqlGeometryBuilder();
            return ToSqlGeometry(ref geomBuilder);
        }

        /// <summary>
        /// Converts to SqlGeometry.
        /// </summary>
        /// <param name="geometryBuilder">The geometry builder.</param>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry(ref SqlGeometryBuilder geometryBuilder)
        {
            geometryBuilder.SetSrid(_srid);
            geometryBuilder.BeginGeometry(OpenGisGeometryType.Point);
            geometryBuilder.BeginFigure(X, Y, Z, M);
            geometryBuilder.EndFigure();
            geometryBuilder.EndGeometry();
            return geometryBuilder.ConstructedGeometry;
        }

        #endregion

        #region Parallel Point Computation

        /// <summary>
        /// Gets the offset point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        private LRSPoint GetOffsetPoint(LRSPoint nextPoint)
        {
            return this - nextPoint;
        }

        /// <summary>
        /// Gets the arc to tangent.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns></returns>
        private double GetAtan(LRSPoint nextPoint)
        {
            var offsetPoint = GetOffsetPoint(nextPoint);
            return Math.Atan2(offsetPoint.Y, offsetPoint.X);
        }

        /// <summary>
        /// Sets the offset bearing.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        internal void SetOffsetBearing(LRSPoint nextPoint)
        {
            if (nextPoint != null)
                OffsetBearing = CalculateOffsetBearing(nextPoint);
        }

        /// <summary>
        /// Calculates the offset bearing.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        private double CalculateOffsetBearing(LRSPoint nextPoint)
        {
            _angle = Util.ToDegrees(GetAtan(nextPoint));
            return (90 - _angle + 360) % 360;
        }

        /// <summary>
        /// Sets the offset angle.
        /// </summary>
        /// <param name="previousPoint">The current point.</param>
        /// <param name="progress">The Linear Measure Progress.</param>
        internal void SetOffsetAngle(LRSPoint previousPoint, LinearMeasureProgress progress)
        {
            _offsetAngle = CalculateOffsetAngle(previousPoint, progress);
        }

        /// <summary>
        /// Calculates the offset angle.
        /// </summary>
        /// <param name="previousPoint">The current point.</param>
        /// <param name="progress">The Linear Measure Progress.</param>
        private double CalculateOffsetAngle(LRSPoint previousPoint, LinearMeasureProgress progress)
        {
            double offsetAngle = 0;

            var previousPointOffsetBearing = previousPoint?.OffsetBearing;

            // Left
            if (progress == LinearMeasureProgress.Increasing)
            {
                if (OffsetBearing == null)
                {
                    if (previousPointOffsetBearing != null) offsetAngle = (double) previousPointOffsetBearing - 90;
                }
                else if (previousPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing - 90;
                else
                    //(360 + b1.OffsetBearing - ((360 - ((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = (360 + (double)OffsetBearing - ((360 - (((double)previousPointOffsetBearing + 180) - (double)OffsetBearing)) / 2)) % 360;
            }
            // Right
            else
            {

                if (OffsetBearing == null)
                {
                    if (previousPointOffsetBearing != null) offsetAngle = (double) previousPointOffsetBearing + 90;
                }
                else if (previousPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing + 90;
                else
                    // (b1.OffsetBearing + ((((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = ((double)OffsetBearing + (((((double)previousPointOffsetBearing + 180) - (double)OffsetBearing)) / 2)) % 360;
            }

            return offsetAngle;
        }

        /// <summary>
        /// Sets the offset distance.
        /// </summary>
        /// <param name="offset">The offset.</param>
        internal void SetOffsetDistance(double offset)
        {
            var offsetBearing = OffsetBearing ?? default(double);
            // offset / (SIN(RADIANS(((OffsetBearing - OffsetAngleLeft) + 360) % 360)))
            OffsetDistance = CalculateOffsetDistance(offset, offsetBearing, _offsetAngle);
        }

        /// <summary>
        /// Calculates the offset distance.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="offsetBearing"></param>
        /// <param name="offsetAngle"></param>
        private static double CalculateOffsetDistance(double offset, double offsetBearing, double offsetAngle)
        {
            // offset / (SIN(RADIANS(((OffsetBearing - OffsetAngleLeft) + 360) % 360)))
            return offset / (Math.Sin(Util.ToRadians(((offsetBearing - offsetAngle) + 360) % 360)));
        }

        /// <summary>
        /// Compute and populate parallel points on bend lines.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="progress"></param>
        /// <param name="points">The points.</param>
        /// <param name="tolerance"></param>
        /// <returns>Point parallel to the current point.</returns>
        internal List<LRSPoint> GetAndPopulateParallelPoints(double offset, double tolerance, LinearMeasureProgress progress, ref List<LRSPoint> points)
        {
            var lrsPoints = new List<LRSPoint>();

            // first equation
            var parallelX = X + (OffsetDistance * Math.Cos(Util.ToRadians(90 - _offsetAngle)));
            var parallelY = Y + (OffsetDistance * Math.Sin(Util.ToRadians(90 - _offsetAngle)));
            var parallelPoint = new LRSPoint(parallelX, parallelY, null, M, _srid);

            var diffInDistance = Math.Round(parallelPoint.GetDistance(this), 5);

            var negativeOffset =  offset < 0;

            var isParallelAngle = negativeOffset
                ? !(_angle > 45 && _angle <= 180 || _angle > -180 && _angle <= -45)
                : (_angle > 45 && _angle <= 180 || _angle > -180 && _angle <= -45);

            if (diffInDistance.EqualsTo(Math.Abs(offset)) || isParallelAngle)
            {
                lrsPoints.Add(new LRSPoint(
                    parallelX,
                    parallelY,
                    null,
                    M,
                    _srid
                    ));
            }
            else
            {
                var previousPoint = GetPreviousPoint(ref points, this);
                var prevRadians = Util.ToRadians(90 - previousPoint._offsetAngle);

                var nextPoint = GetNextPoint(ref points, this);
                var nextRadians = Util.ToRadians(90 - nextPoint._offsetAngle);

                // first point
                var firstPointX = X + (offset * Math.Cos(prevRadians));
                var firstPointY = Y + (offset * Math.Sin(prevRadians));
                var firstPoint = new LRSPoint(firstPointX, firstPointY, null, M, _srid);

                // second point
                var secondPointX = X + (offset * Math.Cos(nextRadians));
                var secondPointY = Y + (offset * Math.Sin(nextRadians));
                var secondPoint = new LRSPoint(secondPointX, secondPointY, null, M, _srid);

                // if computed first point is within tolerance of second point then add only first point
                if (firstPoint.IsXYWithinTolerance(secondPoint, tolerance))
                {
                    lrsPoints.Add(negativeOffset ? secondPoint : firstPoint);
                }
                else
                {
                    // add first point
                    lrsPoints.Add(firstPoint);

                    // compute middle point
                    var fraction = Math.Abs(offset / OffsetDistance);
                    var middleX = (X * (1 - fraction)) + (parallelX * fraction);
                    var middleY = (Y * (1 - fraction)) + (parallelY * fraction);
                    var middlePoint = new LRSPoint(middleX, middleY, null, M, _srid);

                    // if not within tolerance add middle point
                    if (!firstPoint.IsXYWithinTolerance(middlePoint, tolerance))
                        lrsPoints.Add(middlePoint);

                    // add second point
                    lrsPoints.Add(secondPoint);
                }
            }

            return lrsPoints;
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        /// <param name="nextLRSPoint">The next LRS point.</param>
        internal void CalculateSlope(LRSPoint nextLRSPoint)
        {
            Slope = GetSlope(nextLRSPoint, out SlopeType);
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        /// <param name="nextLRSPoint">The next LRS point.</param>
        /// <param name="slopeValue"></param>
        internal double? GetSlope(LRSPoint nextLRSPoint, out SlopeValue slopeValue)
        {
            slopeValue = SlopeValue.None;
            var xDifference = nextLRSPoint.X - X;
            var yDifference = nextLRSPoint.Y - Y;

            if (xDifference.EqualsTo(0))
            {
                slopeValue = yDifference > 0 ? SlopeValue.PositiveInfinity : SlopeValue.NegativeInfinity;
                return null;
            }
            else if (yDifference.EqualsTo(0))
            {
                slopeValue = xDifference > 0 ? SlopeValue.PositiveZero : SlopeValue.NegativeZero;
                return null;
            }

            return yDifference / xDifference;
        }

        #endregion
    }
}