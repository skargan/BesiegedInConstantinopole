#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class ProvincialWarChecks
{
    static int count;static VariableSingleton.CampaignSave baseline;
    static void Check(bool ok,string name){if(!ok)throw new Exception("PROVINCIAL: "+name);count++;}
    static string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    static void Reset(int seed=1701){VariableSingleton.Instance.RestoreValidated(baseline);UnityEngine.Random.InitState(seed);}
    static void Move(int days){typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,baseline.turn+days);ProvincialWar.Advance(VariableSingleton.GetDate());}
    static ProvincialWar.Province P=>ProvincialWar.Find("thrace");
    static void Complete(ProvincialWar.Province p,int maneuver,int tactic,int aftermath)
    {
        Check(ProvincialBattle.Maneuver(p,maneuver),"maneuver accepted");ProvincialBattle.Continue(p);Check(ProvincialBattle.Clash(p,tactic),"clash accepted");ProvincialBattle.Continue(p);
        Check(ProvincialBattle.Aftermath(p,Mathf.Min(aftermath,p.Get("BResult")<0?2:1)),"aftermath accepted");
        if(p.Get("BattleStage")==3){Check(ProvincialBattle.Clash(p,(tactic+1)%3),"renewal accepted");ProvincialBattle.Continue(p);}
        Check(p.Get("BattleStage")==6,"encounter terminates after at most one renewal");
    }
    public static int Run()
    {
        count=0;var original=VariableSingleton.Instance.CaptureState();var rules=ProvincialWar.Rules;int chance=rules.BattleChance,wealth=rules.WealthRecovery,aid=rules.AssistanceChance;
        try {
            typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn);
            ProvincialIntelligence.ResetForRestore();ProvinceCampaign.Ensure();CountryDiplomacy.Ensure();
            baseline=VariableSingleton.Instance.CaptureState();rules.AssistanceChance=0;
            foreach(string resource in new[]{"iProvisions","iMaterials","iAmmunition"}){Check(MarketPurchaseLimits.Limit(resource)==10,"siege daily market limit "+resource);MarketPurchaseLimits.Record(resource,7);Check(MarketPurchaseLimits.Remaining(resource)==3&&MarketPurchaseLimits.Unavailable(resource,4)!=null,"split purchases share daily limit "+resource);MarketPurchaseLimits.Record(resource,3);Check(MarketPurchaseLimits.Remaining(resource)==0,"daily stock exhausted "+resource);}
            var soldOut=VariableSingleton.Instance.CaptureState();VariableSingleton.Instance.RestoreValidated(soldOut);
            foreach(string resource in new[]{"iProvisions","iMaterials","iAmmunition"})Check(MarketPurchaseLimits.Remaining(resource)==0,"market stock remains exhausted after save restoration "+resource);
            typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,VariableSingleton.Turn+1);
            foreach(string resource in new[]{"iProvisions","iMaterials","iAmmunition"})Check(MarketPurchaseLimits.Remaining(resource)==10,"new siege day replenishes only ten "+resource);
            Reset();
            Check(ProvincialWar.Provinces.Count()==11&&P.Garrison==6000,"11 real garrisons; Thrace 6000");
            Reset();P.Authority=20;P.Resistance=30;Move(1);Check(P.Rebels.Troops==0,"low authority does not automatically rebel");
            Check(ProvincialWar.Launch("thrace",out _),"deliberate launch");int expires=P.Get("MusterUntil");Check(P.Rebels.Troops==1000&&expires-ProvincialWar.I("LastDay")>=20&&expires-ProvincialWar.I("LastDay")<=50,"1000 founding troops and bounded hidden muster");
            string before=State();Check(!ProvincialWar.Launch("thrace",out _)&&before==State(),"launch cannot duplicate army or reroll timer");
            Move(2);Check(P.Rebels.Troops==1100&&P.Resistance==29&&P.Get("BattleStage")==0,"hidden recruitment spends resistance and avoids enemy encounter");
            P.Rebels.Troops=999;Check(!ProvincialWar.GiveOrder(P.Rebels,ProvincialWar.Order.Capture,out _),"minimum offensive headcount");P.Rebels.Troops=1000;Check(ProvincialWar.GiveOrder(P.Rebels,ProvincialWar.Order.Capture,out _)&&!P.Protected,"offensive order ends muster");
            Reset();rules.BattleChance=0;P.Resistance=5;P.Rebels.Troops=1000;P.Rebels.Stance=ProvincialWar.Order.Gather;Move(3);Check(P.Rebels.Troops==1300&&P.Resistance==2,"daily recruitment finite and exact");Move(8);Check(P.Rebels.Troops==1500&&P.Resistance==0,"resistance exhausted stops recruitment");
            Reset();P.Rebels.Troops=P.Config.RebelCap-25;P.Resistance=5;Move(1);Check(P.Rebels.Troops==P.Config.RebelCap&&P.Resistance==4,"recruitment respects cap");
            Reset();P.Authority=70;P.Rebels.Control=2000;P.Rebels.Troops=8000;P.Rebels.Stance=ProvincialWar.Order.Hold;Check(P.Ceiling==80,"territory constrains authority");Move(6);Check(P.Authority==70,"six quiet days do not heal authority");Move(7);Check(P.Authority==71,"seventh quiet day restores one authority");ProvincialWar.Gain(P.Rebels,9000);Check(P.TotalControl==10000&&P.Authority==0,"territory capped and authority falls immediately");
            Reset();P.Rebels.Control=2000;P.Authority=50;ProvincialWar.Reclaim(P,500);Check(P.Ceiling==85&&P.Authority==50,"reclaim raises ceiling without free authority");
            Reset();P.Rebels.Troops=2000;P.Rebels.Control=3000;P.Set("MusterUntil",ProvincialWar.Day+50);P.Rebels.Stance=ProvincialWar.Order.Hold;Move(1);Check(P.Rebels.Control==3000,"protected holdings cannot be reclaimed");
            Reset();P.Rebels.Troops=1000;P.Rebels.Stance=ProvincialWar.Order.Pillage;P.Resistance=10;int gold=CouncilActions.Int("iMoney");Move(1);Check(CouncilActions.Int("iMoney")-gold>=100&&CouncilActions.Int("iMoney")-gold<=300&&P.Resistance==9&&P.Authority==99,"pillage pays only destroyed wealth and spends resistance");
            P.Rebels.Troops=999;Move(2);Check(P.Rebels.Stance==ProvincialWar.Order.Gather,"weak army ceases offensive effects before next day");
            Reset();ProvinceCampaign.Set("Agent_thrace",2);gold=CouncilActions.Int("iSecrets");Check(ProvincialWar.Plans("thrace",out _)&&CouncilActions.Int("iSecrets")==gold+2,"plans grant exactly two secrets");Check(!ProvincialWar.Plans("thrace",out _),"plans once per province");Check(ProvinceCampaign.Recall("thrace",out _)&&!P.Known,"recall immediately hides exact intelligence");
            Reset();P.Garrison=500;Move(1);Check(P.Garrison==510,"passive garrison replacements");
            Reset();P.Authority=0;ProvincialWar.Collapse(P);int roll=P.Get("SettlementRoll");ProvincialWar.Collapse(P);Check(P.Get("SettlementRoll")==roll,"collapse draw fixed once");P.Set("SettlementRoll",0);P.Resistance=50;gold=CouncilActions.Int("iMoney");Check(ProvincialWar.Settle(P,0,out _)&&P.Owner==ProvincialWar.Imperial&&P.Garrison==0&&P.Get("Withdrawn")==6000&&CouncilActions.Int("iMoney")==gold-500,"restoration records withdrawal and charges once");Check(!ProvincialWar.Settle(P,0,out _),"settlement not repeated");
            Reset();P.Authority=0;ProvincialWar.Collapse(P);P.Set("SettlementRoll",99);P.Garrison=123;P.Rebels.Troops=789;P.Rebels.Control=2000;gold=CouncilActions.Int("iMoney");ProvincialWar.Settle(P,0,out _);Check(P.Owner==ProvincialWar.Chaos&&P.TotalControl==0&&P.Garrison==123&&P.Rebels.Troops==789&&CouncilActions.Int("iMoney")==gold-500,"failed restoration pays; chaos retains troops and clears claims");Move(89);Check(P.Owner==ProvincialWar.Chaos&&P.Garrison==123,"chaos suspends recovery for90days");Move(90);Check(P.Owner==ProvincialWar.Ottoman&&P.Authority==25&&P.Garrison==133,"chaos returns with surviving pool plus ordinary daily replacement");
            Reset();var a1=new ProvincialWar.Army(P,1);var a2=new ProvincialWar.Army(P,2);a1.Control=1500;a2.Control=1500;Check(ProvincialWar.QualifyingAlly(P)==null,"tied allies do not win arbitrary ownership");a1.Control=1600;Check(ProvincialWar.QualifyingAlly(P).Slot==1,"strict leading claim wins eligibility");a1.Control=1000;a2.Control=900;Check(ProvincialWar.QualifyingAlly(P)==null,"exactly10percent not sufficient");
            Reset();rules.WealthRecovery=0;P.Wealth=87;foreach(var q in ProvincialWar.Provinces)q.Set("NextShipment",ProvincialWar.Day+1000);P.Set("NextShipment",ProvincialWar.Day+1);Move(1);Check(ProvincialWar.I("ConvoyCount")==1&&ProvincialWar.I("C1_Cargo2")==435,"wealth fixes whole-number cargo at departure");int due=ProvincialWar.I("C1_Due");P.Wealth=0;gold=ProvincialWar.I("OttomanGold");Move(1+P.Config.TravelDays);Check(ProvincialWar.I("OttomanGold")==gold+435,"departed gold cargo stays fixed after damage");
            Reset();rules.WealthRecovery=0;var sea=ProvincialWar.Find("bithynia");sea.Wealth=87;foreach(var q in ProvincialWar.Provinces)q.Set("NextShipment",ProvincialWar.Day+1000);sea.Set("NextShipment",ProvincialWar.Day+1);Move(1);int ships=ProvincialWar.I("C1_Cargo5"),rem=sea.Get("Remainder5");Check(ships==6&&rem==9,"fractional ship production preserved");int baseDay=TrainingPool.Day(new DateTime((long)ProvincialWar.I("LastDay")*TimeSpan.TicksPerDay));sea.Wealth=0;gold=CouncilActions.Int("iTurkShips");Move(1+sea.Config.TravelDays);Check(CouncilActions.Int("iTurkShips")==gold+6,"cargo unchanged by later wealth loss");string arrived=State();Move(1+sea.Config.TravelDays);Check(arrived==State(),"arrivals process once");
            Reset();gold=CouncilActions.Int("iTurkShips");NavalState.CompleteDay();Check(CouncilActions.Int("iTurkShips")==gold,"old daily ship grant removed");
            Check(Math.Abs(ProvincialBattle.WinProbability(1000,6000)-.8/37)<1e-10,"squared battle probability");Check(Math.Abs(ProvincialBattle.WinProbability(1000,1000)-.4)<1e-10,"equal armies give40/20/40");
            Check(ProvincialBattle.Matchup(0,1)==1&&ProvincialBattle.Matchup(1,2)==1&&ProvincialBattle.Matchup(2,0)==1,"tactic triangle");
            for(int i=0;i<100;i++){int[] weights={1000+i,2000-i,37};var split=ProvincialBattle.Allocate(weights,i);Check(split.Sum()==i&&split.All(n=>n>=0),"whole-number proportional total "+i);}
            for(int seed=0;seed<80;seed++){
                Reset(9200+seed);P.Rebels.Troops=2000;P.Rebels.Control=1000;P.Authority=80;ProvinceCampaign.Set("Agent_thrace",2);a1=new ProvincialWar.Army(P,1);a1.Troops=1500;ProvincialBattle.Begin(P,0,true);
                var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(State());string situation=ProvincialBattle.Report(P);VariableSingleton.Instance.RestoreValidated(saved);Check(situation==ProvincialBattle.Report(P),"situation survives serialized save "+seed);
                Complete(P,seed%3,seed%3,seed%3);string result=State();int troops=P.Present.Sum(a=>a.Troops);Check(troops>=0&&troops<=3500&&P.Garrison>=0&&P.Garrison<=6000,"actual pools bounded "+seed);Check(P.TotalControl<=10000&&P.Authority<=P.Ceiling,"political bounds "+seed);Check(P.Get("BattleAfter")==ProvincialWar.Day+14,"14day cooldown "+seed);
                string final=ProvincialBattle.Report(P);Check(!ProvincialBattle.Aftermath(P,0)&&final==ProvincialBattle.Report(P),"result package cannot repeat "+seed);
                VariableSingleton.Instance.RestoreValidated(saved);Complete(P,seed%3,seed%3,seed%3);Check(result==State(),"pending battle deterministic save replay "+seed);
            }
            Reset();rules.BattleChance=100;P.Rebels.Troops=1000;P.Rebels.Stance=ProvincialWar.Order.SeekBattle;Move(1);Check(P.Get("BattleStage")==1,"daily enemy/player encounter generated");Complete(P,0,0,0);ProvincialBattle.Continue(P);if(P.Get("Settlement")==0){Move(14);Check(P.Get("BattleStage")==0,"cooldown blocks encounters for preceding13days");}
            Reset();Check(SaveGames.Checkpoint("council"),"actual archive checksum/save succeeds");Check(SaveGames.Load(SaveGames.LatestPath(),out var error),"actual archive load "+error);
            return count;
        } finally {rules.BattleChance=chance;rules.WealthRecovery=wealth;rules.AssistanceChance=aid;VariableSingleton.Instance.RestoreValidated(original);}
    }
}
#endif

