namespace TrollTrack
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

#if DEBUG
            MainTabBar.Items.Remove(AboutTab);
#else
            MainTabBar.Items.Remove(SettingsTab);
#endif
        }
    }
}