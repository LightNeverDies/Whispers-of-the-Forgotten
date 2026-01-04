using UnityEngine;
using System.Collections;

public class LightningEffectController : MonoBehaviour
{
    public Light lightningLight;
    public float flashIntensity = 2.0f;
    public float flashDuration = 5f;
    public float flashCooldown = 30f;
    public float flickerSpeed = 0.2f;

    private float originalIntensity;
    private bool isFlashing = false;

    void Start()
    {
        if (lightningLight == null)
        {
            return;
        }

        originalIntensity = lightningLight.intensity;
    }

    void Update()
    {
        if (!isFlashing)
        {
            StartCoroutine(FlashLightning());
        }
    }

    IEnumerator FlashLightning()
    {
        isFlashing = true;

        yield return new WaitForSeconds(flashCooldown);

        float elapsedTime = 0f;

        while (elapsedTime < flashDuration)
        {
            lightningLight.intensity = Random.Range(0.4f, flashIntensity);

            yield return new WaitForSeconds(flickerSpeed);

            elapsedTime += flickerSpeed;
        }

        lightningLight.intensity = originalIntensity;

        isFlashing = false;
    }
}
