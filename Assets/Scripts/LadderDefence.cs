using UnityEngine;
public class LadderDefence {
 public enum Phase {Waiting,Approach,Raise,Upright,Falling,Down,Breached}
 public class Ladder {public Phase State;public float Age,Climb,Push,Health=2;}
 public Ladder[] Ladders={new Ladder(),new Ladder(),new Ladder()};public float Clock,StringDraw;public bool Loaded,Finished;public int Felled,Breaches;
 public void Tick(float dt){if(Finished)return;Clock+=dt;for(int i=0;i<3;i++){var l=Ladders[i];if(l.State==Phase.Waiting&&Clock>=i*2)l.State=Phase.Approach;if(l.State==Phase.Waiting||l.State==Phase.Down||l.State==Phase.Breached)continue;l.Age+=dt;
   if(l.State==Phase.Approach&&l.Age>=2){l.State=Phase.Raise;l.Age=0;}
   else if(l.State==Phase.Raise&&l.Age>=1.2f){l.State=Phase.Upright;l.Age=0;}
   else if(l.State==Phase.Upright){l.Climb+=dt*.23f;l.Push=Mathf.Max(0,l.Push-dt*.06f);if(l.Climb>=1){l.State=Phase.Breached;Breaches++;}}
   else if(l.State==Phase.Falling&&l.Age>=.7f){l.State=Phase.Down;}
  }if(Clock>=12||Felled+Breaches==3)Finished=true;}
 public void DrawString(float amount){if(Loaded)return;StringDraw=Mathf.Clamp01(StringDraw+Mathf.Max(0,amount));if(StringDraw>=1)Loaded=true;}
 public bool Shoot(int index){if(!Loaded||index<0||index>=3||Finished)return false;var l=Ladders[index];if(l.State==Phase.Waiting||l.State==Phase.Down||l.State==Phase.Breached)return false;Loaded=false;StringDraw=0;l.Health--;l.Climb=Mathf.Max(0,l.Climb-.3f);if(l.Health<=0)Fall(l);return true;}
 public void Push(int index,float amount){if(index<0||index>=3||Finished)return;var l=Ladders[index];if(l.State!=Phase.Upright)return;l.Push=Mathf.Clamp01(l.Push+Mathf.Max(0,amount));if(l.Push>=1)Fall(l);}
 void Fall(Ladder l){if(l.State==Phase.Falling||l.State==Phase.Down)return;l.State=Phase.Falling;l.Age=0;Felled++;}
}
