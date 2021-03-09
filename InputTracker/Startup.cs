using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InputTracker.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Windows.Forms;

namespace InputTracker
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private CaptureInputDotNet.MouseHook mouseHook;
        private CaptureInputDotNet.KeyboardHook keyboardHook;

        private IHubContext<KeyboardHub> keyboardHubContext;
        private IHubContext<MouseHub> mouseHubContext;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private void InitialiseSystemHooksAndProcessMessages()
        {
            mouseHook = new CaptureInputDotNet.MouseHook();

            mouseHook.MouseLeftDownEvent += new CaptureInputDotNet.MouseHook.MouseButtonEventHandler(MouseHook_MouseLeftDownEvent);
            mouseHook.MouseLeftUpEvent += new CaptureInputDotNet.MouseHook.MouseButtonEventHandler(MouseHook_MouseLeftUpEvent);
            mouseHook.MouseRightDownEvent += new CaptureInputDotNet.MouseHook.MouseButtonEventHandler(MouseHook_MouseRightDownEvent);
            mouseHook.MouseRightUpEvent += new CaptureInputDotNet.MouseHook.MouseButtonEventHandler(MouseHook_MouseRightUpEvent);
            mouseHook.MouseScrollEvent += new CaptureInputDotNet.MouseHook.MouseScrollEventHandler(MouseHook_MouseScrollEvent);

            mouseHook.InstallHook();

            keyboardHook = new CaptureInputDotNet.KeyboardHook(CaptureInputDotNet.KeyboardLayout.DVORAK);

            keyboardHook.KeyboardEvent += new CaptureInputDotNet.KeyboardHook.KeyboardEventHandler(KeyboardHook_KeyboardEvent);
            keyboardHook.InstallHook();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run();
        }

        private void KeyboardHook_KeyboardEvent(CaptureInputDotNet.KeyboardEvents kEvent, Keys key)
        {
            if (kEvent == CaptureInputDotNet.KeyboardEvents.KeyDown || kEvent == CaptureInputDotNet.KeyboardEvents.SystemKeyDown) {
                keyboardHubContext.Clients.All.SendAsync("RecieveKeyState", key.ToString(), true);
            } 
            else if (kEvent == CaptureInputDotNet.KeyboardEvents.KeyUp || kEvent == CaptureInputDotNet.KeyboardEvents.SystemKeyUp)
            {
                keyboardHubContext.Clients.All.SendAsync("RecieveKeyState", key.ToString(), false);
            }
        }

        private void MouseHook_MouseLeftDownEvent(CaptureInputDotNet.MouseEvents mouseEvents)
        {
            mouseHubContext.Clients.All.SendAsync("RecieveKeyState", Keys.LButton.ToString(), true);
        }

        private void MouseHook_MouseLeftUpEvent(CaptureInputDotNet.MouseEvents mouseEvents)
        {
            mouseHubContext.Clients.All.SendAsync("RecieveKeyState", Keys.LButton.ToString(), false);
        }

        private void MouseHook_MouseRightDownEvent(CaptureInputDotNet.MouseEvents mouseEvents)
        {
            mouseHubContext.Clients.All.SendAsync("RecieveKeyState", Keys.RButton.ToString(), true);
        }

        private void MouseHook_MouseRightUpEvent(CaptureInputDotNet.MouseEvents mouseEvents)
        {
            mouseHubContext.Clients.All.SendAsync("RecieveKeyState", Keys.RButton.ToString(), false);
        }

        private void MouseHook_MouseScrollEvent(CaptureInputDotNet.MouseEvents mouseEvents, int delta)
        {
            if (delta < 0)
            {
                keyboardHubContext.Clients.All.SendAsync("RecieveKeyState", Keys.Space.ToString(), true);

                var timer = new System.Threading.Timer(x => ReleaseSpace(), null, 50, Timeout.Infinite);
            }
        }

        private void ReleaseSpace()
        {
            keyboardHubContext.Clients.All.SendAsync("RecieveKeyState", Keys.Space.ToString(), false);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<KeyboardHub>("/keyboardHub");
                endpoints.MapHub<MouseHub>("/mouseHub");
            });

            keyboardHubContext = app.ApplicationServices.GetService<IHubContext<KeyboardHub>>();
            mouseHubContext = app.ApplicationServices.GetService<IHubContext<MouseHub>>();

            Thread messageThread = new Thread(() => InitialiseSystemHooksAndProcessMessages());
            messageThread.SetApartmentState(ApartmentState.STA);
            messageThread.Start();
        }

        ~Startup()
        {
            if (mouseHook != null)
            {
                mouseHook.UninstallHook();
                mouseHook.Dispose();
            }
        }
    }
}
