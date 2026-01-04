using UnityEngine;
using System.Collections.Generic;
using System;

public class UVPrints: MonoBehaviour
{
    public Light uvLight;
    public List<GameObject> list;
    public float detectionDistance = 2f;
    public float detectionAngle = 30f;

    private FlashlightController flashlightController;


    private void Start()
    {
        flashlightController = uvLight.GetComponentInParent<FlashlightController>();
    }

    void Update()
    {
        if (list == null || flashlightController == null || uvLight == null) return;

        if (!flashlightController.IsUVLightOn) {
            SetAllGameObjectVisibility(false);
            return;
        }


        foreach (GameObject go in list) { 
            Vector3 directionToText = (go.transform.position - uvLight.transform.position).normalized;
            float angle = Vector3.Angle(uvLight.transform.forward, directionToText);

            if (angle < detectionAngle / 2f && Vector3.Distance(uvLight.transform.position, go.transform.position) <= detectionDistance)
            {
                SetVisibleGameObjects(go, true);
            }
            else
            {
                SetVisibleGameObjects(go, false);
            }

        }
    }

    private void SetVisibleGameObjects(GameObject gameObject, bool visible)
    {
        if (visible)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void SetAllGameObjectVisibility (bool visible)
    {
        foreach (GameObject go in list) { 
            SetVisibleGameObjects (go, visible);
        }
    }
}