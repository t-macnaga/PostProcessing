public interface IPostProcessComponent
{
    bool IsEnabled { get; set; }
    void Render(PostProcessContext context);
}
