using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void ExampleEventDelegate();
    public event ExampleEventDelegate exampleEvent;

    public void CallExampleEvent()
    {
        exampleEvent();
    }
}
