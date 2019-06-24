//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.UnitTests.DDD
{
    public class UtilDataSet
    {
        public class PolygonToLineData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "Util_PolygonToLineData";
            public const string DataFile = @"TestData\Util\PolygonToLine.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');";

            public string InputGeom { get; set; }
        }
    }
}
