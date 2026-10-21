#if UNITY_EDITOR
using System;
public static class DiplomaticRepliesSmokeTest
{
 public static void Run()
 {
  int checks=0;
  foreach(var ally in CouncilActions.Allies)
   for(int religion=0;religion<3;religion++)
    for(int accepted=0;accepted<2;accepted++)
     for(int variant=0;variant<2;variant++)
     {
      string quote=DiplomaticReplies.Quote(ally.Name,accepted==1,religion,variant);
      if(string.IsNullOrWhiteSpace(quote)||quote.Length<50)throw new Exception("Missing diplomatic reply: "+ally.Name);
      if(!DiplomaticReplies.Format(ally.Name,accepted==1,religion,variant).Contains(ally.Name))throw new Exception("Missing attribution");
      checks++;
     }
  foreach(string name in new[]{"Georgia","Trebizond","The Papacy"})
   if(DiplomaticReplies.Quote(name,false,0)==DiplomaticReplies.Quote(name,false,1))throw new Exception("Union reaction missing: "+name);
  UnityEngine.Debug.Log("DIPLOMATIC REPLIES PASS: "+checks+" reply combinations; Union branches verified.");
 }
}
#endif
