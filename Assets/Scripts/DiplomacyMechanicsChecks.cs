#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;

// Run after a test campaign has loaded. Restores the entire campaign and RNG afterwards.
public static class DiplomacyMechanicsChecks
{
    public static string Run()
    {
        var save=VariableSingleton.Instance.CaptureState();var failures=new List<string>();int checks=0;
        Action<bool,string> check=(ok,label)=>{checks++;if(!ok)failures.Add(label);};
        Action<string,int> set=VariableSingleton.SetInt;
        try {
            CountryDiplomacy.BeginCampaign();set("iReligion",0);set("iFocus",0);CountryDiplomacy.SynchronisePolicy();
            check(CountryDiplomacy.Definitions.Length>=31&&DiplomaticChronicles.All.Length==CountryDiplomacy.Definitions.Length,"expanded country and profile catalogues match");
            check(CountryDiplomacy.Index("ragusa")==25&&CountryDiplomacy.Index("ionian_tocco")==26&&CountryDiplomacy.Index("florence")==27&&CountryDiplomacy.Index("milan")==28&&CountryDiplomacy.Index("siena")==29&&CountryDiplomacy.Index("theodoro")==30,"six countries appended without shifting saves");
            check(CountryDiplomacy.Index("lithuania")==13&&CountryDiplomacy.Index("poland")==13&&CountryDiplomacy.Index("aragon")==6&&CountryDiplomacy.Index("sardinia")==6,"shared sovereigns retain one embassy and entitlement");
            check(CountryDiplomacy.Index("moldova")==19&&CountryDiplomacy.Index("gazaria")==2,"country aliases share one record");
            check(CountryDiplomacy.AvailableEnvoys==3&&CountryDiplomacy.TotalEnvoys==3,"three initial envoys");
            float adviser=CountryDiplomacy.AdviserModifier;
            check(CountryDiplomacy.PolicyModifier(3)==0&&CountryDiplomacy.Goodwill(3)==35,"Papal base remains 35");
            set("iReligion",1);check(CountryDiplomacy.PolicyModifier(3)==55&&CountryDiplomacy.Attitude(3)==CountryDiplomacy.AttitudeLevel.Friendly,"full union modifier and Papal attitude");
            set("iReligion",2);check(CountryDiplomacy.PolicyModifier(3)==35&&CountryDiplomacy.Goodwill(3)==35,"symbolic replaces full union");
            set("iReligion",0);check(CountryDiplomacy.PolicyModifier(3)==0&&CountryDiplomacy.EventModifier(1)==-10,"repudiation replaces union and gives temporary penalty");
            set("iMoney",50000);int i=0;var date=VariableSingleton.GetDate();
            check(CountryDiplomacy.Dispatch(i,out _),"dispatch one envoy");
            check(!CountryDiplomacy.Dispatch(i,out _)&&CountryDiplomacy.AvailableEnvoys==2,"prevent duplicate embassy");
            float before=CountryDiplomacy.Goodwill(i);var arrival=date.AddDays(CountryDiplomacy.Definitions[i].EnvoyDays);
            CountryDiplomacy.Advance(arrival);check(CountryDiplomacy.EnvoyState(i)==CountryDiplomacy.EnvoyAssignment.Resident&&CountryDiplomacy.Goodwill(i)==before,"travel grants no resident work");
            CountryDiplomacy.Advance(arrival.AddDays(4));check(Math.Abs(CountryDiplomacy.Goodwill(i)-before-1f)<.001f,"four Wary resident days preserve fractions");
            CountryDiplomacy.Advance(arrival.AddDays(4));check(Math.Abs(CountryDiplomacy.Goodwill(i)-before-1f)<.001f,"same date does not tick twice");
            set("iAidState0",3);check(CountryDiplomacy.Renew(0,false,out _),"refused aid can renew");
            check(!CountryDiplomacy.Renew(0,false,out _),"renewals cannot overlap");
            CountryDiplomacy.Advance(arrival.AddDays(14));check(CountryDiplomacy.RenewalReady(0,false)&&!CountryDiplomacy.RenewalReady(0,true),"ten resident days unlock aid only");
            check(!CountryDiplomacy.Renew(0,false,out _),"unused renewal cannot stack");
            set("iAidState0",1);check(CountryDiplomacy.AidRequestReason(0)!=null,"pending aid blocks repeated requests");
            check(CountryDiplomacy.ApplyEvent("hungary","test_oath",10,1)&&!CountryDiplomacy.ApplyEvent("hungary","test_oath",10,1),"event reward applies once");
            CountryDiplomacy.ChangeGoodwill(0,1000);check(CountryDiplomacy.Goodwill(0)==95&&CountryDiplomacy.FinalChance(0)<=95,"base and final caps");
            CountryDiplomacy.SetAttitude(0,CountryDiplomacy.AttitudeLevel.Friendly);CountryDiplomacy.Gift(0,out _);
            check(CountryDiplomacy.Attitude(0)==CountryDiplomacy.AttitudeLevel.Friendly,"gifts cannot create Allied status");
            CountryDiplomacy.SetAttitude(0,CountryDiplomacy.AttitudeLevel.Allied);CountryDiplomacy.Gift(0,out _);
            check(CountryDiplomacy.Attitude(0)==CountryDiplomacy.AttitudeLevel.Allied,"gifts never demote an ally");
            check(!CountryDiplomacy.Definitions[22].WarEligible&&!CountryDiplomacy.Definitions[23].WarEligible&&!CountryDiplomacy.Definitions[24].WarEligible,"Crimea Mamluks and Morea excluded from generic war");
            check(!AidRouting.BySea(18)&&!AidRouting.BySea(19)&&AidRouting.BySea(22),"new land and sea routes");
            for(int court=25;court<=30;court++)check(!CountryDiplomacy.Definitions[court].WarEligible&&AidRouting.BySea(court)&&CountryDiplomacy.Definitions[court].AidDays>0,"new court has an ordinary maritime aid package: "+CountryDiplomacy.Definitions[court].Id);
            set("iAidState6",1);set("iAidDays6",12);set("iAidState13",3);
            check(CountryDiplomacy.AidRequestReason(CountryDiplomacy.Index("aragon"))!=null&&CountryDiplomacy.AidRequestReason(CountryDiplomacy.Index("sicily"))!=null,"Aragonese possessions cannot duplicate Naples aid");
            check(CountryDiplomacy.AidRequestReason(CountryDiplomacy.Index("lithuania"))!=null&&CountryDiplomacy.AidRequestReason(CountryDiplomacy.Index("poland"))!=null,"Polish and Lithuanian aliases share the answered appeal");
            set("iAidState3",0);int goldBefore=DailyLifeEvents.I("iMoney"),shipsBefore=DailyLifeEvents.I("iShips");
            check(DiplomaticStoryAid.TryGrant("papacy","major",out _),"named preparation aid can arrive once");
            check(DailyLifeEvents.I("iAidState3")==2&&DailyLifeEvents.I("iMoney")==goldBefore+10000&&DailyLifeEvents.I("iShips")==shipsBefore,"story aid canonical state and no transport ships");
            check(!DiplomaticStoryAid.TryGrant("papacy","minor",out _)&&CountryDiplomacy.AidRequestReason(3)!=null,"no duplicated story or ordinary initial aid");
            set("iAidState5",1);set("iAidDays5",4);
            check(!DiplomaticStoryAid.TryGrant("hospitallers","fleet",out _)&&DailyLifeEvents.I("iAidDays5")==4,"story aid preserves pending ordinary commitments");
        } catch(Exception e){failures.Add(e.ToString());}
        finally {VariableSingleton.Instance.RestoreValidated(save);}
        string result="DIPLOMACY_CHECKS checks="+checks+" failures="+failures.Count+"\n"+string.Join("\n",failures);
        if(failures.Count>0)throw new Exception(result);return result;
    }
}

#endif
