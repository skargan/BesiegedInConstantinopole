#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;

public static class MedicalProvisionsChecks
{
    static int checks;
    static VariableSingleton.CampaignSave baseline;
    static int I(string key)=>DailyLifeEvents.I(key);
    static bool Near(double a,double b)=>Math.Abs(a-b)<.0000011;
    static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("MEDICAL PROVISIONS: "+label);checks++;}
    static DateTime Fixture(int patients,double food,int monks=100)
    {
        VariableSingleton.Instance.RestoreValidated(baseline);
        MedicalRecovery.ResetForRestore();MonasticResearch.ResetForRestore();
        VariableSingleton.SetInt("iHealers",monks);VariableSingleton.SetInt("iInfantry",1000);
        SharedProvisions.Set(SharedProvisions.City,food);MonasticResearch.Ensure();
        MedicalRecovery.ApplyCombatLosses(true,patients*2,0,0,0);
        return VariableSingleton.GetDate().Date;
    }
    public static int Run()
    {
        checks=0;var original=VariableSingleton.Instance.CaptureState();baseline=original;
        try {
            var day=Fixture(20,10);Check(Near(MedicalRecovery.DailyProvisionNeed,1),"twenty patients need one extra provision");
            MedicalRecovery.Advance(day.AddDays(1));Check(I("iMedicalLastTreated")==20&&Near(SharedProvisions.Stock(SharedProvisions.City),9),"twenty actual treatments cost exactly one provision");
            Check(Near(MedicalRecovery.LastProvisionCost,1)&&MedicalRecovery.SoonestReturnDays==14,"cost and one treatment day recorded");
            MedicalRecovery.Advance(day.AddDays(1));Check(Near(SharedProvisions.Stock(SharedProvisions.City),9)&&MedicalRecovery.SoonestReturnDays==14,"repeated day cannot charge or treat twice");

            day=Fixture(3,10);Check(Near(MedicalRecovery.DailyProvisionNeed,.15),"small group is fractional rather than rounded up");
            MedicalRecovery.Advance(day.AddDays(15));Check(MedicalRecovery.Total==0&&MedicalRecovery.RecoveredFor("iInfantry")==3,"recovery still requires fifteen treatment days");
            Check(Near(SharedProvisions.Stock(SharedProvisions.City),7.75),"three patients over fifteen days cost exactly 2.25 provisions");
            MedicalRecovery.Advance(day.AddDays(16));Check(Near(SharedProvisions.Stock(SharedProvisions.City),7.75),"recovered patients incur no further treatment charge");

            day=Fixture(3,.125);MedicalRecovery.Advance(day.AddDays(1));
            Check(I("iMedicalLastTreated")==2&&I("iMedicalLastWaiting")==1&&Near(MedicalRecovery.LastProvisionCost,.1),"limited food pays only for two whole patients");
            Check(Near(SharedProvisions.Stock(SharedProvisions.City),.025)&&MedicalRecovery.Total==3,"insufficient remainder is preserved and waiting patient retained");
            MedicalRecovery.Advance(day.AddDays(2));Check(I("iMedicalLastTreated")==0&&Near(SharedProvisions.Stock(SharedProvisions.City),.025)&&MedicalRecovery.SoonestReturnDays==14,"no free progress without treatment ration");
            SharedProvisions.Add(SharedProvisions.City,.125);MedicalRecovery.Advance(day.AddDays(3));
            Check(I("iMedicalLastTreated")==3&&Near(SharedProvisions.Stock(SharedProvisions.City),0),"care resumes once food arrives");

            day=Fixture(1,.049999);MedicalRecovery.Advance(day.AddDays(1));Check(I("iMedicalLastTreated")==0&&Near(SharedProvisions.Stock(SharedProvisions.City),.049999),"less than 0.05 provisions cannot fund one treatment");
            SharedProvisions.Add(SharedProvisions.City,.000001);MedicalRecovery.Advance(day.AddDays(2));Check(I("iMedicalLastTreated")==1&&SharedProvisions.Stock(SharedProvisions.City)==0,"exact fractional threshold funds one treatment");

            day=Fixture(3,10);VariableSingleton.SetInt("iHealers",2);MedicalRecovery.Advance(day.AddDays(1));
            Check(I("iMedicalLastTreated")==2&&Near(SharedProvisions.Stock(SharedProvisions.City),9.9),"monks cap treatment and actual food charge");
            VariableSingleton.SetInt("iHealers",0);MedicalRecovery.Advance(day.AddDays(2));Check(I("iMedicalLastTreated")==0&&Near(SharedProvisions.Stock(SharedProvisions.City),9.9),"no monks means no treatment and no extra food charge");

            day=Fixture(3,10);MedicalRecovery.Advance(day.AddDays(1));
            var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
            SharedProvisions.Set(SharedProvisions.City,0);VariableSingleton.Instance.RestoreValidated(saved);
            Check(Near(MedicalRecovery.LastProvisionCost,.15)&&Near(SharedProvisions.Stock(SharedProvisions.City),9.85),"save restores fractional medical report and city food");
            MedicalRecovery.Advance(day.AddDays(1));Check(Near(SharedProvisions.Stock(SharedProvisions.City),9.85),"save reload cannot repeat already charged day");

            day=Fixture(3,100.15);VariableSingleton.SetInt("iPopulation",100000);VariableSingleton.SetInt("iProvisionCityDay",0);VariableSingleton.SetInt("iHungerDays",0);
            double demand=SharedProvisions.CityDemand;
            SharedProvisions.ConsumeCityDay();MedicalRecovery.Advance(day.AddDays(1));
            Check(Near(demand,100)&&Near(SharedProvisions.CityDemand,100),"wounded remain within inclusive city population ration");
            Check(SharedProvisions.Stock(SharedProvisions.City)==0&&I("iHungerDays")==0&&Near(MedicalRecovery.LastProvisionCost,.15),"ordinary ration and explicit extra care each charged once");
            return checks;
        } finally {VariableSingleton.Instance.RestoreValidated(original);}
    }
}
#endif
