// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Data structure to capture MULTILINESTRING geometry type.
    /// </summary>
    internal class LRSMultiLine : IEnumerable
    {
        internal List<LRSLine> Lines;
        internal int SRID;
        private string wkt;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSMultiLine"/> class.
        /// </summary>
        /// <param name="srid">The srid.</param>
        internal LRSMultiLine(int srid)
        {
            SRID = srid;
            Lines = new List<LRSLine>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsEmpty { get { return !Lines.Any() || Lines.Count == 0; } }

        /// <summary>
        /// Gets a value indicating whether this instance is not empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsNotEmpty { get { return !IsEmpty; } }

        /// <summary>
        /// Gets the number of line segments in the MULTILINESTRING.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        internal int Count { get { return Lines.Any() ? Lines.Count : 0; } }

        /// <summary>
        /// Gets a value indicating whether this instance is MULTILINESTRING.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsMultiLine { get { return Lines.Any() && Lines.Count > 1; } }

        #region Add Lines

        /// <summary>
        /// Adds the line.
        /// </summary>
        /// <param name="lrsLine">The LRS line.</param>
        internal void AddLine(LRSLine line)
        {
            if (line != null && line.Points.Any() && line.IsLine)
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
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(wkt))
                return wkt;

            if (IsEmpty)
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

            if (IsMultiLine)
                wktBuilder.Append(")");

            wkt = wktBuilder.ToString();

            return wkt;
        }

        /// <summary>
        /// Method returns the SqlGeometry form of the MULTILINESTRING
        /// </summary>
        /// <returns>SqlGeometry</returns>
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
            BuildSqlGeometry(ref geomBuilder);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Method builds the SqlGeometry form of the MULTILINESTRING through reference GeometryBuilder.
        /// </summary>
        /// <param name="geomBuilder">Reference SqlGeometryBuilder to be used for building Geometry.</param>
        internal void BuildSqlGeometry(ref SqlGeometryBuilder geomBuilder)
        {
            if (IsEmpty)
                return;

            if (geomBuilder != null)
                geomBuilder = new SqlGeometryBuilder();
            geomBuilder.SetSrid(SRID);

            if (IsMultiLine)
                geomBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);

            // ignore points
            foreach (var line in Lines.Where(line => line.IsLine).ToList())
                line.BuildSqlGeometry(ref geomBuilder, true);

            if (IsMultiLine)
                geomBuilder.EndGeometry();
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
}