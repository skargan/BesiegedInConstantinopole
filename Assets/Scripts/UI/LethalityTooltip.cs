using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class LethalityTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int Unit;
    private GameObject panel;
    public void OnPointerEnter(PointerEventData e) {
        if(panel!=null)return;
        var canvas=GetComponentInParent<Canvas>();if(canvas==null)return;
        panel=CampaignHUD.Panel(canvas.transform,"Lethality detail",new Vector2(.2f,.73f),new Vector2(.78f,.86f));
        panel.GetComponent<Image>().color=new Color(.12f,.07f,.10f,1);panel.GetComponent<Image>().raycastTarget=false;
        var text=CampaignHUD.Label(panel.transform,TroopTraining.Breakdown(Unit),new Vector2(.03f,.05f),new Vector2(.97f,.95f),22);text.raycastTarget=false;
    }
    public void OnPointerExit(PointerEventData e) {Clear();}
    private void OnDisable() {Clear();}
    private void Clear() {if(panel!=null)Destroy(panel);panel=null;}
}
