using UnityEngine;
using UnityEngine.Events;

public class OnToggleEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent onTurnOn;
    [SerializeField] private UnityEvent onTurnOff;

    private bool isOn = false;

    public void Toggle()
    {
        isOn = !isOn;

        if (isOn)
            onTurnOn.Invoke();
        else
            onTurnOff.Invoke();
    }

    public void ResetToggle()
    {
        isOn = false;
    }

}