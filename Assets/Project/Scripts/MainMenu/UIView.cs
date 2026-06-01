using System;
using UnityEngine;

public class UIView : MonoBehaviour
{
    public event Action Opened;
    public event Action Closed;

    public void NotifyOpened()
    {
        Opened?.Invoke();
    }

    public void NotifyClosed()
    {
        Closed?.Invoke();
    }
}
