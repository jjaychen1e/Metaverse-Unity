using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomXRLongPressInteractable : CustomXRBaseInteractable {
    public TextMeshProUGUI debugText;

    protected override void OnSelectEntering(SelectEnterEventArgs args) {
        base.OnSelectEntering(args);
        debugText.text = "OnSelectEntering";
    }

    protected override void OnSelectExiting(SelectExitEventArgs args) {
        base.OnSelectExiting(args);
        debugText.text = "OnSelectExiting";
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args) {
        base.OnHoverEntered(args);
        debugText.text = "OnHoverEntered";
    }

    protected override void OnHoverExited(HoverExitEventArgs args) {
        base.OnHoverExited(args);
        debugText.text = "OnHoverExited";
    }
}