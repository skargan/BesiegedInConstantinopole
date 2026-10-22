using System;
using System.Collections.Generic;
using UnityEngine;
public static class AssaultIncidents {
 [Serializable] public class Choice {
  public string Name;public int R,M,G,P,E,Ammo,Materials,InfantryLoss;public bool Reveal;public int RequiredCharacter=-1;
  public Choice(string name,int r,int m,int g,int p,int e,int ammo,int materials,int loss,bool reveal){Name=name;R=r;M=m;G=g;P=p;E=e;Ammo=ammo;Materials=materials;InfantryLoss=loss;Reveal=reveal;}
  public string Terms=> (R!=0?"Archers "+R.ToString("+0;-0;0")+"% · ":"")+(M!=0?"Melee "+M.ToString("+0;-0;0")+"% · ":"")+(G!=0?"Guns "+G.ToString("+0;-0;0")+"% · ":"")+(P!=0?"Pressure "+P.ToString("+0;-0;0")+" · ":"")+(E!=0?"Exposure "+E.ToString("+0;-0;0")+"% · ":"")+(Ammo>0?Ammo+" ammunition · ":"")+(Materials>0?Materials+" materials · ":"")+(InfantryLoss>0?"Lose "+InfantryLoss+"% infantry · ":"")+(Reveal?"Reveal main effort":"");
 }
 [Serializable] public class Incident {public string Id;public string Title,Condition,Text;public int Phase;public Choice[] Choices;public Incident(string title,int phase,string condition,string text,Choice[] choices){Title=title;Phase=phase;Condition=condition;Text=text;Choices=choices;}}
 [Serializable] class Catalogue {public int SchemaVersion;public Incident[] Incidents;}
 public static readonly Incident[] All=Load();
 static Incident[] Load(){
  var asset=Resources.Load<TextAsset>("Content/AssaultIncidents");
  if(asset==null)throw new InvalidOperationException("Missing authoritative assault catalogue");
  var data=JsonUtility.FromJson<Catalogue>(asset.text);
  if(data==null||data.SchemaVersion!=1)throw new InvalidOperationException("Unsupported assault catalogue schema");
  Validate(data.Incidents);return data.Incidents;
 }
 public static void Validate(Incident[] items){
  if(items==null||items.Length<24)throw new InvalidOperationException("Assault catalogue needs at least 24 incidents");
  var ids=new HashSet<string>();var conditions=new HashSet<string>{"guns","archers","moat","tower","ram","tunnel","wall","cannon","janissaries","infantry","sortie","frontline","always"};
  foreach(var e in items){
   if(e==null||string.IsNullOrWhiteSpace(e.Id)||!ids.Add(e.Id)||string.IsNullOrWhiteSpace(e.Title)||string.IsNullOrWhiteSpace(e.Text)||e.Phase<0||e.Phase>2||!conditions.Contains(e.Condition)||e.Choices==null||e.Choices.Length==0)throw new InvalidOperationException("Invalid or duplicate assault incident: "+e?.Id);
   bool free=false;foreach(var c in e.Choices){if(c==null||string.IsNullOrWhiteSpace(c.Name)||c.Ammo<0||c.Materials<0||c.InfantryLoss<0||c.InfantryLoss>100)throw new InvalidOperationException("Invalid assault choice: "+e.Id);if(c.Ammo==0&&c.Materials==0&&c.RequiredCharacter<0)free=true;}
   if(!free)throw new InvalidOperationException("No unconditional resource-free response: "+e.Id);
  }
 }
 public static bool Eligible(Incident e,PhasedAssault b){switch(e.Condition){case "guns":return CouncilActions.Int("iTurkCannons")>0;case "archers":return b.Archers>0;case "moat":return VariableSingleton.GetFloatVariable("fMoat")>0;case "tower":return b.Tower;case "ram":return b.Ram;case "tunnel":return VariableSingleton.GetBoolVariable("bSecretTurkTunnel");case "wall":return b.Wall<.6f;case "cannon":return b.Cannons>0&&b.Ammo>0;case "janissaries":return b.Type==2;case "infantry":return b.Infantry>0;case "sortie":return b.SortieSent>0;case "frontline":return b.Frontline&&b.GuardsNeeded>0;default:return true;}}
 public static int Select(int phase,PhasedAssault b){
  var list=new List<int>();for(int i=0;i<All.Length;i++)if(All[i].Phase==phase&&Eligible(All[i],b))list.Add(i);if(list.Count==0)return -1;
  int old=DailyLifeEvents.I("iLastAssaultIncident"+phase,-1),prior=DailyLifeEvents.I("iPriorAssaultIncident"+phase,-1);if(list.Count>2){list.Remove(old);list.Remove(prior);}var weighted=new List<int>();
  foreach(int i in list){int w=1;string c=All[i].Condition;if(c=="tower"||c=="ram"||c=="cannon"||c=="tunnel")w+=3;if(c=="janissaries"&&b.Type==2)w+=4;if(c=="wall"&&b.Wall<.4f)w+=3;for(int n=0;n<w;n++)weighted.Add(i);}
  int best=weighted[b.NextRandom(weighted.Count)];if(!b.Practice){VariableSingleton.SetInt("iPriorAssaultIncident"+phase,old);VariableSingleton.SetInt("iLastAssaultIncident"+phase,best);}return best;
 }

 public static string Unavailable(Choice c,PhasedAssault b){if(c.RequiredCharacter>=0&&!b.Practice&&!CourtRoster.InService(c.RequiredCharacter))return "Requires Johannes Grant";if(c.Ammo>0&&(b.Cannons==0||b.Ammo+c.Ammo>(b.Practice?50:CouncilActions.Int("iAmmunition"))))return "Insufficient gun ammunition";if(c.Materials+b.MaterialsSpent>(b.Practice?500:CouncilActions.Int("iMaterials")))return "Insufficient materials";return null;}
 public static bool Choose(int incident,int choice,PhasedAssault b){
  if(b==null||b.Finished||incident<0||incident>=All.Length)return false;
  var e=All[incident];if(choice<0||choice>=e.Choices.Length||b.HasIncident(e.Id))return false;
  if((e.Phase==0&&b.Stage!=PhasedAssault.Phase.Approach)||(e.Phase>0&&b.Stage!=PhasedAssault.Phase.Contact))return false;
  var c=e.Choices[choice];if(Unavailable(c,b)!=null||!b.RecordIncident(e.Id))return false;
  return Apply(c,b);
 }
 public static bool Apply(Choice c,PhasedAssault b){if(b.Finished||Unavailable(c,b)!=null)return false;b.ArcherFactor=Mathf.Clamp(b.ArcherFactor+c.R/100f,.75f,1.25f);b.FootFactor=Mathf.Clamp(b.FootFactor+c.M/100f,.75f,1.25f);b.GunFactor=Mathf.Clamp(b.GunFactor+c.G/100f,.75f,1.25f);b.PressureRelief-=c.P;b.ExposureFactor=Mathf.Clamp(b.ExposureFactor+c.E/100f,.75f,1.25f);b.Ammo+=c.Ammo;b.MaterialsSpent+=c.Materials;int lost=Mathf.Min(b.Infantry,Mathf.CeilToInt(b.Infantry*c.InfantryLoss/100f));b.Infantry-=lost;if(c.Reveal)b.SignalFront=b.MainFront;b.Journal.Add(c.Name+". "+c.Terms+(lost>0?" Actual infantry lost: "+lost:"")+".");return true;}
}
