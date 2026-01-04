using UnityEngine;

public class PowerReactiveObject : MonoBehaviour, IPowerReactive
{
    [Header("Visual & Light")]
    public Renderer[] targetRenderers;
    public Light[] localLights;
    
    [Header("Power Control Settings")]
    [Tooltip("If true, lights will always stay on regardless of power state (materials will still change)")]
    public bool keepLightsOnAlways = false;

    [Header("Materials")]
    public Material powerOnMaterial;
    public Material powerOffMaterial;

    public void SetPower(bool state)
    {
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            foreach (var renderer in targetRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = state ? powerOnMaterial : powerOffMaterial;
                }
            }
        }

        // Only control lights if keepLightsOnAlways is false
        if (!keepLightsOnAlways && localLights != null && localLights.Length > 0)
        {
            foreach (var light in localLights)
            {
                if (light != null)
                {
                    light.enabled = state;
                }
            }
        }
    }
}
