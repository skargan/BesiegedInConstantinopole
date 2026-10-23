using UnityEngine;
using Yarn.Unity;

public static class SupplyEconomy
{
    public static int DailyNeed=>FoodAfterPolitics(PopulationAccounting.Food(false));
    public static int FoodAfterPolitics(int need)=>PoliticalSystem.Passive(1)?Mathf.CeilToInt(need*.95f):need;
    public static long Cost(string resource,int quantity)=>PoliticalSystem.Passive(3)?((long)Price(resource)*quantity*95+99)/100:(long)Price(resource)*quantity;
    public static int Price(string resource)=>resource=="iProvisions"?(VariableSingleton.HasSiegeStarted()?10:2):resource=="iMaterials"?10:resource=="iAmmunition"?50:0;
    public static int DaysLeft=>DailyNeed==0?0:CouncilActions.Int("iProvisions")/DailyNeed;
    public static bool Buy(string resource,int quantity,out string report) {
        report="Choose a quantity.";int price=Price(resource);long cost=Cost(resource,quantity);
        if(price==0 || quantity<=0 || cost>int.MaxValue)return false;
        if(CouncilActions.Int("iMoney")<cost){report="The treasury cannot cover this purchase.";return false;}
        VariableSingleton.ChangeInt("iMoney",-(int)cost);VariableSingleton.ChangeInt(resource,quantity);
        report=quantity.ToString("N0")+" "+(resource=="iProvisions"?"provisions":resource=="iMaterials"?"materials":"ammunition")+" bought for "+cost.ToString("N0")+" gold.";return true;
    }
    public static string CompleteDay() {
        int need=FoodAfterPolitics(PopulationAccounting.Food(true)),stock=CouncilActions.Int("iProvisions"),consumed=Mathf.Min(need,stock);
        VariableSingleton.ChangeInt("iProvisions",-consumed);VariableSingleton.SetInt("iFoodShortfall",need-consumed);
        VariableSingleton.SetInt("iHungerDays",stock<need?CouncilActions.Int("iHungerDays")+1:0);
        return "Daily provisions: "+consumed.ToString("N0")+" consumed; "+CouncilActions.Int("iProvisions").ToString("N0")+" remain. "+(stock<need?"The stores could not provide a full ration.":DaysLeft+" full days of food remain.")+"\n\n";
    }
    public static string PendingNode=>CouncilActions.Int("iFoodShortfall")>0 && CouncilActions.Int("iProvisions")==0 && CouncilActions.Int("iHungerOfferedDay")!=TrainingPool.Day(VariableSingleton.GetDate())?"foodShortage":null;
    public static bool EmergencyTax(out string report) {
        var before=VariableSingleton.Instance.CaptureState();VariableSingleton.ChangeInt("iMoney",DailyNeed*100);VariableSingleton.ChangePercentFloat("fPeopleRep",-.15f);
        report="Collectors call on the households and merchants for an emergency levy.\n\n"+EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState());return true;
    }
    [YarnFunction("FoodNeed")] public static int FoodNeed()=>DailyNeed;
    [YarnFunction("EmergencyGrainCost")] public static int GrainCost()=>(int)Cost("iProvisions",DailyNeed*3);
    [YarnCommand("ResolveFoodShortage")]
    public static void Resolve(string choice) {
        if(CouncilActions.Int("iHungerOfferedDay")==TrainingPool.Day(VariableSingleton.GetDate()))return;
        if(choice=="buy" && !Buy("iProvisions",DailyNeed*3,out _))return;
        if(choice=="requisition"){VariableSingleton.ChangeInt("iProvisions",DailyNeed*2);VariableSingleton.ChangePercentFloat("fNobleRep",-.05f);}
        if(choice=="garrison"){VariableSingleton.ChangeInt("iProvisions",DailyNeed);VariableSingleton.ChangePercentFloat("fTroopRep",-.05f);VariableSingleton.ChangePercentFloat("fPeopleRep",.02f);}
        if(choice=="endure"){VariableSingleton.ChangePercentFloat("fTroopRep",-.02f);VariableSingleton.ChangePercentFloat("fPeopleRep",-.03f);}
        VariableSingleton.SetInt("iHungerOfferedDay",TrainingPool.Day(VariableSingleton.GetDate()));VariableSingleton.SetInt("iFoodShortfall",0);
    }
}
