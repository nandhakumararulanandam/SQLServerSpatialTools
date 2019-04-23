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
        public List<LRSLine> Lines;
        private readonly int SRID;
        private string wkt;

        public LRSMultiLine(int srid)
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
        public bool IsMultiLine { get { return Lines.Any() && Lines.Count > 1; } }

        /// <summary>
        /// Gets the number of line segments in the MULTILINESTRING.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get { return Lines.Any() ? Lines.Count : 0; } }

        #region Add Lines

        /// <summary>
        /// Adds the line.
        /// </summary>
        /// <param name="lrsLine">The LRS line.</param>
        public void AddLine(LRSLine line)
        {
            Lines.Add(line);
        }

        /// <summary>
        /// Adds multiple lines.
        /// </summary>
        /// <param name="lineList">The line list.</param>
        public void AddLines(List<LRSLine> lineList)
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
        public void ScaleMeasure(double offsetMeasure)
        {
            foreach (var line in Lines)
                line.ScaleMeasure(offsetMeasure);
        }

        /// <summary>
        /// Sum the existing measure with the offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        public void TranslateMeasure(double offsetMeasure)
        {
            foreach (var line in Lines)
                line.TranslateMeasure(offsetMeasure);
        }

        /// <summary>
        /// Reverses only the LINESTRING segments order in a MULTILINESTRING
        /// </summary>
        public void ReversLines()
        {
            Lines.Reverse();
        }

        /// <summary>
        /// Reverse both LINESTRING segment and its POINTS
        /// </summary>
        public void ReverseLinesAndPoints()
        {
            ReversLines();
            foreach (var line in Lines)
                line.ReversePoints();
        }

        /// <summary>
        /// Gets the first LINESTRING in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        public LRSLine GetFirstLine()
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
        public LRSLine GetLastLine()
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
        public LRSPoint GetStartPoint()
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
        public LRSPoint GetEndPoint()
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
        public List<LRSLine> RemoveFirst()
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
        public List<LRSLine> RemoveLast()
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

            if(!Lines.Any())
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
                if(line.IsLine)
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
        public SqlGeometry ToSqlGeometry()
        {
            var geomBuilder = new SqlGeometryBuilder();
            geomBuilder.SetSrid(SRID);

            if (IsMultiLine)
                geomBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);

            foreach (LRSLine line in Lines)
            {
                // ignore points
                if (line.IsLine)
                    line.BuildSqlGeometry(ref geomBuilder);
            }

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
        public List<LRSPoint> Points;
        public int SRID;
        public bool IsInRange;
        public bool IsCompletelyInRange;
        private string wkt;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSLine"/> class.
        /// </summary>
        /// <param name="srid">The srid.</param>
        public LRSLine(int srid)
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
        public bool IsLine { get { return Points.Any() && Points.Count > 1; } }

        /// <summary>
        /// Determines whether this instance is point.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is point; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPoint { get { return Points.Any() && Points.Count == 1; } }

        #region Add Points

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="lRSPoint">The l rs point.</param>
        public void AddPoint(LRSPoint lRSPoint)
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
        public void AddPoint(double x, double y, double? z, double? m)
        {
            Points.Add(new LRSPoint(x, y, z, m, SRID));
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Reverses the points of the line.
        /// </summary>
        public void ReversePoints()
        {
            Points.Reverse();
        }

        /// <summary>
        /// Gets the start point.
        /// </summary>
        /// <returns></returns>
        public LRSPoint GetStartPoint()
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
        public double GetStartPointM()
        {
            var currentPoint = GetStartPoint().m;
            return currentPoint.HasValue ? (double)currentPoint : 0;
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <returns></returns>
        public LRSPoint GetEndPoint()
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
        public double GetEndPointM()
        {
            var currentPoint = GetEndPoint().m;
            return currentPoint.HasValue ? (double)currentPoint : 0;
        }

        /// <summary>
        /// Scale the existing measure of geometry by multiplying existing measure with offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        public void ScaleMeasure(double offsetMeasure)
        {
            foreach (var point in Points)
                point.ScaleMeasure(offsetMeasure);
        }

        /// <summary>
        /// Sum the existing measure with the offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        public void TranslateMeasure(double offsetMeasure)
        {
            foreach (var point in Points)
                point.TranslateMeasure(offsetMeasure);
        }

        /// <summary>
        /// Determines whether the line is within the range of start and end measure.
        /// </summary>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns>
        ///   <c>true</c> if [is within range] [the specified start measure]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWithinRange(double startMeasure, double endMeasure, LRSPoint startPoint, LRSPoint endPoint)
        {
            int pointCounter = 0;

            var lastLRSPoint = new LRSPoint(0, 0, null, null, SRID);

            foreach (LRSPoint point in this)
            {
                var currentM = point.m.HasValue ? point.m : 0;

                if (point == endPoint || point == startPoint || (startMeasure > lastLRSPoint.m && startMeasure < currentM))
                    pointCounter++;
                else if (currentM >= startMeasure && currentM <= endMeasure)
                    pointCounter++;
                lastLRSPoint = point;
            }

            IsInRange = pointCounter > 1;
            IsCompletelyInRange = pointCounter == Points.Count;

            return IsInRange;
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
                wktBuilder.AppendFormat("{0} {1} {2}", point.x, point.y, point.m);
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
        internal void BuildSqlGeometry(ref SqlGeometryBuilder geomBuilder)
        {
            geomBuilder.SetSrid(SRID);
            geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            var pointIterator = 1;
            foreach (var point in Points)
            {
                if (pointIterator == 1)
                    geomBuilder.BeginFigure(point.x, point.y, point.z, point.m);
                else
                    geomBuilder.AddLine(point.x, point.y, point.z, point.m);
                pointIterator++;
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
        }

        /// <summary>
        /// Converts to SqlGeometry.
        /// </summary>
        /// <returns></returns>
        public SqlGeometry ToSqlGeometry()
        {
            if (Points.Count < 2)
                return SqlGeometry.Null;

            if (Points.Count == 1)
                return SpatialExtensions.GetPoint(Points.First().x, Points.First().y, Points.First().z, Points.First().m, SRID);
            var geomBuilder = new SqlGeometryBuilder();
            BuildSqlGeometry(ref geomBuilder);
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
        public LRSLine ComputeParallelLine(double offset, LinearMeasureProgress progress)
        {
            CalculateOffsetBearing();
            CalculateOffsetAngle(progress);
            CalculateOffset(offset, progress);

            var parallelLine = new LRSLine(SRID);

            foreach (var point in Points)
            {
                parallelLine.AddPoint(point.GetParallelPoint(offset));
            }

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
        public double x, y;
        public double? z, m;
        public int SRID;

        public double? OffsetBearing;
        public double OffsetAngle;
        public double OffsetDistance;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        /// <param name="srid">The srid.</param>
        public LRSPoint(double x, double y, double? z, double? m, int srid)
        {
            this.x = x;
            this.y = y;
            this.z = m.HasValue ? z : null;
            this.m = m.HasValue ? m : z;
            SRID = srid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        public LRSPoint(SqlGeometry sqlGeometry)
        {
            if (sqlGeometry == null || sqlGeometry.STIsEmpty() || !sqlGeometry.IsPoint())
                return;

            this.x = sqlGeometry.STX.Value;
            this.y = sqlGeometry.STY.Value;
            this.z = sqlGeometry.HasZ ? sqlGeometry.Z.Value : (double?)null;
            this.m = sqlGeometry.HasM ? sqlGeometry.M.Value : (double?)null;
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Translating measure of LRSPoint
        /// </summary>
        /// <param name="offsetMeasure"></param>
        public void TranslateMeasure(double offsetMeasure)
        {
            m += offsetMeasure;
        }

        /// <summary>
        /// Scalinng Measure of LRSPoint
        /// </summary>
        /// <param name="offsetMeasure"></param>
        public void ScaleMeasure(double offsetMeasure)
        {
            m *= offsetMeasure;
        }

        /// <summary>
        /// Gets the offset point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        public double GetOffsetDistance(LRSPoint nextPoint)
        {
            return Math.Sqrt(Math.Pow(nextPoint.x - x, 2) + Math.Pow(nextPoint.y - y, 2));
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
            return new LRSPoint(b.x - a.x, b.y - a.y, null, null, a.SRID);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(LRSPoint a, LRSPoint b)
        {
            return ReferenceEquals(b, null) ? false : a.x == b.x && a.y == b.y && EqualityComparer<double?>.Default.Equals(a.m, b.m);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(LRSPoint a, LRSPoint b)
        {
            return ReferenceEquals(b, null) ? true : a.x != b.x || a.y != b.y || a.m != b.m;
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
                   x == point.x &&
                   y == point.y &&
                   EqualityComparer<double?>.Default.Equals(m, point.m);
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
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<double?>.Default.GetHashCode(z);
            hashCode = hashCode * -1521134295 + EqualityComparer<double?>.Default.GetHashCode(m);
            return hashCode;
        }

        #endregion

        #region Parallel Point Computation

        /// <summary>
        /// Gets the offset point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        public LRSPoint GetOffsetPoint(LRSPoint nextPoint)
        {
            return this - nextPoint;
        }

        /// <summary>
        /// Gets the arc to tangent.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns></returns>
        public double GetAtan(LRSPoint nextPoint)
        {
            var offsetPoint = GetOffsetPoint(nextPoint);
            return Math.Atan2(offsetPoint.y, offsetPoint.x);
        }

        /// <summary>
        /// Sets the offset bearing.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        public void SetOffsetBearing(LRSPoint nextPoint)
        {
            if (nextPoint != null)
                OffsetBearing = (90 - Util.ToDegrees(GetAtan(nextPoint)) + 360) % 360;
        }

        /// <summary>
        /// Sets the offset angle.
        /// </summary>
        /// <param name="currentPoint">The current point.</param>
        /// <param name="progress">The Linear Measure Progress.</param>
        public void SetOffsetAngle(LRSPoint currentPoint, LinearMeasureProgress progress)
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
        public void SetOffsetDistance(double offset)
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
        public LRSPoint GetParallelPoint(double offset)
        {
            var lrsPoint = new LRSPoint(
                    (x + (OffsetDistance * Math.Cos(Util.ToRadians(90 - OffsetAngle)))),
                    (y + (OffsetDistance * Math.Sin(Util.ToRadians(90 - OffsetAngle)))),
                    null,
                    m,
                    SRID
                    );

            return lrsPoint;
        }

        #endregion
    }

    /// <summary>
    /// Enumerator for LRS Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.IEnumerator" />
    public class LRSEnumerator<T> : IEnumerator
    {
        public List<T> ListOfItems;

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