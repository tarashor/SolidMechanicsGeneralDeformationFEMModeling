using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GUI.ViewMOdels;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModelViewModel _model = new ModelViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = _model;
            CommandBindings.Add(new CommandBinding(_model.DrawResult, DrawBeam, CanDrawBeam));
        }

        private void DrawBeam(object sender, ExecutedRoutedEventArgs e)
        {
            DrawResultControl.ShowDrawResult(_model.LinearResult, _model.NonlinearResult, _model.AxesData);
        }

        private void CanDrawBeam(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _model.IsSolved;
        }

        private void Solve(object sender, RoutedEventArgs e)
        {
            _model.Solve();
        }

    }

    
}
