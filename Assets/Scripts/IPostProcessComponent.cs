namespace PostProcess
{
    public interface IPostProcessComponent
    {
        bool IsEnabled { get; set; }
        void Render(PostProcessContext context);
        void FrameCleanup();
    }
}