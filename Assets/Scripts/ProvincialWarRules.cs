using System;
using System.Linq;
using UnityEngine;

// Editable playtest defaults. Troops are headcounts; Ottoman food retains its existing food-day unit.
[Serializable] public sealed class ProvincialWarRules
{
    public int Version,MinimumArmy,LaunchResistance,LaunchTroops,MusterMin,MusterMax,RecruitsPerResistance;
    public int BattleChance,BattleCooldown,ShipmentInterval,ChaosDays,AuthorityRecoveryDays;
    public bool PauseGarrisonDuringUprising;
    public int TerritoryMin,TerritoryMax,PillageMin,PillageMax,PillageGold,SkirmishMin,SkirmishMax;
    public int WealthRecovery,ClandestineMin,ClandestineMax,SecretsAward,PlanCost,RestoreCost;
    public int RestoreChance,StrongRestoreChance,StrongResistance,EndorseChance,LocalChance,AllyClaimThreshold;
    public int CessationChance,PeaceChance,CessationDays;
    public int AssistanceInterval,AssistanceChance,AssistanceCost,AssistanceWealth,AssistanceChaosChance,EnemyRenewChance;
    public int CautionPlains,CautionOther,SpeedPlains,SpeedOther,CoordinationChance,SpeedParticipation,CautionBonus,TacticBonus,LocalBonus,LocalMinimum;
    public int ManeuverLossMin,ManeuverLossMax,ManeuverCap,ClashCap,InconclusiveChance,WinnerLossMin,WinnerLossMax,LoserLossMin,LoserLossMax,DrawLossMin,DrawLossMax;
    public int RoutLow,RoutMedium,RoutHigh,CoverCost,CoverChance,CounterChance,PursuitLossMin,PursuitLossMax,ConsolidateLossMin,ConsolidateLossMax,CounterLossMin,CounterLossMax;
    public int[] WithdrawalMin,WithdrawalMax,EscapeWeights;
    public int VictoryAuthority,DecisiveAuthority,VictoryTerritory,DecisiveTerritory,OttomanSeekStrong,OttomanSeekWeak,OttomanLowAuthority;
    public Province[] Provinces;public Ally[] Allies;public string BalanceNotes;
    [Serializable] public sealed class Province {public string Id;public int Garrison,Replacement,FoodDays,Recruits,Gold,Cannons,Ammunition,Ships,TravelDays,Tribute,RebelCap;public int[] Terrain;}
    [Serializable] public sealed class Ally {public string Id;public int Troops,Cap,Reserve,DailyReplacement;public string[] Targets;}
    static ProvincialWarRules data;
    public static ProvincialWarRules Data {
        get {
            if(data==null){data=JsonUtility.FromJson<ProvincialWarRules>(Resources.Load<TextAsset>("Content/ProvincialWarRules").text);Validate(data);}
            return data;
        }
    }
    public static void Validate(ProvincialWarRules r)
    {
        if(r==null||r.Version!=1||r.Provinces==null||r.Provinces.Length!=WarTheatre.ProvinceIds.Length||r.MinimumArmy<1||r.ShipmentInterval<1||r.BattleCooldown<1||r.RecruitsPerResistance<1||r.MusterMin<0||r.MusterMax<r.MusterMin)throw new Exception("Invalid provincial war rules");
        if(r.Provinces.Select(x=>x.Id).Distinct().Count()!=r.Provinces.Length)throw new Exception("Duplicate province rules");
        foreach(var p in r.Provinces)if(!WarTheatre.ProvinceIds.Contains(p.Id)||p.Garrison<1||p.Replacement<0||p.TravelDays<1||p.Terrain.Length!=3||p.Terrain.Sum()!=100||p.RebelCap<r.LaunchTroops)throw new Exception("Invalid province "+p.Id);
        foreach(var a in r.Allies)if(CountryDiplomacy.Index(a.Id)<0||a.Targets.Any(x=>!WarTheatre.ProvinceIds.Contains(x))||a.Troops<1||a.Cap<a.Troops||a.Reserve<0)throw new Exception("Invalid ally "+a.Id);
    }
}
