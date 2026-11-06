using UnityEngine;

// Traced against the current 1672 x 941 paintings. Points follow visible openings,
// sail edges, masts and hulls, not their bounding rectangles.
public static class PalaceObjectContours
{
    static Vector2[] Trace(params float[] xy){var p=new Vector2[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new Vector2(xy[i*2]/1672f,1-xy[i*2+1]/941f);return p;}
    public static readonly Vector2[] GardenToPalace=Trace(
        55,218, 80,219, 111,224, 153,232, 154,514, 148,529,
        120,533, 91,524, 80,507, 76,489, 84,468, 77,449,
        62,433, 55,414, 54,381, 50,353, 49,311, 51,269);
    public static readonly Vector2[] PalaceToChamber=Trace(
        51,599, 49,527, 48,446, 48,365, 51,339, 58,316,
        69,297, 83,281, 98,271, 111,272, 128,283, 144,300,
        158,319, 169,342, 175,366, 179,443, 181,521, 182,591,
        147,597, 99,602);
    public static readonly Vector2[] ChamberToPalace=Trace(
        82,647, 79,563, 78,474, 77,384, 77,294, 79,234,
        84,211, 94,189, 107,172, 124,158, 143,150, 160,151,
        180,160, 199,174, 217,194, 232,215, 239,239,
        240,323, 240,420, 239,521, 238,624, 191,633, 138,641);
    public static readonly Vector2[][] FleetShips={
        Trace(1017,335,1024,332,1028,320,1030,309,1032,307,1033,332,1043,331,1048,333,1051,336,1047,341,1034,342,1021,340),
        Trace(1080,379,1089,374,1094,353,1099,335,1101,331,1104,351,1104,373,1116,371,1127,368,1137,372,1131,381,1110,387,1090,386),
        Trace(1119,317,1124,313,1128,302,1130,297,1132,296,1133,312,1137,313,1141,316,1138,320,1129,322,1122,321)
    };
}
