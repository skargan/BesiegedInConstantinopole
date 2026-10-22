using System;
using System.IO;
using System.Linq;
using UnityEngine;
public static class SaveGameTests {
    static int checks;
    static void Check(bool v,string label){if(!v)throw new Exception("SAVE TEST: "+label);checks++;}
    public static SaveGames.Archive Sample(){
        var c=new VariableSingleton.CampaignSave{wall="OuterWall",turn=3};
        foreach(string k in new[]{"iMoney","iInfantry","iArchers","iProvisions"})c.ints.Add(new VariableSingleton.IntValue{key=k,value=100});
        c.floats.Add(new VariableSingleton.FloatValue{key="fHealth",value=1});
        var a=new SaveGames.Archive{campaign=c,build=CouncilUI.BuildLabel,campaignId="test-campaign",phase="council",name="Test",sequence=1};a.payloadHash=SaveGames.Integrity(a);return a;
    }
    static void Invalid(SaveGames.Archive a,string reason){bool rejected=false;try{SaveGames.Validate(a);}catch(InvalidDataException){rejected=true;}Check(rejected,reason);}
    public static void Run(){checks=0;var root=Path.Combine(Path.GetTempPath(),"Besieged-save-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        var a=Sample();SaveGames.Validate(a);Check(true,"valid envelope");
        string path=Path.Combine(root,"normal.besieged.json");SaveGames.AtomicWrite(path,JsonUtility.ToJson(a,true));Check(SaveGames.Read(path).campaign.turn==3,"portable roundtrip");
        a.sequence=2;a.campaign.turn=4;a.payloadHash=SaveGames.Integrity(a);SaveGames.AtomicWrite(path,JsonUtility.ToJson(a,true));Check(SaveGames.Read(path).campaign.turn==4&&SaveGames.Read(path+".bak").campaign.turn==3,"atomic replacement keeps last good backup");
        a=Sample();a.saveFormat=999;Invalid(a,"future save format rejected");
        a=Sample();a.campaign.ints.Add(a.campaign.ints[0]);a.payloadHash=SaveGames.Integrity(a);Invalid(a,"duplicate key rejected");
        a=Sample();a.campaign.floats[0].value=float.NaN;a.payloadHash=SaveGames.Integrity(a);Invalid(a,"nonfinite value rejected");
        a=Sample();a.campaign.wall="missing";a.payloadHash=SaveGames.Integrity(a);Invalid(a,"unknown wall rejected");
        a=Sample();a.campaign.ints[0].value++;Invalid(a,"payload checksum catches corruption");
        a=Sample();a.ironman=true;Invalid(a,"mode metadata protected by checksum");
        a=Sample();a.phase="event";a.payloadHash=SaveGames.Integrity(a);Invalid(a,"missing event continuation rejected");
        a=Sample();a.campaign.training.Add(new TrainingPool.Order{unit="missing",quantity=10,readyDay=1});a.payloadHash=SaveGames.Integrity(a);Invalid(a,"invalid delayed order rejected");
        a=Sample();string legacy=Path.Combine(root,"legacy.json");File.WriteAllText(legacy,JsonUtility.ToJson(a.campaign));Check(!SaveGames.Read(legacy).ironman,"legacy migration is Normal");
        Check(AchievementPlatform.Ids.Length==Achievements.Names.Length&&AchievementPlatform.Ids.Distinct().Count()==AchievementPlatform.Ids.Length,"stable unique Steam API names for every game achievement");
        Check(!AchievementPlatform.Eligible(false,false,false,false)&&AchievementPlatform.Eligible(true,false,false,false),"only Ironman earns achievements");
        Check(!AchievementPlatform.Eligible(true,true,false,false)&&!AchievementPlatform.Eligible(true,false,true,false)&&!AchievementPlatform.Eligible(true,false,false,true),"terminal tests and practice cannot award");
        for(int i=0;i<TechnologySystem.All.Length;i++)Check(TechnologyArtwork.Load(TechnologySystem.All[i].Key)!=null,"technology art mapped "+i);
        Check(PopulationAccounting.RoundedFood(100499)==100&&PopulationAccounting.RoundedFood(100500)==101,"rounding boundary uses half-up rounding");
        Check(PopulationAccounting.RoundedFood(0)==0&&PopulationAccounting.RoundedFood(999)==1,"small population rounding");
        var old=Sample().campaign;old.version=1;old.ints.Add(new VariableSingleton.IntValue{key="iPopulation",value=100});old.ints.Add(new VariableSingleton.IntValue{key="iPopulationRemainder",value=499});
        Check(PopulationAccounting.Count(old)==100499,"legacy count preserves individual remainder");old.version=2;old.ints.First(v=>v.key=="iPopulation").value=100499;Check(PopulationAccounting.Count(old)==100499,"new count never multiplies twice");
        Check(Resources.Load<Texture2D>("UI/Maps/Europe1450")!=null,"world map asset imported");
        PlatformRetry();Debug.Log("SAVE_AND_TECH_CHECKS "+checks+" passed");
    }
    class FakePlatform:IAchievementPlatform {public bool ready,unlocked;public int stores;public bool Ready=>ready;public bool IsUnlocked(string id)=>id==AchievementPlatform.Ids[17]&&unlocked;public bool SetAchievement(string id){if(id!=AchievementPlatform.Ids[17])return false;unlocked=true;return true;}public bool StoreStats(){stores++;return false;}}
    static void PlatformRetry(){int i=17;string earned=AchievementPlatform.EarnedKey(i),pending="Besieged.SteamPending."+AchievementPlatform.Ids[i];bool hadEarned=PlayerPrefs.HasKey(earned),hadPending=PlayerPrefs.HasKey(pending);int oldEarned=PlayerPrefs.GetInt(earned),oldPending=PlayerPrefs.GetInt(pending);var fake=new FakePlatform();try{
        AchievementPlatform.Register(fake);AchievementPlatform.Unlock(i);Check(fake.stores==0&&PlayerPrefs.GetInt(pending)==1,"offline unlock queued");fake.ready=true;AchievementPlatform.Register(fake);Check(fake.stores>0&&PlayerPrefs.GetInt(pending)==1,"failed Steam upload remains pending");AchievementPlatform.Sync();Check(PlayerPrefs.GetInt(pending)==1,"memory-only unlock is not server confirmation");AchievementPlatform.ConfirmStored();Check(PlayerPrefs.GetInt(pending)==0,"successful callback confirms upload");
        }finally{AchievementPlatform.Register(null);if(hadEarned)PlayerPrefs.SetInt(earned,oldEarned);else PlayerPrefs.DeleteKey(earned);if(hadPending)PlayerPrefs.SetInt(pending,oldPending);else PlayerPrefs.DeleteKey(pending);PlayerPrefs.Save();}}
    public static void Build(){Run();PhasedAssaultTests.Verify();ConfigurePlaytest.Build();}
}
