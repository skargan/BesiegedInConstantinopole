using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// The existing iWbw save/reset boundary owns all state, including pending phase rolls.
// No sidecar, second garrison or independently ticking intervention simulation.
public static class ProvincialWar
{
    public enum Order { Gather,SeekBattle,Capture,Skirmish,Pillage,Hold }
    public const int Ottoman=0,Imperial=1,Chaos=2,AllyOwnerOffset=3;
    public static ProvincialWarRules Rules=>ProvincialWarRules.Data;
    public static int I(string k)=>DailyLifeEvents.I("iWbw37"+k);
    public static void Put(string k,int v)=>VariableSingleton.SetInt("iWbw37"+k,v);
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static bool Ready=>I("Ready")>0;
    public static int Roll(int a,int b)=>UnityEngine.Random.Range(a,b+1);
    public sealed class Province
    {
        public readonly string Id;
        public Province(string id){Id=WarTheatre.CanonicalProvince(id);}
        public ProvincialWarRules.Province Config=>Rules.Provinces.First(x=>x.Id==Id);
        public int Get(string k)=>I(Id+"_"+k);
        public void Set(string k,int v)=>Put(Id+"_"+k,v);
        public int Owner {get=>Get("Owner");set=>Set("Owner",value);}
        public int Garrison {get=>Get("Garrison");set=>Set("Garrison",Mathf.Max(0,value));}
        public int Wealth {get=>Ready?Get("Wealth"):100;set=>Set("Wealth",Mathf.Clamp(value,0,100));}
        public int Resistance {get=>ProvinceCampaign.Get("R_"+Id);set=>ProvinceCampaign.Set("R_"+Id,Mathf.Clamp(value,0,100));}
        public int Authority {get=>Mathf.Clamp(100-DailyLifeEvents.I("iWbwControlLoss_"+Id),0,Ceiling);set=>VariableSingleton.SetInt("iWbwControlLoss_"+Id,100-Mathf.Clamp(value,0,Ceiling));}
        public int TotalControl=>Armies.Sum(a=>a.Control);
        public int Ceiling=>Mathf.Max(0,10000-TotalControl)/100;
        public IEnumerable<Army> Armies=>Enumerable.Range(0,CountryDiplomacy.Definitions.Length+1).Select(i=>new Army(this,i));
        public IEnumerable<Army> Present=>Armies.Where(a=>a.Troops>0);
        public Army Rebels=>new Army(this,0);
        public bool Pending=>Get("Settlement")>0||Get("BattleStage")>0||Get("Continuation")>0||Get("Assistance")>0;
        public bool Known=>ProvinceCampaign.Agent(Id)==2;
        public bool Hostile=>Owner==Ottoman;
        public bool Protected=>Get("MusterUntil")>I("LastDay")&&Rebels.Troops>0;
        public string Name=>ProvinceCampaign.Name(Id);
    }
    public sealed class Army
    {
        public readonly Province Province;public readonly int Slot;
        public Army(Province p,int slot){Province=p;Slot=slot;}
        public int Country=>Slot-1;
        public bool Rebel=>Slot==0;
        int Get(string key)=>Province.Get("A"+Slot+"_"+key);
        void Set(string key,int v)=>Province.Set("A"+Slot+"_"+key,v);
        public int Troops {get=>Get("Troops");set=>Set("Troops",Mathf.Max(0,value));}
        public int Control {get=>Get("Control");set=>Set("Control",Mathf.Clamp(value,0,10000));}
        public Order Stance {get=>(Order)Get("Order");set=>Set("Order",(int)value);}
        public string Name=>Rebel?"Local rebels":CountryDiplomacy.Definitions[Country].Name;
        public ProvincialWarRules.Ally Definition=>Rebel?null:Rules.Allies.FirstOrDefault(x=>x.Id==CountryDiplomacy.Definitions[Country].Id);
        public int Cap=>Rebel?Province.Config.RebelCap:Definition?.Cap??Troops;
        public bool Protected=>Rebel&&Province.Protected;
        public bool Paused=>!Rebel&&(I("Peace"+Country)>0||I("Pause"+Country)>I("LastDay"));
        public bool Offensive=>Stance>=Order.SeekBattle&&Stance<=Order.Pillage;
    }
    public static IEnumerable<Province> Provinces=>WarTheatre.ProvinceIds.Select(id=>new Province(id));
    public static Province Find(string id)=>new Province(id);
    public static Province Pending=>Provinces.FirstOrDefault(p=>p.Pending);
    public static void Ensure()
    {
        if(Ready||!VariableSingleton.HasSiegeStarted())return;
        bool legacy=ProvinceCampaign.Get("Ready")>0;
        Put("Ready",1);Put("LastDay",Day);Put("SupplyTick",VariableSingleton.CompletedSiegeDays-1);
        ProvinceCampaign.Set("Ready",1);ProvinceCampaign.Set("LastDay",Day);
        if(!legacy&&VariableSingleton.CompletedSiegeDays==0){VariableSingleton.SetInt("iFood",Mathf.Max(0,90-ProvinceCampaign.Get("PreparationFoodLoss")));VariableSingleton.SetInt("iTurkCannons",Mathf.Max(0,12-ProvinceCampaign.Get("PreparationGunLoss")));}
        foreach(var p in Provinces)
        {
            p.Garrison=p.Config.Garrison;p.Wealth=100;p.Set("NextShipment",Day+Rules.ShipmentInterval);
            if(DailyLifeEvents.I("iWbwCaptured_"+p.Id)>0){p.Owner=Imperial;p.Garrison=0;p.Authority=0;}
            if(ProvinceCampaign.Get("Battle_"+p.Id)>0){p.Set("Launched",1);p.Rebels.Troops=Rules.LaunchTroops;}
            if(DailyLifeEvents.I("iWbwLedger_"+p.Id)>0||(p.Id=="macedonia"&&DailyLifeEvents.I("iWbwLedger")>0))p.Set("Plans",1);
        }
        if(ProvincialIntelligence.Assigned>0&&!Provinces.Any(p=>ProvinceCampaign.Agent(p.Id)>0)){
            string id=ProvincialIntelligence.ProvinceId;int state=(int)ProvincialIntelligence.State;
            ProvinceCampaign.Set("Agent_"+id,state==4?3:state==1?1:state==3?4:2);ProvinceCampaign.Set("Arrival_"+id,Mathf.Max(Day,ProvincialIntelligence.I("Due")));
        }
        if(ProvincialIntelligence.State==ProvincialIntelligence.CellState.Operating)ProvinceCampaign.Set("LegacyPending",1);
        else {VariableSingleton.SetInt("iWbwState",0);ProvinceCampaign.Set("LegacyPending",0);}
    }
    public static string LaunchReason(Province p)=>!Ready?"Available during the siege":!p.Hostile?"Province is not Ottoman-held":p.Pending?"Resolve the provincial event first":p.Get("Launched")>0?"This province has already raised its founding army":p.Resistance<Rules.LaunchResistance?"Requires "+Rules.LaunchResistance+" Resistance":null;
    public static bool Launch(string id,out string report)
    {
        Ensure();var p=Find(id);report=LaunchReason(p);if(report!=null)return false;
        p.Set("Launched",1);p.Rebels.Troops=Rules.LaunchTroops;p.Rebels.Stance=Order.Gather;
        p.Set("MusterUntil",I("LastDay")+Roll(Rules.MusterMin,Rules.MusterMax));
        report=p.Name+": "+Rules.LaunchTroops.ToString("N0")+" rebels begin a hidden muster. Offensive orders end concealment immediately.";return true;
    }
    public static string OrderReason(Army a,Order order)=>!a.Province.Hostile?"The province is not contested":a.Province.Pending?"Resolve the provincial event first":a.Troops<=0?"No troops remain":a.Paused?"The expedition is observing a cessation or separate peace":order>=Order.SeekBattle&&order<=Order.Pillage&&a.Troops<Rules.MinimumArmy?"Requires "+Rules.MinimumArmy.ToString("N0")+" troops":order==Order.Pillage&&a.Rebel&&a.Province.Resistance<1?"Pillaging requires Resistance to spend":null;
    public static bool GiveOrder(Army a,Order order,out string report)
    {
        report=OrderReason(a,order);if(report!=null)return false;a.Stance=order;
        if(a.Rebel&&a.Offensive)a.Province.Set("MusterUntil",0);
        report=a.Name+": "+order+". Standing orders continue until changed or no longer possible.";return true;
    }
    public static bool Plans(string id,out string report)
    {
        var p=Find(id);report=p.Get("Plans")>0?"This province's plans have already yielded their secrets":!p.Known?"Requires a resident agent":!p.Hostile?"The province is not Ottoman-held":CouncilActions.Int("iMoney")<Rules.PlanCost?"Insufficient gold":null;
        if(report!=null)return false;p.Set("Plans",1);VariableSingleton.ChangeInt("iMoney",-Rules.PlanCost);VariableSingleton.ChangeInt("iSecrets",Rules.SecretsAward);
        report="Your agent acquires the enemy plans. +"+Rules.SecretsAward+" secrets. This provincial intelligence award cannot be collected again.";return true;
    }
    public static void Gain(Army a,int hundredths){a.Control+=Mathf.Min(Mathf.Max(0,hundredths),10000-a.Province.TotalControl);a.Province.Authority=a.Province.Authority;}
    public static void Reclaim(Province p,int hundredths)
    {
        int remaining=hundredths;foreach(var a in p.Armies.Where(x=>!x.Protected).OrderByDescending(x=>x.Control).ThenBy(x=>x.Slot)){int take=Mathf.Min(remaining,a.Control);a.Control-=take;remaining-=take;if(remaining<=0)break;}
    }
    public static void Disrupt(Province p)=>p.Set("QuietDays",0);
    public static void Collapse(Province p)
    {
        if(!p.Hostile||p.Get("Settlement")>0||p.Get("BattleStage")>0)return;
        if(p.Authority>0&&p.Garrison>0)return;
        p.Set("Settlement",1);p.Set("SettlementRoll",Roll(0,99));
    }
    public static Army QualifyingAlly(Province p)
    {
        var ranked=p.Armies.Where(a=>!a.Rebel&&a.Control>Rules.AllyClaimThreshold*100).OrderByDescending(a=>a.Control).ToArray();
        return ranked.Length==0||(ranked.Length>1&&ranked[0].Control==ranked[1].Control)?null:ranked[0];
    }
    public static bool Settle(Province p,int choice,out string report)
    {
        report="No settlement pending.";if(p.Get("Settlement")!=1||choice<0||choice>2)return false;
        var ally=QualifyingAlly(p);if(choice==1&&ally==null){report="No ally has an uncontested claim above 10%.";return false;}
        if(choice==0&&CouncilActions.Int("iMoney")<Rules.RestoreCost){report="Insufficient gold.";return false;}
        if(choice==0)VariableSingleton.ChangeInt("iMoney",-Rules.RestoreCost);
        int chance=choice==0?(p.Resistance>=Rules.StrongResistance?Rules.StrongRestoreChance:Rules.RestoreChance):choice==1?Rules.EndorseChance:Rules.LocalChance;
        bool success=p.Get("SettlementRoll")<chance;p.Set("Settlement",0);
        ChangeOwner(p,success?(choice==1?ally.Country+AllyOwnerOffset:Imperial):Chaos);
        if(success){p.Set("Withdrawn",p.Get("Withdrawn")+p.Garrison);p.Garrison=0;p.Authority=0;}
        if(!success){foreach(var a in p.Armies)a.Control=0;p.Set("ChaosUntil",I("LastDay")+Rules.ChaosDays);}
        if(success&&choice==1){p.Set("Continuation",1);p.Set("ContinuationRoll",Roll(0,99));}
        report=success?(choice==1?ally.Name+" assumes control of "+p.Name+".":p.Name+" acknowledges imperial rule."):p.Name+" falls into chaos for "+Rules.ChaosDays+" days. Exports and campaigning cease.";
        p.Set("SettlementResult",success?choice+1:4);return true;
    }
    static void ChangeOwner(Province p,int owner)
    {
        p.Owner=owner;VariableSingleton.SetInt("iWbwCaptured_"+p.Id,owner==Imperial?1:0);
        for(int i=0;i<7;i++)p.Set("Remainder"+i,0);
        p.Set("NextShipment",I("LastDay")+Rules.ShipmentInterval);
        p.Set("Assistance",0);p.Set("AssistanceAfter",I("LastDay")+Rules.AssistanceInterval);
    }
    public static string Advance(DateTime date)
    {
        Ensure();if(!Ready)return "";int end=TrainingPool.Day(date);var report=new StringBuilder();
        for(int day=I("LastDay")+1;day<=end;day++)
        {
            if(Pending!=null)break; // Never age a pending battle into another calendar day.
            Put("LastDay",day);Mobilise(day,report);
            if(ProvinceCampaign.Get("LegacyPending")>0){string id=ProvincialIntelligence.ProvinceId;report.Append(ProvincialIntelligence.Advance(new DateTime((long)day*TimeSpan.TicksPerDay)));if(ProvincialIntelligence.State!=ProvincialIntelligence.CellState.Operating){ProvinceCampaign.Set("Agent_"+id,ProvincialIntelligence.State==ProvincialIntelligence.CellState.Lost?0:2);Find(id).Set("Plans",ProvincialIntelligence.Ledger>0?1:0);ProvinceCampaign.Set("LegacyPending",0);VariableSingleton.SetInt("iWbwState",0);}}
            foreach(var p in Provinces)
            {
                AdvanceAgent(p,day,report);
                if(p.Owner==Chaos){if(day>=p.Get("ChaosUntil")){ChangeOwner(p,Ottoman);p.Authority=25;p.Set("QuietDays",0);report.AppendLine(p.Name+": Ottoman administration returns; surviving forces resume operations.");}else continue;}
                if(p.Owner>=AllyOwnerOffset){int country=p.Owner-AllyOwnerOffset;if(I("Pause"+country)>0&&I("Pause"+country)<=day){Put("Pause"+country,0);MoveOn(p,country,report);}}
                if(p.Owner==Imperial&&day>=p.Get("AssistanceAfter")){p.Set("AssistanceAfter",day+Rules.AssistanceInterval);if(Roll(0,99)<Rules.AssistanceChance){p.Set("Assistance",1);p.Set("AssistanceRoll",Roll(0,99));}}
                if(p.Hostile&&!p.Pending)TickConflict(p,day,report);
                p.Wealth+=Rules.WealthRecovery;
                if(p.Owner!=Chaos&&!p.Pending)Dispatch(p,day);
            }
            Arrive(day,report);
        }
        return report.Length==0?"":"\nWar Beyond the Walls\n"+report;
    }
    static void AdvanceAgent(Province p,int day,StringBuilder report)
    {
        int state=ProvinceCampaign.Agent(p.Id);
        if((state==1||state==3)&&ProvinceCampaign.Get("Arrival_"+p.Id)<=day){ProvinceCampaign.Set("Agent_"+p.Id,state==1?2:0);if(state==3)VariableSingleton.ChangeInt("iSpies",1);report.AppendLine(state==1?"Your agent reaches "+p.Name+".":"Your agent returns from "+p.Name+".");}
        if(p.Known&&p.Hostile&&!p.Pending){if(ProvinceCampaign.Get("Mode_"+p.Id)==0)p.Authority-=Roll(Rules.ClandestineMin,Rules.ClandestineMax);else p.Resistance+=Roll(Rules.ClandestineMin,Rules.ClandestineMax);Collapse(p);}
    }
    static void Mobilise(int day,StringBuilder report)
    {
        foreach(var d in Rules.Allies)
        {
            int c=CountryDiplomacy.Index(d.Id);bool ready=CountryDiplomacy.IsIntervening(c)&&CountryDiplomacy.MobilisationDeadline(c)<=day;
            if(d.Id=="morea"&&VariableSingleton.GetBoolVariable("bMoreanDistraction"))ready=true;
            if(!ready||I("Committed"+c)>0)continue;
            var target=d.Targets.Select(Find).FirstOrDefault(p=>p.Hostile&&!p.Pending);if(target==null)continue;
            var a=new Army(target,c+1);a.Troops=d.Troops;a.Stance=Order.Capture;Put("Committed"+c,1);Put("Reserve"+c,d.Reserve);
            report.AppendLine(a.Name+" commits "+a.Troops.ToString("N0")+" troops to "+target.Name+".");
        }
    }
    static void TickConflict(Province p,int day,StringBuilder report)
    {
        bool disturbed=false;
        if(!Rules.PauseGarrisonDuringUprising||p.Rebels.Troops==0)p.Garrison=Mathf.Min(p.Config.Garrison,p.Garrison+p.Config.Replacement);
        foreach(var a in p.Present.ToArray())
        {
            if(a.Paused)continue;
            if(a.Troops<Rules.MinimumArmy&&a.Offensive)a.Stance=Order.Gather;
            if(a.Stance==Order.Gather&&a.Troops<a.Cap)
            {
                if(a.Rebel&&p.Resistance>0){a.Troops+=Mathf.Min(Rules.RecruitsPerResistance,a.Cap-a.Troops);p.Resistance--;}
                else if(!a.Rebel&&a.Definition!=null){int n=Mathf.Min(a.Definition.DailyReplacement,Mathf.Min(a.Cap-a.Troops,I("Reserve"+a.Country)));a.Troops+=n;Put("Reserve"+a.Country,I("Reserve"+a.Country)-n);}
            }
            else if(a.Stance==Order.Capture){int before=p.TotalControl;Gain(a,Roll(Rules.TerritoryMin,Rules.TerritoryMax)*100);disturbed|=p.TotalControl>before;}
            else if(a.Stance==Order.Skirmish){p.Authority--;a.Troops-=Mathf.Min(a.Troops,Roll(Rules.SkirmishMin,Rules.SkirmishMax));disturbed=true;}
            else if(a.Stance==Order.Pillage){if(a.Rebel&&p.Resistance<1){a.Stance=Order.Hold;continue;}int loss=Mathf.Min(p.Wealth,Roll(Rules.PillageMin,Rules.PillageMax));p.Wealth-=loss;p.Authority--;if(a.Rebel){p.Resistance--;VariableSingleton.ChangeInt("iMoney",loss*Rules.PillageGold);}disturbed=true;}
        }
        Collapse(p);if(p.Pending)return;
        var exposed=p.Present.Where(a=>!a.Protected&&!a.Paused).ToArray();var largest=exposed.OrderByDescending(a=>a.Troops).FirstOrDefault();
        bool seek=largest!=null&&(p.Garrison*100L>=largest.Troops*(long)Rules.OttomanSeekStrong||(p.Garrison*100L>=largest.Troops*(long)Rules.OttomanSeekWeak&&p.Authority<Rules.OttomanLowAuthority));
        bool recover=!seek&&(largest==null||p.Garrison*100L>=largest.Troops*(long)Rules.OttomanSeekWeak)&&p.TotalControl>0;
        p.Set("OttomanOrder",seek?1:recover?2:0);
        if(recover&&p.Garrison>=Rules.MinimumArmy)Reclaim(p,Roll(Rules.TerritoryMin,Rules.TerritoryMax)*100);
        if(disturbed)Disrupt(p);else {p.Set("QuietDays",p.Get("QuietDays")+1);if(p.Get("QuietDays")>=Rules.AuthorityRecoveryDays){p.Authority++;p.Set("QuietDays",0);}}
        bool playerSeek=exposed.Any(a=>a.Stance==Order.SeekBattle&&a.Troops>=Rules.MinimumArmy);
        if(day>=p.Get("BattleAfter")&&(seek||playerSeek)&&Roll(0,99)<Rules.BattleChance)
        {
            var lead=seek?exposed.OrderByDescending(a=>a.Offensive).ThenByDescending(a=>a.Troops).ThenBy(a=>a.Slot).First():exposed.Where(a=>a.Stance==Order.SeekBattle&&a.Troops>=Rules.MinimumArmy).OrderByDescending(a=>a.Troops).First();
            ProvincialBattle.Begin(p,lead.Slot,seek);report.AppendLine("Battle in "+p.Name+". Your commanders await instructions.");
        }
    }
    public static string ContinueAlly(Province p)
    {
        if(p.Get("Continuation")<=0)return "";var report=new StringBuilder();int day=I("LastDay");p.Set("Continuation",0);int c=p.Owner-AllyOwnerOffset;if(c<0)return "";int roll=p.Get("ContinuationRoll");
        if(roll<Rules.CessationChance){Put("Pause"+c,day+Rules.CessationDays);report.AppendLine(CountryDiplomacy.Definitions[c].Name+" pauses campaigning for "+Rules.CessationDays+" days.");}
        else if(roll<Rules.CessationChance+Rules.PeaceChance){Put("Peace"+c,1);report.AppendLine(CountryDiplomacy.Definitions[c].Name+" makes a separate peace and retains "+p.Name+".");}
        else MoveOn(p,c,report);
        return report.ToString();
    }
    public static string Assistance(Province p,bool pay)
    {
        if(p.Get("Assistance")<=0)return "";if(pay&&CouncilActions.Int("iMoney")<Rules.AssistanceCost)return "Insufficient gold.";
        p.Set("Assistance",0);if(pay){VariableSingleton.ChangeInt("iMoney",-Rules.AssistanceCost);p.Wealth+=Rules.AssistanceWealth;return "Imperial funds restore local workshops and reassure the magistrates. Wealth improves.";}
        p.Wealth-=Rules.AssistanceWealth;if(p.Get("AssistanceRoll")<Rules.AssistanceChaosChance){ChangeOwner(p,Chaos);foreach(var a in p.Armies)a.Control=0;p.Set("ChaosUntil",I("LastDay")+Rules.ChaosDays);return "Without support, the magistrates abandon their posts. The province falls into chaos.";}
        return "The magistrates find their own means, but workshops close and provincial Wealth falls.";
    }
    static void MoveOn(Province p,int country,StringBuilder report)
    {
        var army=new Army(p,country+1);if(army.Troops<=0)return;
        var next=Provinces.FirstOrDefault(q=>q.Hostile&&!q.Pending&&ProvinceCampaign.Neighbour(p.Id,q.Id));
        if(next==null){army.Stance=Order.Hold;report.AppendLine(army.Name+" holds "+p.Name+": no accessible neighbouring Ottoman province.");return;}
        var arriving=new Army(next,army.Slot);if(arriving.Troops+army.Troops>arriving.Cap){army.Stance=Order.Hold;report.AppendLine(army.Name+" holds: the neighbouring expedition is at its commitment limit.");return;}arriving.Troops+=army.Troops;arriving.Stance=Order.Capture;army.Troops=0;
        report.AppendLine(arriving.Name+" advances from "+p.Name+" into neighbouring "+next.Name+".");
    }
    static int Output(Province p,int index,int quantity)
    {
        int total=p.Get("Remainder"+index)+quantity*p.Wealth;p.Set("Remainder"+index,total%100);return total/100;
    }
    static void Dispatch(Province p,int day)
    {
        if(p.Get("NextShipment")>day)return;p.Set("NextShipment",day+Rules.ShipmentInterval);
        if(p.Owner>=AllyOwnerOffset)return;
        var c=p.Config;int[] cargo=p.Owner==Imperial?new[]{0,0,Output(p,2,c.Tribute),0,0,0}:new[]{Output(p,0,c.FoodDays),Output(p,1,c.Recruits),Output(p,2,c.Gold),Output(p,3,c.Cannons),Output(p,4,c.Ammunition),Output(p,5,c.Ships)};
        int serial=I("ConvoyCount")+1;Put("ConvoyCount",serial);string k="C"+serial+"_";
        Put(k+"Due",day+c.TravelDays);Put(k+"Imperial",p.Owner==Imperial?1:0);Put(k+"Province",Array.IndexOf(WarTheatre.ProvinceIds,p.Id));
        for(int i=0;i<cargo.Length;i++)Put(k+"Cargo"+i,cargo[i]);
    }
    static void Arrive(int day,StringBuilder report)
    {
        for(int n=1;n<=I("ConvoyCount");n++)
        {
            string k="C"+n+"_";if(I(k+"Done")>0||I(k+"Due")>day)continue;Put(k+"Done",1);
            if(I(k+"Imperial")>0){VariableSingleton.ChangeInt("iMoney",I(k+"Cargo2"));report.AppendLine("Provincial tribute arrives: "+I(k+"Cargo2")+" gold.");continue;}
            int food=I(k+"Cargo0"),pressure=ImperialSupply.GetSupplyPressure(),lost=Mathf.Min(food,pressure);VariableSingleton.SetInt("iWbwSupplyLoss",pressure-lost);food-=lost;
            VariableSingleton.ChangeInt("iFood",food);VariableSingleton.ChangeInt("iBalkans",I(k+"Cargo1"));Put("OttomanGold",I("OttomanGold")+I(k+"Cargo2"));
            VariableSingleton.ChangeInt("iTurkCannons",I(k+"Cargo3"));OttomanDirector.AddAmmo(I(k+"Cargo4"));VariableSingleton.ChangeInt("iTurkShips",I(k+"Cargo5"));NavalState.Sync();
            var p=Find(WarTheatre.ProvinceIds[I(k+"Province")]);
            if(p.Known)report.AppendLine(p.Name+" convoy reaches the siege: "+food+" food-days, "+I(k+"Cargo1")+" troops, "+I(k+"Cargo2")+" gold, "+I(k+"Cargo3")+" cannons, "+I(k+"Cargo4")+" ammunition and "+I(k+"Cargo5")+" ships.");
            else report.AppendLine("An Ottoman supply convoy reaches the siege. Its cargo is unknown.");
        }
    }
    public static string SupplyDay()
    {
        Ensure();if(!Ready||I("SupplyTick")>=VariableSingleton.CompletedSiegeDays)return "";Put("SupplyTick",VariableSingleton.CompletedSiegeDays);
        VariableSingleton.SetInt("iFood",Mathf.Max(0,CouncilActions.Int("iFood")-1));if(CouncilActions.Int("iFood")>0)return "";
        int lost=Mathf.CeilToInt(Mathf.Max(0,CouncilActions.Int("iBazouks"))*.01f);VariableSingleton.ChangeInt("iBazouks",-lost);return "Ottoman stores are exhausted. Foraging interrupts today's attack; "+lost+" irregulars desert.";
    }
}
