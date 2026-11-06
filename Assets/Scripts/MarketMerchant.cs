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
        Vector2[] shape=war?new[]{new Vector2(0.240096f,0.537076f),new Vector2(0.243097f,0.544492f),new Vector2(0.249100f,0.550847f),new Vector2(0.259304f,0.554025f),new Vector2(0.269508f,0.550847f),new Vector2(0.277311f,0.542373f),new Vector2(0.279112f,0.530720f),new Vector2(0.276711f,0.522246f),new Vector2(0.273109f,0.516949f),new Vector2(0.273109f,0.504237f),new Vector2(0.270108f,0.490466f),new Vector2(0.265906f,0.479873f),new Vector2(0.274910f,0.476695f),new Vector2(0.282113f,0.463983f),new Vector2(0.286915f,0.450212f),new Vector2(0.289316f,0.430085f),new Vector2(0.288115f,0.417373f),new Vector2(0.282113f,0.409958f),new Vector2(0.278511f,0.396186f),new Vector2(0.280312f,0.370763f),new Vector2(0.273109f,0.350636f),new Vector2(0.259904f,0.337924f),new Vector2(0.246098f,0.335805f),new Vector2(0.231092f,0.333686f),new Vector2(0.215486f,0.334746f),new Vector2(0.214286f,0.350636f),new Vector2(0.211285f,0.370763f),new Vector2(0.207083f,0.382415f),new Vector2(0.205882f,0.395127f),new Vector2(0.207683f,0.415254f),new Vector2(0.211285f,0.439619f),new Vector2(0.214886f,0.459746f),new Vector2(0.220888f,0.474576f),new Vector2(0.229292f,0.483051f),new Vector2(0.240096f,0.492585f),new Vector2(0.244298f,0.503178f),new Vector2(0.243097f,0.518008f),new Vector2(0.240096f,0.525424f)}:
            new[]{new Vector2(0.241029f,0.522848f),new Vector2(0.242823f,0.531350f),new Vector2(0.247608f,0.537726f),new Vector2(0.255981f,0.541977f),new Vector2(0.263158f,0.541977f),new Vector2(0.270933f,0.538789f),new Vector2(0.278110f,0.533475f),new Vector2(0.279904f,0.524973f),new Vector2(0.277512f,0.517535f),new Vector2(0.273325f,0.513284f),new Vector2(0.273923f,0.500531f),new Vector2(0.271531f,0.487779f),new Vector2(0.267344f,0.479277f),new Vector2(0.275718f,0.473964f),new Vector2(0.284689f,0.462274f),new Vector2(0.293062f,0.445271f),new Vector2(0.300239f,0.430393f),new Vector2(0.305024f,0.418704f),new Vector2(0.306818f,0.404888f),new Vector2(0.306220f,0.394261f),new Vector2(0.307416f,0.386823f),new Vector2(0.306220f,0.377258f),new Vector2(0.303230f,0.371945f),new Vector2(0.300239f,0.374070f),new Vector2(0.299641f,0.382572f),new Vector2(0.294856f,0.386823f),new Vector2(0.285885f,0.385760f),new Vector2(0.275718f,0.391073f),new Vector2(0.265550f,0.395324f),new Vector2(0.257775f,0.399575f),new Vector2(0.254187f,0.396387f),new Vector2(0.255981f,0.379384f),new Vector2(0.254187f,0.359192f),new Vector2(0.251794f,0.337938f),new Vector2(0.239234f,0.328374f),new Vector2(0.226077f,0.320935f),new Vector2(0.214713f,0.323061f),new Vector2(0.214713f,0.348565f),new Vector2(0.212919f,0.369819f),new Vector2(0.210526f,0.382572f),new Vector2(0.209330f,0.394261f),new Vector2(0.210526f,0.410202f),new Vector2(0.214115f,0.429330f),new Vector2(0.217105f,0.448459f),new Vector2(0.222488f,0.463337f),new Vector2(0.230263f,0.475027f),new Vector2(0.239234f,0.485654f),new Vector2(0.245215f,0.489904f),new Vector2(0.247010f,0.500531f),new Vector2(0.245215f,0.513284f)};
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
