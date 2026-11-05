using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Every consequence is stored in the authoritative campaign dictionary. No live RNG
// is used to advance voyages, redraw offers, reload, or revisit incident choices.
public static class MercenaryContracts
{
 public enum Status {Unhired,Travelling,Decision,Payment,Arrived,Lost}
 public enum Incident {None,Storm,OffCourse,Greedy,Traitors,Cowards,Disputed,Chaotic,Evasion,Battle,Straits}
 public static int Today=>TrainingPool.Day(VariableSingleton.GetDate());
 static string K(int id,string key)=>"iMerc_"+id+"_"+key;
 public static int Get(int id,string key)=>DailyLifeEvents.I(K(id,key));
 public static void Put(int id,string key,int value)=>VariableSingleton.SetInt(K(id,key),value);
 public static Status State(int id)=>(Status)Get(id,"State");
 public static string StatusText(int id)=>State(id)==Status.Decision?"Decision required":State(id)==Status.Payment?"Arrived · payment due":State(id)==Status.Travelling?"At sea · "+Remaining(id)+" days":State(id).ToString();
 public static int Remaining(int id)=>Math.Max(0,Get(id,"Due")-Today);
 public static int Pending=>Enumerable.Range(0,MercenaryRoster.All.Length).Count(i=>State(i)==Status.Decision||State(i)==Status.Payment);
 public static int Rhodian=>Array.FindIndex(MercenaryRoster.All,c=>c.Order);
 public static bool RhodianReserved=>State(Rhodian)!=Status.Unhired;
 public static int[] Offers {get {int month=VariableSingleton.GetDate().Year*12+VariableSingleton.GetDate().Month;var regular=Enumerable.Range(0,MercenaryRoster.All.Length).Where(i=>i<20&&!MercenaryRoster.All[i].Sanctuary&&!MercenaryRoster.All[i].Order).ToArray();var offers=Enumerable.Range(0,4).Select(i=>regular[(month*4+i)%regular.Length]).ToList();offers.Add(month%2==0?17:18);return offers.ToArray();}}
 public static int PatrolRisk=>NavalState.Dominance==NavalState.Control.Ottoman?12:NavalState.Dominance==NavalState.Control.Contested?6:0;
 public static int StraitRisk(int id)=>MercenaryRoster.All[id].Port=="Caffa"&&!VariableSingleton.GetBoolVariable("bStraitsOpen")?(MercenaryRoster.All[id].Flotilla?15:50):0;
 public static int SafeChance(int id)=>Math.Max(0,100-(MercenaryRoster.All[id].Flotilla?1:5)-MercenaryRoster.All[id].Risk-PatrolRisk-StraitRisk(id));
 // Disjoint additive bands: exactly one incident (or an uninterrupted voyage).
 public static Incident Roll(int id,int roll,int variant)
 {
 var c=MercenaryRoster.All[id];int n=c.Flotilla?1:2;if(roll<n)return Incident.Storm;
 if(!c.Flotilla){n+=3;if(roll<n)return Incident.OffCourse;}
 n+=c.Risk;if(roll<n)return (Incident)((int)Incident.Greedy+Mathf.Clamp(variant,0,4));
 n+=PatrolRisk/2;if(roll<n)return Incident.Evasion;n+=PatrolRisk/2;if(roll<n)return Incident.Battle;
 n+=StraitRisk(id);return roll<n?Incident.Straits:Incident.None;
 }
 public static string HireReason(int id,bool requireOffer=true)
 {
 if(id<0||id>=MercenaryRoster.All.Length)return "Unknown company";var c=MercenaryRoster.All[id];
 if(State(id)!=Status.Unhired)return "This company already has a contract record";
 if(requireOffer&&!Offers.Contains(id))return "This company is not currently offered";
 if(id==TheodoroDiplomacy.Company&&!TechnologySystem.Has("DismountedChampions"))return "Requires Dismounted Champions";
 if(c.Order){int h=CountryDiplomacy.Index("hospitallers");if(h<0)h=CountryDiplomacy.Index("rhodes");if(h<0||DailyLifeEvents.I("iAidState"+h)==1||DailyLifeEvents.I("iAidState"+h)==2)return "The Hospitaller detachment is committed elsewhere";if(CountryDiplomacy.Goodwill(h)<60)return "Requires 60 Hospitaller goodwill";}
 if(DailyLifeEvents.I("iMoney")<c.Gold/2)return "Requires "+(c.Gold/2).ToString("N0")+" gold upfront";return null;
 }
 public static bool Sign(int id,out string report,bool requireOffer=true)
 {
 report=HireReason(id,requireOffer);if(report!=null)return false;var c=MercenaryRoster.All[id];
 int roll=UnityEngine.Random.Range(0,100),variant=UnityEngine.Random.Range(0,5);var incident=Roll(id,roll,variant);
 Put(id,"State",(int)Status.Travelling);Put(id,"Start",Today);Put(id,"Due",Today+c.Days);Put(id,"Price",c.Gold);Put(id,"Paid",c.Gold/2);Put(id,"Incident",(int)incident);Put(id,"IncidentDay",Today+UnityEngine.Random.Range(1,c.Days));Put(id,"Safe",SafeChance(id));Put(id,"EnemyLoss",UnityEngine.Random.Range(1,4));Put(id,"Delay",incident==Incident.Evasion?UnityEngine.Random.Range(8,17):incident==Incident.OffCourse?10:15);
 VariableSingleton.ChangeInt("iMoney",-c.Gold/2);
 if(c.Sanctuary)VariableSingleton.ChangePercentFloat("fNobleRep",-.05f);
 report=c.Name+" signs under "+c.Captain+". "+(c.Gold/2).ToString("N0")+" gold paid; "+(c.Gold/2).ToString("N0")+" due on arrival. Departure: "+c.Port+"; baseline journey "+c.Days+" days.";
 DecisionJournal.Record("merc-contract-"+id,c.Name+" contracted",report,true);return true;
 }
 static void Lose(int id,string reason,bool defect=false)
 {
 if(State(id)==Status.Lost||State(id)==Status.Arrived)return;Put(id,"State",(int)Status.Lost);var c=MercenaryRoster.All[id];
 if(defect){VariableSingleton.ChangeInt("iBazouks",c.Land);reason+=" "+c.Land+" land combatants join the Ottoman infantry.";}
 DecisionJournal.Record("merc-end-"+id,c.Name,reason,true);Put(id,"Ended",Today);
 }
 public static void FailedLocalNegotiation(int id,string reason)=>Lose(id,reason);
 public static string IncidentTitle(Incident kind)=>kind==Incident.Storm?"Horrible Storm":kind==Incident.OffCourse?"Blown off Course":kind==Incident.Evasion?"Ottoman Fleet: Evading Patrols":kind==Incident.Battle?"Ottoman Fleet: An Unequal Battle":kind==Incident.Straits?"Ambush in the Straits":kind==Incident.Disputed?"Unreliable Mercenaries: A Disputed Command":kind==Incident.Chaotic?"Unreliable Mercenaries: An Unexpected Engagement":"Unreliable Mercenaries: "+kind;
 public static string IncidentText(int id)
 {
 switch((Incident)Get(id,"Incident")){
 case Incident.Greedy:return "The captain writes that passage has proved dearer than agreed. His men will continue only if you send another 20% of the original price. The remaining half is still due on arrival.";
 case Incident.Disputed:return "The captain has been overthrown. His son and a handful of loyal men still hold out. A camp agent could settle the succession quietly; he would never return to your service. The usurper will instead honour the agreement for another 50% of its original price.";
 case Incident.Chaotic:return "At a coastal stop, the men quarrelled with the watch and killed three guards. Their transports sailed without them. The lieutenant then married a local lord’s daughter, fought her father’s rival, and left seventeen levies and their lord dead. A settlement involving fifty sheep and six households restored the peace. New passage has finally been found. Fifteen days have been lost.";
 case Incident.Traitors:return "Mehmed’s agents have offered twice your price. The captain has accepted. The company abandons its voyage and enters Ottoman service.";
 case Incident.Cowards:return "Reports of the Sultan’s army have reached the transports. The captain declares the undertaking impossible; his men refuse to sail for Constantinople.";
 case Incident.Storm:return "The convoy has vanished in a terrible storm. Wreckage and a single battered boat are all that have reached the next harbour.";
 case Incident.OffCourse:return "Gales have driven the hired transports far from their course. The captain has found shelter, but ten days must be added to the voyage.";
 case Incident.Evasion:return "Ottoman sails barred the direct passage. The convoy escaped pursuit by taking shelter and sailing a longer course. Arrival is delayed by "+Get(id,"Delay")+" days.";
 case Incident.Battle:return "The convoy was forced into an unequal battle. No company will reach Constantinople. The escort nevertheless took Ottoman ships down with it.";
 case Incident.Straits:return "The ships were intercepted while attempting to run the Bosphorus blockade. Shore guns opened fire as the vessels entered the narrow water. The company has been destroyed.";
 default:return "The voyage continues.";}
 }
 public static string Advance(DateTime date)
 {
 int day=TrainingPool.Day(date);var report=new StringBuilder();for(int id=0;id<MercenaryRoster.All.Length;id++){
 if(State(id)!=Status.Travelling)continue;var c=MercenaryRoster.All[id];
 if(Get(id,"LastTick")>=day)continue;Put(id,"LastTick",day);if(day>Get(id,"HoldUntil"))Put(id,"Travel",Math.Min(c.Days,Get(id,"Travel")+1));
 if(Get(id,"Handled")==0&&Get(id,"IncidentDay")<=day){Put(id,"Handled",1);var kind=(Incident)Get(id,"Incident");if(kind!=Incident.None){string text=IncidentTitle(kind)+" · "+c.Name+"\n"+IncidentText(id);report.AppendLine(text);
 DecisionJournal.Record("merc-incident-"+id,IncidentTitle(kind),c.Name+". "+IncidentText(id),true);
 if(kind==Incident.Greedy||kind==Incident.Disputed){Put(id,"State",(int)Status.Decision);Put(id,"Paused",day);continue;}
 if(kind==Incident.OffCourse||kind==Incident.Chaotic||kind==Incident.Evasion){Put(id,"Due",Get(id,"Due")+Get(id,"Delay"));Put(id,"HoldUntil",day+Get(id,"Delay"));}
 else if(kind==Incident.Traitors){Lose(id,IncidentText(id),true);continue;}
 else {if(kind==Incident.Battle){int lost=Math.Min(NavalState.EnemyShips,Get(id,"EnemyLoss"));VariableSingleton.ChangeInt("iTurkShips",-lost);NavalState.Sync();report.AppendLine(lost+" Ottoman ships sunk.");}Lose(id,IncidentText(id));continue;}
 }}
 if(State(id)==Status.Travelling&&Get(id,"Due")<=day){Put(id,"State",(int)Status.Payment);report.AppendLine(c.Name+" has reached Constantinople. "+Get(id,"Price")/2+" gold is due. Visit the mercenary brokers to pay or turn them away.");}}
 return report.Length>0?"\nMercenary dispatches\n"+report+"\n":"";
 }
 public static string Resolve(int id,int choice)
 {
 var state=State(id);var c=MercenaryRoster.All[id];int price=Get(id,"Price");
 if(state==Status.Payment){if(choice==0){if(DailyLifeEvents.I("iMoney")<price/2)return "Not enough gold for the final payment.";VariableSingleton.ChangeInt("iMoney",-price/2);Put(id,"Paid",Get(id,"Paid")+price/2);Put(id,"State",(int)Status.Arrived);
 VariableSingleton.ChangeInt("iInfantry",c.Infantry);VariableSingleton.ChangeInt("iArchers",c.Archers);VariableSingleton.ChangeInt("iCavalry",c.Cavalry);VariableSingleton.ChangeInt("iShips",c.Ships);VariableSingleton.ChangeInt("iCannons",c.Cannons);VariableSingleton.ChangeInt("iEngineers",c.Engineers);
 foreach(var pair in new[]{new[]{"Infantry",c.Infantry.ToString()},new[]{"Archers",c.Archers.ToString()},new[]{"Cavalry",c.Cavalry.ToString()}})VariableSingleton.SetInt("iMercForeign"+pair[0],DailyLifeEvents.I("iMercForeign"+pair[0])+int.Parse(pair[1]));
 PopulationAccounting.Add(c.Land+c.Ships*SpecialistPersonnel.ShipCrew+c.Cannons*SpecialistPersonnel.CannonCrew+c.Engineers+(c.Sanctuary?150:0));
 string text=c.Name+" arrives under "+c.Captain+". "+c.Package+(c.Sanctuary?"; 150 dependants receive sanctuary.":".");DecisionJournal.Record("merc-end-"+id,c.Name+" arrived",text,true);return text;}
 Lose(id,"Turned away without the promised payment.",true);return c.Name+" has been turned away and joined the Ottomans.";}
 if(state!=Status.Decision)return "This contract requires no decision.";
 var kind=(Incident)Get(id,"Incident");int extra=kind==Incident.Greedy?price/5:price/2;
 if(choice==2){Lose(id,"The imperial court abandons the contract.");return "The contract is abandoned.";}
 if(choice==1&&kind==Incident.Disputed){if(DailyLifeEvents.I("iSpies")<1)return "Requires one camp agent.";VariableSingleton.ChangeInt("iSpies",-1);}
 else {if(DailyLifeEvents.I("iMoney")<extra)return "Not enough gold for the surcharge.";VariableSingleton.ChangeInt("iMoney",-extra);Put(id,"Paid",Get(id,"Paid")+extra);}
 Put(id,"Due",Get(id,"Due")+Math.Max(0,Today-Get(id,"Paused")));Put(id,"State",(int)Status.Travelling);return "The company resumes its journey. The remaining half of the original price is still due on arrival.";
 }
}
