using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One presentation of the existing relationship; no parallel diplomacy state.
public static class DiplomaticSummary
{
    public static string EnvoyLabel(int country){
        switch(CountryDiplomacy.EnvoyState(country)){
            case CountryDiplomacy.EnvoyAssignment.Resident:return "Envoy present";
            case CountryDiplomacy.EnvoyAssignment.Outbound:return "Envoy travelling";
            case CountryDiplomacy.EnvoyAssignment.Returning:return "Envoy returning";
            default:return "No envoy";
        }
    }
    public static void Draw(Transform parent,int country,Vector2 min,Vector2 max,float size=20,string suffix=""){
        var row=CampaignHUD.Panel(parent,"Diplomatic standing",min,max);
        row.GetComponent<Image>().color=Color.clear;row.GetComponent<Image>().raycastTarget=false;
        var badge=CampaignHUD.Panel(row.transform,"Court attitude badge",new Vector2(0,.12f),new Vector2(.065f,.88f));
        ImperialTheme.Panel(badge,false);badge.GetComponent<Image>().raycastTarget=false;
        DiplomaticArtwork.DrawAttitude(badge.transform,CountryDiplomacy.Attitude(country).ToString(),new Vector2(.09f,.09f),new Vector2(.91f,.91f));
        string acceptance=IonianDiplomacy.IsCountry(country)?"Private agreements":CountryDiplomacy.FinalChance(country).ToString("0.##")+"% acceptance";
        var text=CampaignHUD.Label(row.transform,CountryDiplomacy.Attitude(country)+" \u00b7 "+acceptance,new Vector2(.08f,0),new Vector2(.65f,1),size);
        Fit(text,size);text.gameObject.name="Court attitude text";
        var envoy=CampaignHUD.Label(row.transform,"\u00b7 "+EnvoyLabel(country)+suffix,new Vector2(.66f,0),Vector2.one,size);
        Fit(envoy,size);envoy.gameObject.name="Envoy standing";envoy.raycastTarget=true;
        envoy.gameObject.AddComponent<CampaignChoiceTooltip>().Hint=CountryDiplomacy.EnvoyStatus(country);
        var layout=row.AddComponent<DiplomaticStandingLayout>();layout.Badge=badge.GetComponent<RectTransform>();layout.Main=text;layout.Envoy=envoy;layout.Size=size;
    }
    static void Fit(TMP_Text text,float size){text.alignment=TextAlignmentOptions.MidlineLeft;text.enableWordWrapping=false;text.enableAutoSizing=true;text.fontSizeMax=size;text.fontSizeMin=Mathf.Max(14,size-3);}
}
public sealed class DiplomaticStandingLayout:MonoBehaviour
{
    public RectTransform Badge;public TMP_Text Main,Envoy;public float Size;
    Vector2 previous;
    void LateUpdate(){
        var bounds=((RectTransform)transform).rect.size;if(bounds==previous||bounds.x<=0)return;previous=bounds;
        float icon=Mathf.Min(34,bounds.y*.8f),gap=10;
        Main.enableAutoSizing=Envoy.enableAutoSizing=false;Main.fontSize=Envoy.fontSize=Size;
        float mainWidth=Main.GetPreferredValues().x,envoyWidth=Envoy.GetPreferredValues().x;
        float scale=Mathf.Min(1,(bounds.x-icon-gap*3)/(mainWidth+envoyWidth));
        if(scale<1){Main.fontSize=Envoy.fontSize=Mathf.Max(14,Size*scale);mainWidth=Main.GetPreferredValues().x;envoyWidth=Envoy.GetPreferredValues().x;}
        Place(Badge,0,icon,icon);Place(Main.rectTransform,icon+gap,mainWidth+2,bounds.y);Place(Envoy.rectTransform,icon+gap*2+mainWidth,envoyWidth+2,bounds.y);
    }
    static void Place(RectTransform rect,float left,float width,float height){rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.pivot=new Vector2(0,.5f);rect.anchoredPosition=new Vector2(left,0);rect.sizeDelta=new Vector2(width,height);}
}
