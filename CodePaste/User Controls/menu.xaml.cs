using CodePaste.Base_Classes;
using System.Windows;
using System.Windows.Controls;

namespace CodePaste.User_Controls
{
    public class MenuModel : ModelBase
    {
        private string _CurrentView;

        public string CurrentView
        {
            get { return _CurrentView; }
            set
            {
                if (_CurrentView == value) return;

                _CurrentView = value;
                OnPropertyChanged("CurrentView");
            }
        }
    }

    /// <summary>
    /// Interaction logic for menu.xaml
    /// </summary>
    public partial class menu : UserControl
    {
        public menu()
        {
            InitializeComponent();
        }

        private void ChangeView(object sender, RoutedEventArgs e)
        {
            MainControlModel Context = this.DataContext as MainControlModel;
            Context.CurrentView = ((MenuItem)sender).Name;
        }
    }
}