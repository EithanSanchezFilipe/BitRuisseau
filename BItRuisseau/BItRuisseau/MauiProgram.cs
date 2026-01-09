using BitRuisseau.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using BitRuisseau.ViewModel;
namespace BitRuisseau
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Env.Load();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>().UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
            string broker = Env.GetString("MQTT_BROKER") ?? "localhost";
            string username = Env.GetString("MQTT_USERNAME") ?? "user";
            string password = Env.GetString("MQTT_PASSWORD") ?? "pass";
            string clientId = Env.GetString("MQTT_CLIENT_ID") ?? Guid.NewGuid().ToString();

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<MusicService>();
            builder.Services.AddSingleton<PlayerService>();
            builder.Services.AddSingleton<MqttService>(sp =>
                       new MqttService("mqtt.blue.section-inf.ch", "ict", "321", clientId));
            builder.Services.AddSingleton<AgentService>(sp =>
            {
                var mqttService = sp.GetRequiredService<MqttService>();
                var musicService = sp.GetRequiredService<MusicService>();

                return new AgentService(mqttService);
            });
            builder.Services.AddSingleton<HomePageViewModel>();
            builder.Services.AddSingleton<LocalMediaCenterService>();
            builder.Services.AddLucideIcons();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
