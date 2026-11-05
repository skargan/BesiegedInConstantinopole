using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Ragusa's special agreements share ordinary country goodwill and campaign persistence.
// Editable playtest prices/times are gameplay values, not historical measurements.
public static class RagusaDiplomacy
{
    const string Prefix="iDiplomacyRagusa";
    public const int FoodPrice=2500,FoodQuantity=500,FoodTravelDays=14,FoodStormTarget=-38;
    public const int ContractDays=90,ReturnDays=14,SigningGold=5000,VictoryGold=10000;
    public const int SigningRelations=5,VictoryRelations=10;
    public const string ExpeditionReturn="expeditionReturn";
    static readonly string[] Units={"iInfantry","iArchers","iCavalry"};
    static readonly int[] Contingent={500,500,200};
    static int I(string key)=>DailyLifeEvents.I(Prefix+key);
    static void Set(string key,int value)=>VariableSingleton.SetInt(Prefix+key,value);
    static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    static int Month(DateTime date)=>date.Year*12+date.Month;
    static DateTime CalendarDate(int day)=>new DateTime((long)day*TimeSpan.TicksPerDay);
    static string Date(int day)=>CalendarDate(day).ToString("d MMM yyyy");
    static int Country=>CountryDiplomacy.Index("ragusa");
    static void Ensure()
    {
        CountryDiplomacy.Ensure();
        if(I("Ready")!=0)return;
        Set("Ready",1);Set("Favour0",50);Set("Favour1",50);
    }
    public static int ActiveRepresentative=>(Month(VariableSingleton.GetDate())-Month(new DateTime(1452,1,1)))&1;
    public static string RepresentativeName(int representative)=>representative==0?"Johannes Gazulus":representative==1?"Benedetto Cotrugli":"Unknown representative";
    public static string ActiveRepresentativeName=>RepresentativeName(ActiveRepresentative);
    public static int Favour(int representative){Ensure();return representative>=0&&representative<=1?I("Favour"+representative):0;}
    public static void ChangeFavour(int representative,int points){Ensure();if(representative>=0&&representative<=1)Set("Favour"+representative,Mathf.Clamp(I("Favour"+representative)+points,0,100));}
    public static float Relations {get{Ensure();return CountryDiplomacy.Goodwill(Country);}}

    // A spoken refusal conceals a commercial agreement: no second acceptance roll.
    public static bool FoodAtSea=>I("FoodState")==1;
    public static string FoodRequestReason()
    {
        Ensure();
        if(I("FoodMonth")==Month(VariableSingleton.GetDate()))return "This month's provision arrangement has already been made.";
        if(FoodAtSea)return "The previous Ragusan provision shipment is still at sea.";
        if(DailyLifeEvents.I("iMoney")<FoodPrice)return "The arrangement requires "+FoodPrice.ToString("N0")+" gold.";
        return null;
    }
    public static bool RequestFood(out string report)
    {
        report=FoodRequestReason();if(report!=null)return false;
        int serial=I("FoodSerial")+1;Set("FoodSerial",serial);
        Set("FoodMonth",Month(VariableSingleton.GetDate()));Set("FoodState",1);Set("FoodDue",Day+FoodTravelDays);
        VariableSingleton.ChangeInt("iMoney",-FoodPrice);
        report=RagusaText.Get("food_denial_"+(ActiveRepresentative==0?"gazulus":"cotrugli"),"‘Ragusa cannot possibly provision a city under siege. The Republic's position is quite clear.’ "+ActiveRepresentativeName+" turns the paper over. ‘A merchant has, however, made a private arrangement. The freight is included.’")+"\n\n"+FoodPrice.ToString("N0")+" gold paid. A private ship carries "+FoodQuantity+" provisions; expected "+Date(I("FoodDue"))+". The transport does not join your fleet.";
        DecisionJournal.Record("ragusa-food-"+serial,"A private Ragusan arrangement",report,true);return true;
    }
    public static string FoodStatus
    {
        get {
            if(FoodAtSea)return FoodQuantity+" provisions at sea · expected "+Date(I("FoodDue"))+" ("+Math.Max(0,I("FoodDue")-Day)+" days).";
            if(I("FoodState")==2)return "The last private shipment delivered "+FoodQuantity+" provisions.";
            if(I("FoodState")==4)return "The last private shipment was lost at sea.";
            return "No private provision shipment is on its way.";
        }
    }
    // A dedicated storm target allows a private cargo and legacy state aid to coexist.
    public static string ApplyFoodStorm(int severity)
    {
        if(!FoodAtSea)return "No private Ragusan shipment is currently at sea.";
        string report;
        if(severity==1){Set("FoodDue",I("FoodDue")+10);report="Ragusa's private provision ship takes shelter. Its "+FoodQuantity+" provisions are delayed by 10 days; expected "+Date(I("FoodDue"))+".";}
        else {Set("FoodState",4);Set("FoodDue",0);report="Ragusa's private provision ship is lost in the storm. Its "+FoodQuantity+" provisions will not arrive.";}
        int storms=I("FoodStormSerial")+1;Set("FoodStormSerial",storms);
        DecisionJournal.Record("ragusa-food-storm-"+storms,"News of the Ragusan ship",report);return report;
    }

