namespace MixedReality.Toolkit.Themes
{
    public interface IBinder
    {
        void Subscribe(ThemeDataSource themeDataSource);
        void Unsubscribe(ThemeDataSource themeDataSource);

        ThemeDefinition ThemeDefinition { get; set; }
        string ThemeDefinitionItemName { get; set; }
    }
}
