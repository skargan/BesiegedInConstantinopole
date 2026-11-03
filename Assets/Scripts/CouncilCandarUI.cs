using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public partial class CouncilUI
{
    public void CandarCourt(){CandarPage(null);}
    void CandarPage(string report)
    {
        Open("Candar · İsmail Bey","");
        ControlSingleton.Instance.ReturnAction=DiplomaticRelations;
        foreach(Transform child in screen.transform) child.gameObject.SetActive(false);
        SceneBackdrop.Apply(screen,CandarDiplomacy.Unlocked?"candar-open":"candar-closed");
        if(Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+(CandarDiplomacy.Unlocked?"candar-open":"candar-closed"))==null)SceneBackdrop.Apply(screen,"diplomacy");
        if(CandarDiplomacy.Unlocked){var words=DiplomacyText(screen.transform,"",new Vector2(.04f,.025f),new Vector2(.52f,.22f),23);var hit=CampaignHUD.MakeButton(screen.transform,"",new Vector2(.04f,.24f),new Vector2(.34f,.86f),()=>{});hit.gameObject.name="İsmail Bey remarks";hit.GetComponent<Image>().color=Color.clear;hit.gameObject.AddComponent<CandarRulerRemarks>().Bind(words);}
        var p=CampaignHUD.Panel(screen.transform,"Candar negotiations",new Vector2(.54f,.025f),new Vector2(.985f,.975f));
        ImperialTheme.Panel(p,false);
        DiplomacyText(p.transform,"CANDAR · İSMAİL BEY",new Vector2(.04f,.91f),new Vector2(.96f,.98f),28);
        DiplomacyText(p.transform,(CandarDiplomacy.Unlocked?"Secret negotiations · no friendship promised":"Hostile · a guarded door")+"\nGold "+DailyLifeEvents.I("iMoney").ToString("N0"),new Vector2(.04f,.81f),new Vector2(.96f,.91f),21);
        CampaignHUD.MakeButton(p.transform,"Realm and its interests",new Vector2(.04f,.72f),new Vector2(.96f,.80f),CandarRealm);
        if(!CandarDiplomacy.Unlocked){
            string requirements="Strait open: "+(CandarDiplomacy.StraitOpen?"yes":"required")+"\nOrhan Çelebi in service: "+(CandarDiplomacy.OrhanInService?"yes":"required");
            var b=CampaignHUD.MakeButton(p.transform,"Open secret negotiations\n"+requirements,new Vector2(.04f,.46f),new Vector2(.96f,.69f),CandarOpening);
            b.interactable=CandarDiplomacy.UnlockReason==null;
        }else{
            DiplomacyText(p.transform,"✉  Secrets remaining: "+CandarDiplomacy.SecretsRemaining,new Vector2(.04f,.65f),new Vector2(.96f,.72f),23);
            CandarButton(p.transform,"Ransom Christian captives · 3,000",CandarDiplomacy.RansomReason,.53f,CandarDiplomacy.Ransom);
            CandarButton(p.transform,"Purchase a secret · 2,000",CandarDiplomacy.SecretReason,.40f,CandarDiplomacy.PurchaseSecret);
            CandarButton(p.transform,"Disrupt Ottoman preparations · 2,500",CandarDiplomacy.DisruptionReason,.27f,CandarDiplomacy.Disrupt);
        }
        var result=DiplomacyText(p.transform,report??"No banners. No promises of friendship. Only business.",new Vector2(.04f,.085f),new Vector2(.96f,.265f),19);
        result.fontSizeMin=13;result.overflowMode=TextOverflowModes.Overflow;
        CampaignHUD.MakeButton(p.transform,"Return",new Vector2(.20f,.015f),new Vector2(.80f,.075f),DiplomaticRelations);
    }
    delegate bool CandarTransaction(out string report);
    void CandarButton(Transform p,string label,string reason,float y,CandarTransaction transaction)
    {
        var b=CampaignHUD.MakeButton(p,label+(reason==null?"":"\n"+reason),new Vector2(.04f,y),new Vector2(.96f,y+.115f),()=>{
            bool ok=transaction(out var report);if(ok)VariableSingleton.Instance.Save();CandarReport(report);
        });
        b.interactable=reason==null;
        var t=b.GetComponentInChildren<TMP_Text>();t.enableAutoSizing=true;t.fontSizeMin=15;t.fontSizeMax=21;
    }
    void CandarReport(string text)
    {
        CandarPage(null);
        var panel=CampaignHUD.Panel(screen.transform,"Candar arrival report",new Vector2(.06f,.12f),new Vector2(.51f,.88f));ImperialTheme.Panel(panel,false);
        DiplomacyText(panel.transform,text,new Vector2(.04f,.12f),new Vector2(.96f,.95f),22);
        CampaignHUD.MakeButton(panel.transform,"Continue",new Vector2(.20f,.02f),new Vector2(.80f,.10f),CandarCourt);
    }
    void CandarOpening()
    {
        CandarPage(null);
        var p=CampaignHUD.Panel(screen.transform,"A Door Left Unbarred",new Vector2(.06f,.14f),new Vector2(.51f,.88f));ImperialTheme.Panel(p,false);
        DiplomacyText(p.transform,"A Door Left Unbarred\n\nİsmail Bey expects Mehmed to take Constantinople. Yet an emperor who survives might prove useful—and a sultan without enemies might prove dangerous. Through Orhan’s intermediaries, a discreet understanding becomes possible. No banners, no promises of friendship. Only business.",new Vector2(.05f,.22f),new Vector2(.95f,.95f),25);
        bool submitted=false;
        CampaignHUD.MakeButton(p.transform,"Then let us speak discreetly.",new Vector2(.05f,.05f),new Vector2(.95f,.18f),()=>{
            if(submitted)return;submitted=true;bool ok=CandarDiplomacy.OpenNegotiations(out var report);if(ok)VariableSingleton.Instance.Save();CandarReport(report);
        });
        ControlSingleton.Instance.ReturnAction=CandarCourt;
    }
    void CandarRealm()
    {
        CandarReport("Candar and its interests\n\nİsmail Bey rules the principality around Sinop and Kastamonu. Candar retains its own ruler and territory in an unequal relationship with the Ottomans; its historical annexation comes in 1461.\n\nA surviving Constantinople might check Mehmed's power. Open friendship would expose Candar to retaliation. Discreet transactions offer profit and insurance against absorption.\n\nThe proposed 1453 dealings and dialogue are historical fiction.\n\nRansom frees captives; volunteers and specialists are included in the total rescued population. Convoys replenish after seven calendar days. Purchased secrets are distinct, three per campaign. Disruption postpones one eligible tactical operation by a day, once every seven days.");
    }
}
