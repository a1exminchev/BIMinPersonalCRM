using System.Windows;
using Microsoft.Win32;
using BIMinPersonalCRM.ViewModels.Entities;

namespace BIMinPersonalCRM.Views.Dialogs
{
    public partial class AddCompanyDialog : Window
    {
        public CompanyVM Company { get; private set; }

        public AddCompanyDialog()
        {
            InitializeComponent();
            Company = new CompanyVM
            {
                Name = string.Empty,
                Phone = string.Empty,
                Website = string.Empty,
                LogoPath = string.Empty,
                CardColor = "#FFFFFF"
            };
            DataContext = Company;
        }

        private void SelectLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выбор логотипа компании",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var ext = System.IO.Path.GetExtension(dialog.FileName);
                    var unique = System.Guid.NewGuid().ToString("N") + ext;
                    var dest = System.IO.Path.Combine(AppSettings.CompanyLogosFolder, unique);
                    System.IO.File.Copy(dialog.FileName, dest, true);
                    Company.LogoPath = dest;
                }
                catch
                {
                    Company.LogoPath = string.Empty;
                }
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Company.Name))
            {
                MessageBox.Show("Пожалуйста, заполните название компании.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

