using System;
using System.Windows.Forms;
using CSCore.CoreAudioAPI;

using System.Collections.Generic;
using Fleck;
using System.IO;
using MixerController;

namespace MixerControllerF {

    static class Global {
        public static string DEFAULT_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MixerController";
        public static string SETTINGS = "config.txt";
        public static string DEFAULT_PATH = DEFAULT_FOLDER + "\\" + SETTINGS;
    }

    static class Program {
        public static Dictionary<string, SoundApplication> applications = new Dictionary<string, SoundApplication>();

        [MTAThread]
        static void Main() {

            if (!File.Exists(Global.DEFAULT_PATH)) {

                Directory.CreateDirectory(Global.DEFAULT_FOLDER);

                using (FileStream fs = File.Create(Global.DEFAULT_PATH)) {
                    byte[] info = new System.Text.UTF8Encoding(true).GetBytes("0.0.0.0:25565");
                    fs.Write(info, 0, info.Length);
                }
            }

            string ip = "";

            if (File.Exists(Global.DEFAULT_PATH)) {
                using (StreamReader sr = File.OpenText(Global.DEFAULT_PATH)) {
                    ip = sr.ReadLine();
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

                        foreach (KeyValuePair<string, SoundApplication> entry in applications) {
                            to_send += "\"" + entry.Key + "\":\"" + entry.Value.Volume + "\",";
                        }

                        to_send = to_send.Remove(to_send.Length - 1);
                        to_send += "}}";

                        socket.Send(to_send);

                    }

                };
            });

            Application.Run(new DefaultApp());
        }

        public static SoundApplication GetApplication(string app_name) {
            if (applications.ContainsKey(app_name)) {

                applications.TryGetValue(app_name, out SoundApplication app);

                return app;
            }

            return null;
        }

        public static void ChangeVolume(string app_name, float volume) {
            SoundApplication app = GetApplication(app_name);

            if (app == null) return;

            if (app.Volume > 0 && app.AudioInt.IsMuted) {
                app.AudioInt.IsMuted = false;
            }

            app.AudioInt.MasterVolume = (volume / 100);

            RefreshApps();
        }

        public static void ChangeState(string app_name, string state) {
            SoundApplication app = GetApplication(app_name);

            if (app == null) return;

            app.AudioInt.IsMuted = state == "MUTE";

            RefreshApps();
        }

        public static void RefreshApps() {
            applications.Clear();

            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)) {

                using (var sessionEnumerator = sessionManager.GetSessionEnumerator()) {

                    foreach (var session in sessionEnumerator) {

                        var simpleVolume = session.QueryInterface<SimpleAudioVolume>();
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>()) {

                            string name = sessionControl.Process.ProcessName;
                            float current_volume = simpleVolume.IsMuted ? -1 : simpleVolume.MasterVolume;

                            if (!applications.ContainsKey(name)) {
                                applications.Add(name, new SoundApplication(current_volume, ref simpleVolume));
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
}
