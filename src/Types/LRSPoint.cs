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
            this.X = x;
            this.Y = y;
            this.Z = m.HasValue ? z : null;
            this.M = m.HasValue ? m : z;
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

            this.X = sqlGeometry.STX.Value;
            this.Y = sqlGeometry.STY.Value;
            this.Z = sqlGeometry.HasZ ? sqlGeometry.Z.Value : (double?)null;
            this.M = sqlGeometry.HasM ? sqlGeometry.M.Value : (double?)null;
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
            return !(a==b);
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

            wkt =  string.Format("POINT ({0} {1} {2})", X, Y, M);

            return wkt;
        }

        /// <summary>
        /// Converts to SqlGeometry.
        /// </summary>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry()
        {
            return SpatialExtensions.GetPoint(X, Y, Z, M);
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
        /// <param name="currentPoint">The current point.</param>
        /// <param name="progress">The Linear Measure Progress.</param>
        internal void SetOffsetAngle(LRSPoint currentPoint, LinearMeasureProgress progress)
        {
            double offsetAngle;

            var currentPointOffsetBearing = currentPoint?.OffsetBearing;

            // Left
            if (progress == LinearMeasureProgress.Increasing)
            {
                if (OffsetBearing == null)
                    offsetAngle = (double)currentPointOffsetBearing - 90;
                else if (currentPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing - 90;
                else
                    //(360 + b1.OffsetBearing - ((360 - ((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = (360 + (double)OffsetBearing - ((360 - (((double)currentPointOffsetBearing + 180) - (double)OffsetBearing)) / 2)) % 360;
            }
            // Right
            else
            {

                if (OffsetBearing == null)
                    offsetAngle = (double)currentPointOffsetBearing + 90;
                else if (currentPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing + 90;
                else
                    // (b1.OffsetBearing + ((((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = ((double)OffsetBearing + (((((double)currentPointOffsetBearing + 180) - (double)OffsetBearing)) / 2)) % 360;
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
        /// Re calculate the measure.
        /// </summary>
        /// <param name="previousPoint">The previous point.</param>
        /// <param name="totalLength">The total length.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        internal double ReCalculateMeasure(LRSPoint previousPoint, double currentLength, double totalLength, double startMeasure, double endMeasure)
        {
            currentLength += GetDistance(previousPoint);
            M = startMeasure + (currentLength / totalLength) * (endMeasure - startMeasure);
            return currentLength;
        }

        /// <summary>
        /// Gets the parallel point.
        /// </summary>
        /// <param name="offset">The offset value.</param>
        /// <returns>Point parallel to the current point.</returns>
        internal LRSPoint GetParallelPoint(double offset)
        {
            var lrsPoint = new LRSPoint(
                    (X + (OffsetDistance * Math.Cos(Util.ToRadians(90 - OffsetAngle)))),
                    (Y + (OffsetDistance * Math.Sin(Util.ToRadians(90 - OffsetAngle)))),
                    null,
                    M,
                    SRID
                    );

            return lrsPoint;
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        /// <param name="nextLRSPoint">The next LRS point.</param>
        internal void CalculateSlope(LRSPoint nextLRSPoint)
        {
            var xDifference = nextLRSPoint.X - X;

            Slope = xDifference == 0 ? 0 : (nextLRSPoint.Y - Y) / (xDifference);
        }

        #endregion
    }
}