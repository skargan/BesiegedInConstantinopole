using UnityEngine;
public class SwordDuel {
 public int Opponents=3,EnemyHealth=2,Wounds,Parries,Cuts;public bool Finished,Won;public float Clock,Cycle,Stun,Cooldown;
 public float Windup=>.7f-(3-Opponents)*.08f;public float StrikeAt=>Windup+.45f;
 public Vector2 Weapon=>Vector2.Lerp(new Vector2((3-Opponents)%2==0?.84f:.16f,.8f),new Vector2(.5f,.42f),Mathf.Clamp01((Cycle-Windup)/.45f));
 public string Cue=>Stun>0?"OPENING · CUT EXPOSED FLESH":Cycle<Windup?"HE RAISES HIS WEAPON":Cycle<StrikeAt+.05f?"INTERCEPT THE WEAPON":"HE RECOVERS · COUNTER";
 public static float SegmentDistance(Vector2 a,Vector2 b,Vector2 p){Vector2 d=b-a;float t=d.sqrMagnitude<.00001f?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);return Vector2.Distance(a+d*t,p);}
 public string Stroke(Vector2 from,Vector2 to){
  if(Finished||Cooldown>0||(to-from).magnitude<.025f)return "";
  Cooldown=.2f;
  if(Cycle>=Windup-.05f&&Cycle<=StrikeAt+.05f&&SegmentDistance(from,to,Weapon)<.14f){Parries++;Stun=1;Cycle=1.25f;return "PARRY · OPENING";}
  Vector2 middle=(from+to)*.5f;if(middle.x<.18f||middle.x>.84f||middle.y<.06f||middle.y>.98f)return "MISS";
  bool head=middle.y>.76f&&middle.y<.9f&&middle.x>.32f&&middle.x<.65f;
  bool legs=middle.y<.35f&&middle.x>.25f&&middle.x<.72f;
  if(!head&&!legs)return "ARMOUR · DEFLECTED";
  if(Stun<=0&&Cycle<StrikeAt+.05f)return "GUARD · DEFLECTED";
  EnemyHealth-=head?2:1;Cuts++;Stun=.45f;
  if(EnemyHealth<=0){Opponents--;EnemyHealth=2;Cycle=0;Stun=.65f;if(Opponents==0){Finished=true;Won=true;}return "OPPONENT DOWN";}
  return "CUT · KEEP YOUR GUARD";
 }
 public void Tick(float dt){if(Finished)return;Clock+=dt;Cooldown=Mathf.Max(0,Cooldown-dt);Stun=Mathf.Max(0,Stun-dt);if(Stun<=0){float previous=Cycle;Cycle+=dt;if(previous<StrikeAt&&Cycle>=StrikeAt)Wounds+=4;if(Cycle>=2.3f)Cycle-=2.3f;}if(Wounds>=20||Clock>=20){Finished=true;Won=false;}}
 public void Exit(){Finished=true;Won=false;}
}
