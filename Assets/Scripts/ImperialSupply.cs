using UnityEngine;

// Sole entry point for imperial convoy deliveries. Consumption stays in the turn resolver.
public static class ImperialSupply
{
    public const int MaximumDisruption=6;
    public static int GetSupplyPressure()=>Mathf.Clamp(DailyLifeEvents.I("iWbwSupplyLoss"),0,MaximumDisruption);
    public static int QueueDisruption(int days)
    {
        int old=GetSupplyPressure(),next=Mathf.Clamp(old+Mathf.Max(0,days),0,MaximumDisruption);
        VariableSingleton.SetInt("iWbwSupplyLoss",next);return next-old;
    }
    public static int Deliver(int plannedDays,out string report)
    {
        int planned=Mathf.Max(0,plannedDays),loss=Mathf.Min(GetSupplyPressure(),Mathf.Max(0,planned-1));
        // A cancelled/empty shipment does not consume a successful operation.
        if(planned==0){report="";return 0;}
        VariableSingleton.SetInt("iWbwSupplyLoss",GetSupplyPressure()-loss);
        VariableSingleton.SetInt("iWbwLastDeliveryLoss",loss);
        report=loss>0?"The Thessaloniki forwarding depot cannot fulfil its quota. Provincial agents have removed "+loss+" days of stores from this convoy; "+(planned-loss)+" days reach the siege camp.":"";
        return planned-loss;
    }
}
