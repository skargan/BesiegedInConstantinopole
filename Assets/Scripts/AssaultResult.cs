// Frozen encounter result: campaign commits and reports read the same resolved values.
public sealed class AssaultResult {
 public readonly int InfantryDelta,ArcherDelta,CavalryDelta,GuardDelta,AmmoSpent,MaterialsSpent,HealthDelta,EnemyDead,EnemySurvivors;
 public readonly bool Held,Tower,Ram;public readonly int Health;public readonly string Summary,Ledger;
 public AssaultResult(PhasedAssault b){InfantryDelta=b.Infantry-b.InitialInfantry;ArcherDelta=b.Archers-b.InitialArchers;CavalryDelta=b.Cavalry-b.InitialCavalry;GuardDelta=b.Guards-b.InitialGuards;AmmoSpent=b.Ammo;MaterialsSpent=b.MaterialsSpent;HealthDelta=b.Health-b.InitialHealth;EnemyDead=b.Dead;EnemySurvivors=b.EnemyLeft;Held=b.Held;Tower=b.Tower;Ram=b.Ram;Health=b.Health;Summary=b.Result;Ledger=string.Join("\n\n",b.Journal);}
}
