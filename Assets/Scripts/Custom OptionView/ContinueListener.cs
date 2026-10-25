using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections.Generic;

public class ContinueListener : MonoBehaviour
{
    [SerializeField] LineView lineView;
    private void Update()
    {
        if (CampaignHUD.Instance != null && CampaignHUD.Instance.HasModal) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) { lineView.OnContinueClicked(); return; }
        if (!Input.GetMouseButtonDown(0) || EventSystem.current == null) return;
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, hits);
        // Clicking a choice or navigation control must not advance the following line too.
        foreach (var hit in hits)
            if (hit.gameObject.GetComponentInParent<Selectable>() != null) return;
        lineView.OnContinueClicked();
    }
}
