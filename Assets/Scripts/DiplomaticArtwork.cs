using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Art is keyed by court identity, never by the append-only campaign array position.
public static class DiplomaticArtwork
{
    const string Folder="UI/MidnightMosaic/Diplomacy/";
    const int CacheLimit=16;
    sealed class Entry { public Sprite sprite;public Texture2D texture;public int users,used; }
    [Serializable] sealed class Catalogue { public Crop[] items; }
    [Serializable] sealed class Crop { public string key;public int sourceWidth,sourceHeight,x,y,width,height; }
    static readonly Dictionary<string,Entry> cache=new Dictionary<string,Entry>();
    static Dictionary<string,Crop> crops;static int clock;
    public static int CachedCount=>cache.Count;
    public static string CourtId(string id)=>id=="gazaria"?"genoa":id=="ionian_lordships"?"ionian_tocco":(id??"").ToLowerInvariant();
    public static Image DrawFlag(Transform parent,string id,Vector2 min,Vector2 max)=>Draw(parent,"Flags/"+CourtId(id),min,max);
    public static Image DrawAttitude(Transform parent,string attitude,Vector2 min,Vector2 max)=>Draw(parent,"Attitudes/"+(attitude??"neutral").ToLowerInvariant(),min,max);
    static Image Draw(Transform parent,string key,Vector2 min,Vector2 max)
    {
        var entry=Acquire(key);if(entry==null)return null;
        var go=new GameObject("Diplomatic art "+key,typeof(RectTransform),typeof(Image),typeof(DiplomaticArtworkView));go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
        var image=go.GetComponent<Image>();image.sprite=entry.sprite;image.color=Color.white;image.preserveAspect=true;image.raycastTarget=false;
        go.GetComponent<DiplomaticArtworkView>().Key=key;return image;
    }
    static Entry Acquire(string key)
    {
        if(cache.TryGetValue(key,out var ready)){ready.users++;ready.used=++clock;return ready;}
        var texture=Resources.Load<Texture2D>(Folder+key);if(texture==null)return null;
        if(crops==null){crops=new Dictionary<string,Crop>();var resource=Resources.Load<TextAsset>("Content/DiplomaticArtwork");var data=resource==null?null:JsonUtility.FromJson<Catalogue>(resource.text);if(data!=null&&data.items!=null)foreach(var item in data.items)if(!string.IsNullOrEmpty(item.key))crops[item.key]=item;}
        var rect=new Rect(0,0,texture.width,texture.height);
        if(crops.TryGetValue(key,out var crop)&&crop.sourceWidth>0&&crop.sourceHeight>0&&crop.width>0&&crop.height>0&&crop.x>=0&&crop.y>=0&&crop.x+crop.width<=crop.sourceWidth&&crop.y+crop.height<=crop.sourceHeight){
            float sx=(float)texture.width/crop.sourceWidth,sy=(float)texture.height/crop.sourceHeight;rect=new Rect(crop.x*sx,crop.y*sy,crop.width*sx,crop.height*sy);
        }
        var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f));sprite.name="DiplomaticArtwork/"+key;
        var entry=new Entry{sprite=sprite,texture=texture,users=1,used=++clock};cache[key]=entry;Trim();return entry;
    }
    internal static void Release(string key){if(key!=null&&cache.TryGetValue(key,out var entry))entry.users=Math.Max(0,entry.users-1);Trim();}
    static void Trim()
    {
        while(cache.Count>CacheLimit){string victim=null;int oldest=int.MaxValue;foreach(var pair in cache)if(pair.Value.users==0&&pair.Value.used<oldest){victim=pair.Key;oldest=pair.Value.used;}
            if(victim==null)return;var entry=cache[victim];cache.Remove(victim);UnityEngine.Object.Destroy(entry.sprite);Resources.UnloadAsset(entry.texture);
        }
    }
}

// Lifetime follows its Image, so a shared state emblem cannot be evicted while a card displays it.
public sealed class DiplomaticArtworkView : MonoBehaviour
{
    public string Key { get; internal set; }
    void OnDestroy()=>DiplomaticArtwork.Release(Key);
}
