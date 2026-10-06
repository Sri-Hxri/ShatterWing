using UnityEngine;
using UnityEngine.EventSystems;

public class UIAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public bool interactive;
    bool hovered, pressed, selected;
    float scale;
    CanvasGroup group;
    void OnEnable()
    {
        hovered = pressed = selected = false;
        scale = interactive ? 0.97f : 1f;
        transform.localScale = Vector3.one * scale;
        if (!interactive)
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
        }
    }
    void Update()
    {
        float blend = 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime);
        float target = interactive ? (pressed ? 0.98f : hovered || selected ? 1.025f : 1f) : 1f;
        scale = Mathf.Lerp(scale, target, blend);
        transform.localScale = Vector3.one * scale;
        if (group) group.alpha = Mathf.MoveTowards(group.alpha, 1f, Time.unscaledDeltaTime * 7f);
    }
    public void OnPointerEnter(PointerEventData e) { hovered = true; }
    public void OnPointerExit(PointerEventData e) { hovered = pressed = false; }
    public void OnPointerDown(PointerEventData e) { pressed = true; }
    public void OnPointerUp(PointerEventData e) { pressed = false; }
    public void OnSelect(BaseEventData e) { selected = true; }
    public void OnDeselect(BaseEventData e) { selected = pressed = false; }
}
