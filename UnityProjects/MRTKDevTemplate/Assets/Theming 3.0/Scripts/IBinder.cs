namespace MixedReality.Toolkit.Themes
{
    public interface IBinder
    {
        void Subscribe();
        void Unsubscribe();

        ThemeDefinition ThemeDefinition { get; set; }
    }
}
