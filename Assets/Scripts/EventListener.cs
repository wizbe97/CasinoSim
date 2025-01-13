
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour, IGameEventListener
{
    [System.Serializable]
    public class EventResponsePair
    {
        public GameEventSO gameEvent;
        public UnityEvent response;
    }

    public List<EventResponsePair> eventResponsePairs = new();

    private void OnEnable()
    {
        foreach (var pair in eventResponsePairs)
        {
            if (pair.gameEvent != null)
            {
                pair.gameEvent.RegisterListener(this);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var pair in eventResponsePairs)
        {
            if (pair.gameEvent != null)
            {
                pair.gameEvent.UnregisterListener(this);
            }
        }
    }

    public void OnEventRaised()
    {
        foreach (var pair in eventResponsePairs)
        {
            if (pair.gameEvent != null)
            {
                pair.response.Invoke();
            }
        }
    }
}
