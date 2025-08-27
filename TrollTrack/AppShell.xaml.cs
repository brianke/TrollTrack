namespace TrollTrack
{
    public partial class AppShell : Shell, INotifyPropertyChanged
    {
        public AppShell()
        {
            InitializeComponent();

            // Register additional routes for sub-pages
            Routing.RegisterRoute("dashboard", typeof(DashboardView));
            Routing.RegisterRoute("catches", typeof(CatchesView));
            Routing.RegisterRoute("program", typeof(ProgramsView));
            Routing.RegisterRoute("lures", typeof(LuresView));
            Routing.RegisterRoute("analytics", typeof(AnalyticsView));
        }
    }
}
