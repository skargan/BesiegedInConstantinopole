using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Coordinates are authored against the painting; presentation follows its crop at any resolution.
public sealed class TechnologyDiscoveryRoom:MonoBehaviour
{
    public readonly TechnologyDiscoveries.Experiment Experiment=new TechnologyDiscoveries.Experiment();
    public bool Busy {get;private set;}
    public bool CanUse(int index)=>!Busy&&(index>=6 || (!TechnologyDiscoveries.Found(TechnologyDiscoveries.Kind.Alchemy)&&Experiment.Step==index));
    public static readonly string[] Labels={"Grind the herbs","Pour the water","Light the furnace","Tap the little bell","Watch the retort","Taste the distillate","The pigeon's letter","A curious skull","A concealed catch"};
    static readonly string[] Sounds={"grind","pour","fire","bell","boil","choir","bell","growl","coins"};
    static readonly string[] Notes={"The pestle crushes the dry leaves into a fragrant powder.","A thin stream of water darkens the mixture.","The small furnace catches. Warmth gathers beneath the glass.","The little bell gives a clear, bright note.","Bubbles rise through the retort. A clear drop runs down the pipe.","The scholar watches your expression, then smiles. The experiment has yielded something worth recording.","The tiny roll contains observations worth more than its bearer could ever know.","What a fearsome beast it must have been. One of the last dragons, perhaps slain by Saint George himself. Thanks be to God that such terrible lizards no longer trouble the earth.","A concealed catch yields beneath your thumb. Inside lie coins wrapped in faded silk and a folded page of calculations. Whoever hid them here never returned."};
    readonly List<LocationHotspot> spots=new List<LocationHotspot>();
    RawImage painting;TMP_Text caption;GameObject captionBox;AudioSource sound;
    Rect lastUv;Vector2 lastSize;float captionUntil;int hover=-1;
    public static TechnologyDiscoveryRoom Attach(GameObject screen){
        var room=screen.AddComponent<TechnologyDiscoveryRoom>();room.painting=screen.transform.Find("Scene artwork").GetComponent<RawImage>();
        // All discovery geometry sits beneath the opaque technology desk.
        for(int i=0;i<Labels.Length;i++){
            int index=i;var spot=LocationHotspot.Create(room.painting.transform,Labels[i],new Vector2[0],()=>room.Activate(index));
            Destroy(spot.transform.Find("Hover label").gameObject);
            var relay=spot.gameObject.AddComponent<DiscoveryHover>();relay.Room=room;relay.Index=i;room.spots.Add(spot);
        }
        room.captionBox=CampaignHUD.Panel(screen.transform,"Discovery caption",new Vector2(.035f,.025f),new Vector2(.475f,.19f));ImperialTheme.Panel(room.captionBox,false);
        room.caption=CampaignHUD.Label(room.captionBox.transform,"",new Vector2(.04f,.09f),new Vector2(.96f,.91f),23);room.caption.enableAutoSizing=true;room.caption.fontSizeMin=18;room.caption.fontSizeMax=23;
        foreach(var g in room.captionBox.GetComponentsInChildren<Graphic>())g.raycastTarget=false;
        room.captionBox.SetActive(false);room.sound=room.gameObject.AddComponent<AudioSource>();room.sound.playOnAwake=false;room.sound.spatialBlend=0;
        room.Remap();room.Refresh();return room;
    }
    void LateUpdate(){
        Experiment.Tick(Time.unscaledTime);Refresh();
        if(painting.uvRect!=lastUv||painting.rectTransform.rect.size!=lastSize)Remap();
        if(hover<0&&Time.unscaledTime>=captionUntil)captionBox.SetActive(false);
        if(!Settings.Instance.SfxEnabled&&sound.isPlaying)sound.Stop();
    }
    void Refresh(){for(int i=0;i<spots.Count;i++){bool active=CanUse(i);if(spots[i].raycastTarget!=active){spots[i].raycastTarget=active;if(!active&&!Busy)spots[i].SetHighlighted(false);}}}
    void Remap(){lastUv=painting.uvRect;lastSize=painting.rectTransform.rect.size;
        for(int i=0;i<spots.Count;i++){
            var source=TechnologyDiscoveryShapes.Outlines[i];var points=new Vector2[source.Length];
            for(int n=0;n<points.Length;n++)points[n]=TechnologyDiscoveryShapes.Map(source[n],lastUv);
            spots[i].Shape=points;spots[i].SetVerticesDirty();
        }
    }
    public void Hover(int index,bool entered){if(entered&&!CanUse(index))return;hover=entered?index:-1;if(entered)ShowCaption(index==7?Notes[7]:Labels[index],false);}
    public void HoverRail(bool entered){hover=entered?9:-1;if(entered)ShowCaption("Drag the bronze ring to move the desk. Click to slide it to the other side.",false);}
    void ShowCaption(string message,bool linger){
        var desk=transform.Find("Technology desk") as RectTransform;bool left=desk==null||desk.anchorMin.x>.26f;
        var r=(RectTransform)captionBox.transform;r.anchorMin=new Vector2(left?.035f:.525f,.025f);r.anchorMax=new Vector2(left?.475f:.965f,.19f);
        caption.text=message;captionBox.SetActive(true);captionBox.transform.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();float height=caption.GetPreferredValues(message,r.rect.width*.92f,9999).y+24;
        r.anchorMax=new Vector2(r.anchorMax.x,.025f+Mathf.Clamp(height/((RectTransform)transform).rect.height,.065f,.165f));
        if(linger)captionUntil=Time.unscaledTime+3;
    }
    public bool Activate(int index){if(index<0||index>=spots.Count||!CanUse(index))return false;StartCoroutine(Interact(index));return true;}
    IEnumerator Interact(int index){
        Busy=true;hover=-1;Refresh();spots[index].SetHighlighted(true);ShowCaption(Notes[index],true);
        var clip=Resources.Load<AudioClip>("Audio/Discoveries/"+Sounds[index]);
        if(clip!=null&&Settings.Instance.SfxEnabled){sound.clip=clip;sound.volume=Settings.Instance.MasterVolume*Settings.Instance.SfxVolume;sound.Play();}
        // A click is committed immediately. Leaving during the sound cannot lose a collected reward.
        TechnologyDiscoveries.Kind? kind=index==5?TechnologyDiscoveries.Kind.Alchemy:index==6?TechnologyDiscoveries.Kind.Letter:index==7?TechnologyDiscoveries.Kind.Skull:index==8?TechnologyDiscoveries.Kind.Treasure:(TechnologyDiscoveries.Kind?)null;
        if(index<6)Experiment.Advance(index,Time.unscaledTime);
        bool gained=kind.HasValue&&TechnologyDiscoveries.Claim(kind.Value);
        if(gained){if(index==8)FloatReward(index,"+2,500 gold",new Color(1,.78f,.25f),0);if(index!=7)FloatReward(index,"+1 secret",new Color(.2f,.85f,1),index==8?38:0);}
        else if(kind.HasValue)ShowCaption(Notes[index]+"\n<color=#B8CFDA>Already recorded in your journal.</color>",true);
        yield return new WaitForSecondsRealtime(2.5f);sound.Stop();spots[index].SetHighlighted(false);Busy=false;captionUntil=Time.unscaledTime+3;Refresh();
    }
    void FloatReward(int index,string words,Color colour,float offset){
        Vector2 pos=TechnologyDiscoveryShapes.Map(TechnologyDiscoveryShapes.Anchors[index],lastUv);
        pos.x=Mathf.Clamp(pos.x,.02f,.78f);pos.y=Mathf.Clamp(pos.y,.12f,.87f);
        var t=CampaignHUD.Label(transform,words,pos,pos,27);t.name="Discovery reward";t.color=colour;t.fontStyle=FontStyles.Bold;t.raycastTarget=false;
        t.rectTransform.pivot=new Vector2(0,.5f);t.rectTransform.sizeDelta=new Vector2(240,40);t.rectTransform.anchoredPosition=new Vector2(0,offset);
        var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(colour.r,colour.g,colour.b,.45f);shadow.effectDistance=new Vector2(1,-1);
        if(words.Contains("secret")){
            var icon=new GameObject("Secret symbol",typeof(RectTransform),typeof(Image));icon.transform.SetParent(t.transform,false);
            var ir=(RectTransform)icon.transform;ir.anchorMin=new Vector2(-.13f,.1f);ir.anchorMax=new Vector2(-.01f,.9f);ir.offsetMin=ir.offsetMax=Vector2.zero;
            CampaignStatArt.Apply(icon.GetComponent<Image>(),CampaignStatArt.Stat.Secrets);icon.GetComponent<Image>().raycastTarget=false;
        }
        StartCoroutine(Drift(t));
    }
    IEnumerator Drift(TMP_Text text){var r=text.rectTransform;Vector2 start=r.anchoredPosition;Color c=text.color;float age=0;while(age<3){age+=Time.unscaledDeltaTime;r.anchoredPosition=start+new Vector2(70,60)*age/3;text.color=new Color(c.r,c.g,c.b,Mathf.Clamp01((3-age)/.8f));yield return null;}Destroy(text.gameObject);}
    void OnDisable(){StopAllCoroutines();Experiment.Reset();Busy=false;if(sound!=null)sound.Stop();if(captionBox!=null)captionBox.SetActive(false);}
}
public sealed class DiscoveryHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public TechnologyDiscoveryRoom Room;public int Index;
    public void OnPointerEnter(PointerEventData e)=>Room.Hover(Index,true);
    public void OnPointerExit(PointerEventData e)=>Room.Hover(Index,false);
}
