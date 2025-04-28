using UnityEngine.Rendering.Universal;

public class LowResRenderFeature : ScriptableRendererFeature
{
    LowResPass lowResPass;

    public override void Create()
    {
        lowResPass = new LowResPass(RenderPassEvent.BeforeRenderingPostProcessing);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // LowResPass のセットアップはここで行わない
        // renderer.EnqueuePass だけを行う
        renderer.EnqueuePass(lowResPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        // cameraColorTargetHandle を低解像度パスに設定
        lowResPass.Setup(renderer.cameraColorTargetHandle);
    }
}
