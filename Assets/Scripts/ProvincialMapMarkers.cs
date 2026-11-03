using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ProvincialMapMarkers
{
    static GameObject Symbol(Transform parent,int index,Vector2 point,float size,string name,UnityEngine.Events.UnityAction click)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(RawImage),typeof(Button));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=point;rt.sizeDelta=new Vector2(size,size);
        var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/WarTheatreSymbols");image.uvRect=new Rect(index%4*.25f,(3-index/4)*.25f,.25f,.25f);go.GetComponent<Button>().onClick.AddListener(click);return go;
    }
    public static void Add(Transform parent,WarTheatre.Region region)
    {
        var p=ProvincialWar.Find(region.id);var point=region.label+new Vector2(0,.045f);
        if(!ProvincialWar.Ready){if(ProvinceCampaign.Agent(p.Id)>0)Symbol(parent,0,point,30,"Provincial agent "+p.Id,()=>ProvincialConflictUI.Show(p.Id));return;}
        if(p.Owner==ProvincialWar.Imperial)Symbol(parent,3,point,34,"Imperial flag "+p.Id,()=>ProvincialConflictUI.Show(p.Id));
        else if(p.Owner>=ProvincialWar.AllyOwnerOffset){var flag=Symbol(parent,14,point,38,"Controlling flag "+p.Id,()=>ProvincialConflictUI.Show(p.Id));var art=DiplomaticArtwork.DrawFlag(flag.transform,CountryDiplomacy.Definitions[p.Owner-ProvincialWar.AllyOwnerOffset].Id,Vector2.zero,Vector2.one);if(art!=null)flag.GetComponent<RawImage>().color=Color.clear;}
        else if(p.Owner==ProvincialWar.Chaos)Symbol(parent,12,point,34,"Chaos "+p.Id,()=>ProvincialConflictUI.Show(p.Id));
        else if(p.Get("BattleStage")>0||p.Get("Settlement")>0)Symbol(parent,p.Get("BattleStage")>0?4:14,point,38,"Provincial decision "+p.Id,()=>ProvincialConflictUI.Show(p.Id));
        else if(p.Present.Any()){
            var go=Symbol(parent,p.Rebels.Troops>0?2:6,point,36,"Provincial army "+p.Id,()=>ProvincialConflictUI.Show(p.Id));
            var text=CampaignHUD.Label(go.transform,p.Present.Sum(a=>a.Troops).ToString("N0"),new Vector2(-.4f,-.42f),new Vector2(1.4f,.02f),13);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
        }
        else if(p.Resistance>0||p.Known)Symbol(parent,p.Resistance>0?1:0,point,30,"Provincial network "+p.Id,()=>ProvincialConflictUI.Show(p.Id));
    }
}
