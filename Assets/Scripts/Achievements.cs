using UnityEngine;
using UnityEngine.UI;
public static class Achievements
{
    public static readonly string[] Names={"Still We Stand","The Last Signature","The Sealed Bargain","A Season Against the World","The Unbroken Wall","The Emperor's Eyes","A Fleet on the Horizon","Master of the Workshops","Four Voices, One Crown","Bread for the City","Restored Praetorian Guard","Great Cannon","Fellowship of Constantinople","Ten Thousand Fallen","Thirty Thousand Fallen","Fifty Thousand Fallen","Seventy-five Thousand Fallen","Without a Single Repair","Alchemical Genius"};
    public static readonly string[] Descriptions={"Survive the siege or destroy the Ottoman field army.","Be assassinated by your own people while accepting surrender terms.","Conclude a surrender without being killed by your own people.","Hold Constantinople for 120 siege days.","Finish a campaign without abandoning the outer wall.","Build a network of eight agents.","Receive a foreign relief expedition.","Adopt every available technology.","Keep all four political blocs influential and the loyalists strongest.","Recover from famine and rebuild a month's food reserve.","Restore the Praetorian Guard. Restoration event forthcoming.","Bring the Great Cannon into the City's possession.","Recruit seven distinct friendly characters in one campaign.","Cause 10,000 Ottoman troop deaths in one campaign.","Cause 30,000 Ottoman troop deaths in one campaign.","Cause 50,000 Ottoman troop deaths in one campaign.","Cause 75,000 Ottoman troop deaths in one campaign.","Win the siege without repairing any wall during the campaign. Requires a new campaign with repair tracking.","Complete the workshop experiment. Available in Normal and Ironman campaigns."};
    // Stable indices are never reused: the retired faction entry and the unfinished
    // Praetorian restoration stay reserved rather than unlocking through invented rules.
    public static bool Implemented(int index)=>index>=0 && index<Names.Length && index!=8 && index!=10;
    public static string UnavailableReason(int index)=>index==8?"Political factions have been retired.":index==10?"Awaiting the Praetorian restoration event.":"";
    public static void Unlock(int index) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!string.IsNullOrEmpty(VariableSingleton.TestSavePath))return;
