using UnityEngine;
public partial class CouncilUI
{
    public void EmperorSheet()=>EmperorProfileUI.Show(Palace);
    public void EmperorLesson(int stat,int teacher=-1)
    {
        if(teacher>=0&&!GardenConversations.Available(teacher))return;
        int loyalty=teacher==200?DailyLifeEvents.I("iKjoldLoyalty",50):teacher>=100?CharacterPreparations.Loyalty:teacher>=0?DailyLifeEvents.I("iLoyalty"+teacher,50):100;
        if(loyalty<40){CampaignHUD.Instance.ShowReport("An instruction declined","‘Forgive me, Majesty. My duties will not permit it today.’",()=>CampaignHUD.Instance.DismissReport());return;}
        bool ok;string report;
        if(stat==2)ok=EmperorCombat.Train(out report);
        else{ok=EmperorProfile.Value(stat)<10;report=ok?"You devote the day to "+(stat==0?"practising an address, hearing objections and revising your words":"comparing reports and working through their contradictions")+". "+EmperorProfile.Names[stat]+" +25 XP. The major action is spent.":"This attribute has reached ten.";if(ok)EmperorProfile.GainXP(stat,25);}
        Finish(ok,report);
    }
    public void EmperorAffairs()
    {
        Open("Imperial affairs","Personal development takes time. Training spends the major action; conversation and reflection do not.");ControlSingleton.Instance.ReturnAction=Palace;BodyHeight(920);
        CampaignHUD.Label(body,"Health "+VariableSingleton.GetPercentFloat("fHealth")+"   ·   Charisma "+EmperorProfile.Value(0)+"   ·   Intellect "+EmperorProfile.Value(1)+"   ·   Martial skill "+EmperorProfile.Value(2),new Vector2(.03f,.90f),new Vector2(.97f,.99f),26);
        string[] titles={"Look in the mirror · attributes and experience","Practise an address · Charisma +25 XP · major action","Study the council papers · Intellect +25 XP · major action","Practise swordsmanship · Martial +25 XP · health −5 · major action","Share wine with the court · Charisma +15 XP · hangover risk · major action","Review your court journal","Lead next wall assault / cancel","Rest · major action"};
        for(int i=0;i<titles.Length;i++){int k=i;float top=.88f-i*.105f;CampaignHUD.MakeButton(body,titles[i],new Vector2(.06f,top-.087f),new Vector2(.94f,top),()=>{
            if(k==0)EmperorSheet();else if(k<=3)EmperorLesson(k-1);else if(k==4){VariableSingleton.SetInt("iLifeDrankTonight",1);EmperorProfile.GainXP(0,15);Finish(true,"You spend the evening with the court. Charisma +15 XP. A bad hangover may follow; drinking has not improved your health.");}
            else if(k==5){Open("The court journal","Choices recorded in conversation.");BodyHeight(1000);CampaignHUD.Explanation(body,CourtConversationData.Journal(),new Vector2(.04f,.03f),new Vector2(.96f,.97f),25);ControlSingleton.Instance.ReturnAction=EmperorAffairs;}
            else if(k==6)notice.text=EmperorCombat.ToggleWall();else{Hide();manager.ContinueToRest();}
        });}
    }
}
