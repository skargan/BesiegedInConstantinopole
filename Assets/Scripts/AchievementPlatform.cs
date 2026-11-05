using System;
using UnityEngine;

// The same permanent API names are used by the game catalogue and future Steam App Admin.
public interface IAchievementPlatform {
    bool Ready {get;}
    bool IsUnlocked(string apiName);
    bool SetAchievement(string apiName);
    bool StoreStats();
}
public static class AchievementPlatform {
    public static readonly string[] Ids={"ACH_STILL_WE_STAND","ACH_LAST_SIGNATURE","ACH_SEALED_BARGAIN","ACH_SEASON_AGAINST_WORLD","ACH_UNBROKEN_WALL","ACH_EMPERORS_EYES","ACH_FLEET_ON_HORIZON","ACH_MASTER_WORKSHOPS","ACH_FOUR_VOICES","ACH_BREAD_FOR_CITY","ACH_PRAETORIAN_GUARD","ACH_GREAT_CANNON","ACH_FELLOWSHIP","ACH_OTTOMANS_10000","ACH_OTTOMANS_30000","ACH_OTTOMANS_50000","ACH_OTTOMANS_75000","ACH_NO_WALL_REPAIRS","ACH_ALCHEMICAL_GENIUS"};
    static IAchievementPlatform backend;
    static bool uploadInFlight;
    static readonly bool[] submitted=new bool[Ids.Length];
    public static string EarnedKey(int i)=>"Besieged.IronAchievement."+Ids[i];
    static string PendingKey(int i)=>"Besieged.SteamPending."+Ids[i];
    public static bool Earned(int i)=>i>=0 && i<Ids.Length && (PlayerPrefs.GetInt(EarnedKey(i),0)==1||(backend!=null&&backend.Ready&&backend.IsUnlocked(Ids[i])));
    public static bool Eligible(bool ironman,bool terminal,bool test,bool practice)=>ironman&&!terminal&&!test&&!practice;
    public static void Unlock(int i) {
        if(i<0||i>=Ids.Length)return;
        if(PlayerPrefs.GetInt(EarnedKey(i),0)==1){if(PlayerPrefs.GetInt(PendingKey(i),0)==1)Sync();return;}
        PlayerPrefs.SetInt(EarnedKey(i),1);PlayerPrefs.SetInt(PendingKey(i),1);PlayerPrefs.Save();Sync();
    }
    public static void Register(IAchievementPlatform platform){backend=platform;UploadFailed();Sync();}
    public static void Unregister(IAchievementPlatform platform){if(ReferenceEquals(backend,platform)){backend=null;UploadFailed();}}
    public static void Sync(){
        if(backend==null||!backend.Ready||uploadInFlight)return;
        Array.Clear(submitted,0,submitted.Length);bool changed=false;
        for(int i=0;i<Ids.Length;i++)if(PlayerPrefs.GetInt(PendingKey(i),0)==1 && (backend.IsUnlocked(Ids[i])||backend.SetAchievement(Ids[i]))){submitted[i]=true;changed=true;}
        if(!changed)return;
        uploadInFlight=true;
        if(!backend.StoreStats())UploadFailed();
    }
    // Only the successful callback for this submitted batch may clear the outbox.
    // Awards earned while that upload is in flight remain pending for the next batch.
    public static void ConfirmStored(){
        if(backend==null||!backend.Ready||!uploadInFlight)return;
        for(int i=0;i<Ids.Length;i++)if(submitted[i]&&backend.IsUnlocked(Ids[i]))PlayerPrefs.SetInt(PendingKey(i),0);
        PlayerPrefs.Save();UploadFailed();Sync();
    }
    public static void UploadFailed(){uploadInFlight=false;Array.Clear(submitted,0,submitted.Length);}
}
