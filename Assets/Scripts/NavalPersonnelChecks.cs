#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Reflection;
using UnityEngine;

public static class NavalPersonnelChecks
{
    static int checks;
    static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException("NAVAL PERSONNEL: " + label); checks++; }
    static int I(string key) => DailyLifeEvents.I(key);
    static void Set(string key, int value) => VariableSingleton.SetInt(key, value);
    static void Move(int day) => typeof(VariableSingleton).GetField("turn", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, VariableSingleton.EffectiveSiegeTurn + day);
    static string State() => JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    static void Fixture()
    {
        TrainingPool.Orders.Clear(); SpecialistPersonnel.ResetForRestore(); SpecialistPersonnel.EnsureInitialized(); CountryDiplomacy.BeginCampaign();
        NavalState.Initialise(); Set("iShips", 10); Set("iDamagedShips", 0); Set("iEngineers", 8); Set("iMoney", 10000); Set("iMaterials", 1000); Set("iPopulation", 100000); Set("iCannons", 0);
        Set("iNavalRepair", 0); Set("iNavalBase", 0); Set("iNavalStores", 0); Set("iNavalPilots", 0); Set("iFleetPrepared", 0); Set("iLifeMentorDue", 0);
        SpecialistPersonnel.CommitLocalManpower(500);
    }
    public static int Run()
    {
        checks = 0; var original = VariableSingleton.Instance.CaptureState(); var rng = UnityEngine.Random.state;
        try {
            Fixture();
            Check(NavalState.SinkShips(99) == 10 && I("iShips") == 0 && I("iSpecialistLostCrew") == 500 && I("iSpecialistLostEngineers") == 10, "losses clamp to actual hulls and count fifty crew including one engineer");
            Check(I("iEngineers") == 8 && SpecialistPersonnel.LocalManpowerCommitted == 500, "sinking refunds neither specialist availability nor crew manpower");
            string state = State();
            Check(NavalState.SinkShips(-1) == 0 && NavalState.SinkShips(1) == 0 && State() == state, "negative or unavailable loss cannot create casualties");
            Fixture(); NavalState.SinkShipsCommand(1);
            Check(I("iShips") == 9 && I("iSpecialistLostCrew") == 50, "Yarn fire-ship adapter records its real destroyed hull");

            Fixture(); Set("iShips", 3); Set("iFleet", 0); ReliefStormSystem.Apply(2, -1, 10, 5);
            Check(I("iShips") == 0 && I("iSpecialistLostCrew") == 150 && I("iSpecialistLostEngineers") == 3, "exposed storm loss counts actual crews, capped by three ships");
            Fixture(); Set("iFleet", 3); ReliefStormSystem.Apply(2, -1, 10, 5);
            Check(I("iShips") == 10 && I("iSpecialistLostCrew") == 0, "sheltered fleet has no invented storm casualties");
            Fixture(); Set("iShips", 0); Set("iAidState1", 1); Set("iAidDays1", 10); ReliefStormSystem.Apply(2, 1, 10, 5);
            Check(I("iAidState1") == 4 && I("iSpecialistLostCrew") == 0 && I("iEngineers") == 8, "foreign relief loss does not charge local crews or engineers");

            Fixture(); PreparationCampaign.Retreat();
            Check(I("iShips") == 5 && I("iDamagedShips") == 2 && I("iSpecialistLostCrew") == 150 && I("iSpecialistLostEngineers") == 3, "retreat distinguishes five operational, two damaged and three sunk");
            Check(I("iEngineers") == 8 && SpecialistPersonnel.OperationalEngineers == 7, "damaged survivors retain their operational engineers");
            Check(TrainingPool.PlaceShipRepairs(2, out var report), "damaged survivors can enter the unified repair queue");
            Set("iNavalRepair", 2); PreparationCampaign.FleetMultiplier(3, true);
            Check(I("iShips") == 5 && I("iDamagedShips") == 2 && I("iNavalRepair") == 2 && SpecialistPersonnel.RepairingShips == 2, "automatic repair yards cannot recover queued hulls or waste their charge");
            Set("iDamagedShips", 3); PreparationCampaign.FleetMultiplier(3, true);
            Check(I("iShips") == 6 && I("iDamagedShips") == 2 && I("iNavalRepair") == 1, "automatic yards can still restore the unreserved hull only");
            TrainingPool.CompleteDueOrders(VariableSingleton.GetDate().AddDays(5));
            Check(I("iShips") == 8 && I("iDamagedShips") == 0 && I("iEngineers") == 8 && I("iSpecialistLostCrew") == 150, "queued repair returns two engineers per hull without duplicating survivors or deaths");

            Fixture(); Set("iShips", 100); Set("iTurkShips", 100); int venice = CountryDiplomacy.Index("venice"); float goodwill = CountryDiplomacy.Goodwill(venice);
            NavalState.Battle(true); int actual = 100 - I("iShips");
            Check(I("iSpecialistLostCrew") == actual * 50 && I("iSpecialistLostEngineers") == actual && I("iEngineers") == 8, "real naval battle records exact sunk crews without a second engineer deduction");
            Check(CountryDiplomacy.Goodwill(venice) == goodwill + 10 && I("iNavalDiplomacyReward") == 1, "major victory reaches unified maritime relationships");
            var saved = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State());
            Check(!DiplomaticBattleEvents.NavalVictory(true, 100, 50) && CountryDiplomacy.Goodwill(venice) == goodwill + 10, "naval reputation reward cannot be farmed");
            VariableSingleton.Instance.RestoreValidated(saved);
            Check(!DiplomaticBattleEvents.NavalVictory(true, 100, 50) && CountryDiplomacy.Goodwill(venice) == goodwill + 10, "naval reward remains once-only after save restoration");
            Fixture(); goodwill = CountryDiplomacy.Goodwill(venice);
            Check(!DiplomaticBattleEvents.NavalVictory(false, 100, 20) && !DiplomaticBattleEvents.NavalVictory(true, 2, 1) && CountryDiplomacy.Goodwill(venice) == goodwill, "defeat and minor naval incidents grant no major-victory reward");

            Fixture(); Move(1); int hungary = CountryDiplomacy.Index("hungary"), mamluks = CountryDiplomacy.Index("mamluks");
            goodwill = CountryDiplomacy.Goodwill(hungary); float hostileGoodwill = CountryDiplomacy.Goodwill(mamluks);
            Check(DiplomaticBattleEvents.HeldLandDefence(1000, true, true).Length > 0 && CountryDiplomacy.Goodwill(hungary) == goodwill + 5, "major held assault improves relevant partner goodwill");
            Check(CountryDiplomacy.Goodwill(mamluks) == hostileGoodwill && CountryDiplomacy.Attitude(hungary) == CountryDiplomacy.AttitudeLevel.Wary, "defence reward neither improves unrelated hostile court nor promotes attitude");
            Check(DiplomaticBattleEvents.HeldLandDefence(1000, true, true) == "" && DiplomaticBattleEvents.SuccessfulDefenceRewards == 1, "one settled assault cannot count twice");
            Move(2);
            Check(DiplomaticBattleEvents.HeldLandDefence(999, true, true) == "" && DiplomaticBattleEvents.HeldLandDefence(1000, false, true) == "" && DiplomaticBattleEvents.HeldLandDefence(1000, true, false) == "", "minor, lost and fatal assaults cannot earn defence goodwill");
            Check(DiplomaticBattleEvents.HeldLandDefence(1000, true, true).Length > 0 && CountryDiplomacy.Goodwill(hungary) == goodwill + 10, "second distinct major defence earns the final gain");
            saved = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State()); VariableSingleton.Instance.RestoreValidated(saved); Move(3);
            Check(DiplomaticBattleEvents.HeldLandDefence(10000, true, true) == "" && DiplomaticBattleEvents.SuccessfulDefenceRewards == 2 && CountryDiplomacy.Goodwill(hungary) == goodwill + 10, "two-defence campaign ceiling survives loading");
            Debug.Log("NAVAL_PERSONNEL_CHECKS " + checks + " passed"); return checks;
        }
        finally { SpecialistPersonnel.ResetForRestore(); CountryDiplomacy.ResetForRestore(); VariableSingleton.Instance.RestoreValidated(original); UnityEngine.Random.state = rng; }
    }
}
#endif
