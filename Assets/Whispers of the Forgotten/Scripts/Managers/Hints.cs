using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hints : MonoBehaviour
{
    public Text hintText;
    public float hintDuration;
    public bool isLoadingScreen;

    private string currentHint = "";
    private float hintTimer;
    private bool showInitialHints = true;

    string[] initialHints = new string[]
    {
        "Hint: Press W A S D to move.",
        "Hint: Press F to turn on/off or change the flashlight mode.",
        "Hint: Hold RMB to zoom.",
        "Hint: Everything around you is a key (listen, look around).",
        "Hint: The game cannot be saved."
    };

    private void Start()
    {
        if (isLoadingScreen)
        {
            StartCoroutine(ShowInitialHints());
        }
    }

    private void Update()
    {
        if (!showInitialHints && hintText.gameObject.activeSelf)
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0)
            {
                HideHint();
            }
        }
    }

    private IEnumerator ShowInitialHints()
    {
        foreach (string hint in initialHints)
        {
            ShowHint(hint);
            yield return new WaitForSeconds(hintDuration);
        }

        showInitialHints = false;
    }

    public void ShowHint(string hint)
    {
        currentHint = hint;
        hintText.text = currentHint;
        hintText.gameObject.SetActive(true);
        hintTimer = hintDuration;
    }

    public void HideHint()
    {
        currentHint = "";
        hintText.text = currentHint;
        hintText.gameObject.SetActive(false);
        hintTimer = 0f;
    }

    public bool IsShowingHint()
    {
        return hintText.gameObject.activeSelf;
    }
}
