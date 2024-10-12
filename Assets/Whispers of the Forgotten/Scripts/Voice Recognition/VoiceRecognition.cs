using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceRecognitionManager : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private string[] keywords = {"what happened here", "can you see me", "why are you here", "can you help me" };

    public SubtitleManager subtitleManager;

    void Start()
    {
        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        ProcessCommand(args.text);
    }

    void ProcessCommand(string command)
    {
        switch (command.ToLower())
        {
            case "what happened here":
                subtitleManager.ShowSubtitle("Death");
                break;
            case "can you see me":
                subtitleManager.ShowSubtitle("Yes");
                break;
            case "why are you here":
                subtitleManager.ShowSubtitle("Trapped");
                break;
            case "can you help me":
                subtitleManager.ShowSubtitle("No");
                break;
            default:
                subtitleManager = null;
                break;
        }
    }
}
