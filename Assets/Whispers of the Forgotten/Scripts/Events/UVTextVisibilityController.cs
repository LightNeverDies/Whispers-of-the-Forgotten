using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UVTextVisibilityController : MonoBehaviour
{
    public Light uvLight;
    public List<TextMeshPro> targetTexts;
    public Color invisibleColor = new Color(1, 1, 1, 0);
    public Color visibleColor = Color.white;
    public float detectionDistance = 10f;
    public float detectionAngle = 30f;

    public bool missionStarted = false;

    private FlashlightController flashlightController;

    void Start()
    {
        foreach (var text in targetTexts)
        {
            if (text != null)
            {
                text.color = invisibleColor;
            }
        }

        flashlightController = uvLight.GetComponentInParent<FlashlightController>();
    }

    void Update()
    {
        if (!missionStarted || uvLight == null || targetTexts.Count == 0 || flashlightController == null) return;

        if (!flashlightController.IsUVLightOn)
        {
            SetAllTextsVisibility(false);
            return;
        }

        foreach (var text in targetTexts)
        {
            Vector3 directionToText = (text.transform.position - uvLight.transform.position).normalized;
            float angle = Vector3.Angle(uvLight.transform.forward, directionToText);

            if (angle < detectionAngle / 2f && Vector3.Distance(uvLight.transform.position, text.transform.position) <= detectionDistance)
            {
                SetTextVisibility(text, true);
            }
            else
            {
                SetTextVisibility(text, false);
            }
        }
    }

    public void StartMission()
    {
        missionStarted = true;
    }

    private void SetTextVisibility(TextMeshPro text, bool visible)
    {
        if (visible)
        {
            text.color = visibleColor;
        }
        else
        {
            text.color = invisibleColor;
        }
    }

    private void SetAllTextsVisibility(bool visible)
    {
        foreach (var text in targetTexts)
        {
            SetTextVisibility(text, visible);
        }
    }
}
