using System.Windows;
using Microsoft.Win32;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Views.Dialogs
{
    public partial class AddCompanyDialog : Window
    {
        public Company Company { get; private set; }
        public AddCompanyDialog()
        {
            InitializeComponent();
            Company = new Company
            {
                Name = "",
                Phone = "",
                Website = "",
                LogoPath = "",
                CardColor = "#FFFFFF"
            };
            DataContext = Company;
        }

        private void SelectLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Ð’Ñ‹Ð±ÐµÑ€Ð¸Ñ‚Ðµ Ð»Ð¾Ð³Ð¾Ñ‚Ð¸Ð¿ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸",
                Filter = "Ð˜Ð·Ð¾Ð±Ñ€Ð°Ð¶ÐµÐ½Ð¸Ñ|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Ð’ÑÐµ Ñ„Ð°Ð¹Ð»Ñ‹|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                Company.LogoPath = dialog.FileName;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Ð’Ð°Ð»Ð¸Ð´Ð°Ñ†Ð¸Ñ
            if (string.IsNullOrWhiteSpace(Company.Name))
            {
                MessageBox.Show("ÐŸÐ¾Ð¶Ð°Ð»ÑƒÐ¹ÑÑ‚Ð°, Ð²Ð²ÐµÐ´Ð¸Ñ‚Ðµ Ð½Ð°Ð·Ð²Ð°Ð½Ð¸Ðµ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸.", "ÐžÑˆÐ¸Ð±ÐºÐ°", 
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