#endif
        if(!Implemented(index) || !(index==18?SaveGames.DiscoveryAchievementEligible:SaveGames.AchievementEligible))return;
        AchievementPlatform.Unlock(index);
    }
    static int I(string key)=>DailyLifeEvents.I(key);
    public static void BeginCampaign() { VariableSingleton.SetInt("iAchievementRepairTracked",1); }
    public static void TroopLoss(string key,int loss) {
        if(loss<=0 || (key!="iBalkans" && key!="iBazouks" && key!="iJanissaries"))return;
        int total=(int)System.Math.Min(int.MaxValue,(long)I("iAchievementOttomanDead")+loss);
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
        if(VariableSingleton.CompletedSiegeDays>=120 && !VariableSingleton.Instance.EnduringWall.CityLost && VariableSingleton.GetFloatVariable("fHealth")>0)Unlock(3);
        if(I("iSpies")>=8)Unlock(5);
        for(int i=0;i<CouncilActions.Allies.Length;i++)if(I("iAidState"+i)==2){Unlock(6);break;}
        bool allTechnologies=TechnologySystem.All.Length>0;
        foreach(var technology in TechnologySystem.All)if(!VariableSingleton.GetBoolVariable(technology.Key)){allTechnologies=false;break;}
        if(allTechnologies)Unlock(7);
        // A missed full daily ration is the existing game's shortage condition.
        if(VariableSingleton.HasSiegeStarted() && (I("iFoodShortfall")>0 || I("iHungerDays")>0))VariableSingleton.SetInt("iAchievementShortageWitnessed",1);
        if(I("iAchievementShortageWitnessed")==1 && SharedProvisions.CityDemand>0 && SharedProvisions.CityDays>=30)Unlock(9);
        if(TechnologyDiscoveries.Found(TechnologyDiscoveries.Kind.Alchemy))Unlock(18);
        if(I("iGrandCannon")==2)Unlock(11);
        for(int i=0;i<8;i++)if(CourtRoster.InService(i))VariableSingleton.SetInt("iAchievementPerson"+i,1);
        if(CharacterPreparations.InService)VariableSingleton.SetInt("iAchievementPerson"+CharacterPreparations.CommanderId,1);
        if(DailyLifeEvents.I("iKjoldState",1)==1)VariableSingleton.SetInt("iAchievementPerson200",1);
        if(SultanGifts.Known && (I("iShahStatus")==4 || I("iShahStatus")==7))VariableSingleton.SetInt("iAchievementPerson201",1);
        int count=0;foreach(int id in CharacterPortraits.People)if(id!=8 && id!=202 && I("iAchievementPerson"+id)>0)count++;
        if(count>=7)Unlock(12);
    }
    // Call before SaveGames.End marks the campaign terminal. The original rule says
    // finish a campaign; it does not require victory or a minimum survival time.
    public static void CampaignEnded() {
        CheckCampaign();
        if(VariableSingleton.Instance!=null && VariableSingleton.Instance.EnduringWall.GetCodeName()=="OuterWall")Unlock(4);
    }
    public static void Victory() {
        CheckCampaign();
        if(I("iAchievementRepairTracked")==1 && I("iAchievementWallRepaired")==0)Unlock(17);
    }
    public static string IconResourcePath(int index)=>"UI/MidnightMosaic/Achievements/"+AchievementPlatform.Ids[index];
    public static void Show(Transform parent)
    {
        CheckCampaign();
        var panel=CampaignHUD.Panel(parent,"Achievements",new Vector2(.02f,.02f),new Vector2(.98f,.98f));
        ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        var header=CampaignHUD.Label(panel.transform,"Achievements \u00b7 Ironman; Alchemical Genius also in Normal",new Vector2(.04f,.91f),new Vector2(.96f,.99f),33);
        header.enableAutoSizing=true;header.fontSizeMin=22;header.fontSizeMax=33;
        var viewport=CampaignHUD.Panel(panel.transform,"Achievement list",new Vector2(.04f,.14f),new Vector2(.94f,.90f));viewport.AddComponent<RectMask2D>();
        var content=new GameObject("Achievement entries",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);
        var r=content.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,Names.Length*100);
        for(int i=0;i<Names.Length;i++) {
            float top=1f-(float)i/Names.Length, bottom=top-1f/Names.Length;
            bool earned=AchievementPlatform.Earned(i);
            var colour=earned?new Color(.91f,.79f,.49f):new Color(.52f,.54f,.56f);
            var box=CampaignHUD.Panel(content.transform,"Achievement icon "+i,new Vector2(.01f,bottom+.10f/Names.Length),new Vector2(.085f,top-.10f/Names.Length));
            box.GetComponent<UnityEngine.UI.Image>().color=new Color(.04f,.07f,.09f);
            var art=Resources.Load<Texture2D>(IconResourcePath(i));
            if(art!=null){
                var picture=new GameObject("Achievement artwork",typeof(RectTransform),typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.AspectRatioFitter));
                picture.transform.SetParent(box.transform,false);
                var image=picture.GetComponent<UnityEngine.UI.RawImage>();image.texture=art;image.raycastTarget=false;image.color=earned?Color.white:new Color(.46f,.46f,.46f,.85f);
                var aspect=picture.GetComponent<UnityEngine.UI.AspectRatioFitter>();aspect.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=1;
            }else{
                box.GetComponent<UnityEngine.UI.Image>().color=colour;
                var inner=CampaignHUD.Panel(box.transform,"Inset",new Vector2(.08f,.06f),new Vector2(.92f,.94f));inner.GetComponent<UnityEngine.UI.Image>().color=new Color(.04f,.07f,.09f);
                if(earned){var tick=CampaignHUD.Label(box.transform,"\u2713",Vector2.zero,Vector2.one,26);tick.alignment=TMPro.TextAlignmentOptions.Center;tick.color=colour;}
            }
            var title=CampaignHUD.Label(content.transform,Names[i]+(earned?" \u00b7 Earned":!Implemented(i)?" \u00b7 Unavailable":" \u00b7 Locked"),new Vector2(.11f,bottom+.030f),new Vector2(.98f,top-.003f),23);title.color=colour;
            var desc=CampaignHUD.Explanation(content.transform,Descriptions[i]+(Implemented(i)?"":" "+UnavailableReason(i)),new Vector2(.11f,bottom+.003f),new Vector2(.98f,bottom+.030f),18);desc.color=colour;
        }
        var scroll=viewport.AddComponent<ScrollRect>();scroll.content=r;scroll.viewport=viewport.GetComponent<RectTransform>();scroll.horizontal=false;scroll.scrollSensitivity=45;scroll.movementType=ScrollRect.MovementType.Clamped;
        CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.95f,.14f),new Vector2(.965f,.90f));
        CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.65f,.025f),new Vector2(.96f,.095f),()=>Object.Destroy(panel));
    }
}
