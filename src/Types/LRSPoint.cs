// Copyright (c) Microsoft Corporation.  All rights reserved.

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
        private string wkt;
        internal double X, Y;
        internal double? Z, M;
        internal int SRID;
        internal double? Slope;

        internal double? OffsetBearing;
        internal double OffsetAngle;
        internal double OffsetDistance;
        internal int Id { get; set; }

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
            M = m.HasValue ? m : z;
            SRID = srid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        internal LRSPoint(SqlGeometry sqlGeometry)
        {
            if (sqlGeometry == null || sqlGeometry.STIsEmpty() || !sqlGeometry.IsPoint())
                return;

            X = sqlGeometry.STX.Value;
            Y = sqlGeometry.STY.Value;
            Z = sqlGeometry.HasZ ? sqlGeometry.Z.Value : (double?)null;
            M = sqlGeometry.HasM ? sqlGeometry.M.Value : (double?)null;
            SRID = (int)sqlGeometry.STSrid;
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
            return Math.Sqrt(Math.Pow(nextPoint.X - X, 2) + Math.Pow(nextPoint.Y - Y, 2));
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
        internal LRSPoint GetPreviousPoint(ref List<LRSPoint> points, LRSPoint currentPoint)
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
        internal LRSPoint GetNextPoint(ref List<LRSPoint> points, LRSPoint currentPoint)
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
            return new LRSPoint(b.X - a.X, b.Y - a.Y, null, null, a.SRID);
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
            return a.X == b.X && a.Y == b.Y && EqualityComparer<double?>.Default.Equals(a.M, b.M);
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
                   X == point.X &&
                   Y == point.Y &&
                   EqualityComparer<double?>.Default.Equals(M, point.M);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -1911090832;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<double?>.Default.GetHashCode(Z);
            hashCode = hashCode * -1521134295 + EqualityComparer<double?>.Default.GetHashCode(M);
            return hashCode;
        }

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
            if (!string.IsNullOrEmpty(wkt))
                return wkt;

            wkt = string.Format("POINT ({0} {1} {2})", X, Y, M);

            return wkt;
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
            geometryBuilder.SetSrid(SRID);
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
        internal LRSPoint GetOffsetPoint(LRSPoint nextPoint)
        {
            return this - nextPoint;
        }

        /// <summary>
        /// Gets the arc to tangent.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns></returns>
        internal double GetAtan(LRSPoint nextPoint)
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
                OffsetBearing = (90 - Util.ToDegrees(GetAtan(nextPoint)) + 360) % 360;
        }

        /// <summary>
        /// Sets the offset angle.
        /// </summary>
        /// <param name="previousPoint">The current point.</param>
        /// <param name="progress">The Linear Measure Progress.</param>
        internal void SetOffsetAngle(LRSPoint previousPoint, LinearMeasureProgress progress)
        {
            double offsetAngle;

            var previousPointOffsetBearing = previousPoint?.OffsetBearing;

            // Left
            if (progress == LinearMeasureProgress.Increasing)
            {
                if (OffsetBearing == null)
                    offsetAngle = (double)previousPointOffsetBearing - 90;
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
                    offsetAngle = (double)previousPointOffsetBearing + 90;
                else if (previousPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing + 90;
                else
                    // (b1.OffsetBearing + ((((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = ((double)OffsetBearing + (((((double)previousPointOffsetBearing + 180) - (double)OffsetBearing)) / 2)) % 360;
            }

            OffsetAngle = offsetAngle;
        }

        /// <summary>
        /// Sets the offset distance.
        /// </summary>
        /// <param name="offset">The offset.</param>
        internal void SetOffsetDistance(double offset)
        {
            double offsetBearing = OffsetBearing != null ? OffsetBearing.Value : default(double);
            // offset / (SIN(RADIANS(((OffsetBearing - OffsetAngleLeft) + 360) % 360)))
            OffsetDistance = offset / (Math.Sin(Util.ToRadians(((offsetBearing - OffsetAngle) + 360) % 360)));
        }

        /// <summary>
        /// Gets the parallel point.
        /// </summary>
        /// <param name="offset">The offset value.</param>
        /// <returns>Point parallel to the current point.</returns>
        internal List<LRSPoint> GetParallelPoint(double offset, ref List<LRSPoint> points)
        {
            var lrsPoints = new List<LRSPoint>();

            // first equation
            var newX = X + (OffsetDistance * Math.Cos(Util.ToRadians(90 - OffsetAngle)));
            var newY = Y + (OffsetDistance * Math.Sin(Util.ToRadians(90 - OffsetAngle)));

            lrsPoints.Add(new LRSPoint(
                newX,
                newY,
                null,
                M,
                SRID
                ));

            return lrsPoints;
        }

        /// <summary>
        /// Compute and populate parallel points on bend lines.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="points">The points.</param>
        /// <returns></returns>
        internal List<LRSPoint> GetAndPopulateParallelPointsOnBendLines(double offset, ref List<LRSPoint> points)
        {
            var lrsPoints = new List<LRSPoint>();

            // first equation
            var newX = X + (OffsetDistance * Math.Cos(Util.ToRadians(90 - OffsetAngle)));
            var newY = Y + (OffsetDistance * Math.Sin(Util.ToRadians(90 - OffsetAngle)));

            if (OffsetDistance == offset || OffsetAngle == 0)
            {
                lrsPoints.Add(new LRSPoint(
                    newX,
                    newY,
                    null,
                    M,
                    SRID
                    ));
            }
            else
            {
                var previousPoint = GetPreviousPoint(ref points, this);
                var nextPoint = GetNextPoint(ref points, this);

                // first point
                var firstPointX = X + (offset * Math.Cos(Util.ToRadians(90 - previousPoint.OffsetAngle)));
                var firstPointY = Y + (offset * Math.Sin(Util.ToRadians(90 - previousPoint.OffsetAngle)));

                lrsPoints.Add(new LRSPoint(
                   firstPointX,
                   firstPointY,
                   null,
                   M,
                   SRID
                   ));

                // second point
                var secondPointX = X + (offset * Math.Cos(Util.ToRadians(90 - nextPoint.OffsetAngle)));
                var secondPointY = Y + (offset * Math.Sin(Util.ToRadians(90 - nextPoint.OffsetAngle)));


                // construct middle point and second point only if slope is negative else ignore it
                if (Slope <= 0)
                {

                    // middle point computation
                    // fraction of offset 
                    var fraction = offset / OffsetDistance;
                    var middleX = (X * (1 - fraction)) + (newX * fraction);
                    var middleY = (Y * (1 - fraction)) + (newY * fraction);

                    lrsPoints.Add(new LRSPoint(
                       middleX,
                       middleY,
                       null,
                       M,
                       SRID
                       ));

                    lrsPoints.Add(new LRSPoint(
                      secondPointX,
                      secondPointY,
                      null,
                      M,
                      SRID
                      ));
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
            Slope = GetSlope(nextLRSPoint);
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        /// <param name="nextLRSPoint">The next LRS point.</param>
        internal double GetSlope(LRSPoint nextLRSPoint)
        {
            var xDifference = nextLRSPoint.X - X;

            return xDifference == 0 ? 0 : (nextLRSPoint.Y - Y) / (xDifference);
        }

        #endregion
    }
}