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
         * TODO: Have to implement the effect of tolerance
         **/
        public bool IsConnectedGeomSegments()
        {
            if (_g1.STEndPoint().STX == _g2.STStartPoint().STX && _g1.STEndPoint().STY == _g2.STStartPoint().STY)
            {
                return true;
            }
            else if(_g1.STStartPoint().STX == _g2.STStartPoint().STX && _g1.STStartPoint().STY == _g2.STStartPoint().STY)
            {
                return true;
            }
            else if(_g1.STStartPoint().STX == _g2.STEndPoint().STX && _g1.STStartPoint().STY == _g2.STEndPoint().STY)
            {
                return true;
            }
            else if(_g1.STEndPoint().STX == _g2.STEndPoint().STX && _g1.STEndPoint().STY == _g2.STEndPoint().STY)
            {
                return true;
            }
            return false;   // if not matches all the case
        }

        public void SetSrid(int srid)
        {

        }

        public static void log(Object str)
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
    }
}
