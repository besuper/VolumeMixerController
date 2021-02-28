using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;
using CSCore.CoreAudioAPI;

using System.Collections.Generic;
using Fleck;
using System.Reflection;
using System.IO;

namespace MixerControllerF {

    static class Program {

        public static Dictionary<string, float> apps = new Dictionary<string, float>();

        [MTAThread]
        static void Main() {

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MixerController\\config.txt";

            if (!File.Exists(path)) {

                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MixerController");

                using (FileStream fs = File.Create(path)) {
                    byte[] info = new System.Text.UTF8Encoding(true).GetBytes("0.0.0.0:25565");
                    fs.Write(info, 0, info.Length);
                }
            }

            string ip = "";

            if (File.Exists(path)) {
                using (StreamReader sr = File.OpenText(path)) {
                    string s = "";
                    while ((s = sr.ReadLine()) != null) {
                        ip = s;
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var server = new WebSocketServer("ws://" + ip);

            RefreshApps();

            server.Start(socket => {
                socket.OnOpen = () => RefreshApps();
                socket.OnClose = () => RefreshApps();

                socket.OnMessage = message => {

                    if (message.StartsWith("VOLUME")) {
                        ChangeVolume(message.Split(':')[1], float.Parse(message.Split(':')[2]));
                    }

                    if (message.StartsWith("MUTE")) {
                        ChangeState(message.Split(':')[1], message.Split(':')[0]);
                    }

                    if (message.StartsWith("UNMUTE")) {
                        ChangeState(message.Split(':')[1], message.Split(':')[0]);
                    }

                    if (message.StartsWith("APPS")) {

                        RefreshApps();

                        string to_send = "{\"apps\":{";

                        foreach (KeyValuePair<string, float> entry in apps) {
                            to_send += "\"" + entry.Key + "\":\"" + entry.Value + "\",";
                        }

                        to_send = to_send.Remove(to_send.Length - 1);
                        to_send += "}}";

                        socket.Send(to_send);

                    }

                };
            });

            Application.Run(new MyCustomApplicationContext());
        }

        public static void ChangeVolume(string app_name, float volume) {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)) {

                using (var sessionEnumerator = sessionManager.GetSessionEnumerator()) {

                    foreach (var session in sessionEnumerator) {

                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>()) {

                            if (sessionControl.Process.ProcessName.Contains(app_name)) {

                                if (volume > 0 && simpleVolume.IsMuted) {
                                    simpleVolume.IsMuted = false;
                                }

                                simpleVolume.MasterVolume = (volume / 100);
                            }

                            RefreshApps();

                        }

                    }
                }
            }
        }

        public static void ChangeState(string app_name, string state) {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)) {

                using (var sessionEnumerator = sessionManager.GetSessionEnumerator()) {

                    foreach (var session in sessionEnumerator) {

                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>()) {

                            if (sessionControl.Process.ProcessName.Contains(app_name)) {
                                simpleVolume.IsMuted = state == "MUTE";
                            }

                            RefreshApps();
                        }

                    }
                }
            }
        }

        public static void RefreshApps() {
            apps.Clear();

            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)) {

                using (var sessionEnumerator = sessionManager.GetSessionEnumerator()) {

                    foreach (var session in sessionEnumerator) {

                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>()) {

                            string name = sessionControl.Process.ProcessName;
                            float current_volume = simpleVolume.IsMuted ? -1 : simpleVolume.MasterVolume;

                            if (!apps.ContainsKey(name)) {
                                apps.Add(name, current_volume);
                            }
                        }

                    }
                }
            }
        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow) {
            using (var enumerator = new MMDeviceEnumerator()) {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia)) {
                    return AudioSessionManager2.FromMMDevice(device);
                }
            }
        }
    }

    public class MyCustomApplicationContext : ApplicationContext {

        private readonly NotifyIcon trayIcon;

        public MyCustomApplicationContext() {
            trayIcon = new NotifyIcon() {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Settings", Settings),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        void Settings(object sender, EventArgs e) {
            Process.Start("notepad.exe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MixerController\\config.txt");
        }

        void Exit(object sender, EventArgs e) {
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
