using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One preparation choice per flexible month. Locked options remain visible with reasons.
public static class PreparationMenu
{
    public static void Show(Transform parent,Action<string> select)
    {
        var old=parent.Find("Monthly preparation");if(old!=null)UnityEngine.Object.Destroy(old.gameObject);
        var panel=CampaignHUD.Panel(parent,"Monthly preparation",Vector2.zero,Vector2.one);ImperialTheme.Panel(panel);
        CampaignHUD.Label(panel.transform,"SIEGE PREPARATIONS · "+VariableSingleton.GetDate().ToString("MMMM yyyy"),new Vector2(.04f,.9f),new Vector2(.96f,.98f),30);
        CampaignHUD.Explanation(panel.transform,"Choose one main preparation. Time is limited; completed choices cannot be repeated. Career achievements: "+PreparationCampaign.CareerCount,new Vector2(.04f,.81f),new Vector2(.96f,.9f),22);
        var viewport=CampaignHUD.Panel(panel.transform,"Preparation list",new Vector2(.04f,.15f),new Vector2(.92f,.8f));viewport.AddComponent<RectMask2D>();
        var content=new GameObject("Preparation choices",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);
        var rect=content.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);rect.sizeDelta=new Vector2(0,PreparationCampaign.Nodes.Length*140);rect.anchoredPosition=Vector2.zero;
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=rect;scroll.horizontal=false;scroll.scrollSensitivity=40;scroll.movementType=ScrollRect.MovementType.Clamped;
        CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.93f,.15f),new Vector2(.96f,.8f));
        bool chosen=false;
        Action<string> choose=node=>{if(chosen)return;chosen=true;panel.SetActive(false);UnityEngine.Object.Destroy(panel);select(node);};
        for(int i=0;i<PreparationCampaign.Nodes.Length;i++){
            int index=i;var row=CampaignHUD.Panel(content.transform,"Preparation "+i,Vector2.zero,Vector2.one);var r=row.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.offsetMin=new Vector2(5,-(i+1)*140+5);r.offsetMax=new Vector2(-5,-i*140-5);ImperialTheme.Frame(row);
            string reason=PreparationCampaign.Unavailable(i);
            CampaignHUD.Label(row.transform,"Siege Preparation — "+PreparationCampaign.Titles[i],new Vector2(.02f,.58f),new Vector2(.72f,.97f),24);
            CampaignHUD.Explanation(row.transform,PreparationCampaign.Summaries[i]+(reason==null?"":"\n"+reason),new Vector2(.02f,.05f),new Vector2(.72f,.58f),20);
            var button=CampaignHUD.MakeButton(row.transform,reason==null?"Choose":"Unavailable",new Vector2(.76f,.15f),new Vector2(.98f,.85f),()=>choose(PreparationCampaign.Nodes[index]));button.interactable=reason==null;
        }
        CampaignHUD.MakeButton(panel.transform,"Pass this month",new Vector2(.32f,.035f),new Vector2(.68f,.115f),()=>choose("prepPass"));
    }
}
