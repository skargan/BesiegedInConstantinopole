using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Presentation only: every displayed loss is supplied by the assault ledger.
public class BattleCrowdView : MonoBehaviour {
    class Soldier {public RectTransform rect;public RawImage image;public Vector2 offset;public float phase;public bool retired;}
    readonly List<Soldier> soldiers=new List<Soldier>();
    PhasedAssault battle;RectTransform field;float progress,target,age;int lastDead,lastOur;TMP_Text terrain;
    public float Progress=>progress;
    public void Initialise(PhasedAssault state) {
        battle=state;field=GetComponent<RectTransform>();if(GetComponent<RectMask2D>()==null)gameObject.AddComponent<RectMask2D>();
        var random=new System.Random(19);string[] names={"soldier-brown","soldier-red","soldier-janissary"};
        for(int i=0;i<42;i++) {
            var go=new GameObject("Attacker "+i,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(transform,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=Vector2.zero;r.pivot=new Vector2(.5f,0);
            var img=go.GetComponent<RawImage>();img.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+names[i%3]);img.raycastTarget=false;img.color=new Color(.66f,.62f,.56f,.92f);
            soldiers.Add(new Soldier{rect=r,image=img,offset=new Vector2((i%7-3)*.027f+(float)random.NextDouble()*.009f,(i/7)*.025f),phase=(float)random.NextDouble()*6.28f});
        }
        terrain=CampaignHUD.Label(transform,"BATTLEMENTS       MOAT                         OTTOMAN APPROACH",new Vector2(.02f,.01f),new Vector2(.98f,.09f),13);terrain.color=new Color(.83f,.75f,.56f,.8f);terrain.raycastTarget=false;
    }
    public void Advance(float value,bool snap=false){target=Mathf.Clamp01(value);if(snap)progress=target;}
    public void FloatLosses() {
        int enemy=battle.Dead-lastDead,ours=battle.OurDead-lastOur;lastDead=battle.Dead;lastOur=battle.OurDead;
        if(enemy>0)Numbers(enemy,false);if(ours>0)Numbers(ours,true);
    }
    void Numbers(int amount,bool ours){
        int pieces=Mathf.Clamp(amount/100+1,1,5),left=amount;
        for(int i=0;i<pieces;i++){int n=left/(pieces-i);left-=n;var t=CampaignHUD.Label(transform,"−"+n,new Vector2(ours?.04f:.2f+i*.08f,.28f+i*.035f),new Vector2(ours?.2f:.35f+i*.08f,.4f+i*.035f),20);t.color=ours?new Color(.95f,.42f,.32f):new Color(1,.83f,.4f);t.fontStyle=FontStyles.Bold;t.raycastTarget=false;t.gameObject.AddComponent<BattleNumber>();}
    }
    public void Blast(Vector2 point) {
        var nearby=new List<Soldier>();foreach(var s in soldiers)if(!s.retired&&s.image.enabled)nearby.Add(s);
        nearby.Sort((a,b)=>Vector2.Distance(a.rect.anchoredPosition,Vector2.Scale(point,field.rect.size)).CompareTo(Vector2.Distance(b.rect.anchoredPosition,Vector2.Scale(point,field.rect.size))));
        int count=Mathf.Clamp(Mathf.CeilToInt(42f*Mathf.Max(0,battle.Dead-lastDead)/Mathf.Max(1,battle.Attackers)),1,12);
        for(int i=0;i<Mathf.Min(count,nearby.Count);i++){var s=nearby[i];s.retired=true;var fly=s.rect.gameObject.AddComponent<BattleScatter>();fly.Initialise(new Vector2((i%2==0?-1:1)*(35+i*8),80+i*12));}
        for(int i=0;i<16;i++){var g=new GameObject("Blast",typeof(RectTransform),typeof(RawImage));g.transform.SetParent(transform,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=point;r.sizeDelta=new Vector2(45,45);var raw=g.GetComponent<RawImage>();raw.texture=AssaultImpact.Glow;raw.color=new Color(1,.48f,.12f,.8f);raw.raycastTarget=false;g.AddComponent<AssaultImpact>();}
    }
    void Update(){
        if(battle==null||!PhasedAssaultUI.CanAnimate)return;float dt=Time.unscaledDeltaTime;age+=dt;progress=Mathf.MoveTowards(progress,target,dt*.5f);
        int visible=battle.Attackers==0?0:Mathf.Clamp(Mathf.CeilToInt(42f*Mathf.Sqrt((float)battle.EnemyLeft/battle.Attackers)),0,42);
        for(int i=0;i<soldiers.Count;i++){
            var s=soldiers[i];if(s.rect.GetComponent<BattleScatter>()!=null)continue;bool shown=i<visible&&!s.retired;s.image.enabled=shown;if(!shown)continue;
            float depth=Mathf.Clamp01(progress-s.offset.y);Vector2 far=new Vector2(.83f+s.offset.x*.3f,.64f+s.offset.y*.25f),near=new Vector2(.20f+s.offset.x,.16f+s.offset.y*.7f);Vector2 xy=Vector2.Lerp(far,near,depth);
            s.rect.anchoredPosition=new Vector2(xy.x*field.rect.width,xy.y*field.rect.height+Mathf.Sin(age*8+s.phase)*(1+depth));
            float h=Mathf.Lerp(8,34,depth)*field.rect.height/300;s.rect.sizeDelta=new Vector2(h*.66f,h);s.rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(age*5+s.phase)*2);
        }
    }
}
public class BattleNumber:MonoBehaviour {float age;RectTransform r;TMP_Text text;Color colour;void Awake(){r=GetComponent<RectTransform>();text=GetComponent<TMP_Text>();colour=text.color;}void Update(){if(!PhasedAssaultUI.CanAnimate)return;age+=Time.unscaledDeltaTime;r.anchoredPosition+=new Vector2(Mathf.Sin(age*24)*10,36)*Time.unscaledDeltaTime;text.color=new Color(colour.r,colour.g,colour.b,Mathf.Clamp01(1-age/1.3f));if(age>1.3f)Destroy(gameObject);}}
public class BattleScatter:MonoBehaviour {float age;Vector2 velocity;RectTransform rect;public void Initialise(Vector2 v){velocity=v;rect=GetComponent<RectTransform>();GetComponent<RawImage>().enabled=true;}void Update(){if(!PhasedAssaultUI.CanAnimate||rect==null)return;age+=Time.unscaledDeltaTime;rect.anchoredPosition+=velocity*Time.unscaledDeltaTime;velocity.y-=160*Time.unscaledDeltaTime;rect.Rotate(0,0,200*Time.unscaledDeltaTime);if(age>1){GetComponent<RawImage>().enabled=false;Destroy(this);}}}
