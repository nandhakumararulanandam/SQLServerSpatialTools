// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that will return a boolean, if the given sqlgeometries are connected
     */
    public class ConnectedGeometrySegementSink : IGeometrySink110
    {
        private readonly bool isConnected;
        private readonly SqlGeometry _g1, _g2;
        private readonly double _tolerance;
        public ConnectedGeometrySegementSink(SqlGeometry g1, SqlGeometry g2, double tolerance)
        {

            _g1 = g1;
            _g2 = g2;
            _tolerance = tolerance;
        }
        /**
         ** Method will return a boolean if the concern line segements are conneced
         * 
         **/
        public bool IsConnectedGeomSegments()
        {
            // Type casting
            double g1StX = (double)_g1.STStartPoint().STX;
            double g1StY = (double)_g1.STStartPoint().STY;
            double g1EndX = (double)_g1.STEndPoint().STX;
            double g1EndY = (double)_g1.STEndPoint().STY;

            double g2StX = (double)_g2.STStartPoint().STX;
            double g2StY = (double)_g2.STStartPoint().STY;
            double g2EndX = (double)_g2.STEndPoint().STX;
            double g2EndY = (double)_g2.STEndPoint().STY;

            // GetPointsDistance == 0, indicates both the points extreme points touches
            if (GetPointsDistance(g1StX, g1StY, g2StX, g2StY) <= _tolerance)
                return true;    // if the starting point of g1 connected to the starting point of g2
            else if (GetPointsDistance(g1StX, g1StY, g2EndX, g2EndY) <= _tolerance)
                return true;    // if the starting point of g1 connected to the ending point of g2
            else if(GetPointsDistance(g1EndX, g1EndY, g2StX, g2StY) <= _tolerance)
                return true;    // if the ending point of g1 connected to the starting point of g2
            else if(GetPointsDistance(g1EndX, g1EndY, g2EndX, g2EndY) <= _tolerance)
                return true;    // if the ending point of g1 connected to the ending point of g2
            return false;   // if not matches all the case
        }

        public void SetSrid(int srid)
        {
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }
         
        public void EndFigure()
        {
        }

        public void EndGeometry()
        {
        }

        public void BeginGeometry(OpenGisGeometryType type)
        {
        }
        /**
         * Method will get the distance between the given points considered
         **/
        private static double GetPointsDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
    }
}
