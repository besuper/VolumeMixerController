using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace MixerController {
    class Startup {

        public static bool RunOnStartup() {
            try {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue(Application.ProductName, Application.ExecutablePath);
            } catch (Exception) {
                return false;
            }
            return true;
        }

        public static bool RemoveFromStartup() {

            string AppTitle = Application.ProductName;
            string AppPath = Application.ExecutablePath;

            try {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (AppPath == null) {
                    rk.DeleteValue(AppTitle);
                } else {
                    if (rk.GetValue(AppTitle).ToString().ToLower() == AppPath.ToLower()) {
                        rk.DeleteValue(AppTitle);
                    }
                }
            } catch (Exception) {
                return false;
            }
            return true;
        }

        public static bool IsInStartup() {
            try {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                string value = rk.GetValue(Application.ProductName).ToString();
                if (value == null) {
                    return false;
                } else if (!value.ToLower().Equals(Application.ExecutablePath.ToLower())) {
                    return false;
                } else {
                    return true;
                }
            } catch (Exception) {
            }

            return false;
        }

    }
}
