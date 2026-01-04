using Interfaces;
using UnityEngine;

public class FlickeringLightController : MonoBehaviour, IEventEffect, IPowerReactive
{
    public Light lightSource;

    [Header("Flickering Settings")]
    public float minTime = 0.1f;
    public float maxTime = 0.5f;

    private bool shouldFlicker = false;
    private bool hasPower = true;
    private float timeToNextFlicker;

    void Start()
    {
        SetNextFlickerTime();
    }

    void Update()
    {
        if (shouldFlicker)
        {
            timeToNextFlicker -= Time.deltaTime;
            if (timeToNextFlicker <= 0f)
            {
                lightSource.enabled = !lightSource.enabled;
                SetNextFlickerTime();
            }
        }
        else
        {
            lightSource.enabled = hasPower;
        }
    }

    void SetNextFlickerTime()
    {
        timeToNextFlicker = Random.Range(minTime, maxTime);
    }

    public void StartEffect()
    {
        shouldFlicker = true;
        SetNextFlickerTime();
    }

    public void StopEffect()
    {
        shouldFlicker = false;
        lightSource.enabled = hasPower;
    }

    public void SetPower(bool state)
    {
        hasPower = state;

        if (!shouldFlicker)
        {
            lightSource.enabled = hasPower;
        }
    }
}
