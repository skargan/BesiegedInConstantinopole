using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// A display preference, separate from narrative text, territory labels and diplomatic IDs.
public static class DiplomaticNames
{
    public enum Style { Short, Full, Original }
    [Serializable] public class Entry {public string Id,Short,Full,Original,Display;}
    [Serializable] class Catalogue {public int Version;public Entry[] Names;}
    static Entry[] entries;
    static readonly Dictionary<string,TMP_FontAsset> typefaces=new Dictionary<string,TMP_FontAsset>();
    static TMP_FontAsset originalTypeface;
    public static Entry[] All=>entries??(entries=JsonUtility.FromJson<Catalogue>(Resources.Load<TextAsset>("Content/DiplomaticNames").text).Names);
    const string Preference="SavingConstantinople.DiplomaticNameStyle";
    public static Style Mode {get=>(Style)Mathf.Clamp(PlayerPrefs.GetInt(Preference),0,2);set{PlayerPrefs.SetInt(Preference,(int)value);PlayerPrefs.Save();}}
    public static string Name(string id){
        if(id=="candar_realm")id="candar";if(id.StartsWith("sardinia"))id="sardinia";
        var entry=All.FirstOrDefault(e=>e.Id==id);if(entry==null)return id;
        return Mode==Style.Full?entry.Full:Mode==Style.Original?entry.Display:entry.Short;
    }
    public static void Bind(TMP_Text label,string id,string suffix=""){
        var binding=label.GetComponent<RealmNameHeading>()??label.gameObject.AddComponent<RealmNameHeading>();binding.Id=id;binding.Suffix=suffix;binding.Refresh();
    }
    public static void Selector(Transform parent,Vector2 min,Vector2 max){
        var button=CampaignHUD.MakeButton(parent,"Names: "+Mode,min,max,()=>{});button.gameObject.name="Realm naming selector";
        button.gameObject.AddComponent<RealmNameSelectorCaption>();
        button.onClick.AddListener(()=>{Mode=(Style)(((int)Mode+1)%3);button.GetComponentInChildren<TMP_Text>().text="Names: "+Mode;});
    }
    public static void AddFonts(TMP_FontAsset primary){
        // Native serifs precede TMP's generic default; otherwise Cyrillic silently
        // selects a sans face even though the intended family contains every glyph.
        int position=0;
        foreach(string name in new[]{"NotoSerif-Regular","NotoSerifGeorgian-Regular","NotoNaskhArabic-Regular"}){
            var fallback=Typeface(name);if(fallback==null||primary.fallbackFontAssetTable.Contains(fallback))continue;
            primary.fallbackFontAssetTable.Insert(position++,fallback);
        }
    }
    static TMP_FontAsset Typeface(string name){
        if(typefaces.TryGetValue(name,out var cached)&&cached!=null)return cached;
        var source=Resources.Load<Font>("Fonts/"+name);if(source==null)return null;
        var font=TMP_FontAsset.CreateFontAsset(source,90,9,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
        font.name="Diplomatic "+name;var info=font.faceInfo;info.scale=1; font.faceInfo=info;
        font.fallbackFontAssetTable=new List<TMP_FontAsset>();typefaces[name]=font;return font;
    }
    public static TMP_FontAsset OriginalTypeface {
        get {
            if(originalTypeface!=null)return originalTypeface;
            originalTypeface=Typeface("NotoSerif-Regular");if(originalTypeface==null)return ImperialTheme.Font;
            foreach(string name in new[]{"NotoSerifGeorgian-Regular","NotoNaskhArabic-Regular"}){
                var fallback=Typeface(name);if(fallback!=null)originalTypeface.fallbackFontAssetTable.Add(fallback);
            }
            return originalTypeface;
        }
    }
    // Keep original names and their Latin readings in a coherent book face.
    // Fixed plaques can use this helper without acquiring a realm-heading binding.
    public static void ApplyTypeface(TMP_Text label){
        if(label!=null)label.font=Mode==Style.Original?OriginalTypeface:ImperialTheme.Font;
    }
}
public sealed class RealmNameSelectorCaption:MonoBehaviour
{
    DiplomaticNames.Style previous;TMP_Text label;
    void Start(){label=GetComponentInChildren<TMP_Text>();previous=DiplomaticNames.Mode;Refresh();}
    void Refresh(){if(label!=null)label.text="Names: "+DiplomaticNames.Mode;}
    void Update(){if(previous!=DiplomaticNames.Mode){previous=DiplomaticNames.Mode;Refresh();}}
}
public sealed class RealmNameHeading:MonoBehaviour
{
    public string Id,Suffix="";DiplomaticNames.Style previous;TMP_Text label;float maximum;
    public void Refresh(){label=GetComponent<TMP_Text>();if(maximum==0)maximum=label.fontSize;previous=DiplomaticNames.Mode;DiplomaticNames.ApplyTypeface(label);label.text=DiplomaticNames.Name(Id)+Suffix;label.enableAutoSizing=true;label.fontSizeMax=maximum;label.fontSizeMin=14;}
    void Update(){if(previous!=DiplomaticNames.Mode)Refresh();}
}
