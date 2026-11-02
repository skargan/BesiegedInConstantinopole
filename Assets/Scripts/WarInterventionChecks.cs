#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Requires an isolated, initialised test campaign. Exercises the real finite parcel ledger.
public static class WarInterventionChecks
{
    static int checks;
    static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException("INTERVENTION: " + label); checks++; }
    static int I(string key) => DailyLifeEvents.I(key);
    static void Set(string key, int value) => VariableSingleton.SetInt(key, value);
    static void Move(int siegeDay) => typeof(VariableSingleton).GetField("turn", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, VariableSingleton.EffectiveSiegeTurn + siegeDay);
    static string State() => JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    static string Rng() => JsonUtility.ToJson(UnityEngine.Random.state);
    static void Fixture()
    {
        ProvincialIntelligence.ResetForRestore(); CountryDiplomacy.ResetForRestore(); Move(20); CountryDiplomacy.Ensure();
        ProvinceCampaign.Set("Ready", 1); ProvinceCampaign.Set("LastDay", ProvinceCampaign.Day); ProvinceCampaign.Set("FoodTick", 19);
        Set("iFood", 100); Set("iBalkans", 1000); Set("iBazouks", 2000); Set("iJanissaries", 3000); Set("iTurkCannons", 10); Set("iOttAmmo", 50);
        ProvinceCampaign.Set("Reserve0", 5500); ProvinceCampaign.Set("Reserve1", 11000); ProvinceCampaign.Set("Reserve2", 16500);
        for (int i = 0; i < WarTheatre.ProvinceIds.Length; i++) for (int wave = 0; wave < 5; wave++) {
            string k = i + "_" + wave; ProvinceCampaign.Set("Done" + k, 1); ProvinceCampaign.Set("Due" + k, 1000); ProvinceCampaign.Set("Food" + k, 4);
        }
    }
    static void Parcel(int province, int wave = 0, int due = 20)
    {
        string key = province + "_" + wave; ProvinceCampaign.Set("Done" + key, 0); ProvinceCampaign.Set("Due" + key, due);
    }
    static void Activate(int country, int state = 3, int dueOffset = -1)
    {
        Set("iDiplomacyWar_" + country, state); Set("iDiplomacyWarDue_" + country, CountryDiplomacy.Day + dueOffset);
    }
    static void Reload(VariableSingleton.CampaignSave saved) => VariableSingleton.Instance.RestoreValidated(saved);

    public static int Run()
    {
        checks = 0; var original = VariableSingleton.Instance.CaptureState(); var originalRng = UnityEngine.Random.state;
        try {
            int country = CountryDiplomacy.Index("hungary"), province = Array.IndexOf(WarTheatre.ProvinceIds, "thrace");
            var d = CountryDiplomacy.Definitions[country];
            Check(d.ShipmentBudget > 0 && d.ShipmentBudget < 5 && d.DelayDays > 0 && d.TargetProvinces.Contains("thrace"), "editable Hungarian intervention fixture is finite and targets Thrace");
            Fixture(); Parcel(province);
            ProvinceCampaign.SupplyDay();
            Check(ProvinceCampaign.Get("Done0_0") == 1 && I("iBalkans") == 1100 && I("iFood") == 103 && WarInterventions.UsedShipments(country) == 0, "no commitment means ordinary finite delivery");

            Fixture(); Activate(country, 2, 2); Parcel(province);
            Check(!WarTheatre.AtWar(WarTheatre.Find("hungary")), "mobilising court is not yet shown as intervening");
            ProvinceCampaign.SupplyDay();
            Check(ProvinceCampaign.Get("Done0_0") == 1 && WarInterventions.UsedShipments(country) == 0, "mobilisation does not disrupt early deliveries");

            Fixture(); Activate(country, 2, 0); Set("iDiplomacyLastDay", CountryDiplomacy.Day - 1);
            CountryDiplomacy.Advance(VariableSingleton.GetDate()); Parcel(province);
            Check(WarTheatre.AtWar(WarTheatre.Find("hungary")), "due-day mobilisation reaches the same map identity");
            string rng = Rng(); string stoppage = ProvinceCampaign.SupplyDay();
            Check(stoppage == "" && I("iFood") == 99, "intervention notification does not itself stop a supplied Ottoman action");
            Check(ProvinceCampaign.Get("Due0_0") == 20 + d.DelayDays && ProvinceCampaign.Get("Done0_0") == 0 && I("iBalkans") == 1000 && I("iTurkCannons") == 10 && I("iOttAmmo") == 50, "entire original parcel waits without early cargo or duplicate reserves");
            Check(WarInterventions.UsedShipments(country) == 1 && WarInterventions.RemainingShipments(country) == d.ShipmentBudget - 1 && WarInterventions.CountryForShipment(province, 0) == country, "one shipment consumes one finite commitment");
            Check(Rng() == rng, "intervention scheduling consumes no additional randomness");
            var saved = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State());
            string repeated = State(); ProvinceCampaign.SupplyDay();
            Check(State() == repeated, "same supply day is idempotent");
            string report = ProvinceCampaign.TakeDeliveryReport();
            Check(report.Contains(d.Name) && report.Contains("by " + d.DelayDays + " days") && ProvinceCampaign.TakeDeliveryReport() == "", "actual disruption is reported once");
            Reload(saved);
            Check(WarInterventions.UsedShipments(country) == 1 && ProvinceCampaign.Get("Due0_0") == 20 + d.DelayDays && ProvinceCampaign.TakeDeliveryReport().Contains(d.Name), "save round-trip keeps delay, spent budget and unread report");
            Move(20 + d.DelayDays); ProvinceCampaign.SupplyDay();
            Check(ProvinceCampaign.Get("Done0_0") == 1 && I("iBalkans") == 1100 && I("iBazouks") == 2200 && I("iJanissaries") == 3300 && I("iTurkCannons") == 11 && I("iOttAmmo") == 62 && I("iFood") == 102, "delayed cargo arrives once with its original finite troops, food, guns and ammunition");
            Check(WarInterventions.UsedShipments(country) == 1, "rescheduled delivery cannot be delayed a second time");
            repeated = State(); ProvinceCampaign.SupplyDay();
            Check(State() == repeated, "delivered parcel cannot pay out twice");

            Fixture(); Activate(country);
            for (int wave = 0; wave <= d.ShipmentBudget; wave++) Parcel(province, wave);
            ProvinceCampaign.SupplyDay();
            Check(WarInterventions.UsedShipments(country) == d.ShipmentBudget && WarInterventions.RemainingShipments(country) == 0, "intervention budget cannot refill or exceed its definition");
            Check(ProvinceCampaign.Get("Done0_" + d.ShipmentBudget) == 1 && I("iBalkans") == 1100, "first parcel beyond the budget is delivered normally");
            Move(20 + d.DelayDays); ProvinceCampaign.SupplyDay();
            Check(I("iBalkans") == 1000 + (d.ShipmentBudget + 1) * 100, "all delayed finite reserves eventually arrive without multiplication");

            foreach (bool liberated in new[] { false, true }) {
                Fixture(); Activate(country); Parcel(province);
                if (liberated) Set("iWbwCaptured_thrace", 1); else ProvinceCampaign.Set("Battle_thrace", 1);
                ProvinceCampaign.SupplyDay();
                Check(ProvinceCampaign.Get("Done0_0") == 1 && WarInterventions.UsedShipments(country) == 0 && I("iBalkans") == 1000, (liberated ? "liberation" : "rebellion") + " cancels the parcel before allied disruption can spend its budget");
            }
            Fixture(); Activate(country); int other = Array.FindIndex(WarTheatre.ProvinceIds, id => !d.TargetProvinces.Contains(id)); Parcel(other);
            ProvinceCampaign.SupplyDay();
            Check(WarInterventions.UsedShipments(country) == 0 && ProvinceCampaign.Get("Done" + other + "_0") == 1, "intervention cannot affect an unrelated province");
            Fixture(); Activate(country, 3, -2); Parcel(province, 0, 15); ProvinceCampaign.SupplyDay();
            Check(WarInterventions.UsedShipments(country) == 0 && ProvinceCampaign.Get("Done0_0") == 1, "late processing cannot retroactively disrupt a parcel due before mobilisation");
            Fixture(); Activate(country, 3, -100); Parcel(province, 0, 1); ProvinceCampaign.SupplyDay();
            Check(WarInterventions.UsedShipments(country) == 0 && ProvinceCampaign.Get("Done0_0") == 1, "expired disruption window does not restart when catching up a save");

            // Exercise the actual morning boundary rather than only calling Advance by hand.
            Fixture(); Activate(country, 2, 1); Parcel(province, 0, 21); Set("iFleet", 5);
            VariableSingleton.IncrementTurn();
            Check(VariableSingleton.CompletedSiegeDays == 21 && CountryDiplomacy.IsIntervening(country) && I("iFood") == 100, "real day increment mobilises the ally while domestic provisions remain separate from Ottoman stores");
            ProvinceCampaign.SupplyDay();
            Check(ProvinceCampaign.Get("Due0_0") == 21 + d.DelayDays && ProvinceCampaign.Get("Done0_0") == 0 && WarInterventions.UsedShipments(country) == 1, "shipment due on mobilisation day is delayed after the real calendar boundary");

            Fixture(); Activate(country);
            int second = Enumerable.Range(0, CountryDiplomacy.Definitions.Length).First(i => i != country && CountryDiplomacy.Definitions[i].ShipmentBudget > 0 && CountryDiplomacy.Definitions[i].TargetProvinces.Contains("thrace"));
            Activate(second); Parcel(province); ProvinceCampaign.SupplyDay();
            Check(WarInterventions.UsedShipments(country) + WarInterventions.UsedShipments(second) == 1, "two allies cannot claim the same parcel");
            saved = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State()); Reload(saved);
            Check(!WarInterventions.TryDelayShipment(province, 0, ProvinceCampaign.Get("Due0_0")) && WarInterventions.UsedShipments(country) + WarInterventions.UsedShipments(second) == 1, "per-parcel marker survives loading and prevents stacked delay");
            Debug.Log("WAR_INTERVENTION_CHECKS " + checks + " passed"); return checks;
        }
        finally { ProvincialIntelligence.ResetForRestore(); CountryDiplomacy.ResetForRestore(); VariableSingleton.Instance.RestoreValidated(original); UnityEngine.Random.state = originalRng; }
    }
}
#endif
