using System.Collections;
using UnityEngine;
using Interfaces;

/// <summary>
/// Triggers one or more TimedAnimationTrigger actions when the player looks at this object (raycast from screen center).
/// Designed for "look at X -> animate/play audio/subtitles on other objects".
/// </summary>
public class LookEventTrigger : MonoBehaviour
{
    public enum StepType
    {
        TimedAnimationTrigger = 0,
        EventEffect = 1,
        Audio = 2
    }

    [System.Serializable]
    public class LookStep
    {
        [Tooltip("What this step does.")]
        public StepType type = StepType.TimedAnimationTrigger;

        [Header("Timing")]
        [Tooltip("Delay (seconds) before executing this step.")]
        public float delayBefore = 0f;

        [Tooltip("Delay (seconds) after executing this step.")]
        public float delayAfter = 0f;

        [Header("Power (optional)")]
        [Tooltip("If true, this step only runs when power is ON.")]
        public bool requirePowerOn = false;

        [Header("TimedAnimationTrigger step")]
        public TimedAnimationTrigger timedAction;

        [Tooltip("If true, TimedAnimationTrigger will be resolved by component index from a GameObject (useful when you have 2+ TimedAnimationTrigger components on the same object).")]
        public bool resolveTimedActionByIndex = false;

        [Tooltip("GameObject to pull TimedAnimationTrigger components from. If null, uses this LookEventTrigger's GameObject.")]
        public GameObject timedActionObject;

        [Tooltip("Which TimedAnimationTrigger component to use from the object (0 = first, 1 = second, etc.).")]
        public int timedActionComponentIndex = 0;

        [Header("EventEffect step")]
        [Tooltip("Any component that implements IEventEffect (e.g., FlickeringLightController).")]
        public MonoBehaviour eventEffectBehaviour;

        [Tooltip("If true, calls StartEffect(); if false, calls StopEffect().")]
        public bool startEffect = true;

        [Header("Audio step")]
        public AudioSource audioSource;

        [Tooltip("If true, plays the audioSource; if false, stops it.")]
        public bool playAudio = true;

        [Tooltip("If true, applies random pitch variation (0.9 to 1.1) before playing.")]
        public bool useRandomPitch = false;
    }

    [Header("Raycast")]
    [Tooltip("Camera used for the look ray. If null, will use Camera.main.")]
    public Camera mainCamera;

    [Tooltip("Max distance for look detection.")]
    public float maxDistance = 3f;

    [Tooltip("Layers that can be hit by the look ray. Leave as 'Nothing' to use everything.")]
    public LayerMask targetLayer;

    [Tooltip("Layers that can block the ray. Leave as 'Nothing' to use everything except targetLayer.")]
    public LayerMask obstructionLayer;

    [Header("Gaze Trigger")]
    [Tooltip("How long (seconds) the player must keep looking at this object before triggering.")]
    public float gazeTimeToTrigger = 0.2f;

    [Tooltip("If true, triggers only once.")]
    public bool triggerOnce = true;

    [Tooltip("Cooldown between triggers (only used when triggerOnce is false).")]
    public float repeatCooldown = 1.0f;

    [Header("Sequence")]
    [Tooltip("If true, runs actions in order instead of all at once.")]
    public bool runSequentially = true;

    [Tooltip("If true, waits for each TimedAnimationTrigger step to complete before moving to the next step.")]
    public bool waitForTimedActionToComplete = true;

    [Tooltip("Delay between actions when using the old 'actions' array (only used when runSequentially is true and steps is empty).")]
    public float delayBetweenActions = 0.25f;

    [Tooltip("If true and triggerOnce is false, resets TimedAnimationTrigger(s) before each trigger so they can play again.")]
    public bool resetTimedActionsOnRepeat = true;

    [Header("Actions")]
    [Tooltip("Actions to trigger when looked at. Each TimedAnimationTrigger can have its own Animator, AudioSource and subtitles.")]
    public TimedAnimationTrigger[] actions;

    [Tooltip("Optional ordered steps. If not empty, these will be used instead of 'actions'.")]
    public LookStep[] steps;

    [Header("Power Requirement (optional)")]
    [Tooltip("If set, can be used to require power ON for triggering.")]
    public PowerButtonController powerController;

    [Tooltip("If true, the trigger (and any steps) will only run when power is ON.")]
    public bool requirePowerOnToTrigger = false;

    [Tooltip("If true and PowerButtonController is missing, treat it as powered ON.")]
    public bool assumePowerOnIfMissing = true;

    [Header("Optional: Subtitle on Look")]
    public SubtitleManager subtitleManager;
    public bool showSubtitleOnLook = false;

    [TextArea(2, 4)]
    public string lookSubtitleText = "";

