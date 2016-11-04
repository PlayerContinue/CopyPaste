using CodePaste.User_Controls;
using System.IO;
using System.Windows;

namespace CodePaste
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainControlModel(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "test.xml"), this);
        }
    }
}