    public static bool ContractActive=>I("ContractState")==1||I("ContractState")==2;
    public static int CommittedTroops=>I("ContractState")==1?Contingent.Sum():0;
    public static int PeopleAbroad=>I("ContractState")==1?Contingent.Sum():I("ContractState")==2?I("Survivors0")+I("Survivors1")+I("Survivors2"):0;
    public static string ContractReason()
    {
        Ensure();
        if(ContractActive)return "The previous contingent has not yet returned.";
        for(int n=0;n<Units.Length;n++)if(DailyLifeEvents.I(Units[n])<Contingent[n])return "The contract requires 500 available infantry, 500 archers and 200 cavalry.";
        return null;
    }
    public static bool StartContract(out string report)
    {
        report=ContractReason();if(report!=null)return false;
        int serial=I("ContractSerial")+1;Set("ContractSerial",serial);Set("ContractState",1);
        Set("ContractDue",Day+ContractDays);Set("ContractRepresentative",ActiveRepresentative);Set("ReturnDue",0);Set("Victory",0);
        for(int n=0;n<Units.Length;n++) {
            // Deployment is a transfer, not a casualty. ChangeInt would record it as a death.
            VariableSingleton.SetInt(Units[n],DailyLifeEvents.I(Units[n])-Contingent[n]);Set("Survivors"+n,Contingent[n]);Set("Losses"+n,0);
        }
        VariableSingleton.ChangeInt("iMoney",SigningGold);CountryDiplomacy.ChangeGoodwill(Country,SigningRelations);ChangeFavour(ActiveRepresentative,SigningRelations);
        report=RagusaText.Get("war_contract_"+(ActiveRepresentative==0?"gazulus":"cotrugli"),"Your secretary reads the service agreement twice. The Ragusan representative waits, with the patience of a man who has already had his copy read three times.")+"\n\n500 infantry, 500 archers and 200 cavalry leave the city's service for a ninety-day Ragusan contract. You receive "+SigningGold.ToString("N0")+" gold; relations with Ragusa and "+ActiveRepresentativeName+"'s favour each improve by "+SigningRelations+". News is expected "+Date(I("ContractDue"))+". Survivors then need fourteen days to return.";
        DecisionJournal.Record("ragusa-contract-"+serial,"Troops hired to Ragusa",report,true);return true;
    }
    public static string ContractStatus
    {
        get {
            if(I("ContractState")==1)return "On contract: 500 infantry · 500 archers · 200 cavalry. Report expected "+Date(I("ContractDue"))+".";
            if(I("ContractState")==2)return (I("Victory")==1?"Victory":"Defeat")+" · Returning: "+I("Survivors0")+" infantry · "+I("Survivors1")+" archers · "+I("Survivors2")+" cavalry. Expected "+Date(I("ReturnDue"))+".";
            if(I("ContractState")==3)return "The last contingent has returned. Losses: "+I("Losses0")+" infantry · "+I("Losses1")+" archers · "+I("Losses2")+" cavalry.";
            return "No Ragusan service contract is active.";
        }
    }
    // Separate from UI reads: advance only from the campaign's calendar loop.
    public static string Advance(DateTime date)
    {
        int today=TrainingPool.Day(date);var reports=new List<string>();
        if(FoodAtSea&&I("FoodDue")<=today) {
            int arrivalDay=I("FoodDue");
            Set("FoodState",2);Set("FoodDue",0);DeliverFood(FoodQuantity);
            string report=RagusaText.Get("food_albanian_arrival","A private ship from Ragusa arrives. Its papers mention no agreement with the Republic; its master is equally firm about being paid already.")+"\n\n"+FoodQuantity+" provisions unloaded. No transport ships are added to your fleet.";
            reports.Add(report);DecisionJournal.RecordAt(CalendarDate(arrivalDay),"ragusa-food-arrival-"+I("FoodSerial"),"Ragusan provisions arrive",report);
        }
        if(I("ContractState")==1&&I("ContractDue")<=today) {
            // One campaign RNG roll chooses the result; each troop type then rolls its own percentage loss.
            bool victory=UnityEngine.Random.Range(0,2)==0;Set("Victory",victory?1:0);
            int due=I("ContractDue")+ReturnDays;Set("ReturnDue",due);Set("ContractState",2);
            for(int n=0;n<Units.Length;n++) {
                int percent=UnityEngine.Random.Range(1,victory?21:51);
                int lost=(Contingent[n]*percent+99)/100,survived=Contingent[n]-lost;
                Set("Losses"+n,lost);Set("Survivors"+n,survived);RecruitmentLogistics.RecordLoss(Units[n],lost);Achievements.TroopLoss(Units[n],lost);
                TrainingPool.Orders.Add(new TrainingPool.Order{unit=Units[n],quantity=survived,readyDay=due,kind=ExpeditionReturn,accountingVersion=1});
            }
            if(victory){VariableSingleton.ChangeInt("iMoney",VictoryGold);CountryDiplomacy.ChangeGoodwill(Country,VictoryRelations);ChangeFavour(I("ContractRepresentative"),VictoryRelations);}
            string report=RagusaText.Get(victory?"war_day90_victory":"war_day90_defeat",victory?"Ragusa reports a victory.":"Ragusa reports a defeat.")+"\n\n"+(victory?"The final payment is "+VictoryGold.ToString("N0")+" gold; Ragusan relations and the contracting representative's favour improve by "+VictoryRelations+".":"There is no victory payment.")+" Losses: "+I("Losses0")+" infantry, "+I("Losses1")+" archers and "+I("Losses2")+" cavalry. Survivors are expected "+Date(due)+".";
            reports.Add(report);DecisionJournal.RecordAt(CalendarDate(I("ContractDue")),"ragusa-contract-result-"+I("ContractSerial"),"News of the Ragusan contingent",report);
        }
        // TrainingPool.CompleteDueOrders runs before this hook each calendar day.
        if(I("ContractState")==2&&I("ReturnDue")<=today&&!TrainingPool.Orders.Any(o=>o!=null&&o.kind==ExpeditionReturn&&o.readyDay==I("ReturnDue"))) {
            Set("ContractState",3);string report=RagusaText.Get(I("Victory")==1?"war_day104_victory_return":"war_day104_defeat_return","The surviving Ragusan contingent is back in the city's service.")+"\n\nReturned: "+I("Survivors0")+" infantry, "+I("Survivors1")+" archers and "+I("Survivors2")+" cavalry.";
            reports.Add(report);DecisionJournal.RecordAt(CalendarDate(I("ReturnDue")),"ragusa-contract-return-"+I("ContractSerial"),"The contingent returns",report);
        }
        return reports.Count==0?"":string.Join("\n\n",reports)+"\n\n";
    }
    // One delayed cargo transaction through the campaign's shared physical stock.
    // Existing fractional provisions are preserved; transports never become warships.
    static void DeliverFood(int quantity)=>SharedProvisions.Add(SharedProvisions.City,quantity);
}
