using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UVTextVisibilityController : MonoBehaviour
{
    public Light uvLight;  // UV светлина
    public List<TextMeshPro> targetTexts;  // Списък с TextMeshPro компоненти
    public Color invisibleColor = new Color(1, 1, 1, 0);  // Цвят за невидимо състояние (прозрачен)
    public Color visibleColor = Color.white;  // Цвят за видимо състояние
    public float detectionDistance = 10f;  // Дистанция за проверка на видимостта с UV светлина
    public float detectionAngle = 30f;  // Ъгъл за проверка дали UV светлината е насочена към текста

    private bool missionStarted = false;  // Променлива за проверка на състоянието на мисията

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
        if (flashlightController == null)
        {
            Debug.LogError("FlashlightController not found!");
        }
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
