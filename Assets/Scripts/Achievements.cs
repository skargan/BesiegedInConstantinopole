using UnityEngine;
using UnityEngine.UI;
public static class Achievements
{
    public static readonly string[] Names={"Still We Stand","The Last Signature","The Sealed Bargain","A Season Against the World","The Unbroken Wall","The Emperor's Eyes","A Fleet on the Horizon","Master of the Workshops","Four Voices, One Crown","Bread for the City","Restored Praetorian Guard","Great Cannon","Fellowship of Constantinople","Ten Thousand Fallen","Thirty Thousand Fallen","Fifty Thousand Fallen","Seventy-five Thousand Fallen","Without a Single Repair"};
    public static readonly string[] Descriptions={"Survive the siege or destroy the Ottoman field army.","Be assassinated by your own people while accepting surrender terms.","Conclude a surrender without being killed by your own people.","Hold Constantinople for 120 siege days.","Finish a campaign without abandoning the outer wall.","Build a network of eight agents.","Receive a foreign relief expedition.","Adopt every available technology.","Keep all four political blocs influential and the loyalists strongest.","Recover from famine and rebuild a month's food reserve.","Restore the Praetorian Guard. Restoration event forthcoming.","Bring the Great Cannon into the City's possession.","Recruit seven distinct friendly characters in one campaign.","Cause 10,000 Ottoman troop deaths in one campaign.","Cause 30,000 Ottoman troop deaths in one campaign.","Cause 50,000 Ottoman troop deaths in one campaign.","Cause 75,000 Ottoman troop deaths in one campaign.","Win the siege without repairing any wall during the campaign. Requires a new campaign with repair tracking."};
    public static void Unlock(int index) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!string.IsNullOrEmpty(VariableSingleton.TestSavePath))return;
#endif
        if(index<0 || index>=Names.Length || PlayerPrefs.GetInt("BesiegedAchievement"+index,0)==1)return;
        PlayerPrefs.SetInt("BesiegedAchievement"+index,1);PlayerPrefs.Save();
    }
    static int I(string key)=>DailyLifeEvents.I(key);
    public static void BeginCampaign() { VariableSingleton.SetInt("iAchievementRepairTracked",1); }
    public static void TroopLoss(string key,int loss) {
        if(loss<=0 || (key!="iBalkans" && key!="iBazouks" && key!="iJanissaries"))return;
        int total=I("iAchievementOttomanDead")+loss;
        VariableSingleton.SetInt("iAchievementOttomanDead",total);
        int[] thresholds={10000,30000,50000,75000};
        for(int i=0;i<thresholds.Length;i++)if(total>=thresholds[i])Unlock(13+i);
    }
    public static void WallChanged(string key,float before,float after) {
        if(after>before && (key=="fOuterWall" || key=="fMiddleWall" || key=="fInnerWall" || key=="fGoldenHornWall" || key=="fMarmaraWall"))
            VariableSingleton.SetInt("iAchievementWallRepaired",1);
    }
    public static void CheckCampaign() {
        if(!VariableSingleton.IsInitialized)return;
        if(I("iGrandCannon")==2)Unlock(11);
        for(int i=0;i<8;i++)if(CourtRoster.InService(i))VariableSingleton.SetInt("iAchievementPerson"+i,1);
        if(CharacterPreparations.InService)VariableSingleton.SetInt("iAchievementPerson"+CharacterPreparations.CommanderId,1);
        if(DailyLifeEvents.I("iKjoldState",1)==1)VariableSingleton.SetInt("iAchievementPerson200",1);
        if(SultanGifts.Known && (I("iShahStatus")==4 || I("iShahStatus")==7))VariableSingleton.SetInt("iAchievementPerson201",1);
        int count=0;foreach(int id in CharacterPortraits.People)if(id!=8 && id!=202 && I("iAchievementPerson"+id)>0)count++;
        if(count>=7)Unlock(12);
    }
    public static void Victory() {
        CheckCampaign();
        if(I("iAchievementRepairTracked")==1 && I("iAchievementWallRepaired")==0)Unlock(17);
    }
    public static void Show(Transform parent)
    {
        CheckCampaign();
        var panel=CampaignHUD.Panel(parent,"Achievements",new Vector2(.02f,.02f),new Vector2(.98f,.98f));
        ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        CampaignHUD.Label(panel.transform,"Achievements",new Vector2(.04f,.91f),new Vector2(.96f,.99f),33);
        var viewport=CampaignHUD.Panel(panel.transform,"Achievement list",new Vector2(.04f,.14f),new Vector2(.94f,.90f));viewport.AddComponent<RectMask2D>();
        var content=new GameObject("Achievement entries",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);
        var r=content.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,Names.Length*100);
        for(int i=0;i<Names.Length;i++) {
            float top=1f-(float)i/Names.Length, bottom=top-1f/Names.Length;
            bool earned=PlayerPrefs.GetInt("BesiegedAchievement"+i,0)==1;
            var colour=earned?new Color(.91f,.79f,.49f):new Color(.52f,.54f,.56f);
            var box=CampaignHUD.Panel(content.transform,"Checkbox "+i,new Vector2(.012f,bottom+.018f),new Vector2(.043f,top-.018f));
            box.GetComponent<Image>().color=colour;
            var inner=CampaignHUD.Panel(box.transform,"Inset",new Vector2(.08f,.06f),new Vector2(.92f,.94f));inner.GetComponent<Image>().color=new Color(.04f,.07f,.09f);
            if(earned) {var tick=CampaignHUD.Label(box.transform,"✓",Vector2.zero,Vector2.one,26);tick.alignment=TMPro.TextAlignmentOptions.Center;tick.color=colour;}
            var title=CampaignHUD.Label(content.transform,Names[i]+(earned?" · Earned":i>=3&&i<=10?" · Planned":" · Locked"),new Vector2(.06f,bottom+.030f),new Vector2(.98f,top-.003f),23);title.color=colour;
            var desc=CampaignHUD.Explanation(content.transform,Descriptions[i],new Vector2(.06f,bottom+.003f),new Vector2(.98f,bottom+.030f),18);desc.color=colour;
        }
        var scroll=viewport.AddComponent<ScrollRect>();scroll.content=r;scroll.viewport=viewport.GetComponent<RectTransform>();scroll.horizontal=false;scroll.scrollSensitivity=45;scroll.movementType=ScrollRect.MovementType.Clamped;
        CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.95f,.14f),new Vector2(.965f,.90f));
        CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.65f,.025f),new Vector2(.96f,.095f),()=>Object.Destroy(panel));
    }
}
