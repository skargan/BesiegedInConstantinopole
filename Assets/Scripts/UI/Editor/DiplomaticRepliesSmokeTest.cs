#if UNITY_EDITOR
using System;
public static class DiplomaticRepliesSmokeTest
{
 public static void Run()
 {
  int checks=0;
  if(CouncilActions.Allies.Length!=DiplomaticChronicles.All.Length)throw new Exception("Diplomatic profile roster mismatch");
  if(CouncilActions.Allies[CouncilActions.AqQoyunluIndex].Name!="Aq Qoyunlu" || CouncilActions.AqQoyunluIndex!=17)throw new Exception("Aq Qoyunlu save index or embassy recipient changed");
  if(CouncilActions.AllyIndexForRealm("venice")!=1 || CouncilActions.AllyIndexForRealm("genoa")!=2 || CouncilActions.AllyIndexForRealm("aq_qoyunlu")!=17)throw new Exception("Realm routing mismatch");
  if(CouncilActions.AllyIndexForRealm("morea")!=24 || CouncilActions.AllyIndexForRealm("bosnia")!=21 || CouncilActions.AllyIndexForRealm("ragusa")!=-1 || CouncilActions.AllyIndexForRealm(null)!=-1)throw new Exception("Appended or deferred realm routing mismatch");
  if(!AidRouting.BySea(17) || !AidRouting.Route(17).Contains("Trebizond") || PoliticalSystem.Western(17))throw new Exception("Aq Qoyunlu route or political alignment incorrect");
  foreach(var ally in CouncilActions.Allies)
   for(int religion=0;religion<3;religion++)
    for(int accepted=0;accepted<2;accepted++)
     for(int variant=0;variant<2;variant++)
     {
      string quote=DiplomaticReplies.Quote(ally.Name,accepted==1,religion,variant);
      if(string.IsNullOrWhiteSpace(quote)||quote.Length<50)throw new Exception("Missing diplomatic reply: "+ally.Name);
      string court=ally.Name=="Turkmen Confederations"?"the Aq Qoyunlu court":ally.Name;
      if(!DiplomaticReplies.Format(ally.Name,accepted==1,religion,variant).Contains(court))throw new Exception("Missing attribution");
      checks++;
     }
  foreach(string name in new[]{"Georgia","Trebizond","The Papacy"})
   if(DiplomaticReplies.Quote(name,false,0)==DiplomaticReplies.Quote(name,false,1))throw new Exception("Union reaction missing: "+name);
  UnityEngine.Debug.Log("DIPLOMATIC REPLIES PASS: "+checks+" reply combinations; Union branches verified.");
 }
}
#endif
