using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public sealed class MercenaryMapMarkers:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
 int company;TMP_Text hover;Action clear;
 public static readonly Vector2 Malta=new Vector2(.225f,.15f);
 public static Vector2 Position(int id)
 {
 var c=MercenaryRoster.All[id];var end=WarTheatre.Find("constantinople").label;
 Vector2[] route=c.Port=="Caffa"?new[]{WarTheatre.Find("gazaria").label,new Vector2(.58f,.70f),new Vector2(.51f,.62f),end}:c.Port=="Venice"?new[]{WarTheatre.Find("venice").label,new Vector2(.14f,.75f),new Vector2(.21f,.57f),new Vector2(.30f,.30f),new Vector2(.36f,.18f),new Vector2(.40f,.30f),new Vector2(.42f,.45f),end}:c.Port=="Cyprus"?new[]{WarTheatre.Find("cyprus").label,new Vector2(.51f,.16f),new Vector2(.43f,.19f),new Vector2(.40f,.30f),new Vector2(.42f,.45f),end}:new[]{Malta,new Vector2(.30f,.19f),new Vector2(.36f,.18f),new Vector2(.40f,.30f),new Vector2(.42f,.45f),end};
 float t=Mathf.Clamp01(MercenaryContracts.Get(id,"Travel")/(float)c.Days)*(route.Length-1);int segment=Mathf.Min(route.Length-2,Mathf.FloorToInt(t));return Vector2.Lerp(route[segment],route[segment+1],t-segment);
 }
 public static void Add(Transform parent,TMP_Text hover,Action clear)
 {
 for(int id=0;id<MercenaryRoster.All.Length;id++){
 var state=MercenaryContracts.State(id);if(state!=MercenaryContracts.Status.Travelling&&state!=MercenaryContracts.Status.Decision&&state!=MercenaryContracts.Status.Payment)continue;
 int selected=id;var go=new GameObject("Mercenary voyage "+id,typeof(RectTransform),typeof(RawImage),typeof(Button),typeof(MercenaryMapMarkers));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=Position(id);r.sizeDelta=new Vector2(32,32);r.anchoredPosition=new Vector2((id%3-1)*8,(id%2)*8);
 var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/WarTheatreSymbols");image.uvRect=new Rect(.25f,.5f,.25f,.25f);go.GetComponent<Button>().onClick.AddListener(()=>MercenaryUI.Show(selected));var marker=go.GetComponent<MercenaryMapMarkers>();marker.company=id;marker.hover=hover;marker.clear=clear;
 }
 }
 public void OnPointerEnter(PointerEventData e){clear?.Invoke();var c=MercenaryRoster.All[company];hover.text=c.Name+" · "+c.Port+" · "+MercenaryContracts.StatusText(company)+"\n"+c.Package+" · Paid "+MercenaryContracts.Get(company,"Paid").ToString("N0")+" · Due "+(MercenaryContracts.Get(company,"Price")/2).ToString("N0");}
 public void OnPointerExit(PointerEventData e){clear?.Invoke();}
}
