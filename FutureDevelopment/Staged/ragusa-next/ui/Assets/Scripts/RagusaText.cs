using System;
using UnityEngine;

// Authoring text and measured art coordinates are editable in Resources/Content/RagusaNarrative.json.
public static class RagusaText
{
    [Serializable] public class Passage { public string id, speaker, text; }
    [Serializable] public class Character { public string id, name, role, description; public string[] hover; }
    [Serializable] public class Point { public float x, y; }
    [Serializable] public class Figure { public string id; public int hitPriority; public Point[] polygon; }
    [Serializable] public class Scene { public string foreground, resource; public Figure[] characters; }
    [Serializable] public class Content { public string realmTitle, realmText; public Passage[] passages; public Character[] characters; public Scene[] scenes; }
    static Content content;
    public static Content Data {
        get {
            if(content!=null)return content;
            var asset=Resources.Load<TextAsset>("Content/RagusaNarrative");
            content=asset==null?new Content():JsonUtility.FromJson<Content>(asset.text);
            return content;
        }
    }
    public static string Get(string key,string fallback="") {
        if(Data.passages!=null)foreach(var p in Data.passages)if(p.id==key)return p.text;
        return fallback;
    }
    public static Character Person(string id) {
        if(Data.characters!=null)foreach(var p in Data.characters)if(p.id==id)return p;
        return new Character{id=id,name=id=="gazulus"?"Johannes Gazulus":"Benedetto Cotrugli",role="Ragusan representative",description="",hover=new string[0]};
    }
    public static Scene Painting(string foreground) {
        if(Data.scenes!=null)foreach(var p in Data.scenes)if(p.foreground==foreground)return p;
        return null;
    }
}
