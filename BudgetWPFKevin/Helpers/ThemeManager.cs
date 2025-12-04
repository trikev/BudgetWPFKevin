using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BudgetWPFKevin.Helpers
{
    public class ThemeManager
    {
        private static ThemeManager _instance;
        private DispatcherTimer _timer;
        private bool _lastThemeWasDark;

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private ThemeManager()
        {
            StartThemePolling();
        }

        public void ApplyTheme()
        {
            bool isDarkTheme = IsWindowsDarkThemeEnabled();
            _lastThemeWasDark = isDarkTheme;

            var app = Application.Current;

            if (isDarkTheme)
            {
                ApplyDarkTheme(app);
            }
            else
            {
                ApplyLightTheme(app);
            }
        }

        private bool IsWindowsDarkThemeEnabled()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    return value is int intValue && intValue == 0;
                }
            }
            catch
            {
                return true;
            }
        }

        // ✨ ENKLARE METOD - Kolla varje sekund om temat ändrats
        private void StartThemePolling()
        {
            _lastThemeWasDark = IsWindowsDarkThemeEnabled();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (sender, args) =>
            {
                bool currentThemeIsDark = IsWindowsDarkThemeEnabled();
                if (currentThemeIsDark != _lastThemeWasDark)
                {
                    System.Diagnostics.Debug.WriteLine($"Tema ändrat till: {(currentThemeIsDark ? "Mörkt" : "Ljust")}");
                    ApplyTheme();
                }
            };

            _timer.Start();
            System.Diagnostics.Debug.WriteLine("ThemePolling startad!");
        }

        private void ApplyDarkTheme(Application app)
        {
            var accentColor = GetWindowsAccentColor();

            app.Resources["WindowBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
            app.Resources["ControlBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
            app.Resources["ControlBackgroundHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"));
            app.Resources["SurfaceBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252526"));

            app.Resources["PrimaryTextBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
            app.Resources["DisabledTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6D6D6D"));

            app.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F46"));
            app.Resources["BorderHoverBrush"] = new SolidColorBrush(accentColor);

            app.Resources["SystemAccentBrush"] = new SolidColorBrush(accentColor);
            app.Resources["SystemAccentLightBrush"] = new SolidColorBrush(LightenColor(accentColor, 0.2f));
            app.Resources["SystemAccentDarkBrush"] = new SolidColorBrush(DarkenColor(accentColor, 0.2f));

           
        }

        private void ApplyLightTheme(Application app)
        {
            var accentColor = GetWindowsAccentColor();

            app.Resources["WindowBackgroundBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["ControlBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
            app.Resources["ControlBackgroundHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E8E8"));
            app.Resources["SurfaceBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFAFA"));

            app.Resources["PrimaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
            app.Resources["SecondaryTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#616161"));
            app.Resources["DisabledTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E"));

            app.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD"));
            app.Resources["BorderHoverBrush"] = new SolidColorBrush(accentColor);

            app.Resources["SystemAccentBrush"] = new SolidColorBrush(accentColor);
            app.Resources["SystemAccentLightBrush"] = new SolidColorBrush(LightenColor(accentColor, 0.2f));
            app.Resources["SystemAccentDarkBrush"] = new SolidColorBrush(DarkenColor(accentColor, 0.2f));

           
        }

        private Color GetWindowsAccentColor()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\DWM"))
                {
                    var value = key?.GetValue("AccentColor");
                    if (value is int colorValue)
                    {
                        byte a = (byte)((colorValue >> 24) & 0xFF);
                        byte b = (byte)((colorValue >> 16) & 0xFF);
                        byte g = (byte)((colorValue >> 8) & 0xFF);
                        byte r = (byte)(colorValue & 0xFF);

                        return Color.FromArgb(a, r, g, b);
                    }
                }
            }
            catch { }

            return (Color)ColorConverter.ConvertFromString("#0078D4");
        }

        private Color LightenColor(Color color, float amount)
        {
            return Color.FromArgb(
                color.A,
                (byte)Math.Min(255, color.R + (255 - color.R) * amount),
                (byte)Math.Min(255, color.G + (255 - color.G) * amount),
                (byte)Math.Min(255, color.B + (255 - color.B) * amount)
            );
        }

        private Color DarkenColor(Color color, float amount)
        {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * (1 - amount)),
                (byte)(color.G * (1 - amount)),
                (byte)(color.B * (1 - amount))
            );
        }

        public void Dispose()
        {
            _timer?.Stop();
        }
    }
}