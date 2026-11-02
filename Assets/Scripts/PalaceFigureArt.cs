using System;
using System.Collections.Generic;
using UnityEngine;

// Presentation-only lookup. Figure assets use the existing court/conversation IDs.
// Measured alpha trims are separate data; source PNG bytes and identity portraits stay untouched.
public static class PalaceFigureArt
{
    const string Folder="UI/Identity/CourtFigures/";
    sealed class Entry { public Sprite sprite;public Texture2D texture;public int users,used; }
    static readonly Dictionary<int,Entry> cached=new Dictionary<int,Entry>();
    static int clock;
    public static int CachedCount=>cached.Count;
    [Serializable] sealed class Layout { public Crop[] figures; }
    [Serializable] sealed class Crop { public int id,sourceWidth,sourceHeight,x,y,width,height; }
    static Dictionary<int,Crop> crops;
    public static string Resource(int person)=>Folder+person;
    public static Sprite Acquire(int person)=>Load(person,true);
    public static void Release(int person){if(cached.TryGetValue(person,out var entry))entry.users=Math.Max(0,entry.users-1);Trim();}
    static void Trim(int keep=-1)
    {
        while(cached.Count>4){int oldest=-1,time=int.MaxValue;foreach(var pair in cached)if(pair.Key!=keep&&pair.Value.users==0&&pair.Value.used<time){oldest=pair.Key;time=pair.Value.used;}
            if(oldest<0)return;var entry=cached[oldest];cached.Remove(oldest);if(entry.sprite!=null)UnityEngine.Object.Destroy(entry.sprite);if(entry.texture!=null)Resources.UnloadAsset(entry.texture);
        }
    }
    public static Sprite Get(int person)=>Load(person,false);
    static Sprite Load(int person,bool pin)
    {
        if(cached.TryGetValue(person,out var existing)){existing.used=++clock;if(pin)existing.users++;return existing.sprite;}
        Sprite sprite=null;
        var texture=Resources.Load<Texture2D>(Resource(person));
        if(texture!=null){
            if(crops==null){
                crops=new Dictionary<int,Crop>();var asset=Resources.Load<TextAsset>("Content/CourtFigures");
                var layout=asset==null?null:JsonUtility.FromJson<Layout>(asset.text);
                if(layout!=null&&layout.figures!=null)foreach(var crop in layout.figures)crops[crop.id]=crop;
            }
            var rect=new Rect(0,0,texture.width,texture.height);
            if(crops.TryGetValue(person,out var cut)&&cut.sourceWidth>0&&cut.sourceHeight>0&&cut.width>0&&cut.height>0){
                float sx=(float)texture.width/cut.sourceWidth,sy=(float)texture.height/cut.sourceHeight;
                if(cut.x>=0&&cut.y>=0&&cut.x+cut.width<=cut.sourceWidth&&cut.y+cut.height<=cut.sourceHeight)
                    rect=new Rect(cut.x*sx,cut.y*sy,cut.width*sx,cut.height*sy);
            }
            sprite=Sprite.Create(texture,rect,new Vector2(.5f,0));sprite.name="Palace figure "+person;
        }
        if(sprite!=null){cached[person]=new Entry{sprite=sprite,texture=texture,used=++clock,users=pin?1:0};Trim(person);}return sprite;
    }
}
