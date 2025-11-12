using UnityEngine;
using UnityEngine.EventSystems;


public class EventTriggerListener : EventTrigger
{
    public delegate bool BoolDelete(GameObject go);
    public delegate void VoidDelegate(GameObject go);
    public delegate void VoidDelegateWithAxisEvetData(GameObject go, AxisEventData eventData);
    public delegate void VoidDelegateWithPointEvetData(GameObject go, PointerEventData eventData);
    public VoidDelegateWithPointEvetData onClick;
    public VoidDelegateWithPointEvetData onDown;
    public VoidDelegateWithPointEvetData onEnter;
    public VoidDelegateWithPointEvetData onExit;
    public VoidDelegateWithPointEvetData onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegateWithPointEvetData onDrag;
    public VoidDelegateWithPointEvetData onDrop;
    public VoidDelegate onDeselect;
    public VoidDelegate onScroll;
    public VoidDelegateWithAxisEvetData onMove;
    public VoidDelegate onInitializePotentialDrag;
    public VoidDelegateWithPointEvetData onBeginDrag;
    public VoidDelegateWithPointEvetData onEndDrag;
    public VoidDelegate onSubmit;
    public VoidDelegate onCancel;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject, eventData);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject, eventData);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject, eventData);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject, eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject, eventData);
    }
    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null) onDrag(gameObject, eventData);
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (onDrop != null) onDrop(gameObject, eventData);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (onDeselect != null) onDeselect(gameObject);
    }

    public override void OnScroll(PointerEventData eventData)
    {
        if (onScroll != null) onScroll(gameObject);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (onMove != null) onMove(gameObject, eventData);
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (onInitializePotentialDrag != null) onInitializePotentialDrag(gameObject);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null) onBeginDrag(gameObject, eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null) onEndDrag(gameObject, eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (onSubmit != null) onSubmit(gameObject);
    }

    public override void OnCancel(BaseEventData eventData)
    {
        if (onCancel != null) onCancel(gameObject);
    }
    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }
}

