using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// Fictional dialogue; cosmetic RNG never advances campaign randomness.
public sealed class CandarRulerRemarks:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler,ISelectHandler,IDeselectHandler {
 static readonly string[] lines=new string[]{"You expect another spring, Emperor? So do the leaves.","Mehmed wants your city. I worry about what he will want afterwards.","Your walls are impressive. Your creditors speak highly of them.","I prefer an emperor across the water to a sultan at my gate.","Let us conduct business before either of us becomes history.","Rome has fallen so often. You have made a profession of postponing it.","A secret is merely a fact whose price has not yet been agreed.","Of course I am loyal to the Sultan. We are discussing commerce.","Do not mistake my discretion for affection.","Mehmed is young. He has not yet learned to leave anything unconquered.","I have sent him assurances. They travel more cheaply than soldiers.","You have very little land for a man with so many titles.","My merchants favour peace. Their finest profits suggest otherwise.","A captive has a price. Ask him what freedom is worth.","Your gold bears a reassuring face. I hope you remain available to mint it.","The sea connects us. Fortunately, it also separates us.","I hear your court is divided. At least there is still something to divide.","Orhan has an excellent claim. Claims are safest in somebody else’s house.","I would offer advice freely, but you might undervalue it.","The Sultan calls it destiny. His quartermasters use other words.","One misplaced instruction can accomplish what a hundred brave men cannot.","You need another day. How fortunate that I know men who waste them.","I do not oppose great empires. I merely prefer to stand outside them.","Your ancestors understood survival. Their descendants became particular about appearances.","A locked gate is discourteous. An unguarded one is expensive.","Keep your promises modest, Emperor. You may survive to fulfil them.","Every conqueror admires independence until he encounters someone else’s.","If these walls could speak, I would have to dismiss the guards.","I hope you win. Quietly, of course.","Do come again. It would disappoint me to learn our last transaction was charitable."};
 static readonly System.Random random=new System.Random();
 TMP_Text words;Image image;float until,ready;int last=-1;
 public void Bind(TMP_Text label){words=label;image=GetComponent<Image>();}
 void Speak(){if(Time.unscaledTime<ready)return;int n;do{n=random.Next(lines.Length);}while(n==last);last=n;words.text="İsmail Bey: “"+lines[n]+"”";until=Time.unscaledTime+10;ready=until;}
 void Glow(bool on){if(image!=null)image.color=new Color(1,.8f,.35f,on?.08f:0);}
 public void OnPointerEnter(PointerEventData e){Glow(true);Speak();}
 public void OnPointerExit(PointerEventData e){Glow(false);}
 public void OnPointerClick(PointerEventData e){Speak();}
 public void OnSelect(BaseEventData e){Glow(true);Speak();}
 public void OnDeselect(BaseEventData e){Glow(false);}
 void Update(){if(words!=null&&until>0&&Time.unscaledTime>=until){words.text="";until=0;}}
}