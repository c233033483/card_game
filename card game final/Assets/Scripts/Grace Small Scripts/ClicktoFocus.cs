using System.Collections;
using UnityEngine;

public class ClicktoFocus : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject[] uiToHide;
    [SerializeField] private float focusDistance = 5f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private float moveDuration = 1.25f;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine routine;
    private bool isFocused;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    public void ToggleFocus()
    {
        if (isFocused) ReturnToOriginal();
        else FocusOnTarget();
    }

    private void FocusOnTarget()
    {
        if (target == null || cam == null) return;

        originalPosition = cam.transform.position;
        originalRotation = cam.transform.rotation;

        SetUIActive(false);

        Vector3 lookPoint = target.position + Vector3.up * heightOffset;
        Vector3 direction = (cam.transform.position - lookPoint).normalized;
        if (direction.sqrMagnitude < 0.0001f) direction = -cam.transform.forward;

        Vector3 destination = lookPoint + direction * focusDistance;
        Quaternion rotation = Quaternion.LookRotation(lookPoint - destination, Vector3.up);

        isFocused = true;
        StartMove(destination, rotation, null);
    }

    private void ReturnToOriginal()
    {
        isFocused = false;
        StartMove(originalPosition, originalRotation, () => SetUIActive(true));
    }

    private void SetUIActive(bool active)
    {
        foreach (GameObject ui in uiToHide)
        {
            if (ui != null) ui.SetActive(active);
        }
    }

    private void StartMove(Vector3 endPos, Quaternion endRot, System.Action onComplete)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(MoveCamera(endPos, endRot, onComplete));
    }

    private IEnumerator MoveCamera(Vector3 endPos, Quaternion endRot, System.Action onComplete)
    {
        Transform t = cam.transform;
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = easing.Evaluate(Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.LerpUnclamped(startPos, endPos, k);
            t.rotation = Quaternion.SlerpUnclamped(startRot, endRot, k);
            yield return null;
        }

        t.position = endPos;
        t.rotation = endRot;
        routine = null;
        onComplete?.Invoke();
    }
}