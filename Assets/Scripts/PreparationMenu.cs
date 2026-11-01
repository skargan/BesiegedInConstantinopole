using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One preparation choice per flexible month. Locked options remain visible with reasons.
public static class PreparationMenu
{
    public const int VisibleChoices=7;
    public static void Show(Transform parent,Action<string> select)
    {
        var old=parent.Find("Monthly preparation");if(old!=null){old.gameObject.SetActive(false);UnityEngine.Object.Destroy(old.gameObject);}
        var panel=CampaignHUD.Panel(parent,"Monthly preparation",Vector2.zero,Vector2.one);ImperialTheme.Panel(panel);
        CampaignHUD.Label(panel.transform,"SIEGE PREPARATIONS · "+VariableSingleton.GetDate().ToString("MMMM yyyy"),new Vector2(.04f,.91f),new Vector2(.96f,.98f),30);
        CampaignHUD.Explanation(panel.transform,"Choose one main preparation. Completed choices cannot be repeated. Career achievements: "+PreparationCampaign.CareerCount,new Vector2(.04f,.85f),new Vector2(.96f,.91f),20);
        var viewport=CampaignHUD.Panel(panel.transform,"Preparation list",new Vector2(.04f,.13f),new Vector2(.92f,.84f));viewport.AddComponent<RectMask2D>();
        var content=new GameObject("Preparation choices",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);
        var rect=content.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=Vector2.zero;
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=rect;scroll.horizontal=false;scroll.scrollSensitivity=40;scroll.movementType=ScrollRect.MovementType.Clamped;
        CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.93f,.13f),new Vector2(.96f,.84f));
        bool chosen=false;
        Action<string> choose=node=>{if(chosen)return;chosen=true;panel.SetActive(false);UnityEngine.Object.Destroy(panel);select(node);};
        var reasons=Enumerable.Range(0,PreparationCampaign.Nodes.Length).Select(i=>PreparationCampaign.Unavailable(i)).ToArray();
        // Catalogue index remains the identity; ordering changes only the presentation.
        var order=Enumerable.Range(0,reasons.Length).OrderBy(i=>reasons[i]==null?0:1).ThenBy(i=>i).ToArray();
        var rows=new RectTransform[order.Length];
        for(int position=0;position<order.Length;position++){
            int index=order[position];string reason=reasons[index];
            var row=CampaignHUD.Panel(content.transform,"Preparation "+index,Vector2.zero,Vector2.one);rows[position]=row.GetComponent<RectTransform>();ImperialTheme.Frame(row);
            var title=CampaignHUD.Label(row.transform,PreparationCampaign.Titles[index],new Vector2(.02f,.52f),new Vector2(.73f,.96f),20);title.alignment=TextAlignmentOptions.MidlineLeft;
            var description=CampaignHUD.Explanation(row.transform,PreparationCampaign.Summaries[index],new Vector2(.02f,.05f),new Vector2(.73f,.52f),17);description.alignment=TextAlignmentOptions.MidlineLeft;
            if(reason==null){
                var button=CampaignHUD.MakeTurnButton(row.transform,"Choose",new Vector2(.77f,.14f),new Vector2(.98f,.86f),()=>choose(PreparationCampaign.Nodes[index]));ImperialTheme.Button(button);TurnActionButton.Mark(button,"Commits this month's preparation.");
            }else{
                var state=CampaignHUD.Label(row.transform,reason,new Vector2(.76f,.06f),new Vector2(.98f,.94f),17);state.alignment=TextAlignmentOptions.Center;state.color=new Color(.66f,.66f,.60f);
                title.color=new Color(.70f,.69f,.62f);description.color=new Color(.63f,.63f,.59f);
            }
        }
        Canvas.ForceUpdateCanvases();viewport.AddComponent<PreparationListLayout>().Configure(rect,rows);
        CampaignHUD.MakeTurnButton(panel.transform,"Pass this month",new Vector2(.32f,.025f),new Vector2(.68f,.10f),()=>choose("prepPass"));
    }
}

// Row height follows the actual viewport, including changes between windowed and fullscreen.
public sealed class PreparationListLayout : MonoBehaviour
{
    RectTransform content;
    RectTransform[] rows;
    public void Configure(RectTransform target,RectTransform[] choices){content=target;rows=choices;Arrange();}
    void OnRectTransformDimensionsChange(){Arrange();}
    void Arrange()
    {
        if(content==null||rows==null)return;
        float height=((RectTransform)transform).rect.height;
        if(height<=0)return;
        float stride=height/PreparationMenu.VisibleChoices;
        content.sizeDelta=new Vector2(0,Mathf.Max(height,rows.Length*stride));
        for(int i=0;i<rows.Length;i++){
            var row=rows[i];row.anchorMin=new Vector2(0,1);row.anchorMax=Vector2.one;row.pivot=new Vector2(.5f,1);
            row.offsetMin=new Vector2(5,-(i+1)*stride+3);row.offsetMax=new Vector2(-5,-i*stride-3);
        }
    }
}
