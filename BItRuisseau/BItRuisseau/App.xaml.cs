using BitRuisseau.Services;

namespace BitRuisseau
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage())
            {
                Title = "BitRuisseau"
            };

            window.Destroying += OnWindowDestroying;

            return window;
        }

        private async void OnWindowDestroying(object? sender, EventArgs e)
        {
            try
            {
                var agent = _services.GetService<AgentService>();
                if (agent != null)
                {
                    await agent.StopAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during shutdown: {ex}");
            }
        }
    }
}
