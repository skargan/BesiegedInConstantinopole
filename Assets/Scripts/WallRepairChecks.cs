#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Opt-in checks. Run() requires an initialised, isolated test campaign; it restores it afterwards.
public static class WallRepairChecks
{
    static int checks;
    static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException("WALL REPAIR: " + label); checks++; }
    static bool Near(float a, float b) => Mathf.Abs(a - b) < .001f;
    static int I(string key) => DailyLifeEvents.I(key);
    static string Rng() => JsonUtility.ToJson(UnityEngine.Random.state);
    static string State() => JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    static void Set(string key, int value) => VariableSingleton.SetInt(key, value);
    static void Integrity(float value) => VariableSingleton.ChangePercentFloat("fOuterWall", value - VariableSingleton.GetFloatVariable("fOuterWall"));
    static WallRepairRules.RepairPlan Plan(string wall = "fOuterWall", int n = 10, float integrity = .5f, int engineers = 30, int gold = 10000, int materials = 1000, int cap = 10)
        => WallRepairRules.Calculate(wall, n, integrity, engineers, gold, materials, cap);

    public static void VerifyPreview()
    {
        checks = 0; PreviewChecks(); Debug.Log("WALL_REPAIR_PREVIEW_CHECKS " + checks + " passed");
    }

    static void PreviewChecks()
    {
        var p = Plan();
        Check(p.Engineers == 10 && p.Gold == 500 && p.Materials == 30 && p.MinimumRestored == 10 && p.MaximumRestored == 15, "ten engineers cost 500/30 and restore 10–15");
        Check(Plan("fMiddleWall").Materials == 20 && Plan("fInnerWall").Materials == 10, "middle and inner material rates");
        Check(Plan(n: 20).Engineers == 10, "ordinary work detail capped at ten");
        p = Plan(n: 20, cap: 20);
        Check(p.Engineers == 20 && p.Gold == 1000 && p.Materials == 60 && p.MaximumRestored == 30, "masonry allows twenty without material discount");
        Check(Plan(n: 3).MaximumRestored == 4 && Plan(n: 1).MaximumRestored == 1, "odd repair upper bound rounds down");
        Check(Plan(engineers: 7).MaximumEngineers == 7 && Plan(engineers: 0).MaximumEngineers == 0, "availability constrains assignment");
        Check(Plan(gold: 499).MaximumEngineers == 9 && Plan(gold: 49).MaximumEngineers == 0, "gold boundary constrains assignment");
        Check(Plan(materials: 29).MaximumEngineers == 9 && Plan(materials: 2).MaximumEngineers == 0, "material boundary constrains assignment");
        Check(Plan(n: 20, integrity: .90f, cap: 20).MaximumEngineers == 10, "exact ten-point gap has no floating-point phantom engineer");
        p = Plan(n: 20, integrity: .976f, cap: 20);
        Check(p.Engineers == 3 && Near(p.MinimumRestored, 2.4f) && Near(p.MaximumRestored, 2.4f) && p.BonusCapped, "fractional gap rounds assignment up, output caps at actual gap");
        p = Plan(n: 3, integrity: .965f);
        Check(Near(p.MinimumRestored, 3) && Near(p.MaximumRestored, 3.5f) && p.BonusCapped, "only bonus restoration is capped when base fits");
        Check(!Plan(integrity: 1).CanRepair && !Plan("fMoat").CanRepair && !Plan("iSeaIntegrity").CanRepair, "full walls, moat and sea wall cannot enter land-wall action");
        Check(!Plan(n: 0).CanRepair && !Plan(n: -1).CanRepair, "non-positive assignment cannot commit");
        Check(WallRepairRules.GoldFor(10, .05f) == 475 && WallRepairRules.GoldFor(10, .15f) == 425, "existing labour modifier applies to the fifty-gold base");
        p = WallRepairRules.Calculate("fOuterWall", 10, .5f, 30, 474, 1000, 10, .05f);
        Check(p.Engineers == 9 && p.Gold <= 474 && p.Materials == 27, "Notaras five-percent discount participates in the gold affordability cap");
        p = WallRepairRules.Calculate("fOuterWall", 10, .5f, 30, 425, 1000, 10, .15f);
        Check(p.Engineers == 10 && p.Gold == 425 && p.Materials == 30, "commander discount affords ten at exactly 425 gold without discounting materials");
        Check(WallRepairRules.Calculate("fOuterWall", 10, .5f, 30, 424, 1000, 10, .15f).Engineers == 9 && WallRepairRules.Calculate("fOuterWall", 10, .5f, 30, 425, 29, 10, .15f).Engineers == 9, "discounted one-gold and one-material shortfalls independently cap the detail");
    }

    static void Fixture(int engineers = 30, float integrity = .5f)
    {
        TrainingPool.Orders.Clear(); SpecialistPersonnel.ResetForRestore(); MedicalRecovery.ResetForRestore();
        Set("iEngineers", engineers); Set("iMoney", 10000); Set("iMaterials", 1000); Set("iHealers", 100); Set("iProvisions", 10000);
        Set("iLifeMentorDue", 0); VariableSingleton.ChangeBool(CourtRoster.Keys[2], false);
        VariableSingleton.ChangeBool("bTechRomanConcrete", false); Integrity(integrity);
        SpecialistPersonnel.EnsureInitialized();
    }

    public static int Run()
    {
        checks = 0;
        var original = VariableSingleton.Instance.CaptureState(); var originalRng = UnityEngine.Random.state;
        try {
            PreviewChecks(); Fixture();
            string state = State(), rng = Rng();
            for (int n = 0; n <= 25; n++) WallRepairRules.Preview("fOuterWall", n);
            Check(State() == state && Rng() == rng, "preview is read-only and consumes no gameplay RNG");
            Check(!WallRepairRules.TryRepair("fOuterWall", 11, out var report) && State() == state && Rng() == rng, "stale or over-cap request rejected atomically");
            Set("iMaterials", 29); state = State(); rng = Rng();
            Check(!WallRepairRules.TryRepair("fOuterWall", 10, out report) && State() == state && Rng() == rng, "insufficient exact assignment cannot partially debit");
            Fixture(0); state = State(); rng = Rng();
            Check(!WallRepairRules.TryRepair("fOuterWall", 1, out report) && State() == state && Rng() == rng, "zero available engineers cannot debit or roll");
            Fixture(); VariableSingleton.ChangeBool("bTechRomanConcrete", true);
            Check(WallRepairRules.Preview("fOuterWall", 20).Engineers == 20 && WallRepairRules.Materials("fOuterWall", 20) == 60, "stable RomanConcrete flag enables cap twenty");
            Check(DefencePosture.RepairMaterials == 30, "existing sea-wall material discount remains separate");

            // Find a seeded real RNG sequence for every casualty result (0..99), then
            // exercise the actual commit for all hundred: precisely 95/2/3 tasks.
            var seeds = new Dictionary<int, int>();
            for (int seed = 0; seed < 20000 && seeds.Count < 100; seed++) {
                UnityEngine.Random.InitState(seed); UnityEngine.Random.Range(10, 16);
                int roll = UnityEngine.Random.Range(0, 100); if (!seeds.ContainsKey(roll)) seeds.Add(roll, seed);
            }
            Check(seeds.Count == 100, "seeded coverage of every casualty band");
            int killedTasks = 0, woundedTasks = 0, safeTasks = 0; var outputs = new HashSet<int>();
            foreach (var sample in seeds) {
                Fixture(); UnityEngine.Random.InitState(sample.Value);
                int expectedPoints = UnityEngine.Random.Range(10, 16); UnityEngine.Random.Range(0, 100); string expectedRng = Rng();
                UnityEngine.Random.InitState(sample.Value);
                int turn = VariableSingleton.Turn; DateTime day = VariableSingleton.GetDate();
                Check(WallRepairRules.TryRepair("fOuterWall", 10, out report), "repair commits for casualty sample " + sample.Key);
                int killed = sample.Key >= 95 && sample.Key < 97 ? 1 : 0, wounded = sample.Key >= 97 ? 1 : 0;
                Check(Near(VariableSingleton.GetFloatVariable("fOuterWall") * 100, 50 + expectedPoints), "full output precedes casualty " + sample.Key);
                Check(Rng() == expectedRng, "exactly one output roll and one task casualty roll " + sample.Key);
                Check(I("iMoney") == 9500 && I("iMaterials") == 970 && SpecialistPersonnel.AvailableEngineers == 20, "single debit and reservation " + sample.Key);
                Check(SpecialistPersonnel.RecuperatingEngineers == 10 - killed - wounded && SpecialistPersonnel.WoundedEngineers == wounded && I("iSpecialistLostEngineers") == killed, "one mutually exclusive casualty " + sample.Key);
                Check(VariableSingleton.Turn == turn && VariableSingleton.GetDate() == day, "restoration does not prematurely advance the day " + sample.Key);
                Check(report.Contains("Engineers assigned: 10") && report.Contains("500 gold") && report.Contains("30 materials") && report.Contains(day.AddDays(2).ToString("d MMM yyyy")), "complete outcome report " + sample.Key);
                outputs.Add(expectedPoints); killedTasks += killed; woundedTasks += wounded; if (killed + wounded == 0) safeTasks++;
            }
            Check(safeTasks == 95 && killedTasks == 2 && woundedTasks == 3, "task probability partition is 95/2/3");
            Check(outputs.Count == 6 && outputs.Contains(10) && outputs.Contains(15), "inclusive output endpoints and every integer between occur");

            foreach (int n in new[] { 1, 3, 20 }) {
                Fixture(); VariableSingleton.ChangeBool("bTechRomanConcrete", true); UnityEngine.Random.InitState(123);
                int expected = UnityEngine.Random.Range(n, n * 3 / 2 + 1); UnityEngine.Random.InitState(123);
                Check(WallRepairRules.TryRepair("fOuterWall", n, out report) && Near(VariableSingleton.GetFloatVariable("fOuterWall") * 100, 50 + expected), "actual output for " + n + " engineers");
            }
            Fixture(30, .976f);
            Check(WallRepairRules.TryRepair("fOuterWall", 3, out report) && Near(VariableSingleton.GetFloatVariable("fOuterWall"), 1) && report.Contains("+" + 2.4f.ToString("0.##") + " points"), "actual overrepair caps precisely and reports fractional gain");

            Fixture(); DateTime assigned = VariableSingleton.GetDate(); UnityEngine.Random.InitState(seeds[0]);
            WallRepairRules.TryRepair("fOuterWall", 10, out report);
            SpecialistPersonnel.Advance(assigned.AddDays(1)); TrainingPool.CompleteDueOrders(assigned.AddDays(1));
            Check(SpecialistPersonnel.AvailableEngineers == 20 && SpecialistPersonnel.RecuperatingEngineers == 10, "repair personnel absent on D+1");
            var saved = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State());
            SpecialistPersonnel.Advance(assigned.AddDays(2));
            Check(SpecialistPersonnel.AvailableEngineers == 30 && SpecialistPersonnel.RecuperatingEngineers == 0, "all safe workers return on D+2");
            VariableSingleton.Instance.RestoreValidated(saved);
            Check(SpecialistPersonnel.AvailableEngineers == 20 && SpecialistPersonnel.RecuperatingEngineers == 10, "save round-trip preserves pending repair recovery");
            SpecialistPersonnel.Advance(assigned.AddDays(2)); SpecialistPersonnel.Advance(assigned.AddDays(2));
            Check(SpecialistPersonnel.AvailableEngineers == 30 && SpecialistPersonnel.RecuperatingEngineers == 0, "repeated daily processing cannot duplicate returns");

            Fixture(); UnityEngine.Random.InitState(seeds[99]); WallRepairRules.TryRepair("fOuterWall", 10, out report);
            saved = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State());
            SpecialistPersonnel.Advance(assigned.AddDays(2));
            Check(SpecialistPersonnel.AvailableEngineers == 29 && SpecialistPersonnel.WoundedEngineers == 1, "wounded engineer never joins ordinary D+2 return");
            VariableSingleton.Instance.RestoreValidated(saved);
            Check(SpecialistPersonnel.RecuperatingEngineers == 9 && SpecialistPersonnel.WoundedEngineers == 1, "save round-trip preserves wounded separately");
            Set("iHealers", 0); MedicalRecovery.Advance(assigned.AddDays(20));
            Check(SpecialistPersonnel.WoundedEngineers == 1, "wounded engineer waits without free healing when no monks are available");
            Set("iHealers", 100); MedicalRecovery.Advance(assigned.AddDays(35)); SpecialistPersonnel.Advance(assigned.AddDays(35));
            Check(SpecialistPersonnel.WoundedEngineers == 0 && SpecialistPersonnel.AvailableEngineers == 30, "medical treatment eventually returns the wounded engineer exactly once");
            Debug.Log("WALL_REPAIR_CHECKS " + checks + " passed"); return checks;
        }
        finally { SpecialistPersonnel.ResetForRestore(); VariableSingleton.Instance.RestoreValidated(original); UnityEngine.Random.state = originalRng; }
    }
}
#endif
