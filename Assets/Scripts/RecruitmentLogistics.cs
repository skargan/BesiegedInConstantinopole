using System;

// Read manpower from the campaign and pending orders; never maintain a second pool.
public static class RecruitmentLogistics
{
    const string TrackedKey = "iRecruitmentLossesTracked";
    static readonly string[] LossUnits = {"iInfantry", "iArchers", "iCavalry", "iVarangians"};
    static string LossKey(string unit) => "iRecruitmentLost" + unit.Substring(1);
    static int NonNegative(string key) => Math.Max(0, DailyLifeEvents.I(key));
    static int Count(long value) => (int)Math.Min(int.MaxValue, Math.Max(0L, value));

    public static bool IsRegular(string unit) => unit == "iInfantry" || unit == "iArchers" || unit == "iCavalry";
    public static int ManpowerPool => Count(PopulationAccounting.People / 10);
    public static int Serving => Count((long)NonNegative("iInfantry") + NonNegative("iArchers") + NonNegative("iCavalry"));
    public static int Queued
    {
        get
        {
            long count = 0;
            foreach(var order in TrainingPool.Orders)
                if(order != null && IsRegular(order.unit)) count += Math.Max(0, order.quantity);
            return Count(count);
        }
    }
    public static int ExternalRegulars=>Math.Min(Serving,NonNegative("iMercForeignInfantry")+NonNegative("iMercForeignArchers")+NonNegative("iMercForeignCavalry"));
    public static int AvailableManpower => Count((long)ManpowerPool - (Serving-ExternalRegulars) - Queued - MedicalRecovery.RegularWounded - SpecialistPersonnel.LocalManpowerCommitted - RagusaDiplomacy.CommittedTroops);
    public static bool CanRecruit(string unit, int quantity)
    {
        if(quantity<=0)return false;
        if(IsRegular(unit))return quantity<=AvailableManpower;
        return SpecialistPersonnel.CanStart(unit,quantity,out _);
    }
    public static int MaximumOrder(string unit, int affordable) => SpecialistPersonnel.MaximumOrder(unit,Math.Max(0, IsRegular(unit) ? Math.Min(affordable, AvailableManpower) : affordable));

    public static bool LossesTracked => DailyLifeEvents.I(TrackedKey) == 1;
    public static int Losses(string unit) => Array.IndexOf(LossUnits, unit) < 0 ? 0 : NonNegative(LossKey(unit));
    public static string LossText(string unit) => LossesTracked ? Losses(unit).ToString("N0") : "—";

    public static void BeginCampaign()
    {
        ResetForRestore();
        VariableSingleton.SetInt(TrackedKey, 1);
    }
    // Saved integers restore the marker afterwards. Legacy saves have no reliable history.
    public static void ResetForRestore()
    {
        VariableSingleton.SetInt(TrackedKey, 0);
        foreach(string unit in LossUnits) VariableSingleton.SetInt(LossKey(unit), 0);
    }
    public static void RecordLoss(string unit, int actualLoss)
    {
        if(IsRegular(unit)&&actualLoss>0){string key="iMercForeign"+unit.Substring(1);int total=NonNegative(unit),foreign=NonNegative(key);if(total>0&&foreign>0)VariableSingleton.SetInt(key,Math.Max(0,foreign-(int)Math.Round(actualLoss*(double)foreign/total)));}
        if(!LossesTracked || actualLoss <= 0 || Array.IndexOf(LossUnits, unit) < 0) return;
        VariableSingleton.SetInt(LossKey(unit), Count((long)Losses(unit) + actualLoss));
    }
}
