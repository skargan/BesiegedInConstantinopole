using Yarn.Unity;

// Compatibility entrypoints for retired faction obligations and older compiled dialogue.
// No obligation can schedule, charge resources, damage civic support or block progress.
// Full former implementation: Documentation/Retired systems/PoliticalDemands.cs.txt.
public static class PoliticalDemands
{
    public static int Active=>-1;
    public static string PendingNode=>null;
    public static int Mature(int bloc)=>0;
    public static void Schedule() { }
    [YarnFunction("PoliticalCreditor")] public static string Creditor()=>"Former claimant";
    [YarnFunction("PoliticalPayment")] public static float Payment()=>0;
    [YarnFunction("PoliticalCanPay")] public static bool CanPay()=>false;
    [YarnFunction("PoliticalConcession")] public static string Concession()=>"No concession is required.";
    [YarnFunction("PoliticalCanEmpower")] public static bool CanEmpower()=>false;
    [YarnCommand("ResolvePoliticalDemand")] public static void Resolve(string choice) { }
}
