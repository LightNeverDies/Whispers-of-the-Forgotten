using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Digital Glitch effect for Post-Processing Stack v2 (Built-in Render Pipeline)
/// This works with the old Post-Processing Stack v2 system
/// </summary>
[System.Serializable]
[PostProcess(typeof(DigitalGlitchRenderer), PostProcessEvent.AfterStack, "Custom/Digital Glitch")]
public sealed class DigitalGlitchPostProcess : PostProcessEffectSettings
{
    [Tooltip("Intensity of the glitch effect (0 = no effect, 1 = full effect)")]
    [Range(0f, 1f)]
    public FloatParameter intensity = new FloatParameter { value = 0f };
    
    [Tooltip("Speed of the glitch effect")]
    public FloatParameter speed = new FloatParameter { value = 10f };
    
    [Tooltip("Amount of horizontal displacement")]
    public FloatParameter horizontalDisplacement = new FloatParameter { value = 0.1f };
    
    [Tooltip("Amount of vertical displacement")]
    public FloatParameter verticalDisplacement = new FloatParameter { value = 0.05f };
    
    [Tooltip("Color shift intensity")]
    public FloatParameter colorShift = new FloatParameter { value = 0.2f };
    
    [Tooltip("Noise intensity")]
    public FloatParameter noiseIntensity = new FloatParameter { value = 0.3f };
    
    [Tooltip("Block size for the glitch effect")]
    public FloatParameter blockSize = new FloatParameter { value = 10f };
    
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value && intensity.value > 0f;
    }
}

/// <summary>
/// Renderer for Digital Glitch effect in Post-Processing Stack v2
/// </summary>
public sealed class DigitalGlitchRenderer : PostProcessEffectRenderer<DigitalGlitchPostProcess>
{
    private static readonly int Intensity = Shader.PropertyToID("_Intensity");
    private static readonly int Speed = Shader.PropertyToID("_Speed");
    private static readonly int HorizontalDisplacement = Shader.PropertyToID("_HorizontalDisplacement");
    private static readonly int VerticalDisplacement = Shader.PropertyToID("_VerticalDisplacement");
    private static readonly int ColorShift = Shader.PropertyToID("_ColorShift");
    private static readonly int NoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int BlockSize = Shader.PropertyToID("_BlockSize");
    private static readonly int Time = Shader.PropertyToID("_Time");
    
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/DigitalGlitch"));
        
        sheet.properties.SetFloat(Intensity, settings.intensity);
        sheet.properties.SetFloat(Speed, settings.speed);
        sheet.properties.SetFloat(HorizontalDisplacement, settings.horizontalDisplacement);
        sheet.properties.SetFloat(VerticalDisplacement, settings.verticalDisplacement);
        sheet.properties.SetFloat(ColorShift, settings.colorShift);
        sheet.properties.SetFloat(NoiseIntensity, settings.noiseIntensity);
        sheet.properties.SetFloat(BlockSize, settings.blockSize);
        sheet.properties.SetFloat(Time, UnityEngine.Time.time);
        
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}

