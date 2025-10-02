using System.Windows;
using System.Windows.Controls;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.ViewModels;

namespace BIMinPersonalCRM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OrdersDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            if (DataContext is MainViewModel vm && e.Row.Item is Order order)
            {
                vm.EnsureUniqueOrderName(order);
            }
        }
    }
}
