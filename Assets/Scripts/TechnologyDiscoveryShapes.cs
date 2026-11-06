using UnityEngine;

// Hand-traced against Scenes/technologies-compact.png (1672 x 941).
// One boundary drives both the visible gold outline and pointer hit-testing.
public static class TechnologyDiscoveryShapes
{
    const float Width=1672,Height=941;
    // Round only the last 1.25 painting pixels at each authored corner. This keeps
    // narrow necks and fingers of the silhouette, without coarse bevels or ovals.
    static Vector2[] Trace(params float[] xy){
        int count=xy.Length/2;var p=new Vector2[count*4];
        for(int i=0;i<count;i++){
            Vector2 corner=new Vector2(xy[i*2],xy[i*2+1]);int prev=(i+count-1)%count,next=(i+1)%count;
            Vector2 a=new Vector2(xy[prev*2],xy[prev*2+1]),b=new Vector2(xy[next*2],xy[next*2+1]);
            Vector2 from=Vector2.MoveTowards(corner,a,Mathf.Min(1.25f,Vector2.Distance(corner,a)*.15f));
            Vector2 to=Vector2.MoveTowards(corner,b,Mathf.Min(1.25f,Vector2.Distance(corner,b)*.15f));
            for(int n=0;n<4;n++){float t=n/3f;Vector2 q=(1-t)*(1-t)*from+2*(1-t)*t*corner+t*t*to;p[i*4+n]=new Vector2(q.x/Width,1-q.y/Height);}
        }return p;
    }
    public static readonly Vector2[][] Outlines={
        Trace(550,516,559,515,570,516,578,516,583,508,590,496,593,493,596,494,600,497,599,501,591,510,584,519,590,519,594,521,593,525,591,530,590,540,588,549,584,555,586,557,588,559,585,562,582,564,572,565,561,564,557,562,555,560,558,554,556,550,553,540,552,529,550,522),
        Trace(617,405,621,403,626,403,631,404,636,407,641,410,645,416,647,422,647,427,645,432,641,435,635,439,630,438,624,435,620,431,618,425,617,418),
        Trace(620,519,630,518,641,518,652,519,653,523,652,530,654,538,656,548,657,553,656,559,655,561,650,561,648,560,645,554,637,553,628,553,618,554,614,560,611,561,608,560,609,553,611,546,613,539,616,530),
        Trace(680,520,682,518,685,519,687,521,688,524,687,529,686,534,690,536,694,539,696,546,696,551,699,553,701,556,701,559,698,562,687,563,679,562,673,562,668,560,667,558,669,552,672,550,674,548,674,543,674,539,677,536,680,534,681,528),
        Trace(712,414,714,404,722,396,732,393,746,395,760,402,773,414,781,428,782,439,779,444,774,441,775,430,769,419,758,409,745,403,734,401,727,406,725,419,729,432,741,444,748,458,746,474,737,485,724,490,708,485,698,478,692,468,692,455,698,443,709,433),
        Trace(755,498,760,496,769,495,780,494,791,495,800,496,805,498,805,502,802,509,799,515,795,521,790,526,785,530,783,535,783,543,784,546,787,548,792,549,798,551,801,554,801,556,798,558,794,560,780,561,766,560,761,558,759,555,761,553,764,551,770,550,774,548,777,542,777,536,775,531,770,527,764,523,761,517,758,511,756,504),
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
