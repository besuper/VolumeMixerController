using MixerControllerF;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MixerController {
    public class DefaultApp : ApplicationContext {

        private readonly NotifyIcon trayIcon;

        public DefaultApp() {

            MenuItem StartupItem = new MenuItem {
                Checked = Startup.IsInStartup(),
                Text = "Startup"
            };
            StartupItem.Click += new EventHandler(StartupClick);

            trayIcon = new NotifyIcon() {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    StartupItem,
                    new MenuItem("-"),
                    new MenuItem("Settings", Settings),
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        void Settings(object sender, EventArgs e) {
            Process.Start("notepad.exe", Global.DEFAULT_PATH);
        }

        void Exit(object sender, EventArgs e) {
            trayIcon.Visible = false;

            Application.Exit();
        }

        void StartupClick(object sender, EventArgs e) {
            MenuItem item = (MenuItem)sender;

            if(item.Checked && Startup.IsInStartup()) {
                Startup.RemoveFromStartup();
            }

            if(!item.Checked && !Startup.IsInStartup()) {
                Startup.RunOnStartup();
            }

            item.Checked = !item.Checked;
        }
    }
}
