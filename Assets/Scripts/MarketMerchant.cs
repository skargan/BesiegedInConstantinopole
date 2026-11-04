using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Cosmetic market chatter never reads or advances the campaign random stream.
public sealed class MarketMerchant : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [Serializable] public sealed class Lines {public string[] remarks;}
    static string[] remarks;
    LocationHotspot hotspot;RawImage painting;GameObject speech;TMP_Text words;
    Vector2[] sourceShape;Rect previousUv;Vector2 previousSize;
    public string CurrentRemark=>words==null?"":words.text;
    public bool Speaking=>speech!=null&&speech.activeSelf;
    public static int RemarkCount=>Load().Length;
    static string[] Load(){if(remarks==null){var asset=Resources.Load<TextAsset>("Content/MarketMerchant");remarks=asset==null?new[]{"Are you buying, or shall I weigh your opinions?"}:JsonUtility.FromJson<Lines>(asset.text).remarks;}return remarks;}
    public static void Attach(GameObject screen)
    {
        var art=screen.transform.Find("Scene artwork");if(art==null)return;
        var painting=art.GetComponent<RawImage>();if(painting==null)return;
        bool war=VariableSingleton.HasSiegeStarted();
        // Traced around the visible merchant; neighbouring customers are excluded.
        Vector2[] shape=war?new[]{new Vector2(.237f,.548f),new Vector2(.250f,.558f),new Vector2(.270f,.553f),new Vector2(.280f,.536f),new Vector2(.277f,.505f),new Vector2(.286f,.484f),new Vector2(.278f,.465f),new Vector2(.277f,.435f),new Vector2(.285f,.411f),new Vector2(.274f,.394f),new Vector2(.264f,.397f),new Vector2(.249f,.337f),new Vector2(.214f,.338f),new Vector2(.217f,.388f),new Vector2(.205f,.414f),new Vector2(.208f,.458f),new Vector2(.220f,.491f),new Vector2(.240f,.501f),new Vector2(.236f,.527f)}:
            new[]{new Vector2(.239f,.536f),new Vector2(.251f,.546f),new Vector2(.273f,.542f),new Vector2(.282f,.525f),new Vector2(.278f,.498f),new Vector2(.273f,.483f),new Vector2(.290f,.474f),new Vector2(.300f,.451f),new Vector2(.309f,.417f),new Vector2(.308f,.389f),new Vector2(.297f,.385f),new Vector2(.291f,.415f),new Vector2(.279f,.422f),new Vector2(.275f,.371f),new Vector2(.250f,.349f),new Vector2(.216f,.332f),new Vector2(.214f,.380f),new Vector2(.209f,.410f),new Vector2(.214f,.452f),new Vector2(.226f,.475f),new Vector2(.244f,.482f),new Vector2(.239f,.510f)};
        var spot=LocationHotspot.Create(art,"Merchant",shape,null);
        var behaviour=spot.gameObject.AddComponent<MarketMerchant>();behaviour.hotspot=spot;behaviour.painting=painting;behaviour.sourceShape=shape;
        spot.Action=()=>behaviour.Speak();
        // A fixed caption stays clear of both the merchant and the purchase desk.
        behaviour.speech=CampaignHUD.Panel(screen.transform,"Merchant speech",new Vector2(.025f,.735f),new Vector2(.49f,.965f));ImperialTheme.Panel(behaviour.speech,false);
        var title=CampaignHUD.Label(behaviour.speech.transform,"THE GRAIN MERCHANT",new Vector2(.04f,.73f),new Vector2(.96f,.95f),22);title.color=ImperialTheme.Gold;
        behaviour.words=CampaignHUD.Label(behaviour.speech.transform,"",new Vector2(.04f,.09f),new Vector2(.96f,.71f),25);
        behaviour.words.enableAutoSizing=true;behaviour.words.fontSizeMin=20;behaviour.words.fontSizeMax=25;behaviour.words.alignment=TextAlignmentOptions.MidlineLeft;
        foreach(var graphic in behaviour.speech.GetComponentsInChildren<Graphic>())graphic.raycastTarget=false;
        behaviour.speech.SetActive(false);behaviour.Remap();
    }
    void LateUpdate(){if(painting!=null&&(painting.uvRect!=previousUv||painting.rectTransform.rect.size!=previousSize))Remap();if(hotspot!=null)hotspot.SetSpent(CharacterChatter.Spent(CharacterChatter.GrainMerchant));}
    void Remap(){previousUv=painting.uvRect;previousSize=painting.rectTransform.rect.size;hotspot.Shape=sourceShape.Select(p=>new Vector2((p.x-previousUv.x)/previousUv.width,(p.y-previousUv.y)/previousUv.height)).ToArray();hotspot.SetVerticesDirty();}
    public void Speak(){words.text=CharacterChatter.Request(CharacterChatter.GrainMerchant,Load(),CharacterChatter.MerchantFarewell);speech.SetActive(true);hotspot.SetSpent(CharacterChatter.Spent(CharacterChatter.GrainMerchant));hotspot.SetHighlighted(true);var caption=transform.Find("Hover label");if(caption!=null)caption.gameObject.SetActive(false);}
    public void OnPointerEnter(PointerEventData e)=>Speak();
    public void OnPointerExit(PointerEventData e){if(speech!=null)speech.SetActive(false);if(hotspot!=null)hotspot.SetHighlighted(false);}
    void OnDisable(){OnPointerExit(null);}
}
