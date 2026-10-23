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
    public static readonly string[] Ids={"ACH_STILL_WE_STAND","ACH_LAST_SIGNATURE","ACH_SEALED_BARGAIN","ACH_SEASON_AGAINST_WORLD","ACH_UNBROKEN_WALL","ACH_EMPERORS_EYES","ACH_FLEET_ON_HORIZON","ACH_MASTER_WORKSHOPS","ACH_FOUR_VOICES","ACH_BREAD_FOR_CITY","ACH_PRAETORIAN_GUARD","ACH_GREAT_CANNON","ACH_FELLOWSHIP","ACH_OTTOMANS_10000","ACH_OTTOMANS_30000","ACH_OTTOMANS_50000","ACH_OTTOMANS_75000","ACH_NO_WALL_REPAIRS"};
    static IAchievementPlatform backend;
    public static string EarnedKey(int i)=>"Besieged.IronAchievement."+Ids[i];
    static string PendingKey(int i)=>"Besieged.SteamPending."+Ids[i];
    public static bool Earned(int i)=>PlayerPrefs.GetInt(EarnedKey(i),0)==1||(backend!=null&&backend.Ready&&backend.IsUnlocked(Ids[i]));
    public static bool Eligible(bool ironman,bool terminal,bool test,bool practice)=>ironman&&!terminal&&!test&&!practice;
    public static void Unlock(int i){if(i<0||i>=Ids.Length)return;PlayerPrefs.SetInt(EarnedKey(i),1);PlayerPrefs.SetInt(PendingKey(i),1);PlayerPrefs.Save();Sync();}
    public static void Register(IAchievementPlatform platform){backend=platform;Sync();}
    public static void Sync(){if(backend==null||!backend.Ready)return;bool changed=false;for(int i=0;i<Ids.Length;i++)if(PlayerPrefs.GetInt(PendingKey(i),0)==1){if(backend.IsUnlocked(Ids[i])||backend.SetAchievement(Ids[i]))changed=true;}if(changed)backend.StoreStats();PlayerPrefs.Save();}
    // Call only after Steam's successful UserStatsStored_t callback. A failed/offline upload stays pending.
    public static void ConfirmStored(){if(backend==null||!backend.Ready)return;for(int i=0;i<Ids.Length;i++)if(PlayerPrefs.GetInt(PendingKey(i),0)==1&&backend.IsUnlocked(Ids[i]))PlayerPrefs.SetInt(PendingKey(i),0);PlayerPrefs.Save();}
}
