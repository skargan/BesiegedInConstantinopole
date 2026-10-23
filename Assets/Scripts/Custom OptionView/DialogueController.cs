using UnityEngine;
using System.Linq;
using Yarn.Unity;

public class DialogueController : MonoBehaviour
{
    string startNode = "story0";
    [SerializeField] DialogueRunner dialogueRunner;
    public void SetStartNode(string nodeName) { startNode = nodeName; }
    private void OnEnable() { StartCoroutine(BeginDialogue()); }
    private System.Collections.IEnumerator BeginDialogue()
    {
        // Let the runner and its views finish Start before requesting the first line.
        yield return null;
        if (dialogueRunner.IsDialogueRunning) dialogueRunner.Stop();
        var eventUI=GetComponentInParent<EventUI>();
        eventUI.BeginEvent(startNode);
        // The choices view collects every paragraph and displays the decision immediately.
        foreach(var line in GetComponentsInChildren<LineView>(true)) line.gameObject.SetActive(false);
        dialogueRunner.SetDialogueViews(dialogueRunner.dialogueViews.Where(v=>!(v is LineView)).ToArray());
        foreach(var button in eventUI.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            if(button.name=="ButtonToContinue") button.gameObject.SetActive(false);
        dialogueRunner.onDialogueComplete.RemoveListener(eventUI.FinishEvent);
        dialogueRunner.onDialogueComplete.AddListener(eventUI.FinishEvent);
        dialogueRunner.StartDialogue(startNode);
    }
}
