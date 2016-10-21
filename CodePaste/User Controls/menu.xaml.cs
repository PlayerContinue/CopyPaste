using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodePaste.Base_Classes;

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
