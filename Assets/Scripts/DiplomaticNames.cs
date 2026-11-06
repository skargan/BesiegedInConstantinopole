using System;
using System.Linq;
using TMPro;
using UnityEngine;

// A display preference, separate from narrative text, territory labels and diplomatic IDs.
public static class DiplomaticNames
{
    public enum Style { Short, Full, Original }
    [Serializable] public class Entry {public string Id,Short,Full,Original,Display;}
    [Serializable] class Catalogue {public int Version;public Entry[] Names;}
    static Entry[] entries;
    public static Entry[] All=>entries??(entries=JsonUtility.FromJson<Catalogue>(Resources.Load<TextAsset>("Content/DiplomaticNames").text).Names);
    const string Preference="SavingConstantinople.DiplomaticNameStyle";
    public static Style Mode {get=>(Style)Mathf.Clamp(PlayerPrefs.GetInt(Preference),0,2);set{PlayerPrefs.SetInt(Preference,(int)value);PlayerPrefs.Save();}}
    public static string Name(string id){
        if(id=="candar_realm")id="candar";if(id.StartsWith("sardinia"))id="sardinia";
        var entry=All.FirstOrDefault(e=>e.Id==id);if(entry==null)return id;
        return Mode==Style.Full?entry.Full:Mode==Style.Original?entry.Display:entry.Short;
    }
    public static void Bind(TMP_Text label,string id,string suffix=""){
        var binding=label.gameObject.AddComponent<RealmNameHeading>();binding.Id=id;binding.Suffix=suffix;binding.Refresh();
    }
    public static void Selector(Transform parent,Vector2 min,Vector2 max){
        var button=CampaignHUD.MakeButton(parent,"Names: "+Mode,min,max,()=>{});button.gameObject.name="Realm naming selector";
        button.onClick.AddListener(()=>{Mode=(Style)(((int)Mode+1)%3);button.GetComponentInChildren<TMP_Text>().text="Names: "+Mode;});
    }
    public static void AddFonts(TMP_FontAsset primary){
        foreach(string name in new[]{"NotoSerif-Regular","NotoSerifGeorgian-Regular","NotoNaskhArabic-Regular"}){
            var font=Resources.Load<Font>("Fonts/"+name);if(font==null)continue;
            var fallback=TMP_FontAsset.CreateFontAsset(font,90,9,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            primary.fallbackFontAssetTable.Add(fallback);
        }
    }
}
public sealed class RealmNameHeading:MonoBehaviour
{
    public string Id,Suffix="";DiplomaticNames.Style previous;TMP_Text label;float maximum;
    public void Refresh(){label=GetComponent<TMP_Text>();if(maximum==0)maximum=label.fontSize;previous=DiplomaticNames.Mode;label.text=DiplomaticNames.Name(Id)+Suffix;label.enableAutoSizing=true;label.fontSizeMax=maximum;label.fontSizeMin=14;}
    void Update(){if(previous!=DiplomaticNames.Mode)Refresh();}
}
