using System;

// One shared allowance per council turn: preparation month before siege, day afterwards.
// These three proposed values remain provisional pending the designer's reply.
public static class MarketPurchaseLimits
{
    public const int ProvisionsPerTurn = 500;
    public const int MaterialsPerTurn = 100;
    public const int AmmunitionPerTurn = 20;
    const string Stamp = "iMarketPurchaseTurn";
    static readonly string[] Counters = {"iMarketProvisionsBought","iMarketMaterialsBought","iMarketAmmunitionBought"};
    static int Index(string resource)=>resource=="iProvisions"?0:resource=="iMaterials"?1:resource=="iAmmunition"?2:-1;
    public static int Limit(string resource)
    {
        int index=Index(resource);
        return index==0?ProvisionsPerTurn:index==1?MaterialsPerTurn:index==2?AmmunitionPerTurn:0;
    }
    public static int Purchased(string resource)
    {
        int index=Index(resource);
        if(index<0 || DailyLifeEvents.I(Stamp)!=VariableSingleton.Turn+1)return 0;
        return Math.Max(0,DailyLifeEvents.I(Counters[index]));
    }
    public static int Remaining(string resource)=>Math.Max(0,Limit(resource)-Purchased(resource));
    public static string Unavailable(string resource,int quantity)
    {
        if(Index(resource)<0 || quantity<=0)return "Enter a whole quantity greater than zero.";
        int left=Remaining(resource);
        if(quantity>left)return left==0?"No more available this turn.":left.ToString("N0")+" remain available this turn.";
        return null;
    }
    // Called only after the purchase has passed every validation.
    public static void Record(string resource,int quantity)
    {
        int index=Index(resource);
        if(index<0 || Unavailable(resource,quantity)!=null)throw new InvalidOperationException("Cannot record an invalid market purchase.");
        if(DailyLifeEvents.I(Stamp)!=VariableSingleton.Turn+1) {
            foreach(string key in Counters)VariableSingleton.SetInt(key,0);
            VariableSingleton.SetInt(Stamp,VariableSingleton.Turn+1);
        }
        VariableSingleton.SetInt(Counters[index],Purchased(resource)+quantity);
    }
    // Invoke before saved integer values are loaded; legacy archives then cannot inherit another run's purchases.
    public static void ResetForRestore()
    {
        VariableSingleton.SetInt(Stamp,0);
        foreach(string key in Counters)VariableSingleton.SetInt(key,0);
    }
}

