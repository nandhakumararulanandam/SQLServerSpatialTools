// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools
{
    /// <summary>
    /// Data structure to capture MULTILINESTRING geometry type.
    /// </summary>
    internal class LRSMultiLine
    {
        public List<LRSLine> Lines;

        public LRSMultiLine()
        {
            Lines = new List<LRSLine>();
        }

        /// <summary>
        /// Adds the line.
        /// </summary>
        /// <param name="lrsLine">The LRS line.</param>
        public void AddLine(LRSLine lrsLine)
        {
            Lines.Add(lrsLine);
        }

        /// <summary>
        /// Reverses the lines.
        /// </summary>
        public void ReversLines()
        {
            Lines.Reverse();
        }
    }

    /// <summary>
    /// Data structure to capture LINESTRING geometry type.
    /// </summary>
    internal class LRSLine
    {
        public List<LRSPoint> Points;

        public LRSLine()
        {
            Points = new List<LRSPoint>();
        }

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
            Points.Add(new LRSPoint(x, y, z, m));
        }

        /// <summary>
        /// Reverses the points of the line.
        /// </summary>
        public void ReversePoints()
        {
            Points.Reverse();
        }

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

            var parallelLine = new LRSLine();

            foreach(var point in Points)
            {
                parallelLine.AddPoint(point.GetParallelPoint(offset));
            }           

            return parallelLine;
        }
    }

    /// <summary>
    /// Data structure to capture POINT geometry type.
    /// </summary>
    internal class LRSPoint
    {
        // Fields.
        public readonly double x, y;
        public readonly double? z, m;

        public double? OffsetBearing;
        public double OffsetAngle;
        public double OffsetDistance;

        // Constructors.
        public LRSPoint(double x, double y, double? z, double? m)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.m = m;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static LRSPoint operator -(LRSPoint a, LRSPoint b)
        {
            return new LRSPoint(b.x - a.x, b.y - a.y, null, null);
        }

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
                    m
                    );

            return lrsPoint;
        }
    }
}