// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that will shift an input geometry by a given amount in the x and
    /// y directions.  It directs its output to another sink, and can therefore be used in a pipeline if desired.
    /// </summary>
    public class ShiftGeometrySink : IGeometrySink110
    {
        private readonly IGeometrySink110 target;  // the target sink
		private readonly double xShift;         // How much to shift in the x direction.
		private readonly double yShift;         // How much to shift in the y direction.

        // We take an amount to shift in the x and y directions, as well as a target sink, to which
        // we will pipe our result.
        public ShiftGeometrySink(double xShift, double yShift, IGeometrySink110 target)
        {
            this.target = target;
            this.xShift = xShift;
            this.yShift = yShift;
        }

        // Just pass through without change.
        public void SetSrid(int srid)
        {
            target.SetSrid(srid);
        }

        // Just pass through without change.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            target.BeginGeometry(type);
        }

        // Each BeginFigure call will just move the start point by the required amount.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            target.BeginFigure(x + xShift, y + yShift, z, m);
        }

        // Each AddLine call will just move the endpoint by the required amount.
        public void AddLine(double x, double y, double? z, double? m)
        {
            target.AddLine(x + xShift, y + yShift, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }

        // Just pass through without change.
        public void EndFigure()
        {
            target.EndFigure();
        }

        // Just pass through without change.
        public void EndGeometry()
        {
            target.EndGeometry();
        }
    }
}
