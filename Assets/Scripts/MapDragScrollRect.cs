using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shares the research desk's grab cursor; ScrollRect retains all pan behaviour.
public sealed class MapDragScrollRect : ScrollRect
{
    public override void OnBeginDrag(PointerEventData e) {
        base.OnBeginDrag(e);
        if(IsActive() && e.button==PointerEventData.InputButton.Left) ByzantineCursor.SetGripping(this,true);
    }
    public override void OnEndDrag(PointerEventData e) {
        base.OnEndDrag(e);
        ByzantineCursor.SetGripping(this,false);
    }
    protected override void OnDisable() {
        ByzantineCursor.SetGripping(this,false);
        base.OnDisable();
    }
    void OnApplicationFocus(bool focused) {if(!focused) ByzantineCursor.SetGripping(this,false);}
}
