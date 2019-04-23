// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools
{
    /// <summary>
    /// Data structure to capture MULTILINESTRING geometry type.
    /// </summary>
    internal class LRSMultiLine : IEnumerable
    {
        internal List<LRSLine> Lines;
        private readonly int SRID;
        private string wkt;

        internal LRSMultiLine(int srid)
        {
            SRID = srid;
            Lines = new List<LRSLine>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is MULTILINESTRING.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsMultiLine { get { return Lines.Any() && Lines.Count > 1; } }

        /// <summary>
        /// Gets the number of line segments in the MULTILINESTRING.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        internal int Count { get { return Lines.Any() ? Lines.Count : 0; } }

        #region Add Lines

        /// <summary>
        /// Adds the line.
        /// </summary>
        /// <param name="lrsLine">The LRS line.</param>
        internal void AddLine(LRSLine line)
        {
            Lines.Add(line);
        }

        /// <summary>
        /// Adds multiple lines.
        /// </summary>
        /// <param name="lineList">The line list.</param>
        internal void AddLines(List<LRSLine> lineList)
        {
            if (lineList != null && lineList.Any())
                Lines.AddRange(lineList.ToArray());
        }

        #endregion

        #region Line Manipulation

        /// <summary>
        /// Scale the existing measure of geometry by multiplying existing measure with offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void ScaleMeasure(double offsetMeasure)
        {
            Lines.ForEach(line => line.ScaleMeasure(offsetMeasure));
        }

        /// <summary>
        /// Sum the existing measure with the offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void TranslateMeasure(double offsetMeasure)
        {
            Lines.ForEach(line => line.TranslateMeasure(offsetMeasure));
        }

        /// <summary>
        /// Reverses only the LINESTRING segments order in a MULTILINESTRING
        /// </summary>
        internal void ReversLines()
        {
            Lines.Reverse();
        }

        /// <summary>
        /// Removes the collinear points.
        /// </summary>
        internal void RemoveCollinearPoints()
        {
            // First calculate the slope to remove collinear points.
            CalculateSlope();
            Lines.ForEach(line => line.RemoveCollinearPoints());
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        internal void CalculateSlope()
        {
            Lines.ForEach(line => line.CalculateSlope());
        }

        /// <summary>
        /// Reverse both LINESTRING segment and its POINTS
        /// </summary>
        internal void ReverseLinesAndPoints()
        {
            ReversLines();
            Lines.ForEach(line => line.ReversePoints());
        }

        /// <summary>
        /// Gets the first LINESTRING in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal LRSLine GetFirstLine()
        {
            if (Lines.Any())
            {
                return Lines.First();
            }
            return null;
        }

        /// <summary>
        /// Gets the last LINESTRING in a MULTILINESTRING.
        /// </summary>
        /// <returns></returns>
        internal LRSLine GetLastLine()
        {
            if (Lines.Any())
            {
                return Lines.Last();
            }
            return null;
        }

        /// <summary>
        /// Gets the start POINT in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetStartPoint()
        {
            if (Lines.Any())
            {
                return Lines.First().Points.First();
            }
            return null;
        }

        /// <summary>
        /// Gets the end POINT in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetEndPoint()
        {
            if (Lines.Any())
            {
                return Lines.Last().Points.Last();
            }
            return null;
        }

        /// <summary>
        /// Removes the first.
        /// </summary>
        /// <returns></returns>
        internal List<LRSLine> RemoveFirst()
        {
            if (Lines.Any())
            {
                Lines.RemoveAt(0);
                return Lines;
            }
            return null;
        }

        /// <summary>
        /// Removes the last.
        /// </summary>
        /// <returns></returns>
        internal List<LRSLine> RemoveLast()
        {
            if (Lines.Any())
            {
                Lines.RemoveAt(Lines.Count - 1);
                return Lines;
            }
            return null;
        }

        #endregion

        #region Data Structure Conversion

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

            if (!Lines.Any())
            {
                wkt = string.Empty;
                return "MULTILINESTRING EMPTY";
            }

            var wktBuilder = new StringBuilder();

            if (IsMultiLine)
                wktBuilder.Append("MULTILINESTRING(");

            var lineIterator = 1;

            foreach (var line in Lines)
            {
                if (line.IsLine)
                    wktBuilder.Append(line.ToString());

                if (lineIterator != Lines.Count)
                    wktBuilder.Append(", ");
                lineIterator++;
            }
            wktBuilder.Append(")");
            wkt = wktBuilder.ToString();

            return wkt;
        }

        /// <summary>
        /// Method returns the SqlGeometry form of the MULTILINESTRING
        /// </summary>
        /// <param name="srid"></param>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry()
        {
            var geomBuilder = new SqlGeometryBuilder();
            geomBuilder.SetSrid(SRID);

            if (IsMultiLine)
                geomBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);

            // ignore points
            Lines.Where(line => line.IsLine).ToList().ForEach(e => e.BuildSqlGeometry(ref geomBuilder, true));

            if (IsMultiLine)
                geomBuilder.EndGeometry();

            return geomBuilder.ConstructedGeometry;
        }

        #endregion

        #region Enumeration

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public LRSEnumerator<LRSLine> GetEnumerator()
        {
            return new LRSEnumerator<LRSLine>(Lines);
        }

        /// <summary>
        /// Returns an enumerator that iterates through each LINESTRING in a MULTILINESTRING.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Data structure to capture LINESTRING geometry type.
    /// </summary>
    internal class LRSLine : IEnumerable
    {
        internal List<LRSPoint> Points;
        internal int SRID;
        internal bool IsInRange;
        internal bool IsCompletelyInRange;
        private string wkt;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSLine"/> class.
        /// </summary>
        /// <param name="srid">The srid.</param>
        internal LRSLine(int srid)
        {
            SRID = srid;
            Points = new List<LRSPoint>();
        }

        /// <summary>
        /// Determines whether this instance is line.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is line; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsLine { get { return Points.Any() && Points.Count > 1; } }

        /// <summary>
        /// Determines whether this instance is point.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is point; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsPoint { get { return Points.Any() && Points.Count == 1; } }

        #region Add Points

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="lRSPoint">The l rs point.</param>
        internal void AddPoint(LRSPoint lRSPoint)
        {
            Points.Add(lRSPoint);
        }

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        internal void AddPoint(double x, double y, double? z, double? m)
        {
            Points.Add(new LRSPoint(x, y, z, m, SRID));
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Reverses the points of the line.
        /// </summary>
        internal void ReversePoints()
        {
            Points.Reverse();
        }

        /// <summary>
        /// Gets the start point.
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetStartPoint()
        {
            if (Points.Any())
            {
                return Points.First();
            }
            return null;
        }

        /// <summary>
        /// Gets the start point measure.
        /// </summary>
        /// <returns></returns>
        internal double GetStartPointM()
        {
            var currentPoint = GetStartPoint().M;
            return currentPoint.HasValue ? (double)currentPoint : 0;
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetEndPoint()
        {
            if (Points.Any())
            {
                return Points.Last();
            }
            return null;
        }

        /// <summary>
        /// Gets the end point m.
        /// </summary>
        /// <returns></returns>
        internal double GetEndPointM()
        {
            var currentPoint = GetEndPoint().M;
            return currentPoint.HasValue ? (double)currentPoint : 0;
        }

        /// <summary>
        /// Scale the existing measure of geometry by multiplying existing measure with offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void ScaleMeasure(double offsetMeasure)
        {
            Points.ForEach(point => point.ScaleMeasure(offsetMeasure));
        }

        /// <summary>
        /// Sum the existing measure with the offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void TranslateMeasure(double offsetMeasure)
        {
            Points.ForEach(point => point.TranslateMeasure(offsetMeasure));
        }

        /// <summary>
        /// Removes the collinear points.
        /// </summary>
        internal void RemoveCollinearPoints()
        {
            var pointCounter = 0;
            var pointsToRemove = new List<LRSPoint>();

            foreach (var point in this)
            {
                // ignore first point and last point
                if (pointCounter == 0 || pointCounter == Points.Count() - 1)
                {
                    pointCounter++;
                    continue;
                }

                var previousPoint = Points[pointCounter - 1];
                var nextPoint = Points[pointCounter + 1];

                // if slope of AB = BC = CA then they are collinear
                if (previousPoint.Slope == point.Slope && point.Slope == nextPoint.Slope)
                    pointsToRemove.Add(point);
                pointCounter++;
            }

            // remove the collinear points
            RemovePoints(pointsToRemove);
        }

        /// <summary>
        /// Removes the points.
        /// </summary>
        /// <param name="pointsToRemove">The points to remove.</param>
        internal void RemovePoints(List<LRSPoint> pointsToRemove)
        {
            pointsToRemove.ForEach(pointToRemove => Points.Remove(pointToRemove));
        }

        /// <summary>
        /// Determines whether the line is within the range of start and end measure.
        /// </summary>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns>
        ///   <c>true</c> if [is within range] [the specified start measure]; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsWithinRange(double startMeasure, double endMeasure, LRSPoint startPoint, LRSPoint endPoint)
        {
            int pointCounter = 0;

            var lastLRSPoint = new LRSPoint(0, 0, null, null, SRID);

            foreach (LRSPoint point in this)
            {
                var currentM = point.M.HasValue ? point.M : 0;

                if (point == endPoint || point == startPoint || (startMeasure > lastLRSPoint.M && startMeasure < currentM))
                    pointCounter++;
                else if (currentM >= startMeasure && currentM <= endMeasure)
                    pointCounter++;
                lastLRSPoint = point;
            }

            IsInRange = pointCounter > 1;
            IsCompletelyInRange = pointCounter == Points.Count;

            return IsInRange;
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        internal void CalculateSlope()
        {
            var pointIterator = 1;
            foreach (var point in Points)
            {
                // last point; next point is the first point
                if (pointIterator == Points.Count)
                    point.CalculateSlope(Points.First());
                else
                    point.CalculateSlope(Points[pointIterator]);

                pointIterator++;
            }
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

            if (!IsLine && !IsPoint)
            {
                wkt = string.Empty;
                return wkt;
            }

            var wktBuilder = new StringBuilder();

            if (IsLine)
                wktBuilder.Append("LINESTRING(");
            else if (IsPoint)
                wktBuilder.Append("POINT(");

            var pointIterator = 1;

            foreach (var point in Points)
            {
                wktBuilder.AppendFormat("{0} {1} {2}", point.X, point.Y, point.M);
                if (pointIterator != Points.Count)
                    wktBuilder.Append(", ");
                pointIterator++;
            }
            wktBuilder.Append(")");
            wkt = wktBuilder.ToString();

            return wkt;
        }

        /// <summary>
        /// Builds the SQL geometry.
        /// </summary>
        /// <param name="geomBuilder">The geom builder.</param>
        internal void BuildSqlGeometry(ref SqlGeometryBuilder geomBuilder, bool isFromMultiLine = false)
        {
            if(!isFromMultiLine)
                geomBuilder.SetSrid(SRID);
            geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            var pointIterator = 1;
            foreach (var point in Points)
            {
                if (pointIterator == 1)
                    geomBuilder.BeginFigure(point.X, point.Y, point.Z, point.M);
                else
                    geomBuilder.AddLine(point.X, point.Y, point.Z, point.M);
                pointIterator++;
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
        }

        /// <summary>
        /// Converts to SqlGeometry.
        /// </summary>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry()
        {
            if (Points.Count < 2)
                return SqlGeometry.Null;

            if (Points.Count == 1)
                return SpatialExtensions.GetPoint(Points.First().X, Points.First().Y, Points.First().Z, Points.First().M, SRID);
            var geomBuilder = new SqlGeometryBuilder();
            BuildSqlGeometry(ref geomBuilder, true);
            return geomBuilder.ConstructedGeometry;
        }

        #endregion

        #region Modules for Offset Angle Calculation
        /// <summary>
        /// Calculate offset bearings for all points.
        /// </summary>
        private void CalculateOffsetBearing()
        {
            var pointCount = Points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = Points[i];
                if (i != pointCount - 1)
                {
                    var nextPoint = Points[i + 1];
                    currentPoint.SetOffsetBearing(nextPoint);
                }
                else
                {
                    currentPoint.OffsetBearing = null;
                }
            }
        }

        /// <summary>
        /// Calculate offset angle between points
        /// </summary>
        /// <param name="progress"></param>
        private void CalculateOffsetAngle(LinearMeasureProgress progress)
        {
            var pointCount = Points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = Points[i];
                if (i != pointCount - 1)
                {
                    // last point
                    var nextPoint = Points[i + 1];
                    nextPoint.SetOffsetAngle(currentPoint, progress);
                }
                else
                {
                    // first point
                    Points[0].SetOffsetAngle(currentPoint, progress);
                }
            }
        }

        /// <summary>
        /// Calculate offset distance between points 
        /// </summary>
        /// <param name="offset">Offset value</param>
        /// <param name="progress">Measure Progress</param>
        private void CalculateOffset(double offset, LinearMeasureProgress progress)
        {
            var pointCount = Points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = Points[i];
                if (i != pointCount - 1)
                {
                    // other points point
                    currentPoint.SetOffsetDistance(offset);
                }
                else
                {
                    // for the last point specified offset is offset distance.
                    // if decrease offset distance should be negative
                    currentPoint.OffsetDistance = progress == LinearMeasureProgress.Increasing ? offset : -offset;
                }
            }
        }

        /// <summary> 
        /// Compute a parallel line to input line segment 
        /// </summary>
        /// <param name="offset">Offset Value</param>
        /// <param name="progress">Measure Progress</param>
        /// <returns>Parallel Line</returns>
        internal LRSLine ComputeParallelLine(double offset, LinearMeasureProgress progress)
        {
            CalculateOffsetBearing();
            CalculateOffsetAngle(progress);
            CalculateOffset(offset, progress);

            var parallelLine = new LRSLine(SRID);
            Points.ForEach(point => parallelLine.AddPoint(point.GetParallelPoint(offset)));
            return parallelLine;
        }

        #endregion

        #region Enumeration

        /// <summary>
        /// Returns an enumerator that iterates through each POINT in a LINESTRING.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public LRSEnumerator<LRSPoint> GetEnumerator()
        {
            return new LRSEnumerator<LRSPoint>(Points);
        }

        #endregion
    }

    /// <summary>
    /// Data structure to capture POINT geometry type.
    /// </summary>
    internal class LRSPoint
    {
        // Fields.
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
        /// Gets the offset point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        internal double GetOffsetDistance(LRSPoint nextPoint)
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
            return ReferenceEquals(b, null) ? false : a.X == b.X && a.Y == b.Y && EqualityComparer<double?>.Default.Equals(a.M, b.M);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(LRSPoint a, LRSPoint b)
        {
            return ReferenceEquals(b, null) ? true : a.X != b.X || a.Y != b.Y || a.M != b.M;
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

    /// <summary>
    /// Enumerator for LRS Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.IEnumerator" />
    internal class LRSEnumerator<T> : IEnumerator
    {
        internal List<T> ListOfItems;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public LRSEnumerator(List<T> list)
        {
            ListOfItems = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < ListOfItems.Count);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public T Current
        {
            get
            {
                try
                {
                    return ListOfItems[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}