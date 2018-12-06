using System;
using System.Collections.Generic;
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

namespace proiect_pass_storage {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private UserData data;

        public MainWindow() {
            InitializeComponent();
        }
        public MainWindow(UserData data) {
            InitializeComponent();
            this.data = data;
            buttonCreateResourse.Click += ButtonCreateResourse_Click;
        }

        private void ButtonCreateResourse_Click(object sender, RoutedEventArgs e) {
            gridCreatePassword.Visibility = Visibility.Visible;
        }
    }
}
