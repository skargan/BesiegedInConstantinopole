// Route and ownership of transport ships are separate from the delivered cargo.
public static class AidRouting
{
    public static bool BySea(int ally)=>ally>=0 && ally<CountryDiplomacy.Definitions.Length && CountryDiplomacy.Definitions[ally].AidBySea;
    public static string Route(int ally)=>ally==CouncilActions.AqQoyunluIndex?"Via Trebizond · sea route":BySea(ally)?"Sea route":"Land route";
    public static int Warships(int ally)=>DailyLifeEvents.I("iAidWarships"+ally,DailyLifeEvents.I("iAidState"+ally)==1?CouncilActions.Allies[ally].Ships:0);
    public static string Cargo(int ally)=>DiplomaticStoryAid.DeliveredCargo(ally)??CouncilActions.Allies[ally].CargoFor(Warships(ally));
}
