using System.Windows;
using BIMinPersonalCRM.Services;

namespace BIMinPersonalCRM
{
    /// <summary>
    ///     Главный класс приложения WPF.
    ///     Используется для обработки событий уровня приложения (Startup, Exit и т. д.).
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Initialize();
        }
    }
}
