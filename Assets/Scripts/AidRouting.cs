// Route and ownership of transport ships are separate from the delivered cargo.
public static class AidRouting
{
    public static bool BySea(int ally)=>ally>=0 && ally<CouncilActions.Allies.Length && ally!=0 && ally!=4 && ally!=7 && ally!=13 && ally!=14;
    public static string Route(int ally)=>BySea(ally)?"Sea route":"Land route";
    public static int Warships(int ally)=>DailyLifeEvents.I("iAidWarships"+ally,CouncilActions.Int("iAidState"+ally)==1?CouncilActions.Allies[ally].Ships:0);
    public static string Cargo(int ally)=>CouncilActions.Allies[ally].CargoFor(Warships(ally));
}
