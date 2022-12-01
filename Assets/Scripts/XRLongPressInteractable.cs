using System;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class XRLongPressInteractable : CustomXRBaseInteractable {
    [Serializable]
    public class OnLongPressStartEvent : UnityEvent { }

    public OnLongPressStartEvent onLongPressStart = new OnLongPressStartEvent();


    [Serializable]
    public class OnLongPressEndEvent : UnityEvent { }

    public OnLongPressEndEvent onLongPressEnd = new OnLongPressEndEvent();

    private enum LongPressStatusType {
        Idle,
        Pressing
    }

    private LongPressStatusType _longPressStatus = LongPressStatusType.Idle;

    private LongPressStatusType LongPressStatus {
        get => _longPressStatus;
        set {
            if (_longPressStatus == value) return;
            _longPressStatus = value;
            switch (_longPressStatus) {
                case LongPressStatusType.Idle:
                    onLongPressEnd.Invoke();
                    break;
                case LongPressStatusType.Pressing:
                    onLongPressStart.Invoke();
                    break;
            }
        }
    }


    protected override void OnSelectEntering(SelectEnterEventArgs args) {
        base.OnSelectEntering(args);
        LongPressStatus = LongPressStatusType.Pressing;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args) {
        base.OnSelectExiting(args);
        LongPressStatus = LongPressStatusType.Idle;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args) {
        base.OnHoverEntered(args);
    }

    protected override void OnHoverExited(HoverExitEventArgs args) {
        base.OnHoverExited(args);
        LongPressStatus = LongPressStatusType.Idle;
    }
}