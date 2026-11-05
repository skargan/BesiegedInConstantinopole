using System;
using System.Text;
using UnityEngine;
public static class CircassianOutpost
{
 public const int EstablishmentCost=5000,FoodQuantity=100,GoldQuantity=350,FoodReserve=2400,JourneyDays=14,Company=20;
 static int I(string key)=>DailyLifeEvents.I("iDiplomacyCopa"+key);
 static void Set(string key,int v)=>VariableSingleton.SetInt("iDiplomacyCopa"+key,v);
 public static bool Established=>I("Established")>0;
 public static bool Food=>I("FoodMode")>0;
 public static string Cargo=>Food?FoodQuantity+" provisions":GoldQuantity+" gold";
 public static string EstablishReason=>Established?"The compound is already established":DailyLifeEvents.I("iMoney")<EstablishmentCost?"Requires 5,000 gold":null;
 public static string Establish(){string reason=EstablishReason;if(reason!=null)return reason;VariableSingleton.ChangeInt("iMoney",-EstablishmentCost);Set("Established",1);Set("Next",CountryDiplomacy.Day+7);SharedProvisions.Set("CopaExport",FoodReserve);DecisionJournal.Record("copa-compound","An imperial trading compound at Copa","Local merchants and Circassian lords have agreed to an imperial compound within the existing port. Weekly consignments may be taken as gold or provisions.",true);return "The compound is established. The first consignment can depart in seven days.";}
 public static string SetCargo(bool food){if(!Established)return "Establish the trading compound first.";Set("FoodMode",food?1:0);return "Future departures carry "+Cargo+". Cargo already at sea is unchanged.";}
 public static string RecruitReason=>MercenaryContracts.HireReason(Company,false);
 public static string Recruit(){string reason=RecruitReason;if(reason!=null)return reason;int chance=Established?95:80;bool success=UnityEngine.Random.Range(0,100)<chance;if(!MercenaryContracts.Sign(Company,out var report,false))return report;
 if(!success){MercenaryContracts.FailedLocalNegotiation(Company,"The messenger is killed before securing the company's passage. The advance payment is lost.");return "Your messenger never returns. The local agreement has failed, and the advance payment is lost.";}
 return "The household agrees to serve. Its voyage and remaining payment are now listed with your mercenary contracts.\n"+report;}
 public static string Summary=>!Established?"No imperial trading compound.":"Future cargo: "+Cargo+"\nExportable food reserve: "+SharedProvisions.Format(SharedProvisions.Stock("CopaExport"))+" provisions\n"+(!VariableSingleton.GetBoolVariable("bStraitsOpen")?"Departures held: Bosphorus passage closed.":"Departures every seven days · 14-day baseline voyage.");
 public static string Advance(DateTime date)
 {
 if(!Established)return "";int day=TrainingPool.Day(date);var report=new StringBuilder();
 if(day>=I("Next")&&VariableSingleton.GetBoolVariable("bStraitsOpen")){
 Set("Next",day+7);int qty=Food?(int)Math.Min(FoodQuantity,SharedProvisions.Stock("CopaExport")):GoldQuantity;
 if(qty>0){if(Food)SharedProvisions.Take("CopaExport",qty);int serial=I("Serial")+1;Set("Serial",serial);Set("ShipType"+serial,Food?1:0);Set("ShipQty"+serial,qty);int roll=UnityEngine.Random.Range(0,100),patrol=MercenaryContracts.PatrolRisk;int fate=roll<1?1:roll<2?2:roll<2+patrol/2?3:roll<2+patrol?4:0;Set("ShipFate"+serial,fate);Set("ShipDue"+serial,day+JourneyDays+(fate==2?UnityEngine.Random.Range(5,16):fate==3?UnityEngine.Random.Range(8,17):0));Set("ShipLoss"+serial,UnityEngine.Random.Range(1,4));}
 }
 for(int s=1;s<=I("Serial");s++){int due=I("ShipDue"+s);if(due==0||day<due)continue;Set("ShipDue"+s,0);int fate=I("ShipFate"+s),qty=I("ShipQty"+s);string cargo=qty+(I("ShipType"+s)>0?" provisions":" gold");string text;
 if(fate==1)text="A terrible storm destroys Copa's convoy carrying "+cargo+".";
 else if(fate==4){int lost=Math.Min(NavalState.EnemyShips,I("ShipLoss"+s));VariableSingleton.ChangeInt("iTurkShips",-lost);NavalState.Sync();text="Ottoman ships destroy Copa's convoy carrying "+cargo+". Its escort sinks "+lost+" Ottoman vessels.";}
 else {if(I("ShipType"+s)>0)SharedProvisions.Add(SharedProvisions.City,qty);else VariableSingleton.ChangeInt("iMoney",qty);text="Copa's trading convoy delivers "+cargo+(fate==2?" after being blown off course.":fate==3?" after evading Ottoman patrols.":".");}
 report.AppendLine(text);DecisionJournal.RecordAt(new DateTime((long)due*TimeSpan.TicksPerDay),"copa-convoy-"+s,"News from the Circassian coast",text);
 }
 return report.ToString();
 }
}
