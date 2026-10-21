using UnityEngine;
using Yarn.Unity;
public static class SiegeOpeningIntelligence {
    public static int RecruitsForRoll(int roll) => roll<50?1:roll<80?2:roll<97?3:5;
    [YarnFunction("OpeningRecruits")] public static int Roll() => RecruitsForRoll(Random.Range(0,100));
}
