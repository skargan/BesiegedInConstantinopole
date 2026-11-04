using UnityEngine;
using Yarn.Unity;

public static class SupplyEconomy
{
    public static int DailyNeed=>(int)System.Math.Ceiling(SharedProvisions.CityDemand);
    // Compatibility helper for older checks; faction ration discounts are retired.
    public static int FoodAfterPolitics(int need)=>need;
    public static long Cost(string resource,int quantity)=>(long)Price(resource)*quantity;
    public static int Price(string resource)=>resource=="iProvisions"?(VariableSingleton.HasSiegeStarted()?10:2):resource=="iMaterials"?10:resource=="iAmmunition"?50:0;
    public static int DaysLeft=>(int)System.Math.Floor(SharedProvisions.CityDays);
    public static string PurchaseUnavailable(string resource,int quantity) {
        int price=Price(resource);long cost=Cost(resource,quantity);
        if(price==0 || quantity<=0 || cost>int.MaxValue)return "Enter a valid whole quantity.";
        string unavailable=MarketPurchaseLimits.Unavailable(resource,quantity);if(unavailable!=null)return unavailable;
        if(CouncilActions.Int("iMoney")<cost)return "The treasury cannot cover this purchase.";
        if((long)CouncilActions.Int(resource)+quantity>int.MaxValue)return "The stores cannot hold that quantity.";
        return null;
    }
    public static bool Buy(string resource,int quantity,out string report) {
        report=PurchaseUnavailable(resource,quantity);if(report!=null)return false;
        long cost=Cost(resource,quantity);
        VariableSingleton.ChangeInt("iMoney",-(int)cost);VariableSingleton.ChangeInt(resource,quantity);
        MarketPurchaseLimits.Record(resource,quantity);
        report=quantity.ToString("N0")+" "+(resource=="iProvisions"?"provisions":resource=="iMaterials"?"materials":"ammunition")+" bought for "+cost.ToString("N0")+" gold.";return true;
    }
    public static string CompleteDay() {
        return SharedProvisions.ConsumeCityDay();
    }
    public static string PendingNode=>null; // Daily hunger is resolved once by shared provisions; no repeated mandatory ration event.
    public static bool EmergencyTax(out string report) {
        var before=VariableSingleton.Instance.CaptureState();VariableSingleton.ChangeInt("iMoney",DailyNeed*100);VariableSingleton.ChangePercentFloat("fPeopleRep",-.15f);
        report="Collectors call on the households and merchants for an emergency levy.\n\n"+EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState());return true;
    }
    [YarnFunction("FoodNeed")] public static int FoodNeed()=>DailyNeed;
    [YarnFunction("EmergencyGrainCost")] public static int GrainCost()=>(int)Cost("iProvisions",DailyNeed*3);
    [YarnFunction("CanBuyEmergencyGrain")] public static bool CanBuyEmergencyGrain()=>PurchaseUnavailable("iProvisions",DailyNeed*3)==null;
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

