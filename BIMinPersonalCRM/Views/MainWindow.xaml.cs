using System.Windows;
using System.Windows.Controls;
using BIMinPersonalCRM.ViewModels;
using BIMinPersonalCRM.ViewModels.Entities;

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

            if (DataContext is MainVM vm && e.Row.Item is OrderVM order)
            {
                vm.EnsureUniqueOrderName(order);
            }
        }
    }
}
