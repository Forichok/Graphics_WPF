using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DevExpress.Mvvm;
using Graphics_WPF;
using LiveCharts;
using LiveCharts.Events;
using LiveCharts.Wpf;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Graphics_WPF
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly GraphicsModel _graphicsModel;


        #region Properties

        public SeriesCollection Series => _graphicsModel.Series;
        public IEnumerable<EquationModel> Equations => _graphicsModel.Equations;
        public ZoomingOptions ZoomingMode { get; set; }

        public double SelectedX { get; set; }
        public double SelectedY { get; set; }

        public bool SelectedIsExist { get; private set; }

        public double MaxRange { get; }
        public double MinRange { get; }

        public string MouseX { get; private set; }
        public string MouseY { get; private set; }
        #endregion


        #region Constructor

        public MainViewModel()
        {
            _graphicsModel = new GraphicsModel();
            ZoomingMode = ZoomingOptions.Xy;

            SelectedX = 0;
            SelectedY = 0;

            MaxRange = 10000;
            MinRange = -10000;                       
        }

        #endregion


        #region Commands

        public ICommand LoadCommand => new DelegateCommand(LoadFromFile);
        public ICommand AppendCommand => new DelegateCommand(AppendFromFile);
        public ICommand UpdateCommand => new DelegateCommand(Update);
        public ICommand CreateTangentCommand => new DelegateCommand(CreateTangent);
        public ICommand CreateNormalCommand => new DelegateCommand(CreateNormal);
        public ICommand DataClickCommand => new DelegateCommand<ChartPoint>(SellectPoint);
        public ICommand RangeChangedCommand => new DelegateCommand<RangeChangedEventArgs>(Resize);
        public ICommand ChangeColorCommand => new DelegateCommand<MouseEventArgs>(ChangeColor);

        #endregion


        #region Events
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is CartesianChart chart)
            {
                Point point;
                try
                {
                    point = chart.ConvertToChartValues(e.GetPosition(chart));
                }
                catch (Exception)
                {
                    point = new Point(0, 0);
                }

                MouseX = point.X.ToString("F");
                MouseY = point.Y.ToString("F");
                RaisePropertyChanged("MouseX");
                RaisePropertyChanged("MouseY");
            }
        }
        #endregion


        #region Other private functions

        private void LoadFromFile()
        {
            var dialog = new OpenFileDialog { Filter = "txt|*.txt" };

            if (dialog.ShowDialog() == true)
            {
                var patch = dialog.FileName;
                try
                {
                    _graphicsModel.LoadFromFile(patch);
                }
                catch (Exception er)
                {
                    MessageBox.Show("Eror in file : " + er.Message);
                }
            }
        }

        private void AppendFromFile()
        {
            var dialog = new OpenFileDialog { Filter = "txt|*.txt" };

            if (dialog.ShowDialog() == true)
            {
                var patch = dialog.FileName;
                try
                {
                    _graphicsModel.AppendFromFile(patch);
                }
                catch (Exception er)
                {
                    MessageBox.Show("Eror in file : " + er.Message);
                }
            }
        }

        private void SellectPoint(ChartPoint point)
        {
            if (point == null) return;

            SelectedX = point.X;
            SelectedY = point.Y;

            if (SelectedIsExist == false)
                SelectedIsExist = true;
        }

        private void Resize(RangeChangedEventArgs eventArgs)
        {
            if (eventArgs.Axis is Axis axisX)
            {
                try
                {
                    _graphicsModel.RerangeX(axisX.ActualMinValue, axisX.ActualMaxValue);
                }
                catch (Exception)
                {
                    //ignore
                }
            }
        }

        private void Update()
        {
            _graphicsModel.Uppdate();
        }

        private void ChangeColor(MouseEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                var color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                var ellipse = e.Source as Ellipse;
                if (ellipse?.DataContext is EquationModel model)
                    model.Brush = new SolidColorBrush(color);

                Update();
            }
        }

        private void CreateTangent()
        {
            _graphicsModel.CreateTangentFromPoint(SelectedX, SelectedY);
        }

        private void CreateNormal()
        {
            _graphicsModel.CreateNormalFromPoint(SelectedX, SelectedY);
        }
        #endregion

    }
}