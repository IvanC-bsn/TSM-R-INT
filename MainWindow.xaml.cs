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

namespace TubeSheetRobot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ParserXML parser = new ParserXML();
            parser.ParseFile();
            var viewModel = new TubesheetModel(parser.diameter, parser.pitch, parser.listOfElements);
            this.DataContext = viewModel;

            //var listOfAllelements = parser.listOfElements;
            //var diameter = parser._diameter;
            //var pitch = parser._pitch;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = ((e.OriginalSource as Button).Content as TubeM);
            ((e.OriginalSource as Button).Content as TubeM).modelT.SetCoordinates(a);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((e.OriginalSource as Button).DataContext as Model).ResetCoordinates();
        }
    }
}
