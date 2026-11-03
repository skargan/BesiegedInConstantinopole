using System;
using System.Linq;
// Diplomacy now commits real expeditions. Legacy convoy-delay budgets are retired.
public static class WarInterventions
{
    public static int UsedShipments(int country)=>0;
    public static int RemainingShipments(int country)=>0;
    public static int CountryForShipment(int province,int wave)=>-1;
    public static int DelayForShipment(int province,int wave)=>0;
    public static bool TryDelayShipment(int province,int wave,int siegeDay)=>false;
    public static string TakeReport()=>"";
    public static string Terms(int country)
    {
        if(country<0||country>=CountryDiplomacy.Definitions.Length)return "";
        var a=ProvincialWar.Rules.Allies.FirstOrDefault(x=>x.Id==CountryDiplomacy.Definitions[country].Id);
        return a==null?"This agreement does not commit a field army.":"Commits "+a.Troops.ToString("N0")+" troops, with "+a.Reserve.ToString("N0")+" replacements held in reserve. The expedition contests provincial territory and the local garrison.";
    }
    public static string Summary(int country)
    {
        if(!ProvincialWar.Ready||ProvincialWar.I("Committed"+country)==0)return Terms(country);
        string status=ProvincialWar.I("Peace"+country)>0?"Separate peace; external campaigning has ended.":ProvincialWar.I("Pause"+country)>ProvincialWar.I("LastDay")?"Cessation: "+(ProvincialWar.I("Pause"+country)-ProvincialWar.I("LastDay"))+" days remain.":"Expedition active.";
        return status+"\n"+string.Join("\n",ProvincialWar.Provinces.Select(p=>new ProvincialWar.Army(p,country+1)).Where(a=>a.Troops>0||a.Control>0).Select(a=>a.Province.Name+": "+a.Troops.ToString("N0")+" troops · "+(a.Control/100f).ToString("0.##")+"% territory"))+"\nCommitted reserve: "+ProvincialWar.I("Reserve"+country).ToString("N0");
    }
}
