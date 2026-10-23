using System.Linq;
using Yarn.Unity;
public static class RecurringEvents
{
    public static int FleetDefensiveDamage=>CouncilActions.Int("iShips")*4*(TechnologySystem.Has("ArtilleryShips")?2:1);
    public static bool HarvestDue => CouncilActions.Int("iHarvestDay")>0 && CouncilActions.Int("iHarvestDay")<=TrainingPool.Day(VariableSingleton.GetDate());
    public static bool Eligible(int id)
    {
        if(VariableSingleton.CompletedSiegeDays<=0 || TrainingPool.Day(VariableSingleton.GetDate())<DailyLifeEvents.I("iLifeCooldownrandom"+id)) return false;
        switch(id) {
            case 18: return false; // Replaced by the independent fleet operation clock.
            case 23: return VariableSingleton.CompletedSiegeDays>=7 && CouncilActions.Int("iArtilleryConvoy")==0;
            case 1: return CouncilActions.Int("iInfantry")>=100 && CouncilActions.Int("iArchers")>=50;
            case 5: return CouncilActions.Int("iCannons")>0;
            case 8: return CouncilActions.Int("iHarvestDay")==0;
            case 14: return VariableSingleton.Instance.EnduringWall.GetCodeName()=="OuterWall";
            case 16: return CouncilActions.Int("iCavalry")>0;
            case 17: return CouncilActions.Int("iTurkCannons")>0;
            case 20: return CouncilActions.Int("iEngineers")>0;
            case 21: return CouncilActions.Int("iProvisions")>=150;
            default: return id>=0 && id<=22;
        }
    }
    [YarnCommand("StormRaid")] public static void StormRaid()
    {
        int total=CouncilActions.Int("iBalkans")+CouncilActions.Int("iBazouks")+CouncilActions.Int("iJanissaries");int kills=UnityEngine.Mathf.Min(100,total);if(total==0)return;
        int b=UnityEngine.Mathf.Min(CouncilActions.Int("iBalkans"),UnityEngine.Mathf.RoundToInt(kills*CouncilActions.Int("iBalkans")/(float)total));
        int j=UnityEngine.Mathf.Min(CouncilActions.Int("iJanissaries"),UnityEngine.Mathf.RoundToInt(kills*CouncilActions.Int("iJanissaries")/(float)total));
        int z=UnityEngine.Mathf.Min(CouncilActions.Int("iBazouks"),kills-b-j);
        VariableSingleton.ChangeInt("iBalkans",-b);VariableSingleton.ChangeInt("iJanissaries",-j);VariableSingleton.ChangeInt("iBazouks",-z);
    }
    [YarnCommand("RndFraction")]
    public static void RndFraction(string key,float min,float max) => VariableSingleton.ChangePercentFloat(key,UnityEngine.Random.Range(System.Math.Min(min,max),System.Math.Max(min,max)));
    [YarnCommand("PlantGardens")]
    public static void PlantGardens() => VariableSingleton.SetInt("iHarvestDay",TrainingPool.Day(VariableSingleton.GetDate())+25);
    [YarnCommand("Conscript")]
    public static void Conscript(int count) {if(count>0) TrainingPool.Orders.Add(new TrainingPool.Order {unit="iInfantry",quantity=count,readyDay=TrainingPool.Day(VariableSingleton.GetDate())+5});}
    [YarnFunction("FleetReportPreview")]
    public static string FleetReportPreview() {
        string[] effects={"Enemy provisions -3","Gold +750","Provisions +150","Enemy casualties -"+FleetDefensiveDamage,"50 infantry and 25 archers arrive","Fleet lost"};
        return effects[UnityEngine.Mathf.Clamp(CouncilActions.Int("iFleet"),0,5)];
    }
    [YarnCommand("FleetReport")]
    public static void FleetReport()
    {
        if(CouncilActions.Int("iShips")<=0) return;
        int mission=CouncilActions.Int("iFleet");
        if(mission<0||mission>4)return;
        float multiplier=PreparationCampaign.FleetMultiplier(mission,true);
        switch(mission) {
            case 0: VariableSingleton.ChangeInt("iFood",UnityEngine.Mathf.RoundToInt(-3*multiplier));break;
            case 1: VariableSingleton.ChangeInt("iMoney",UnityEngine.Mathf.RoundToInt(750*multiplier));break;
            case 2: VariableSingleton.ChangeInt("iProvisions",UnityEngine.Mathf.RoundToInt(150*multiplier));break;
            case 3: VariableSingleton.ChangeInt("iBazouks",UnityEngine.Mathf.RoundToInt(-FleetDefensiveDamage*multiplier));break;
            case 4: VariableSingleton.ChangeInt("iInfantry",UnityEngine.Mathf.RoundToInt(50*multiplier));VariableSingleton.ChangeInt("iArchers",UnityEngine.Mathf.RoundToInt(25*multiplier));break;
        }
    }
}
