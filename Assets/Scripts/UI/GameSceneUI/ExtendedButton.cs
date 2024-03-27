using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExtendedButton : Button
{
    private bool isButtonEnabled = true;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (isButtonEnabled)
        {
            StartCoroutine(EnableButtonAfterTime(0.5f));
            base.OnPointerClick(eventData);
        }
    }

    private IEnumerator EnableButtonAfterTime(float time)
    {
        isButtonEnabled = false;
        yield return new WaitForSeconds(time);
        isButtonEnabled = true;
    }
}
