using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    // Five civic and logistical ledgers; detailed defence and gated enemy reports follow below.
    float CityStatistics()
    {
        var panel=SpyPanel("City at a Glance",0,650);
        CampaignHUD.Label(panel.transform,"CITY AT A GLANCE",new Vector2(.018f,.94f),new Vector2(.98f,.995f),28);
        var city=new List<string[]>();for(int i=0;i<6;i++)city.Add(StatRow(i));
        city.Add(Pair("City rations / day",N(SupplyEconomy.DailyNeed)));city.Add(Pair("City ration days",SupplyEconomy.DaysLeft+" days"));
        var troops=new List<string[]>();for(int i=6;i<10;i++)troops.Add(StatRow(i));
        troops.Add(Pair("Cannons",I("iCannons")));troops.Add(Pair("Ammunition",I("iAmmunition")));troops.Add(Pair("Warships",I("iShips")));troops.Add(Pair("Damaged ships",I("iDamagedShips")));
        var support=new List<string[]>();for(int i=10;i<14;i++)support.Add(StatRow(i));
        support.Add(Pair("Galata relations",VariableSingleton.GetPercentFloat("fGalataRep")));
        support.Add(Pair("Daily merchant levy",I("iIncome")+" gold"));
        int religion=CouncilActions.Int("iReligion");support.Add(Pair("Church policy",religion==1?"Full Union":religion==2?"Partial Union":"Orthodox"));
        StatsLedger(panel.transform,"CITY & STORES",city,0,true,CampaignStatusStrip.GroupColours[0]);
        StatsLedger(panel.transform,"GARRISON & EQUIPMENT",troops,1,true,CampaignStatusStrip.GroupColours[1]);
        StatsLedger(panel.transform,"MORALE & SUPPORT",support,2,true,CampaignStatusStrip.GroupColours[2]);
        int court=Enumerable.Range(0,8).Count(CourtRoster.InService)+(CharacterPreparations.InService?1:0);
        if(SultanGifts.Known&&(DailyLifeEvents.I("iShahStatus")==4||DailyLifeEvents.I("iShahStatus")==7))court++;
        var people=new List<string[]> {
            Pair("Engineers available",I("iEngineers")),
            Pair("Monks",I("iHealers")),Pair("Agents in camp",I("iSpies")),Pair("Agents abroad",N(ProvinceCampaign.Assigned)),
            Pair("Characters in service",N(court)),
            Pair("Technologies adopted",N(TechnologySystem.All.Count(t=>VariableSingleton.GetBoolVariable(t.Key))))
        };
        var logistics=new List<string[]> {
            Pair("Manpower pool",N(RecruitmentLogistics.ManpowerPool)),Pair("Available manpower",N(RecruitmentLogistics.AvailableManpower)),
            Pair("Regulars in service",N(RecruitmentLogistics.Serving)),Pair("Regular recruits due",N(RecruitmentLogistics.Queued)),
            Pair("Ships being built",N(Queued("iShips"))),Pair("Cannons being built",N(Queued("iCannons"))),
            Pair("Fleet report",CouncilActions.Int("iShips")>0&&CouncilActions.Int("iFleet")>=0?FleetOperations.DaysRemaining+" active days":"No active mission"),
            Pair("Sea control",NavalState.SeaControl)
        };
        StatsLedger(panel.transform,"SPECIALISTS & COURT",people,0,false,CampaignStatusStrip.GroupColours[0]);
        StatsLedger(panel.transform,"RECRUITMENT & LOGISTICS",logistics,1,false,CampaignStatusStrip.GroupColours[1]);


        var medical=SpyPanel("Medical service summary",664,88);
        var care=CampaignHUD.Label(medical.transform,"<b>MEDICAL SERVICE</b>   Wounded: "+N(MedicalRecovery.Total)+"   ·   Capacity: "+N(MedicalRecovery.Capacity)+"   ·   Free places: "+N(MedicalRecovery.FreePlaces)+"\nTreatment provisions: "+N(MedicalRecovery.DailyProvisionNeed)+" / day"+(MedicalRecovery.Total>0?"   ·   Earliest recovery: "+MedicalRecovery.SoonestReturnDays+" treatment days":""),new Vector2(.022f,.08f),new Vector2(.77f,.94f),22);
        care.enableAutoSizing=true;care.fontSizeMin=18;care.fontSizeMax=22;
        CampaignHUD.MakeButton(medical.transform,"Infirmary",new Vector2(.80f,.18f),new Vector2(.98f,.82f),Infirmary);
        var summary=SpyPanel("City briefing",766,96);
        CampaignHUD.Label(summary.transform,CityAtGlanceText(),new Vector2(.022f,.1f),new Vector2(.98f,.92f),22);
        var links=SpyPanel("City statistics navigation",876,68);
        string[] names={"Recruitment logistics","Specialists","Society and politics"};
        UnityEngine.Events.UnityAction[] actions={Training,Specialists,Factions};
        for(int i=0;i<3;i++)CampaignHUD.MakeButton(links.transform,names[i],new Vector2(.01f+i*.332f,.12f),new Vector2(.322f+i*.332f,.88f),actions[i]);
        return 958;
    }
    static string[] Pair(string label,string value)=>new[]{label,value};
    static string N(long value)=>CampaignStatistics.Number(value);
    static string I(string key)=>N(CouncilActions.Int(key));
    static string[] StatRow(int index)=>Pair(CampaignStatistics.Header[index].Name,CampaignStatistics.Header[index].Value);
    static long Queued(string unit)=>TrainingPool.Orders.Where(o=>o!=null&&o.unit==unit).Sum(o=>(long)o.quantity);
    void StatsLedger(Transform parent,string title,List<string[]> rows,int column,bool top,Color accent)
    {
        float width=top?.319f:.4835f;
        float left=.012f+column*(top?.329f:.4935f);
        var card=CampaignHUD.Panel(parent,title,new Vector2(left,top?.505f:.02f),new Vector2(left+width,top?.935f:.49f));
        card.GetComponent<Image>().color=ImperialTheme.Navy;
        var head=CampaignHUD.Label(card.transform,title,new Vector2(.035f,.865f),new Vector2(.97f,.98f),20);
        head.color=accent;head.enableAutoSizing=true;head.fontSizeMin=17;head.fontSizeMax=20;
        for(int i=0;i<rows.Count;i++) {
            float y=.855f-i*.102f;
            var row=CampaignHUD.Panel(card.transform,title+" row "+i,new Vector2(.02f,y-.10f),new Vector2(.98f,y));
            row.GetComponent<Image>().color=i%2==0?new Color(.07f,.12f,.16f,1):Color.clear;
            var name=CampaignHUD.Label(row.transform,rows[i][0],new Vector2(.02f,.02f),new Vector2(.69f,.98f),22);
            var value=CampaignHUD.Label(row.transform,rows[i][1],new Vector2(.70f,.02f),new Vector2(.985f,.98f),23);
            value.alignment=TextAlignmentOptions.MidlineRight;name.alignment=TextAlignmentOptions.MidlineLeft;
            foreach(var text in new[]{name,value}) {text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=text.fontSize;text.enableWordWrapping=false;}
        }
    }
}
