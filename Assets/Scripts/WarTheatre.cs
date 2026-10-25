using System;
using System.Linq;
using UnityEngine;

// Artwork-space coordinates: correctable separately from rules and saved state.
public static class WarTheatre
{
    [Serializable] public class Region { public string id,name,kind,role,group,note,referenceName; public Vector2[] points; public Vector2 label,front; }
    [Serializable] public class Catalogue { public int version; public Region[] regions; }
    static Catalogue catalogue;
    public static Catalogue Data {get {if(catalogue==null){catalogue=JsonUtility.FromJson<Catalogue>(Resources.Load<TextAsset>("Content/WarTheatre").text);Validate(catalogue);}return catalogue;}}
    public static readonly string[] ProvinceIds={"thrace","bulgaria","macedonia","bithynia","karasi","aydin","mentese","germiyan","anatolia","armeniakon","candar"};
    // Append-only save indices: do not reorder when map districts change.
    public static readonly string[] SavedProvinceIds={"thrace","bulgaria","macedonia","bithynia","karasi","saruhan","aydin","mentese","germiyan","anatolia","armeniakon","candar"};
    public static string CanonicalProvince(string id)=>id=="saruhan"?"aydin":id;
    public static Region Find(string id)=>Data.regions.FirstOrDefault(r=>r.id==CanonicalProvince(id));
    public static int Control(Region r)=>Mathf.Clamp(100-DailyLifeEvents.I("iWbwControlLoss_"+r.id),0,100);
    public static int Resistance(Region r)=>Mathf.Clamp(DailyLifeEvents.I("iWbwResistance_"+r.id),0,3);
    public static bool Captured(Region r)=>DailyLifeEvents.I("iWbwCaptured_"+r.id)>0;
    public static bool AtWar(Region r)=>DailyLifeEvents.I("iWbwWar_"+r.group)>0;
    public static string Status(Region r)=>Captured(r)?"Under imperial control":Resistance(r)>=2?"Armed insurrection":Resistance(r)==1?"Resistance network":Control(r)<60?"Ottoman authority weakened":"Ottoman administration";
    public static Color Tint(Region r){
        if(r.kind=="province"){if(Captured(r))return new Color(.59f,.25f,.77f,.48f);if(Resistance(r)>=2)return new Color(.95f,.35f,.10f,.48f);return Color.Lerp(new Color(.85f,.60f,.13f,.43f),new Color(.12f,.56f,.40f,.36f),Control(r)/100f);}
        if(r.kind=="neutral")return new Color(.60f,.57f,.46f,.28f);
        if(r.id=="morea"||r.kind=="capital")return new Color(.66f,.28f,.82f,.5f);
        int hash=0;foreach(char c in r.group)hash=(hash*31+c)&0x7fffffff;
        var color=Color.HSVToRGB((hash%360)/360f,.48f,.87f);color.a=.35f;return color;
    }
    public static void Validate(Catalogue c){
        if(c==null||c.version!=1||c.regions==null||c.regions.Count(r=>r.kind=="province")!=11)throw new Exception("Eleven theatre regions required");
        var ids=new System.Collections.Generic.HashSet<string>();foreach(var r in c.regions){if(!ids.Add(r.id)||r.label.x<0||r.label.x>1||r.label.y<0||r.label.y>1)throw new Exception("Invalid theatre entry "+r.id);if(r.kind!="colony"&&r.kind!="capital"&&(r.points==null||r.points.Length<3))throw new Exception("Missing territory polygon "+r.id);}
        foreach(var id in ProvinceIds)if(!ids.Contains(id))throw new Exception("Missing stable province "+id);
    }
}
