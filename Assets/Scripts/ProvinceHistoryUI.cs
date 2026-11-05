using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Read-only history is deliberately separate from the campaign and province ledgers.
public static class ProvinceHistoryUI
{
    [Serializable] public class Entry {public string id,title,kicker;public string[] paragraphs,sources;}
    [Serializable] class Catalogue {public Entry[] entries;}
    static Catalogue data;static GameObject root;
    public static Entry Find(string id){if(data==null)data=JsonUtility.FromJson<Catalogue>(Resources.Load<TextAsset>("Content/ProvinceHistories").text);return data.entries.FirstOrDefault(e=>e.id==WarTheatre.CanonicalProvince(id));}
    public static bool Close(){if(root==null)return false;root.SetActive(false);UnityEngine.Object.Destroy(root);root=null;return true;}
    public static void Show(string id)
    {
        var entry=Find(id);if(entry==null)return;Close();
        root=new GameObject("Province history",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=260;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        root.GetComponent<Image>().color=ImperialTheme.Navy;SceneBackdrop.Apply(root,"Provinces/"+id);
        var panel=CampaignHUD.Panel(root.transform,"History reading panel",new Vector2(.33f,.055f),new Vector2(.975f,.945f));ImperialTheme.Panel(panel,false);
        Label(panel.transform,"History · "+entry.title,.87f,.98f,31);
        var kicker=Label(panel.transform,entry.kicker,.75f,.87f,23);kicker.fontStyle=FontStyles.Italic;kicker.color=new Color(.67f,.81f,.89f);
        var body=Label(panel.transform,string.Join("\n\n",entry.paragraphs),.19f,.745f,24);body.name="Province historical account";body.lineSpacing=3;
        var note=Label(panel.transform,"The map uses broad historical regions; medieval boundaries changed over time.",.11f,.18f,18);note.color=new Color(.67f,.81f,.89f);note.fontStyle=FontStyles.Italic;
        var back=CampaignHUD.MakeButton(panel.transform,"Return to province",new Vector2(.25f,.025f),new Vector2(.75f,.10f),()=>Close());back.name="History return";
    }
    static TMP_Text Label(Transform parent,string value,float low,float high,int size){var t=CampaignHUD.Label(parent,value,new Vector2(.035f,low),new Vector2(.965f,high),size);t.enableAutoSizing=true;t.fontSizeMax=size;t.fontSizeMin=size==24?19:size-2;return t;}
}
