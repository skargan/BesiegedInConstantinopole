using UnityEngine;

// Hand-traced against Scenes/technologies-compact.png (1672 x 941).
// One boundary drives both the visible gold outline and pointer hit-testing.
public static class TechnologyDiscoveryShapes
{
    const float Width=1672,Height=941;
    static Vector2[] Trace(params float[] xy){var p=new Vector2[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new Vector2(xy[i*2]/Width,1-xy[i*2+1]/Height);return p;}
    public static readonly Vector2[][] Outlines={
        Trace(550,516,578,516,590,496,595,494,600,498,584,519,594,520,592,531,588,550,584,555,588,559,582,564,561,564,555,560,558,554,553,540),
        Trace(617,405,629,403,641,410,647,425,645,433,635,439,624,435,618,425),
        Trace(620,519,652,519,652,530,657,553,655,561,648,560,645,554,618,554,614,560,608,560,613,539),
        Trace(680,520,685,519,688,524,686,534,694,539,696,551,701,556,698,562,673,562,667,558,669,552,674,548,674,539,680,534),
        Trace(712,414,714,404,722,396,732,393,746,395,760,402,773,414,781,428,782,439,779,444,774,441,775,430,769,419,758,409,745,403,734,401,727,406,725,419,729,432,741,444,748,458,746,474,737,485,724,490,708,485,698,478,692,468,692,455,698,443,709,433),
        Trace(755,498,769,495,791,495,805,498,802,509,795,521,785,530,783,543,787,548,798,551,801,556,794,560,766,560,759,555,764,551,774,548,777,542,775,531,764,523,758,511),
        Trace(583,147,612,139,619,140,620,145,616,148,588,154,583,152),
        Trace(1386,157,1393,146,1412,136,1429,126,1440,119,1457,116,1465,110,1479,109,1491,114,1499,125,1501,138,1509,146,1512,158,1508,171,1499,180,1490,190,1479,198,1462,203,1437,209,1412,210,1404,201,1417,201,1436,196,1454,188,1460,180,1463,171,1454,167,1446,170,1434,174,1416,180,1400,178,1389,170),
        Trace(1415,407,1419,404,1424,404,1428,407,1429,412,1426,416,1421,418,1416,415,1414,411)
    };
    // Interior points for reward placement and resolution-aware interaction verification.
    public static readonly Vector2[] Anchors={
        new Vector2(573/Width,1-540/Height),new Vector2(634/Width,1-423/Height),
        new Vector2(634/Width,1-541/Height),new Vector2(684/Width,1-550/Height),
        new Vector2(720/Width,1-462/Height),new Vector2(780/Width,1-512/Height),
        new Vector2(602/Width,1-147/Height),new Vector2(1478/Width,1-149/Height),
        new Vector2(1421/Width,1-411/Height)
    };
    public static Vector2 Map(Vector2 p,Rect uv)=>new Vector2((p.x-uv.x)/uv.width,(p.y-uv.y)/uv.height);
}
