using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using org.mariuszgromada.math.mxparser;

namespace Graphics_WPF
{
    internal class EquationModel : ViewModelBase
    {
        private readonly double _defaultRange;
        private readonly double _stepMult;        
        private readonly Expression _expression;
        private readonly Argument _argument;
        

        #region Properties

        public uint LineWidth { get; set; }
        public string StrExpression => _expression.getExpressionString();
        public bool IsEnabled { get; set; } = true;
        public string VariableName { get; }
        public Brush Brush { get; set; }


        #endregion


        #region Constructors

        public EquationModel(Expression optimizedExpression, Color color,
                        double stepMult = 1, string variableName = "x", double defaultRange = 100)
        {
            _stepMult = stepMult;
            VariableName = variableName;
            _defaultRange = defaultRange;
            LineWidth = 1;

            _expression = optimizedExpression;
            _argument = new Argument(VariableName, 0);
            _expression.addArguments(_argument);

            Brush = new SolidColorBrush(color);

        }

        #endregion


        #region Public methods

        public LineSeries GetSeriesInRange(Range range)
        {
            var chartVales = CalculateRange(range);

            var lineSeries = new LineSeries { Values = chartVales };
            return lineSeries;
        }

        public LineSeries GetSeriesInRange(double leftLimit, double rightLimit)
        {
            var range = new Range(leftLimit, rightLimit);
            return GetSeriesInRange(range);
        }

        public ObservablePoint CalculateInPoint(double point)
        {
            try
            {
                _argument.setArgumentValue(point);

                var resInPoint = _expression.calculate();
                var observablePoint = new ObservablePoint(point, resInPoint);

                return observablePoint;
            }
            catch (Exception)
            {
                return new ObservablePoint(point, double.NaN);
            }
        }

        private ChartValues<ObservablePoint> CalculateRange(Range range)
        {

            var points = new ChartValues<ObservablePoint>();
            var tasks = new List<Task<ObservablePoint>>();
            var step = range.Length() / (_defaultRange * _stepMult);

            for (var i = range.LeftLimit; i <= range.RightLimit; i += step)
            {
                var pointResult = CalculateInPoint(i);
               // var taskPoint = Task.Factory.StartNew(()=>CalculateInPoint(i));
                //tasks.Add(taskPoint);
                //if (points.Count != 0)
                //{
                //    //TODO Y scale
                //    var lastPoint = points.Last();
                //    var difference = Math.Abs(pointResult.Y - lastPoint.Y);
                //    var differenceLimit = range.Length() / (10 );
                //    if (difference > differenceLimit)
                //    {
                //        lastPoint.Y = double.NaN;
                //    }
                //}

                points.Add(pointResult);
            }
            
            //foreach (var task in tasks)
            //{
            //    points.Add(task.Result);
            //}
            return points;
        }
        #endregion

    }
}
