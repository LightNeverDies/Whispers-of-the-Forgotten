using UnityEngine;

public class FlickeringLightController : MonoBehaviour
{
    public Light lightSource;
    public bool shouldFlicker = false;
    private float timeToNextFlicker;
    public float minTime;
    public float maxTime;

    void Start()
    {
        SetNextFlickerTime();
    }

    void Update()
    {
        if (shouldFlicker)
        {
            timeToNextFlicker -= Time.deltaTime;
            if (timeToNextFlicker <= 0)
            {
                lightSource.enabled = !lightSource.enabled;
                SetNextFlickerTime();
            }
        }
    }

    void SetNextFlickerTime()
    {
        timeToNextFlicker = Random.Range(minTime, maxTime);
    }

    public void StartFlickering()
    {
        shouldFlicker = true;
        SetNextFlickerTime();
    }

    public void StopFlickering()
    {
        shouldFlicker = false;
        lightSource.enabled = true;
    }
}
