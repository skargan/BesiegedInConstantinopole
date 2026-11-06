#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;

// Opt-in integration checks against a live, initialised campaign. All campaign
// and RNG state is restored, including unfinished personnel and diplomatic orders.
public static class MonasticResearchChecks
{
    static int checks;
    static void Check(bool condition,string label){if(!condition)throw new InvalidOperationException("MONASTIC RESEARCH: "+label);checks++;}
    static int I(string key)=>DailyLifeEvents.I(key);
    static void Set(string key,int value)=>VariableSingleton.SetInt(key,value);
    static bool Food(double expected)=>Math.Abs(SharedProvisions.Stock(SharedProvisions.City)-expected)<.00001;
    static DateTime Date=>VariableSingleton.GetDate().Date;
    static int Day=>TrainingPool.Day(Date);
    static void Fixture(int monks)
    {
        TrainingPool.Orders.Clear();SpecialistPersonnel.ResetForRestore();MedicalRecovery.ResetForRestore();MonasticResearch.ResetForRestore();CountryDiplomacy.ResetForRestore();
        Set("iHealers",monks);Set("iMoney",100000);Set("iProvisions",1000);Set("iSecrets",0);Set("iEngineers",10);Set("iInfantry",100);
        Set("iLifeMentorDue",0);Set("iMedicalLastDay",Day);
        SpecialistPersonnel.EnsureInitialized();MonasticResearch.Ensure();CountryDiplomacy.Ensure();
    }
    public static int Run()
    {
        checks=0;var original=VariableSingleton.Instance.CaptureState();var rng=UnityEngine.Random.state;
        int oldThreshold=MonasticResearch.Rules.ProgressPerSecret,oldRate=MonasticResearch.Rules.ProgressPerMonkPerDay;
        try{
            MonasticResearch.Rules.ProgressPerSecret=100;MonasticResearch.Rules.ProgressPerMonkPerDay=1;
            var preparation=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(original));
            preparation.turn=0;preparation.ints.Find(v=>v.key=="iPreparationVersion").value=2;
            VariableSingleton.Instance.RestoreValidated(preparation);Fixture(10);Set("iMonasticActive",6);Set("iMonasticProgress",64);
            DateTime preparationStart=Date;
            string preparationReport=CampaignPersonnelCalendar.Advance(preparationStart,preparationStart.AddMonths(2));
            Check(MonasticResearch.Progress==64&&I("iSecrets")==0,"preparation month jumps produce no progress or secrets and preserve earlier work");
            Check(MonasticResearch.DailyProgress==0&&MonasticResearch.DaysToDiscovery(6)==0&&!preparationReport.Contains("Monastic research continues"),"preparation screens and reports do not promise ongoing production or a discovery ETA");
            var pausedSave=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
            VariableSingleton.Instance.RestoreValidated(pausedSave);
            MonasticResearch.Advance(preparationStart.AddMonths(2));
            Check(MonasticResearch.Progress==64&&I("iSecrets")==0,"saved preparation progress cannot be awarded again");
            var siege=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(original));
            siege.turn=PreparationCampaign.SiegeTurn;siege.ints.Find(v=>v.key=="iPreparationVersion").value=2;
            VariableSingleton.Instance.RestoreValidated(siege);Fixture(10);Set("iMonasticActive",6);Set("iMonasticProgress",98);
            DateTime siegeStart=MonasticResearch.ResearchStart;Set("iMonasticLastDay",TrainingPool.Day(siegeStart.AddDays(-5)));
            string firstDay=CampaignPersonnelCalendar.Advance(siegeStart.AddDays(-5),siegeStart);
            Check(I("iSecrets")==1&&MonasticResearch.Progress==4&&firstDay.Contains("Knowledge recovered: +1 Secret"),"crossing into the siege counts only its first eligible day, not preceding preparation days");
            MonasticResearch.Advance(siegeStart);Check(I("iSecrets")==1&&MonasticResearch.Progress==4,"first siege day is idempotent");
            Check(MonasticResearch.DailyProgress==6&&MonasticResearch.DaysToDiscovery(6)==16,"daily production and truthful ETA resume during siege");
            Fixture(10);
            Check(MonasticResearch.ResearchMonks==0&&MonasticResearch.InfirmaryMonks==10&&MonasticResearch.Progress==0,"new service assigns everyone to infirmary with no retroactive progress");
            Check(!MonasticResearch.ApplyAllocation(11,out var report)&&!MonasticResearch.ApplyAllocation(-1,out report),"allocation cannot exceed roster or go negative");
            Check(MonasticResearch.ApplyAllocation(6,out report)&&MonasticResearch.ResearchMonks==0&&MedicalRecovery.Capacity==10,"confirmation leaves today's allocation intact");
            Check(MonasticResearch.HasPending&&MonasticResearch.PendingDate==Date.AddDays(1),"confirmed change begins next calendar day");
            Check(MonasticResearch.Advance(Date)==0&&I("iSecrets")==0,"processing current day does nothing");
            Set("iMonasticProgress",98);
            string daily=CampaignPersonnelCalendar.Advance(Date,Date.AddDays(1));
            Check(I("iSecrets")==1&&MonasticResearch.Progress==4&&daily.Contains("Knowledge recovered: +1 Secret"),"overflow awards currency and carries remainder into daily report");
            Check(MonasticResearch.ResearchMonks==6&&MedicalRecovery.Capacity==4&&!MonasticResearch.HasPending,"one allocation is shared by medicine and research");
            int secrets=I("iSecrets"),progress=MonasticResearch.Progress;
            for(int i=0;i<5;i++){string summary=MonasticResearch.Summary;int places=MedicalRecovery.Capacity;Check(places==4&&summary.Contains("6 assigned to research"),"screen reads show stable allocation");}
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(1));
            Check(I("iSecrets")==secrets&&MonasticResearch.Progress==progress,"same date cannot duplicate discoveries");

            Fixture(250);Set("iMonasticProgress",98);MonasticResearch.ApplyAllocation(250,out report);
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(1));
            Check(I("iSecrets")==3&&MonasticResearch.Progress==48,"multiple discoveries and surplus are all retained");
            Fixture(10);Set("iMonasticActive",6);Set("iMonasticProgress",64);MonasticResearch.ApplyAllocation(0,out report);
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(4));
            Check(MonasticResearch.Progress==64&&I("iSecrets")==0&&MedicalRecovery.Capacity==10,"research pauses without losing accumulated work");
            Fixture(10);MonasticResearch.ApplyAllocation(6,out report);MonasticResearch.ApplyAllocation(0,out report);
            Check(!MonasticResearch.HasPending&&MonasticResearch.ResearchMonks==0,"reselecting current duties cancels a pending change");
            Set("iMonasticActive",6);MonasticResearch.ApplyAllocation(8,out report);Set("iHealers",3);
            Check(MonasticResearch.ResearchMonks==3&&MonasticResearch.PendingResearchMonks==3&&MonasticResearch.InfirmaryMonks==0,"losses clamp active and pending allocations");

            Fixture(10);MedicalRecovery.ApplyCombatLosses(true,16,0,0,0);
            Check(MedicalRecovery.Total==8,"initial wounded admitted at current capacity");
            MonasticResearch.ApplyAllocation(6,out report);TrainingPool.Place("iHealers",5,MedicalRecovery.MonkCost);
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(6));
            Check(MedicalRecovery.Monks==15&&MonasticResearch.ResearchMonks==6&&MedicalRecovery.Capacity==9,"new recruits join the infirmary; research allocation stays fixed");
            Check(MonasticResearch.Progress==36&&I("iSecrets")==0,"six researchers generate six daily progress across dated arrivals");
            Check(MedicalRecovery.Total==8&&Food(998.4)&&I("iMedicalLastTreated")==8,"first four days treat four patients, recruited monks expand care on dayfive");
            Fixture(0);MedicalRecovery.AdmitWoundedEngineers(1);TrainingPool.Place("iHealers",1,MedicalRecovery.MonkCost);
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(6));
            Check(MedicalRecovery.SoonestReturnDays==13&&Food(999.9),"a monk arriving dayfive provides only two treatmentdays through daysix");
            Fixture(5);MedicalRecovery.ApplyCombatLosses(true,10,0,0,0);MonasticResearch.ApplyAllocation(5,out report);
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(30));
            Check(MedicalRecovery.Total==5&&MedicalRecovery.SoonestReturnDays==15&&MedicalRecovery.FreePlaces==0&&I("iProvisions")==1000,"reassignment never evicts patients or gives free treatment");
            Check(I("iSecrets")==1&&MonasticResearch.Progress==50,"research works while infirmary is deliberately paused");

            Fixture(1);MedicalRecovery.AdmitWoundedEngineers(1);
            daily=CampaignPersonnelCalendar.Advance(Date,Date.AddDays(30));
            Check(MedicalRecovery.Total==0&&MedicalRecovery.RecoveredFor("iEngineers")==1&&Food(999.25),"month advance completes actual treatmentdays once");
            Check(daily.Contains("1 recovered patients")&&daily.Contains("0.75 provisions used"),"month-end report retains returns that happened before final day");
            Fixture(10);MonasticResearch.ApplyAllocation(6,out report);Set("iMonasticProgress",98);
            var pendingSave=VariableSingleton.Instance.CaptureState();VariableSingleton.Instance.RestoreValidated(pendingSave);
            Check(MonasticResearch.HasPending&&MonasticResearch.PendingResearchMonks==6&&MonasticResearch.Progress==98,"pending allocation and progress survive save-load");
            CampaignPersonnelCalendar.Advance(Date,Date.AddDays(1));var processedSave=VariableSingleton.Instance.CaptureState();
            VariableSingleton.Instance.RestoreValidated(processedSave);CampaignPersonnelCalendar.Advance(Date,Date.AddDays(1));
            Check(I("iSecrets")==1&&MonasticResearch.Progress==4,"restored processed date does not award again");
            var legacySave=VariableSingleton.Instance.CaptureState();legacySave.ints.RemoveAll(p=>p.key.StartsWith("iMonastic"));
            VariableSingleton.Instance.RestoreValidated(legacySave);
            Check(MonasticResearch.ResearchMonks==0&&MonasticResearch.Progress==0&&MonasticResearch.LastProcessedDay==Day&&MonasticResearch.InfirmaryMonks==MedicalRecovery.Monks,"old saves initialise safely at their current date");
            Check(MonasticResearch.Advance(Date)==0&&I("iSecrets")==1,"opening migrated system creates no secret windfall");
            Debug.Log("MONASTIC_RESEARCH_CHECKS "+checks+" passed");return checks;
        }finally{
            MonasticResearch.Rules.ProgressPerSecret=oldThreshold;MonasticResearch.Rules.ProgressPerMonkPerDay=oldRate;
            VariableSingleton.Instance.RestoreValidated(original);UnityEngine.Random.state=rng;
        }
    }
}
#endif