    public float lookSubtitleDuration = 2f;

    private float gazeTimer = 0f;
    private bool hasTriggered = false;
    private float lastTriggerTime = -9999f;
    private bool isRunning = false;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (subtitleManager == null)
        {
            subtitleManager = FindObjectOfType<SubtitleManager>();
        }

        if (powerController == null)
        {
            powerController = FindObjectOfType<PowerButtonController>();
        }

        ValidateConfiguration();
    }

    private void Update()
    {
        if (mainCamera == null) return;

        int rayMask = targetLayer.value != 0 ? targetLayer.value : ~0;
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, rayMask))
        {
            // Only trigger when the ray is hitting THIS object (or its children).
            LookEventTrigger triggerOnHit = hit.collider.GetComponentInParent<LookEventTrigger>();
            if (triggerOnHit != this)
            {
                ResetGaze();
                return;
            }

            // Obstruction check between camera and hit point (similar style used in other scripts).
            Vector3 camPos = mainCamera.transform.position;
            Vector3 directionToHit = hit.point - camPos;
            float distanceToHit = Vector3.Distance(camPos, hit.point);

            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~targetLayer;
            if (Physics.Raycast(camPos, directionToHit.normalized, out RaycastHit obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                if (obstructionHit.collider != hit.collider)
                {
                    // If the obstruction is not part of this trigger object, block.
                    LookEventTrigger obstructionTrigger = obstructionHit.collider.GetComponentInParent<LookEventTrigger>();
                    if (obstructionTrigger != this)
                    {
                        ResetGaze();
                        return;
                    }
                }
            }

            // Gaze timer
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= gazeTimeToTrigger)
            {
                TryTrigger();
                // prevent immediate re-trigger while still looking
                gazeTimer = 0f;
            }
        }
        else
        {
            ResetGaze();
        }
    }

    private void ResetGaze()
    {
        gazeTimer = 0f;
    }

    private void TryTrigger()
    {
        if (triggerOnce && hasTriggered) return;
        if (!triggerOnce && Time.time < lastTriggerTime + repeatCooldown) return;
        if (isRunning) return;

        if (requirePowerOnToTrigger && !IsPowerOn())
        {
            return;
        }

        lastTriggerTime = Time.time;

        if (showSubtitleOnLook && subtitleManager != null && !string.IsNullOrEmpty(lookSubtitleText))
        {
            subtitleManager.ShowSubtitle(lookSubtitleText, lookSubtitleDuration);
        }

        // If repeatable, optionally reset timed actions so they can fire again.
        if (!triggerOnce && resetTimedActionsOnRepeat)
        {
            ResetTimedActionsUsedByThisTrigger();
        }

        if (runSequentially)
        {
            StartCoroutine(RunSequenceCoroutine());
        }
        else
        {
            TriggerAllAtOnce();
        }

        hasTriggered = true;
    }

    private bool IsPowerOn()
    {
        if (powerController == null) return assumePowerOnIfMissing;
        return powerController.hasPower;
    }

    private void ResetTimedActionsUsedByThisTrigger()
    {
        if (steps != null && steps.Length > 0)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i] != null && steps[i].type == StepType.TimedAnimationTrigger)
                {
                    TimedAnimationTrigger a = ResolveTimedAction(steps[i]);
                    if (a != null) a.ResetTrigger();
                }
            }
        }

        if (actions != null && actions.Length > 0)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                {
                    actions[i].ResetTrigger();
                }
            }
        }
    }

    private void TriggerAllAtOnce()
    {
        // Prefer steps if provided, otherwise use old 'actions' array.
        if (steps != null && steps.Length > 0)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                ExecuteStepImmediate(steps[i]);
            }
            return;
        }

        if (actions != null)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                {
                    actions[i].ManualTrigger();
                }
            }
        }
    }

    private void ExecuteStepImmediate(LookStep step)
    {
        if (step == null) return;
        if (step.requirePowerOn && !IsPowerOn()) return;

        switch (step.type)
        {
            case StepType.TimedAnimationTrigger:
                {
                    TimedAnimationTrigger a = ResolveTimedAction(step);
                    if (a != null) a.ManualTrigger();
                }
                break;

            case StepType.EventEffect:
                if (step.eventEffectBehaviour != null)
                {
                    IEventEffect effect = step.eventEffectBehaviour as IEventEffect;
                    if (effect != null)
                    {
                        if (step.startEffect) effect.StartEffect();
                        else effect.StopEffect();
                    }
                }
                break;

            case StepType.Audio:
                if (step.audioSource != null)
                {
                    step.audioSource.gameObject.SetActive(true);
                    if (step.playAudio)
                    {
                        if (step.useRandomPitch)
                        {
                            step.audioSource.pitch = Random.Range(0.9f, 1.1f);
                        }
                        step.audioSource.Play();
                    }
                    else
                    {
                        step.audioSource.Stop();
                    }
                }
                break;
        }
    }

    private IEnumerator RunSequenceCoroutine()
    {
        isRunning = true;

        // Prefer steps if provided, otherwise sequence through old 'actions' array.
        if (steps != null && steps.Length > 0)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                LookStep step = steps[i];
                if (step == null) continue;

                if (step.delayBefore > 0f)
                    yield return new WaitForSeconds(step.delayBefore);

                if (step.requirePowerOn && !IsPowerOn())
                {
                    // Skip this step if power is off.
                    if (step.delayAfter > 0f)
                        yield return new WaitForSeconds(step.delayAfter);
                    continue;
                }

                if (step.type == StepType.TimedAnimationTrigger)
                {
                    TimedAnimationTrigger a = ResolveTimedAction(step);
                    if (a != null)
                    {
                        if (waitForTimedActionToComplete)
                        {
                            yield return StartCoroutine(a.TriggerNowRoutine());
                        }
                        else
                        {
                            a.ManualTrigger();
                        }
                    }
                }
                else if (step.type == StepType.EventEffect)
                {
                    if (step.eventEffectBehaviour != null)
                    {
                        IEventEffect effect = step.eventEffectBehaviour as IEventEffect;
                        if (effect != null)
                        {
                            if (step.startEffect) effect.StartEffect();
                            else effect.StopEffect();
                        }
                    }
                }
                else if (step.type == StepType.Audio)
                {
                    if (step.audioSource != null)
                    {
                        step.audioSource.gameObject.SetActive(true);
                        if (step.playAudio)
                        {
                            if (step.useRandomPitch)
                            {
                                step.audioSource.pitch = Random.Range(0.9f, 1.1f);
                            }
                            step.audioSource.Play();
                        }
                        else
                        {
                            step.audioSource.Stop();
                        }
                    }
                }

                if (step.delayAfter > 0f)
                    yield return new WaitForSeconds(step.delayAfter);
            }
        }
        else
        {
            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    TimedAnimationTrigger a = actions[i];
                    if (a == null) continue;

                    if (waitForTimedActionToComplete)
                        yield return StartCoroutine(a.TriggerNowRoutine());
                    else
                        a.ManualTrigger();

                    if (delayBetweenActions > 0f)
                        yield return new WaitForSeconds(delayBetweenActions);
                }
            }
        }

        isRunning = false;
    }

    private TimedAnimationTrigger ResolveTimedAction(LookStep step)
    {
        if (step == null) return null;

        // Default behavior: use direct reference
        if (!step.resolveTimedActionByIndex)
            return step.timedAction;

        // Indexed resolution: use provided object or fallback to this trigger's object
        GameObject obj = step.timedActionObject != null ? step.timedActionObject : gameObject;
        TimedAnimationTrigger[] list = obj.GetComponents<TimedAnimationTrigger>();
        if (list == null || list.Length == 0) return null;

        int idx = step.timedActionComponentIndex;
        if (idx < 0) idx = 0;
        if (idx >= list.Length) idx = list.Length - 1;
        return list[idx];
    }

    private void ValidateConfiguration()
    {
        if (steps == null || steps.Length == 0) return;

        for (int i = 0; i < steps.Length; i++)
        {
            LookStep a = steps[i];
            if (a == null) continue;

            if (a.type == StepType.TimedAnimationTrigger && ResolveTimedAction(a) == null)
            {
                Debug.LogWarning($"[LookEventTrigger] Step {i} is TimedAnimationTrigger but Timed Action is not assigned (or index resolution failed) on '{name}'.", this);
            }

            for (int j = i + 1; j < steps.Length; j++)
            {
                LookStep b = steps[j];
                if (b == null) continue;

                if (a.type == StepType.TimedAnimationTrigger && b.type == StepType.TimedAnimationTrigger)
                {
                    TimedAnimationTrigger ra = ResolveTimedAction(a);
                    TimedAnimationTrigger rb = ResolveTimedAction(b);
                    if (ra != null && rb != null && ra == rb)
                    {
                        Debug.LogWarning(
                            $"[LookEventTrigger] Steps {i} and {j} reference the SAME TimedAnimationTrigger ('{ra.name}') on '{name}'. " +
                            "That component fires only once; use two separate TimedAnimationTrigger components (one bool=true, one bool=false) AND make sure each step points to a different one (use 'resolveTimedActionByIndex' with different indices).",
                            this
                        );
                    }
                }
            }
        }
    }
}


