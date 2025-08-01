namespace TrollTrack
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register additional routes for sub-pages
            Routing.RegisterRoute("dashboard", typeof(MVVM.Views.Dashboard));
            Routing.RegisterRoute("catches", typeof(MVVM.Views.Catches));
            Routing.RegisterRoute("program", typeof(MVVM.Views.Program));
            Routing.RegisterRoute("lures", typeof(MVVM.Views.Lures));
            Routing.RegisterRoute("analytics", typeof(MVVM.Views.Analytics));
        }
    }
}
