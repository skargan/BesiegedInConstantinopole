using System;
using UnityEngine;
// Land-wall work is immediate. Personnel recovery is dated separately by SpecialistPersonnel.
// Moat and sea-wall actions retain their own rules.
public static class WallRepairRules
{
    public const int GoldPerEngineer = 50;
    public const int RecoveryDays = 2;
    public const int DefaultAssignment = 10;

    public sealed class RepairPlan
    {
        public int Engineers, MaximumEngineers, AvailableEngineers, AssignmentCap;
        public int Gold, Materials, MaximumRolledPoints;
        public float Before, MinimumRestored, MaximumRestored;
        public bool BonusCapped;
        public string Unavailable;
        public bool CanRepair => Engineers > 0 && Unavailable == null;
    }

    public static int MaterialsPerEngineer(string wall) => wall == "fOuterWall" ? 3 : wall == "fMiddleWall" ? 2 : wall == "fInnerWall" ? 1 : 0;
    // Compatibility for existing callers that display the former ten-person work detail.
    public static int BaseMaterials(string wall) => MaterialsPerEngineer(wall) * DefaultAssignment;
    public static int Materials(string wall) => BaseMaterials(wall);
    public static int Materials(string wall, int engineers) => MaterialsPerEngineer(wall) * Mathf.Max(0, engineers);
    public static float LabourDiscount => CourtRoster.Bonus(2, .05f, .15f);
    public static int Labour => GoldFor(DefaultAssignment, LabourDiscount);
    public static int GoldFor(int engineers, float discount) => Mathf.RoundToInt(GoldPerEngineer * Mathf.Max(0, engineers) * (1f - Mathf.Clamp01(discount)));
    public static DateTime ReturnDate => VariableSingleton.GetDate().Date.AddDays(RecoveryDays);

    // Opening or dragging the slider never spends RNG.
    public static RepairPlan Calculate(string wall, int requestedEngineers, float integrity, int availableEngineers,
        int gold, int materials, int assignmentCap, float labourDiscount = 0)
    {
        var plan = new RepairPlan {
            Before = Mathf.Clamp01(integrity) * 100f,
            AvailableEngineers = Mathf.Max(0, availableEngineers),
            AssignmentCap = Mathf.Max(0, assignmentCap)
        };
        int materialRate = MaterialsPerEngineer(wall);
        float missing = Mathf.Max(0, 100f - plan.Before);
        if (materialRate == 0) plan.Unavailable = "Choose a land wall to repair.";
        else if (missing <= 0) plan.Unavailable = "This wall is already at full integrity.";
        else if (availableEngineers <= 0) plan.Unavailable = "No engineers are available. Assigned or wounded engineers must return first.";
        else if (materials < materialRate) plan.Unavailable = "At least " + materialRate + " materials are needed for one engineer.";
        else if (gold < GoldFor(1, labourDiscount)) plan.Unavailable = "At least " + GoldFor(1, labourDiscount) + " gold is needed for one engineer.";

        // 90% stored as a float must not turn ten missing points into eleven.
        int missingLimit = missing > 0 ? Mathf.Max(1, Mathf.CeilToInt(missing - .0001f)) : 0;
        int limit = Mathf.Min(plan.AssignmentCap, Mathf.Min(plan.AvailableEngineers, missingLimit));
        if (plan.Unavailable == null)
            for (int n = 1; n <= limit; n++)
            {
                if (GoldFor(n, labourDiscount) > gold || materialRate * n > materials) break;
                plan.MaximumEngineers = n;
            }
        plan.Engineers = Mathf.Clamp(requestedEngineers, 0, plan.MaximumEngineers);
        plan.Gold = GoldFor(plan.Engineers, labourDiscount);
        plan.Materials = materialRate * plan.Engineers;
        plan.MaximumRolledPoints = plan.Engineers * 3 / 2;
        plan.MinimumRestored = Mathf.Min(missing, plan.Engineers);
        plan.MaximumRestored = Mathf.Min(missing, plan.MaximumRolledPoints);
        plan.BonusCapped = plan.MaximumRolledPoints > missing + .0001f;
        if (plan.Unavailable == null && plan.MaximumEngineers == 0) plan.Unavailable = "No engineers can be assigned to this repair.";
        return plan;
    }

    public static RepairPlan Preview(string wall, int engineers)
    {
        float integrity = MaterialsPerEngineer(wall) == 0 ? 1f : VariableSingleton.GetFloatVariable(wall);
        return Calculate(wall, engineers, integrity, SpecialistPersonnel.AvailableEngineers,
            CouncilActions.Int("iMoney"), CouncilActions.Int("iMaterials"), TechnologySystem.WallRepairEngineerCap, LabourDiscount);
    }

    // Legacy callers receive the largest affordable detail of up to ten engineers.
    public static bool TryRepair(string wall, out string report)
    {
        return TryRepair(wall, Preview(wall, DefaultAssignment).Engineers, out report);
    }

    public static bool TryRepair(string wall, int engineers, out string report)
    {
        var plan = Preview(wall, engineers);
        report = plan.Unavailable ?? "Select at least one engineer.";
        if (!plan.CanRepair) return false;
        // Do not spend on a different detail if affordability changed after the preview.
        if (engineers != plan.Engineers) {
            report = "Only " + plan.MaximumEngineers + " engineers can now be assigned. Review the repair before confirming.";
            return false;
        }
        if (!SpecialistPersonnel.TryReserveWallRepair(engineers, out var reservation)) {
            report = "Those engineers are no longer available. Review the repair before confirming.";
            return false;
        }

        VariableSingleton.ChangeInt("iMoney", -plan.Gold);
        VariableSingleton.ChangeInt("iMaterials", -plan.Materials);
        int points = UnityEngine.Random.Range(engineers, plan.MaximumRolledPoints + 1);
        VariableSingleton.ChangePercentFloat(wall, points / 100f);
        float after = VariableSingleton.GetFloatVariable(wall) * 100f;

        // One task roll, after the repair: 95 none / 2 killed / 3 wounded.
        int casualtyRoll = UnityEngine.Random.Range(0, 100);
        int killed = casualtyRoll >= 95 && casualtyRoll < 97 ? 1 : 0;
        int wounded = casualtyRoll >= 97 ? 1 : 0;
        DateTime returnDate = ReturnDate;
        SpecialistPersonnel.CompleteWallRepair(reservation, killed, wounded, returnDate);
        int recuperating = engineers - killed - wounded;
        string casualty = killed > 0 ? "One engineer was killed." : wounded > 0 ? "One engineer was wounded and enters medical care; he returns when treatment is complete." : "No casualties.";
        string recovery = recuperating > 0
            ? recuperating + (recuperating == 1 ? " engineer recuperating; returns " : " engineers recuperating; return ") + returnDate.ToString("d MMM yyyy") + ", before orders."
            : "No engineers awaiting ordinary recuperation.";
        report = "The masons secure the new stonework before the enemy's next attack.\n\n"
            + "Engineers assigned: " + engineers + ".\n"
            + "Integrity restored: +" + (after - plan.Before).ToString("0.##") + " points (" + plan.Before.ToString("0.##") + "% → " + after.ToString("0.##") + "%).\n"
            + "Spent: " + plan.Gold.ToString("N0") + " gold; " + plan.Materials + " materials.\n\n"
            + casualty + "\n" + recovery;
        return true;
    }
}
