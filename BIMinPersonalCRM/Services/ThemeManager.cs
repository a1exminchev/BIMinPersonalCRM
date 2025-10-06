using System;
using System.Diagnostics;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace BIMinPersonalCRM.Services
{
    public static class ThemeManager
    {
        private static readonly Color LightPrimary = Color.FromRgb(0x19, 0x76, 0xD2);
        private static readonly Color LightSecondary = Color.FromRgb(0x03, 0xA9, 0xF4);
        private static readonly Color DarkPrimary = Color.FromRgb(0x64, 0xB5, 0xF6);
        private static readonly Color DarkSecondary = Color.FromRgb(0x4F, 0xC3, 0xF7);

        private static bool _isLightTheme = true;

        public static bool IsLightTheme => _isLightTheme;

        public static event EventHandler? ThemeChanged;

        public static void Initialize(bool defaultLight = true)
        {
            try
            {
                var preference = UserPreferences.GetThemePreference();
                ApplyTheme(preference ?? defaultLight, persist: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load theme preference: {ex}");
                ApplyTheme(defaultLight, persist: false);
            }
        }

        public static void ToggleTheme()
        {
            ApplyTheme(!IsLightTheme);
        }

        public static void ApplyTheme(bool useLightTheme, bool persist = true)
        {
            _isLightTheme = useLightTheme;

            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            if (useLightTheme)
            {
                theme.SetBaseTheme(Theme.Light);
                theme.SetPrimaryColor(LightPrimary);
                theme.SetSecondaryColor(LightSecondary);
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);
                theme.SetPrimaryColor(DarkPrimary);
                theme.SetSecondaryColor(DarkSecondary);
            }

            paletteHelper.SetTheme(theme);

            if (persist)
            {
                UserPreferences.SetThemePreference(useLightTheme);
            }

            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
