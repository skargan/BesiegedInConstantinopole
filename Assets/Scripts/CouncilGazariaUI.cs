using System;
using UnityEngine;
using TMPro;
public partial class CouncilUI
{
 public void GazariaCourt(int speaker=0)
 {
 var pane=MilitaryCanvas("Genoese Gazaria · Caffa","caffa-harbour","Between the steppe and the sea.");ControlSingleton.Instance.ReturnAction=DiplomaticRelations;
 int index=CountryDiplomacy.Index("gazaria");
 DiplomaticNames.Bind(CompactText(pane,"CAFFA",.04f,.91f,.96f,.98f,30),"gazaria");CompactText(pane,"Goodwill "+CountryDiplomacy.Goodwill(index).ToString("0.##")+" · "+CountryDiplomacy.EnvoyStatus(index),.04f,.84f,.96f,.91f,21);
 CompactText(pane,speaker==0?"Borruele Grimaldi · Consul":"Dorino Grimaldi · Bank of St George",.04f,.74f,.96f,.84f,26);
 var words=CompactText(pane,speaker==0?
 "Caffa’s quays are crowded with grain ships, interpreters and men who have crossed half the steppe. Genoa’s cross hangs above the harbour, but decisions here answer to local dangers as much as distant councils.\n\n‘Our merchants need the straits open, Emperor. They also need to know that their next cargo will not become somebody’s contribution to a war. Give us a passage that can be sailed, and there will be much to discuss.’":
 "The Bank’s representative brings accounts as carefully guarded as any fortress. Credit binds ships, warehouses and councillors across the sea; a missed payment can travel further than an army.\n\n‘A government may promise what its treasury cannot provide. We prefer to examine the revenues first. There are men willing to secure this coast, if the arrangements can be made to endure.’",.04f,.30f,.96f,.73f,24);words.overflowMode=TextOverflowModes.Truncate;
 CampaignHUD.MakeButton(pane,speaker==0?"Hear the Bank’s representative":"Speak with the consul",new Vector2(.04f,.21f),new Vector2(.96f,.29f),()=>GazariaCourt(1-speaker));
 CampaignHUD.MakeButton(pane,"Embassy and relations",new Vector2(.04f,.115f),new Vector2(.96f,.195f),()=>AdvancedDiplomacy(index));
 CampaignHUD.MakeButton(pane,"Return",new Vector2(.23f,.025f),new Vector2(.77f,.095f),()=>ControlSingleton.Instance.GoBack());
 DecisionJournal.Record("gazaria-introduction","The harbour of Caffa","Imperial representatives have spoken with Caffa’s consul and a representative of the Bank of St George. Gazaria is approached separately from Genoa; no transfer of government has been agreed.",true);
 }
}
