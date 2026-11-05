using UnityEngine;
// Once-only agreements persist in the authoritative campaign state.
public static class TheodoroDiplomacy
{
 public const int Company=21;
 static bool Done(string key)=>DailyLifeEvents.I("iTheodoro"+key)>0;
 public static bool Ties=>Done("Ties");
 public static bool Archery=>Done("Archery");
 public static string Reason(int action){
 if(action==0)return Ties?"Imperial ties already renewed":null;
 if(action==1)return Archery?"Archery cooperation already agreed":DailyLifeEvents.I("iMoney")<1500?"Requires 1,500 gold":null;
 if(!TechnologySystem.Has("DismountedChampions"))return "Requires Dismounted Champions";
 return MercenaryContracts.HireReason(Company,false);
 }
 public static bool Agree(int action,out string report){
 report=Reason(action);if(report!=null)return false;
 if(action==2)return MercenaryContracts.Sign(Company,out report,false);
 if(action==0){VariableSingleton.SetInt("iTheodoroTies",1);CountryDiplomacy.ChangeGoodwill(CountryDiplomacy.Index("theodoro"),10);report="Letters pass between Mangup and Constantinople, renewing the old imperial ties. Theodoro goodwill +10 points.";DecisionJournal.Record("theodoro-ties","Imperial ties renewed",report,true);}
 else {VariableSingleton.SetInt("iTheodoroArchery",1);VariableSingleton.ChangeInt("iMoney",-1500);report="Theodoro's instructors share their methods with the City's archers. Archer lethality +5%. Gold spent: 1,500.";DecisionJournal.Record("theodoro-archery","Archery cooperation with Theodoro",report,true);}
 return true;
 }
}
