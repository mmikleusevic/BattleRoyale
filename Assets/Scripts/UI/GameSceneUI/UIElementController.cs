using System;
using System.Collections.Generic;
using UnityEngine;

public class UIElementController : MonoBehaviour
{
    private Queue<Action> eventQueue = new Queue<Action>();

    public void AddEvent(Action eventAction)
    {
        eventQueue.Enqueue(eventAction);
        ProcessNextEvent();
    }

    private void ProcessNextEvent()
    {
        if (eventQueue.Count > 0)
        {
            Action nextEvent = eventQueue.Peek();
            nextEvent.Invoke();
        }
    }

    public void CloseUIElement()
    {
        if (eventQueue.Count > 0)
        {
            eventQueue.Dequeue();
        }

        ProcessNextEvent();
    }
}
