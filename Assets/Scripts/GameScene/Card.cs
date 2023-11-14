using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject highlight;

    private bool isActive = false;
    private float rotationPerSecond = 180f;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActive)
        {
            //TODO Replace with animation
            StartCoroutine(RotateCard());

            isActive = true;
        }

        //TODO temp only
        //Show();
    }

    private IEnumerator RotateCard()
    {
        float amountRotated = 0f;

        while (amountRotated < rotationPerSecond)
        {
            float frameRotation = rotationPerSecond * Time.deltaTime;

            transform.Rotate(frameRotation, 0, 0);

            amountRotated += frameRotation;

            yield return new WaitForEndOfFrame();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //TODO temp only
        //Hide();
    }

    private void Show()
    {
        highlight.SetActive(true);
    }

    private void Hide()
    {
        highlight.SetActive(false);
    }
}
