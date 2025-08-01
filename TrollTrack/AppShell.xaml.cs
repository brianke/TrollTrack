namespace TrollTrack
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register additional routes for sub-pages
            Routing.RegisterRoute("dashboard", typeof(Views.Dashboard));
            Routing.RegisterRoute("catches", typeof(Views.Catches));
            Routing.RegisterRoute("program", typeof(Views.Program));
            Routing.RegisterRoute("lures", typeof(Views.Lures));
            Routing.RegisterRoute("analytics", typeof(Views.Analytics));
        }
    }
}
