using NLog;
using NLog.Config;
using NLog.Targets;
using web.Data;
using LogLevel = NLog.LogLevel;

namespace web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new LoggingConfiguration();

            // FileTarget�𐶐��� LoggingConfiguration�ɐݒ�
            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // fileTarget�̃v���p�e�B��ݒ�
            fileTarget.Name = "f";
            fileTarget.FileName = "C:\\Users\\shuitt\\�h�L�������g\\programing\\share\\web\\web\\Logs\\${shortdate}.log";
            fileTarget.Layout = "${longdate} [${uppercase:${level}}] ${message}";

            // LoggingRule���`
            var rule1 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule1);

            // �ݒ��L����
            LogManager.Configuration = config;
            Logger log = LogManager.GetCurrentClassLogger();
            log.Debug("program is start");
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}