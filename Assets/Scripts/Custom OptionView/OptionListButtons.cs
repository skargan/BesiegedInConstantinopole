using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Yarn.Unity
{
    public class OptionListButtons : DialogueViewBase
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform optionPanel;

        [SerializeField] OptionButton optionButtonPrefab;
        [SerializeField] LayoutSourcer layoutSourcer;

        [SerializeField] MarkupPalette palette;

        [SerializeField] float fadeTime = 0.1f;

        [SerializeField] bool showUnavailableOptions = false;

        [Header("Last Line Components")]
        [SerializeField] TextMeshProUGUI lastLineText;
        [SerializeField] GameObject lastLineContainer;

        [SerializeField] TextMeshProUGUI lastLineCharacterNameText;
        [SerializeField] GameObject lastLineCharacterNameContainer;

        // A cached pool of OptionView objects so that we can reuse them
        List<OptionButton> optionButtons = new List<OptionButton>();

        // The method we should call when an option has been selected.
        Action<int> OnOptionSelected;

        // The line we saw most recently.
        LocalizedLine lastSeenLine;
        private string paragraphs="";
        public override void DialogueStarted() { paragraphs="";lastSeenLine=null; }

        private UnityEngine.UI.ScrollRect choiceScroll;
        private UnityEngine.UI.ScrollRect storyScroll;
        private void EnsureScroll()
        {
            if(choiceScroll!=null) return;
            ImperialTheme.Panel(gameObject);
            var viewport=new GameObject("Scrollable choices",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.RectMask2D),typeof(UnityEngine.UI.ScrollRect));
            viewport.transform.SetParent(optionPanel.parent,false);
            var r=viewport.GetComponent<RectTransform>();r.anchorMin=optionPanel.anchorMin;r.anchorMax=optionPanel.anchorMax;r.offsetMin=optionPanel.offsetMin;r.offsetMax=optionPanel.offsetMax;
            viewport.GetComponent<UnityEngine.UI.Image>().color=ImperialTheme.Navy;
            choiceScroll=viewport.GetComponent<UnityEngine.UI.ScrollRect>();choiceScroll.viewport=r;choiceScroll.horizontal=false;choiceScroll.vertical=true;choiceScroll.scrollSensitivity=45;
            choiceScroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            optionPanel.SetParent(viewport.transform,false);optionPanel.anchorMin=new Vector2(0,1);optionPanel.anchorMax=Vector2.one;optionPanel.pivot=new Vector2(.5f,1);
            choiceScroll.content=optionPanel;
            CampaignHUD.AddScrollbar(choiceScroll,viewport.transform,new Vector2(.984f,.01f),new Vector2(1,.99f));
            var hint=transform.Find("Choice scroll hint");if(hint!=null) hint.gameObject.SetActive(false);
            var story=CampaignHUD.Panel(transform,"Event narrative",new Vector2(.025f,.035f),new Vector2(.43f,.675f));
            story.GetComponent<UnityEngine.UI.Image>().color=ImperialTheme.Navy;ImperialTheme.Frame(story);
            story.AddComponent<UnityEngine.UI.RectMask2D>();
            storyScroll=story.AddComponent<UnityEngine.UI.ScrollRect>();storyScroll.horizontal=false;storyScroll.scrollSensitivity=45;
            storyScroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            storyScroll.viewport=story.GetComponent<RectTransform>();
            lastLineText.transform.SetParent(story.transform,false);
            var textRect=lastLineText.rectTransform;textRect.anchorMin=new Vector2(0,1);textRect.anchorMax=Vector2.one;textRect.pivot=new Vector2(.5f,1);
            textRect.offsetMin=textRect.offsetMax=Vector2.zero;
            lastLineText.fontStyle=FontStyles.Normal;lastLineText.font=ImperialTheme.Font;lastLineText.enableAutoSizing=false;lastLineText.fontSize=24;lastLineText.margin=new Vector4(12,12,28,12);
            storyScroll.content=textRect;CampaignHUD.AddScrollbar(storyScroll,story.transform,new Vector2(.98f,.01f),new Vector2(1,.99f));
            Canvas.ForceUpdateCanvases();
        }
        public void Start()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Reset()
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // Don't do anything with this line except note it and
            // immediately indicate that we're finished with it. RunOptions
            // will use it to display the text of the previous line.
            lastSeenLine = dialogueLine;
            paragraphs+=(paragraphs.Length>0 ? "\n\n" : "")+(palette!=null ? LineView.PaletteMarkedUpText(dialogueLine.Text,palette) : dialogueLine.Text.Text);
            FindObjectOfType<EventUI>()?.ObserveLine(dialogueLine.Text.Text);
            onDialogueLineFinished();
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            // If we don't already have enough option views, create more
            if (dialogueOptions.Length > optionButtons.Count)
            {
                for(int i = optionButtons.Count; i < dialogueOptions.Length; i++)
                {
                    var optionView = CreateNewOptionView();
                    optionView.gameObject.SetActive(false);
                }
            }

            EnsureScroll();
            EventCardArt.Refresh(transform,FindObjectOfType<EventUI>()?.Node ?? "",paragraphs);
            FindObjectOfType<EventUI>()?.PresentChoices();
            foreach(var old in optionButtons) old.gameObject.SetActive(false);
            float usedHeight=0;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Set up all of the option views
            int optionViewsCreated = 0;

            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                var optionButton = optionButtons[i];
                var option = dialogueOptions[i];

                if (option.IsAvailable == false && showUnavailableOptions == false)
                {
                    optionButton.interactable = false;
                    //continue;
                }
                else optionButton.interactable = true;

                RectTransform layout = optionButton.GetComponent<RectTransform>();
                optionButton.gameObject.SetActive(true);
                optionButton.palette = this.palette;
                optionButton.Option = option;
                float height=optionButton.PreferredHeight(choiceScroll.viewport.rect.width);
                layout.anchorMin=new Vector2(0,1);layout.anchorMax=Vector2.one;layout.pivot=new Vector2(.5f,1);
                layout.offsetMin=new Vector2(4,-usedHeight-height);layout.offsetMax=new Vector2(-4,-usedHeight-8);
                usedHeight+=height+8;

                // The first available option is selected by default
                if (optionViewsCreated == 0)
                {
                    optionButton.Select();
                }

                optionViewsCreated += 1;
            }

            optionPanel.sizeDelta=new Vector2(0,Mathf.Max(usedHeight,choiceScroll.viewport.rect.height));
            optionPanel.anchoredPosition=Vector2.zero;
            choiceScroll.verticalNormalizedPosition=1;
            // Update the last line, if one is configured
            if (lastLineContainer != null)
            {
                if (lastSeenLine != null)
                {
                    // if we have a last line character name container
                    // and the last line has a character then we show the nameplate
                    // otherwise we turn off the nameplate
                    var line = lastSeenLine.Text;
                    if (lastLineCharacterNameContainer != null)
                    {
                        if (string.IsNullOrWhiteSpace(lastSeenLine.CharacterName))
                        {
                            lastLineCharacterNameContainer.SetActive(false);
                        }
                        else
                        {
                            line = lastSeenLine.TextWithoutCharacterName;
                            lastLineCharacterNameContainer.SetActive(true);
                            lastLineCharacterNameText.text = lastSeenLine.CharacterName;
                        }
                    }

                    if (palette != null)
                    {
                        lastLineText.text = LineView.PaletteMarkedUpText(line, palette);
                    }
                    else
                    {
                        lastLineText.text = line.Text;
                    }

                    lastLineText.text=paragraphs;
                    lastLineText.rectTransform.sizeDelta=new Vector2(0,Mathf.Max(storyScroll.viewport.rect.height,lastLineText.GetPreferredValues(paragraphs,storyScroll.viewport.rect.width-40,9999).y+30));
                    lastLineText.rectTransform.anchoredPosition=Vector2.zero;storyScroll.verticalNormalizedPosition=1;
                    lastLineContainer.SetActive(true);
                }
                else
                {
                    lastLineContainer.SetActive(false);
                }
            }

            // Note the delegate to call when an option is selected
            OnOptionSelected = onOptionSelected;

            // sometimes (not always) the TMP layout in conjunction with the
            // content size fitters doesn't update the rect transform
            // until the next frame, and you get a weird pop as it resizes
            // just forcing this to happen now instead of then
            Relayout();

            // Fade it all in
            StartCoroutine(Effects.FadeAlpha(canvasGroup, 0, 1, fadeTime));

            /// <summary>
            /// Creates and configures a new <see cref="OptionView"/>, and adds
            /// it to <see cref="optionViews"/>.
            /// </summary>
            OptionButton CreateNewOptionView()
            {
                var optionButton = Instantiate(optionButtonPrefab);
                optionButton.transform.SetParent(optionPanel, false);
                optionButton.transform.SetAsLastSibling();
                
                optionButton.OnOptionSelected = OptionViewWasSelected;
                optionButtons.Add(optionButton);

                return optionButton;
            }

            /// <summary>
            /// Called by <see cref="OptionView"/> objects.
            /// </summary>
            void OptionViewWasSelected(DialogueOption option)
            {
                if (!canvasGroup.interactable) return;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                StartCoroutine(OptionViewWasSelectedInternal(option));

                IEnumerator OptionViewWasSelectedInternal(DialogueOption selectedOption)
                {
                    yield return StartCoroutine(FadeAndDisableOptionViews(canvasGroup, 1, 0, fadeTime));
                    paragraphs="";
                    OnOptionSelected(selectedOption.DialogueOptionID);
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// If options are still shown dismisses them.
        /// </remarks>
        public override void DialogueComplete()
        {
            // do we still have any options being shown?
            if (canvasGroup.alpha > 0)
            {
                StopAllCoroutines();
                lastSeenLine = null;
                OnOptionSelected = null;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                StartCoroutine(FadeAndDisableOptionViews(canvasGroup, canvasGroup.alpha, 0, fadeTime));
            }
        }

        /// <summary>
        /// Fades canvas and then disables all option views.
        /// </summary>
        private IEnumerator FadeAndDisableOptionViews(CanvasGroup canvasGroup, float from, float to, float fadeTime)
        {
            yield return Effects.FadeAlpha(canvasGroup, from, to, fadeTime);

            // Hide all existing option views
            foreach (var optionButton in optionButtons)
            {
                optionButton.gameObject.SetActive(false);
            }
        }

        public void OnEnable()
        {
            Relayout();
        }

        private void Relayout()
        {
            // Force re-layout
            var layouts = GetComponentsInChildren<UnityEngine.UI.LayoutGroup>();

            // Perform the first pass of re-layout. This will update the inner horizontal group's sizing, based on the text size.
            foreach (var layout in layouts)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }

            // Perform the second pass of re-layout. This will update the outer vertical group's positioning of the individual elements.
            foreach (var layout in layouts)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
        }
    }
}
