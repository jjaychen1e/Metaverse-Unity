using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomXRBaseInteractable : XRBaseInteractable {
    protected override void Awake() {
        var mCollider = GetComponent<Collider>();
        if (mCollider == null) {
            var boxCollider = gameObject.AddComponent<BoxCollider>();
            var rect = GetComponent<RectTransform>().rect;
            boxCollider.size = new Vector3(rect.width, rect.height, 1);
            mCollider = boxCollider;
            DebugHelper.Log(name + " Create: " + boxCollider.size.ToString());
        }
        else {
            if (mCollider is BoxCollider boxCollider) {
                DebugHelper.Log(name + " " + boxCollider.size.ToString());
            }
        }

        // XRBaseInteractable uses the collider in Awake, so we need to call it after we've set up the collider
        base.Awake();
    }
}