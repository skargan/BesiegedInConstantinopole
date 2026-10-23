using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;


namespace Yarn.Unity
{
    public class OptionButton : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] bool showCharacterName = false;

        public Action<DialogueOption> OnOptionSelected;
        public MarkupPalette palette;

        DialogueOption _option;

        bool hasSubmittedOptionSelection = false;

        public DialogueOption Option
        {
            get => _option;

            set
            {
                _option = value;

                hasSubmittedOptionSelection = false;

                // When we're given an Option, use its text and update our
                // interactibility.
                Markup.MarkupParseResult line;
                // Options are complete sentences. A colon in a cost or warning is not a speaker label.
                line = value.Line.Text;

                if (palette != null)
                {
                    text.text = LineView.PaletteMarkedUpText(line, palette);
                }
                else
                {
                    text.text = line.Text;
                }

                text.text=System.Text.RegularExpressions.Regex.Replace(text.text,@"<color=[^>]+><i>\[Focus - [^\]]+\]</i></color>\s*", "");
                text.text=text.text.Replace(".<color", ". <color");
                text.font = ImperialTheme.Font;
                text.fontSize = 24;text.fontStyle=FontStyles.Normal;
                text.enableAutoSizing = true;
                text.fontSizeMin = 20; text.fontSizeMax = 26;
                text.enableWordWrapping = true;
                text.color = new Color(.95f,.91f,.82f);
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.margin = new Vector4(18,8,18,8);
                var tr = text.rectTransform;
                tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                tr.offsetMin = tr.offsetMax = Vector2.zero;
                var background = GetComponent<UnityEngine.UI.Image>();
                ImperialTheme.Button(this);
                var border=GetComponent<UnityEngine.UI.Outline>();if(border==null)border=gameObject.AddComponent<UnityEngine.UI.Outline>();border.enabled=false;
                var badge = GetComponentInChildren<FocusIcon>(true);
                if(badge != null) badge.gameObject.SetActive(false);
                int focus = -1;
                if(value.Line.Metadata != null)
                    foreach(var tag in value.Line.Metadata)
                        if(tag.StartsWith("focus:")) int.TryParse(tag.Substring(6), out focus);
                if(focus >= 0 && focus < 5)
                {
                    if(badge == null)
                    {
                        var obj = new GameObject("Focus emblem",typeof(RectTransform),typeof(FocusIcon));
                        obj.transform.SetParent(transform,false); badge=obj.GetComponent<FocusIcon>();
                        var rect=obj.GetComponent<RectTransform>();
                        rect.anchorMin=rect.anchorMax=new Vector2(0,.5f); rect.pivot=new Vector2(0,.5f);
                        rect.anchoredPosition=new Vector2(12,0); rect.sizeDelta=new Vector2(68,68);
                        badge.raycastTarget=false;
                    }
                    badge.Focus=focus; badge.SetVerticesDirty(); badge.gameObject.SetActive(true);
                    text.margin=new Vector4(94,8,22,8);
                    // Always retain a written label alongside the emblem.
                    text.text="<b>"+FocusIcon.Names[focus]+"</b> — "+text.text;
                }
                interactable = value.IsAvailable;
            }
        }

        public float PreferredHeight(float width)
        {
            text.enableAutoSizing=false;text.fontSize=24;
            return Mathf.Max(94,text.GetPreferredValues(text.text,Mathf.Max(100,width-80),9999).y+28);
        }

        // If we receive a submit or click event, invoke our "we just selected
        // this option" handler.
        public void OnSubmit(BaseEventData eventData)
        {
            InvokeOptionSelected();
        }

        public void InvokeOptionSelected()
        {
            // turns out that Selectable subclasses aren't intrinsically interactive/non-interactive
            // based on their canvasgroup, you still need to check at the moment of interaction
            if (!IsInteractable())
            {
                return;
            }

            // We only want to invoke this once, because it's an error to
            // submit an option when the Dialogue Runner isn't expecting it. To
            // prevent this, we'll only invoke this if the flag hasn't been cleared already.
            if (hasSubmittedOptionSelection == false)
            {
                ButtonAudio.Play();
                FindObjectOfType<EventUI>()?.RecordChoice(Option.Line.Text.Text);
                OnOptionSelected.Invoke(Option);
                hasSubmittedOptionSelection = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button!=PointerEventData.InputButton.Left) return;
            InvokeOptionSelected();
        }

        // If we mouse-over, we're telling the UI system that this element is
        // the currently 'selected' (i.e. focused) element. 
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
        }
    }
}