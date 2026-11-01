using UnityEngine;
using Yarn.Unity;

// Retired faction compatibility boundary. Civic morale/support is a separate live system.
// Old serialized calls and Yarn commands remain harmless while saves are migrated.
// Full former implementation: Documentation/Retired systems/PoliticalSystem.cs.txt.
public static class PoliticalSystem
{
    public static readonly string[] Names={"Imperial Loyalists","Orthodox Hardliners","Western Unionists","Ottoman Pragmatists"};
    public static readonly Color[] Colours={new Color(.62f,.43f,.17f),new Color(.39f,.28f,.52f),new Color(.19f,.48f,.63f),new Color(.54f,.22f,.19f)};
    public static readonly string[] Beliefs={"","","",""};
    public static readonly string[] Orders={"Unavailable","Unavailable","Unavailable","Unavailable"};
    public static readonly string[] Effects={"","","",""};
    public static readonly string[] Passives={"","","",""};
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static bool Passive(int bloc)=>false;
    public static int Owed(int bloc)=>0;
    public static string FavourUnavailable(int bloc)=>"Political factions are no longer active.";
    public static string AdvancePolitics()=>"";
    public static int Share(int bloc)=>0;
    public static bool Banned(int bloc)=>false;
    public static string Allegiance(int character)=>"Unaffiliated";
    [YarnCommand("SetAllegiance")] public static void SetAllegiance(int character,int bloc) { }
    public static void Gain(int bloc,int amount) { }
    // Geographic classification retained for legacy diplomacy checks; it grants no benefit.
    public static bool Western(int i)=>i==1 || i==2 || i==3 || i==5 || i==6 || i==7 || i==8 || i==9 || i==12 || i==13 || i==14 || i==15;
    public static bool CanAid(int bloc)=>false;
    public static bool Aid(int bloc,out string report) {report=FavourUnavailable(bloc);return false;}
    public static string BanishPreview(int bloc)=>"Political faction banishment is no longer available.";
    public static bool Banish(int bloc,out string report) {report=BanishPreview(bloc);return false;}
    public static void DrawPie(Transform parent) { }
}
