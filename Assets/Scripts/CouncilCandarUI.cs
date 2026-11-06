using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    Transform candarDesk;
    public void CandarCourt()=>CandarPage();
    void CandarPage()
    {
        Open("Candar · İsmail Bey","");
        ControlSingleton.Instance.ReturnAction=DiplomaticRelations;
        foreach(Transform child in screen.transform)child.gameObject.SetActive(false);
        string scene=CandarDiplomacy.Unlocked?"candar-open":"candar-closed";
        var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+scene);bool hasCandarPainting=texture!=null;
        if(texture==null)texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/diplomacy");
        var art=new GameObject("Candar painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));art.transform.SetParent(screen.transform,false);
        var rect=art.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        var image=art.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
        var fit=art.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9f:(float)texture.width/texture.height;
        if(CandarDiplomacy.Unlocked)CandarCharacter(art.transform,hasCandarPainting);
        var panel=CampaignHUD.Panel(art.transform,"Candar negotiations",new Vector2(.57f,.025f),new Vector2(.985f,.975f));
        ImperialTheme.Panel(panel,false);candarDesk=panel.transform;
        DiplomaticNames.Bind(CandarLabel(candarDesk,"CANDAR · İSMAİL BEY",new Vector2(.04f,.91f),new Vector2(.96f,.98f),28),"candar"," · İsmail Bey");
        CandarLabel(candarDesk,(CandarDiplomacy.Unlocked?"Secret negotiations · no friendship promised":"Hostile · a guarded door")+"\nGold "+DailyLifeEvents.I("iMoney").ToString("N0"),new Vector2(.04f,.81f),new Vector2(.96f,.91f),21);
        CandarPlainButton(candarDesk,"Realm and its interests",new Vector2(.04f,.72f),new Vector2(.96f,.80f),CandarRealm);
        if(!CandarDiplomacy.Unlocked) {
            string requirements="Strait open: "+(CandarDiplomacy.StraitOpen?"yes":"required")+"\nOrhan Çelebi in service: "+(CandarDiplomacy.OrhanInService?"yes":"required");
            var button=CandarPlainButton(candarDesk,"Open secret negotiations\n"+requirements,new Vector2(.04f,.46f),new Vector2(.96f,.69f),CandarOpening);
            button.interactable=CandarDiplomacy.UnlockReason==null;
        }else {
            var icon=new GameObject("Sealed intelligence",typeof(RectTransform));icon.transform.SetParent(candarDesk,false);
            var iconRect=icon.GetComponent<RectTransform>();iconRect.anchorMin=new Vector2(.04f,.65f);iconRect.anchorMax=new Vector2(.15f,.71f);iconRect.offsetMin=iconRect.offsetMax=Vector2.zero;
            var emblem=new GameObject("Parchment and quill",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));emblem.transform.SetParent(icon.transform,false);
            var iconImage=emblem.GetComponent<RawImage>();iconImage.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Council/spies");iconImage.raycastTarget=false;
            var iconFit=emblem.GetComponent<AspectRatioFitter>();iconFit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;iconFit.aspectRatio=1;
            CandarLabel(candarDesk,"Secrets remaining: "+CandarDiplomacy.SecretsRemaining,new Vector2(.17f,.65f),new Vector2(.96f,.72f),23);
            CandarOrderButton(0,"Ransom Christian captives · 3,000",CandarDiplomacy.RansomReason,.53f);
            CandarOrderButton(1,"Purchase a secret · 2,000",CandarDiplomacy.SecretReason,.40f);
            CandarOrderButton(2,"Disrupt Ottoman preparations · 2,500",CandarDiplomacy.DisruptionReason,.27f);
        }
        CandarLabel(candarDesk,"No banners. No promises of friendship. Only business.",new Vector2(.04f,.105f),new Vector2(.96f,.245f),19);
        CandarPlainButton(candarDesk,"Return",new Vector2(.20f,.025f),new Vector2(.80f,.09f),DiplomaticRelations);
    }
    void CandarCharacter(Transform painting,bool hasCandarPainting)
    {
        var speechPanel=CampaignHUD.Panel(painting,"İsmail Bey's words",new Vector2(.025f,.025f),new Vector2(.54f,.22f));
        speechPanel.GetComponent<Image>().color=new Color(.012f,.028f,.04f,.90f);ImperialTheme.Panel(speechPanel,false);
        var words=CandarLabel(speechPanel.transform,"",new Vector2(.03f,.28f),new Vector2(.97f,.97f),22);
        var data=Resources.Load<TextAsset>("Content/CandarHoverShape");
        var shape=data==null?null:JsonUtility.FromJson<CandarRulerRemarks.HoverShape>(data.text);
        // Do not pretend the generic fallback contains Ismail Bey. His silhouette is
        // interactive only when both the original painting and verified shape exist.
        CandarRulerRemarks remarks=null;LocationHotspot hotspot=null;
        if(hasCandarPainting&&shape!=null&&shape.points!=null&&shape.points.Length>=3) {
            hotspot=LocationHotspot.Create(painting,"İsmail Bey",shape.points,()=>remarks.Speak());
            remarks=hotspot.gameObject.AddComponent<CandarRulerRemarks>();
        }else remarks=speechPanel.AddComponent<CandarRulerRemarks>();
        remarks.Bind(words,hotspot);
        speechPanel.transform.SetAsLastSibling();
        var speak=CandarPlainButton(speechPanel.transform,"Speak with İsmail Bey",new Vector2(.20f,.035f),new Vector2(.80f,.255f),remarks.Speak);
        speak.gameObject.AddComponent<CandarRemarkFocus>().source=remarks;
    }
    void CandarOrderButton(int order,string title,string reason,float y)
    {
        var button=CandarPlainButton(candarDesk,title+(reason==null?" · review":"\n"+reason),new Vector2(.04f,y),new Vector2(.96f,y+.115f),()=>CandarReview(order));
        // Reviews remain readable when unavailable; confirmation rechecks eligibility.
        if(reason!=null)button.GetComponentInChildren<TMP_Text>().color=new Color(.73f,.68f,.57f);
    }
    string CandarReason(int order)=>order==0?CandarDiplomacy.RansomReason:order==1?CandarDiplomacy.SecretReason:CandarDiplomacy.DisruptionReason;
    void CandarReview(int order)
    {
        string title=order==0?"Ransom Christian captives":order==1?"Purchase a secret":"Disrupt Ottoman preparations";
        string terms=order==0?"Pay 3,000 gold. A rescued group arrives immediately; every row is rolled independently.\n\nAlways: 20–60 infantry, 15–30 archers.\n50% each: 1–5 cavalry, 1–5 engineers, 1–5 monks.\n5%: one envoy.\n10%: 1–3 Varangian guards.\n\nTotal rescued population: 150–300 people, including every volunteer listed above. Volunteers are not counted twice.\n\nIf paid today, next captive convoy: "+VariableSingleton.GetDate().AddDays(CandarDiplomacy.ReplenishmentDays).ToString("d MMM yyyy")+" (seven calendar days after payment).":order==1?"Pay 2,000 gold to receive one distinct secret immediately.\n\nThere are three named secrets in this campaign. A purchased secret is never sold to you again.\n\nSecrets remaining: "+CandarDiplomacy.SecretsRemaining+".":"Pay 2,500 gold. The next eligible Ottoman tactical operation is postponed by one calendar day, then resumes.\n\nFood consumption and naval pressure continue. Emergency resupply, an advance through a fallen wall, awaiting orders and reinforcement notices are excluded. No completed attack is undone.\n\nIf paid today, next payment available: "+VariableSingleton.GetDate().AddDays(CandarDiplomacy.ReplenishmentDays).ToString("d MMM yyyy")+" (seven calendar days after payment). A previous disruption must also have been used.";
        CandarPage();ClearCandarDesk();
        CandarLabel(candarDesk,title,new Vector2(.04f,.875f),new Vector2(.96f,.975f),27);
        CandarScroll(candarDesk,terms+"\n\nThis transaction does not advance the day.",new Vector2(.04f,.29f),new Vector2(.96f,.855f),21);
        string reason=CandarReason(order);
        var reasonLabel=CandarLabel(candarDesk,reason??"Review the terms before paying.",new Vector2(.04f,.17f),new Vector2(.96f,.275f),18);
        bool submitted=false;
        var confirm=CandarPlainButton(candarDesk,"Confirm payment",new Vector2(.04f,.09f),new Vector2(.96f,.16f),()=>{
            if(submitted)return;string report;bool ok=order==0?CandarDiplomacy.Ransom(out report):order==1?CandarDiplomacy.PurchaseSecret(out report):CandarDiplomacy.Disrupt(out report);
            if(ok){submitted=true;VariableSingleton.Instance.Save();CandarReport(report);}else reasonLabel.text=report;
        });confirm.interactable=reason==null;
        CandarPlainButton(candarDesk,"Cancel",new Vector2(.20f,.02f),new Vector2(.80f,.08f),CandarCourt);
        ControlSingleton.Instance.ReturnAction=CandarCourt;
    }
    void ClearCandarDesk(){foreach(Transform child in candarDesk)if(child.name!="Imperial goldwork")child.gameObject.SetActive(false);}
    void CandarReport(string text)
    {
        CandarPage();ClearCandarDesk();
        CandarLabel(candarDesk,"An understanding with Candar",new Vector2(.04f,.88f),new Vector2(.96f,.975f),27);
        CandarScroll(candarDesk,text,new Vector2(.04f,.12f),new Vector2(.96f,.855f),22);
        CandarPlainButton(candarDesk,"Return to Candar",new Vector2(.20f,.025f),new Vector2(.80f,.095f),CandarCourt);
        ControlSingleton.Instance.ReturnAction=CandarCourt;
    }
    void CandarOpening()
    {
        CandarPage();ClearCandarDesk();
        CandarLabel(candarDesk,"A Door Left Unbarred",new Vector2(.04f,.87f),new Vector2(.96f,.975f),27);
        CandarScroll(candarDesk,"İsmail Bey expects Mehmed to take Constantinople. Yet an emperor who survives might prove useful—and a sultan without enemies might prove dangerous. Through Orhan’s intermediaries, a discreet understanding becomes possible. No banners, no promises of friendship. Only business.\n\nYour intermediaries await the imperial reply.",new Vector2(.04f,.25f),new Vector2(.96f,.85f),23);
        bool submitted=false;
        CandarPlainButton(candarDesk,"Then let us speak discreetly.",new Vector2(.04f,.13f),new Vector2(.96f,.22f),()=>{
            if(submitted)return;bool ok=CandarDiplomacy.OpenNegotiations(out var report);if(ok){submitted=true;VariableSingleton.Instance.Save();}CandarReport(report);
        });
        CandarPlainButton(candarDesk,"Return",new Vector2(.20f,.025f),new Vector2(.80f,.095f),CandarCourt);
        ControlSingleton.Instance.ReturnAction=CandarCourt;
    }
    void CandarRealm()=>CandarReport("Candar and its interests\n\nİsmail Bey rules Sinop and Kastamonu under the shadow of Ottoman power. Candar retains its own ruler and territory, but the Sultan's favour is an uncertain protection.\n\nA surviving Constantinople might check Mehmed's power. Open friendship would expose Candar to retaliation. Discreet transactions offer profit and insurance against absorption.\n\nRansom frees captives; volunteers and specialists are included in the total rescued population. Convoys replenish after seven calendar days. Purchased secrets are distinct, three per campaign. Disruption postpones one eligible tactical operation by a day, once every seven days.");
    static TMP_Text CandarLabel(Transform parent,string text,Vector2 min,Vector2 max,float size)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);label.enableAutoSizing=true;label.fontSizeMin=Mathf.Max(16,size-4);label.fontSizeMax=size;label.alignment=TextAlignmentOptions.MidlineLeft;label.overflowMode=TextOverflowModes.Ellipsis;return label;
    }
    static Button CandarPlainButton(Transform parent,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
    {
        var b=CampaignHUD.MakeButton(parent,text,min,max,action);ImperialTheme.Button(b);var t=b.GetComponentInChildren<TMP_Text>();t.fontSizeMin=16;t.fontSizeMax=22;return b;
    }
    static void CandarScroll(Transform parent,string text,Vector2 min,Vector2 max,float size)
    {
        var view=CampaignHUD.Panel(parent,"Candar reading panel",min,max);view.GetComponent<Image>().color=new Color(.012f,.028f,.04f,.75f);view.AddComponent<RectMask2D>();
        var scroll=view.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;scroll.viewport=view.GetComponent<RectTransform>();
        var content=CampaignHUD.Label(view.transform,text,new Vector2(.025f,1),new Vector2(.965f,1),size);content.rectTransform.pivot=new Vector2(.5f,1);content.rectTransform.anchoredPosition=Vector2.zero;content.alignment=TextAlignmentOptions.TopLeft;content.margin=new Vector4(0,6,0,12);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=content.rectTransform;
        var rail=CampaignHUD.Panel(parent,"Candar scroll rail",new Vector2(max.x-.012f,min.y),max);rail.GetComponent<Image>().color=new Color(.2f,.18f,.13f,.7f);
        var bar=rail.AddComponent<Scrollbar>();bar.direction=Scrollbar.Direction.BottomToTop;
        var handle=CampaignHUD.Panel(rail.transform,"Gold scroll handle",Vector2.zero,Vector2.one);handle.GetComponent<Image>().color=new Color(.72f,.55f,.25f,1);bar.handleRect=handle.GetComponent<RectTransform>();bar.targetGraphic=handle.GetComponent<Image>();
        scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
    }
}
