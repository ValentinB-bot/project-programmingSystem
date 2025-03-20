using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasySaveV2._0
{
    /// <summary>
    /// Logique d'interaction pour ParameterWindow.xaml
    /// </summary>
    public partial class ParameterWindow : Window
    {
        public ParameterWindow()
        {
            InitializeComponent();
            DataContext = (ViewModel)Application.Current.MainWindow.DataContext;
            
        }

    }
}
