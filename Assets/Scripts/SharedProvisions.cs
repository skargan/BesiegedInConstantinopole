using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Yarn.Unity;

// One physical food unit feeds 1,000 people for one day. Persist millionths in the
// existing campaign dictionary; whole-value legacy callers are adapters, not a second stock.
public static class SharedProvisions
{
    public const string City="iProvisions", Camp="iFood";
    const long Scale=1000000;
    static readonly string[] Land={"iBalkans","iBazouks","iJanissaries"};
    static string Whole(string key)=>key==City||key==Camp?key:"iProvisionStock_"+key;
    static string Part(string key)=>"iProvisionPart_"+key;
    static int I(string key)=>DailyLifeEvents.I(key);
    public static long OttomanPeople=>Land.Sum(k=>(long)Math.Max(0,I(k)));
    // City population is inclusive of the garrison. Recruiting residents does not add mouths.
    public static long CityPeople=>Math.Max(PopulationAccounting.People,(long)RecruitmentLogistics.Serving+RecruitmentLogistics.Queued+Math.Max(0,I("iVarangians"))+MedicalRecovery.RegularWounded);
    public static double CityDemand=>Math.Max(0,CityPeople-RagusaDiplomacy.PeopleAbroad)/1000d;
    public static double OttomanDemand=>OttomanPeople/1000d;
    public static double ReferenceDemand {get {Ensure();return I("iProvisionReferencePeople")/1000d;}}
    public static string Format(double amount)=>amount.ToString("N3",CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
    public static void Ensure()
    {
        if(!VariableSingleton.IsInitialized||I("iProvisionModel")>0)return;
        int oldDays=Math.Max(0,I(Camp));
        VariableSingleton.SetInt("iProvisionModel",1);
        VariableSingleton.SetInt("iProvisionReferencePeople",(int)Math.Min(int.MaxValue,OttomanPeople));
        Set(Camp,oldDays*OttomanDemand);
    }
    public static void ClearFraction(string key){if(key==City||key==Camp)VariableSingleton.SetInt(Part(key),0);}
    public static void ResetForRestore()
    {
        foreach(var pair in VariableSingleton.Instance.CaptureState().ints)
            if(pair.key.StartsWith("iProvision")&&pair.key!=City)VariableSingleton.SetInt(pair.key,0);
        VariableSingleton.SetInt("iHungerDays",0);VariableSingleton.SetInt("iFoodShortfall",0);
    }
    static long Units(string key)=>Math.Max(0,(long)I(Whole(key))*Scale+I(Part(key)));
    public static double Stock(string key){if(key==Camp)Ensure();return Units(key)/(double)Scale;}
    static void Write(string key,long units)
    {
        units=Math.Max(0,Math.Min((long)int.MaxValue*Scale,units));
        VariableSingleton.SetInt(Whole(key),(int)(units/Scale));
        VariableSingleton.SetInt(Part(key),(int)(units%Scale));
    }
    static long Quantise(double amount)
    {
        if(double.IsNaN(amount)||double.IsInfinity(amount))throw new ArgumentOutOfRangeException(nameof(amount));
        return (long)Math.Round(Math.Max(-int.MaxValue,Math.Min(int.MaxValue,amount))*Scale,MidpointRounding.AwayFromZero);
    }
    public static void Set(string key,double amount)=>Write(key,Quantise(amount));
    public static void Add(string key,double amount){if(key==Camp)Ensure();Write(key,Units(key)+Quantise(amount));}
    public static double Take(string key,double amount)
    {
        if(key==Camp)Ensure();long take=Math.Min(Units(key),Math.Max(0,Quantise(amount)));Write(key,Units(key)-take);return take/(double)Scale;
    }
    public static double Transfer(string from,string to,double amount)
    {
        if(from==to)return 0;double taken=Take(from,amount);if(to!=null)Add(to,taken);return taken;
    }
    public static double CityDays=>CityDemand>0?Stock(City)/CityDemand:0;
    public static double CampDays=>OttomanDemand>0?Stock(Camp)/OttomanDemand:0;
    [YarnCommand("EnemyFoodDays")]
    public static void ChangeEnemyFoodDays(int days)=>Add(Camp,days*ReferenceDemand);
    static bool ClaimDay(string key)
    {
        int day=TrainingPool.Day(VariableSingleton.GetDate());if(I(key)==day)return false;VariableSingleton.SetInt(key,day);return true;
    }
    public static string ConsumeCityDay()
    {
        if(!ClaimDay("iProvisionCityDay"))return "";
        double need=CityDemand,used=Take(City,need),missing=Math.Max(0,need-used);
        int days=missing>.0000005?I("iHungerDays")+1:0;VariableSingleton.SetInt("iHungerDays",days);
        Set("cityShortfall",missing);VariableSingleton.SetInt("iFoodShortfall",(int)Math.Ceiling(missing));
        double penalty=need>0?days*missing/need:0;int others=(int)Math.Floor(penalty/2+1e-10);
        VariableSingleton.ChangePercentFloat("fPeopleRep",-(float)(penalty/100));
        foreach(var key in new[]{"fTroopRep","fPriestRep","fNobleRep"})VariableSingleton.ChangePercentFloat(key,-others/100f);
        Set("cityPenalty",penalty);VariableSingleton.SetInt("iProvisionCommunityPenalty",others);
        return "Daily provisions: "+Format(used)+" consumed; "+Format(Stock(City))+" remain. Demand: "+Format(need)+". "+
            (days>0?"Shortage day "+days+": public morale −"+Format(penalty)+" points; army morale, clergy and noble support −"+others+" each.":Math.Floor(CityDays)+" full days of food remain.")+"\n\n";
    }
    public static string ConsumeOttomanDay()
    {
        Ensure();if(!ClaimDay("iProvisionOttomanDay"))return "";
        double need=OttomanDemand,used=Take(Camp,need),missing=Math.Max(0,need-used);
        int days=missing>.0000005?I("iProvisionOttomanHungerDays")+1:0;
        VariableSingleton.SetInt("iProvisionOttomanHungerDays",days);Set("ottomanShortfall",missing);
        double exact=Stock("deserterRemainder")+(need>0?100d*days*missing/need:0);
        int losses=(int)Math.Min(OttomanPeople,Math.Floor(exact+1e-10));
        Set("deserterRemainder",OttomanPeople>losses?exact-Math.Floor(exact+1e-10):0);
        long total=OttomanPeople;var split=Land.Select(k=>new {Key=k,Count=Math.Max(0,I(k)),Exact=total>0?(decimal)Math.Max(0,I(k))*losses/total:0}).ToArray();
        var allocated=split.Select(x=>(int)Math.Floor(x.Exact)).ToArray();int remaining=losses-allocated.Sum();
        foreach(int index in Enumerable.Range(0,split.Length).OrderByDescending(i=>split[i].Exact-allocated[i]).ThenBy(i=>i))
            if(remaining>0&&allocated[index]<split[index].Count){allocated[index]++;remaining--;}
        // Deserters leave the roster alive; they are not Ottoman deaths for achievements.
        for(int i=0;i<Land.Length;i++)if(allocated[i]>0)VariableSingleton.SetInt(Land[i],Math.Max(0,I(Land[i])-allocated[i]));
        VariableSingleton.SetInt("iProvisionDeserters",losses);
        if(days==0)return "";
        if(I("iSpies")<4)return "Ottoman foraging parties leave the camp. Hunger interrupts today's operation; the full losses are unknown.";
        return "Ottoman food shortage, day "+days+". "+losses.ToString("N0")+" soldiers desert. Demand before desertions: "+Format(need)+" provisions; unmet: "+Format(missing)+".";
    }
    public static string CityReport=>Format(Stock(City))+" provisions · "+Format(CityDemand)+" / day · shortage "+I("iHungerDays")+" days";
    public static string CampStockReport=>Format(Stock(Camp))+" provisions"+(I("iSpies")>=4?" · "+Format(CampDays)+" days at current strength":"");
    public static string CampConsumptionReport=>Format(OttomanDemand)+" provisions / day · shortage "+I("iProvisionOttomanHungerDays")+" days · deserters today "+I("iProvisionDeserters").ToString("N0");
}
