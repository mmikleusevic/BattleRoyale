using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
