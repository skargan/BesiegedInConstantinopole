using System;
using System.Collections.Generic;
using System.Linq;

// Observe public city accounts only. Enemy intelligence and private flags never enter this ledger.
public static class PalaceAdministration
{
    static readonly string[] Keys={"iMoney","iMaterials","iAmmunition","iSecrets","iInfantry","iArchers","iCavalry","iVarangians","iShips","iCannons","iEngineers","iHealers","iEnvoys"};
    static readonly string[] Names={"Gold","Materials","Ammunition","Secrets","Infantry","Archers","Cavalry","Varangian Guard","Warships","Cannons","Engineers","Monks","Envoys"};
    static Dictionary<string,int> balances;
    static double provisions;
    static readonly List<string> orders=new List<string>();
    public static void Reset(){balances=null;orders.Clear();}
    public static void Baseline(){balances=Keys.ToDictionary(k=>k,k=>DailyLifeEvents.I(k));provisions=SharedProvisions.Stock(SharedProvisions.City);orders.Clear();}
    public static void RecordOrder(string title,string detail){if(!VariableSingleton.IsInitialized)return;if(balances==null)Baseline();orders.Add(title+(string.IsNullOrEmpty(detail)?"":" — "+detail));Checkpoint();}
    public static void Checkpoint(){
        if(!VariableSingleton.IsInitialized)return;if(balances==null){Baseline();return;}
        var lines=new List<string>(orders);
        for(int i=0;i<Keys.Length;i++){
            long delta=(long)DailyLifeEvents.I(Keys[i])-balances[Keys[i]];
            if(delta!=0)lines.Add(Names[i]+": "+(delta>0?"+":"")+delta.ToString("N0"));
        }
        double food=SharedProvisions.Stock(SharedProvisions.City)-provisions;
        if(Math.Abs(food)>.0000005)lines.Add("Provisions: "+(food>0?"+":"")+SharedProvisions.Format(food));
        if(lines.Count>0)DecisionJournal.Record("administration-"+VariableSingleton.Turn+"-"+DecisionJournal.Entries.Count,"Palace accounts",string.Join("\n",lines));
        Baseline();
    }
    public static string Period(string stamp){
        if(!DateTime.TryParseExact(stamp,"d MMM yyyy",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var date))return stamp;
        if(date<new DateTime(1453,4,6)&&date.Day==1)return date.ToString("d MMMM",System.Globalization.CultureInfo.InvariantCulture)+" – "+date.AddMonths(1).ToString("d MMMM yyyy",System.Globalization.CultureInfo.InvariantCulture)+"  (Siege preparation)";
        return date.ToString("d MMMM yyyy",System.Globalization.CultureInfo.InvariantCulture)+(date>=new DateTime(1453,4,6)?"  · Siege day "+((date-new DateTime(1453,4,6)).Days+1):"  (Siege preparation)");
    }
}
