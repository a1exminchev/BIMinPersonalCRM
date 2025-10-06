namespace BIMinPersonalCRM
{
    public static class AppSettings
    {
        public readonly static string BaseFolder = System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                        "BIMin",
                        "CRM");

        public readonly static string DataFile = System.IO.Path.Combine(BaseFolder, "data.json");
        public readonly static string CompanyLogosFolder = System.IO.Path.Combine(BaseFolder, "CompanyLogos");
        public readonly static string EmployeeAvatarsFolder = System.IO.Path.Combine(BaseFolder, "EmployeeAvatars");
        public readonly static string PreferencesFile = System.IO.Path.Combine(BaseFolder, "settings.json");
    }
}