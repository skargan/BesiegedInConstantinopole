#if BESIEGED_STEAMWORKS_NET
using System;
using Steamworks;

// Optional adapter. The existing Steam host owns Init, RunCallbacks and Shutdown.
// Connect only after Init succeeds; the readiness delegate must include stats readiness
// for SDK versions that require an explicit stats request. No App ID is embedded here.
public sealed class SteamworksAchievementAdapter : IAchievementPlatform, IDisposable
{
    readonly Func<bool> statsReady;
    readonly ulong gameId;
    readonly Callback<UserStatsStored_t> stored;
    bool disposed;
    public SteamworksAchievementAdapter(Func<bool> readiness)
    {
        if(readiness==null)throw new ArgumentNullException(nameof(readiness));
        statsReady=readiness;gameId=SteamUtils.GetAppID().m_AppId;
        stored=Callback<UserStatsStored_t>.Create(OnStored);
        AchievementPlatform.Register(this);
    }
    public bool Ready=>!disposed && statsReady();
    public bool IsUnlocked(string apiName)=>Ready && SteamUserStats.GetAchievement(apiName,out bool earned) && earned;
    public bool SetAchievement(string apiName)=>Ready && SteamUserStats.SetAchievement(apiName);
    public bool StoreStats()=>Ready && SteamUserStats.StoreStats();
    void OnStored(UserStatsStored_t result)
    {
        if(disposed || result.m_nGameID!=gameId)return;
        if(result.m_eResult==EResult.k_EResultOK)AchievementPlatform.ConfirmStored();
        else AchievementPlatform.UploadFailed();
    }
    public void Dispose(){if(disposed)return;disposed=true;AchievementPlatform.Unregister(this);stored.Dispose();}
}
#endif
