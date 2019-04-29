// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Types
{
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
        /// Gets a value indicating whether this instance is empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsEmpty { get { return !Points.Any() || Points.Count == 0; } }

        /// <summary>
        /// Gets a value indicating whether this instance is not empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsNotEmpty { get { return !IsEmpty; } }

        /// <summary>
        /// Gets the number of POINTs in this LINESTRING.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        internal int Count { get { return Points.Any() ? Points.Count : 0; } }

        /// <summary>
        /// Determines whether this instance is LINESTRING.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is line; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsLine { get { return Points.Any() && Points.Count > 1; } }

        /// <summary>
        /// Determines whether this instance is POINT.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is point; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsPoint { get { return Points.Any() && Points.Count == 1; } }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        internal double Length { get; private set; }

        #region Add Points

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="lRSPoint">The l rs point.</param>
        internal void AddPoint(LRSPoint lRSPoint)
        {
            UpdateLength(lRSPoint);
            Points.Add(lRSPoint);
        }

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="pointGeometry">The SqlGeometry point.</param>
        internal void AddPoint(SqlGeometry pointGeometry)
        {
            Points.Add(new LRSPoint(pointGeometry));
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
            AddPoint(new LRSPoint(x, y, z, m, SRID));
        }

        private void UpdateLength(LRSPoint currentPoint)
        {
            if (Points.Any() && Points.Count > 0)
            {
                var previousPoint = Points.Last();
                Length += previousPoint.GetDistance(currentPoint);
            }
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
        /// Gets the point at m.
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetPointAtM(double measure)
        {
            return Points.FirstOrDefault(e => e.M == measure);
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
            // If the input segment has only two points; then there is no way of collinearity 
            // so its no-op
            if (Points.Count <= 2)
                return;

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

                // for each point previous and next point slope is compared
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
        /// Determines whether the line is within the range of clip start and end measure.
        /// </summary>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns>
        ///   <c>true</c> if [is within range] [the specified start measure]; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsWithinRange(double startMeasure, double endMeasure, LRSPoint clipStartPoint, LRSPoint clipEndPoint)
        {
            int pointCounter = 0;

            var lastLRSPoint = new LRSPoint(0, 0, null, null, SRID);

            foreach (LRSPoint point in this)
            {
                var currentM = point.M.HasValue ? point.M : 0;

                if (point == clipEndPoint || point == clipStartPoint || (startMeasure > lastLRSPoint.M && startMeasure < currentM))
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

            if (IsEmpty)
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
        /// Converts to SqlGeometry.
        /// </summary>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry()
        {
            var geomBuilder = new SqlGeometryBuilder();
            return ToSqlGeometry(ref geomBuilder);
        }

        /// <summary>
        /// Method returns the SqlGeometry form of the MULTILINESTRING
        /// </summary>
        /// <param name="geomBuilder">Reference SqlGeometryBuilder to be used for building Geometry.</param>
        /// <returns>SqlGeometry</returns>
        internal SqlGeometry ToSqlGeometry(ref SqlGeometryBuilder geomBuilder)
        {
            // ignore if the line has only one point.
            if (Points.Count < 2)
                return SqlGeometry.Null;
            BuildSqlGeometry(ref geomBuilder);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Builds the SqlGeometry form LINESTRING or POINT
        /// </summary>
        /// <param name="geomBuilder">The geom builder.</param>
        internal void BuildSqlGeometry(ref SqlGeometryBuilder geomBuilder, bool isFromMultiLine = false)
        {
            if (!isFromMultiLine)
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
                // if not last point
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

        /// <summary>
        /// Populates the measures.
        /// </summary>
        /// <param name="segmentLength">Length of the segment.</param>
        /// <param name="currentLength">Length of the current.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns></returns>
        internal double PopulateMeasures(double segmentLength, double currentLength, double startMeasure, double endMeasure)
        {
            var pointCount = Points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                if (i == pointCount - 1)
                    Points[i].M = GetEndPointM();
                else if (i == 0)
                    Points[i].M = GetStartPointM();
                else
                {
                    var currentPoint = Points[i];
                    return currentPoint.ReCalculateMeasure(Points[i - 1], currentLength, segmentLength, startMeasure, endMeasure);
                }
            }
            return 0;
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
}