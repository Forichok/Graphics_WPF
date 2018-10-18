using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using DevExpress.Mvvm;
using LiveCharts;
using LiveCharts.Wpf;
using org.mariuszgromada.math.mxparser;

namespace Graphics_WPF
{
    internal class GraphicsModel : ViewModelBase
    {
        private static readonly ColorSet Colors;

        private const double DefaultRange = 10;
        private const double StepMult = 2;

        private Range _currentRange;

        #region Properties
        public ObservableCollection<EquationModel> Equations { get; set; }
        public SeriesCollection Series { get; }

        #endregion
        
        #region Constructor

        static GraphicsModel()
        {
            Colors = new ColorSet();
        }

        public GraphicsModel()
        {
            Equations = new ObservableCollection<EquationModel>();
            Equations.CollectionChanged += UppdateSeries;

            _currentRange = new Range(-DefaultRange, DefaultRange);

            Series = new SeriesCollection();
        }

        #endregion


        #region Public methods

        public void LoadFromFile(string patch)
        {
            if (Equations.Any())
                Equations.Clear();

            if (Series.Any())
                Series.Clear();
            
            var result = Load(patch);
            foreach (var equation in result)
            {
                Equations.Add(equation);
            }
        }

        public void AppendFromFile(string patch)
        {
            var result = Load(patch);
            foreach (var equation in result)
            {
                Equations.Add(equation);
            }
        }

        public void RerangeX(double axisXActualMinValue, double axisXActualMaxValue)
        {
            var seriesList = new List<LineSeries>();
            _currentRange = new Range(axisXActualMinValue, axisXActualMaxValue);
            foreach (var equation in Equations)
            {
                if (!equation.IsEnabled) continue;

                var lineSeries = GetSeries(equation);
                seriesList.Add(lineSeries);
            }

            if (Series.Any())
                Series.Clear();

            Series.AddRange(seriesList);
        }

        public void Uppdate()
        {
            RerangeX(_currentRange.LeftLimit, _currentRange.RightLimit);
        }

        public void CreateTangentFromPoint(double x, double y)
        {
            var equation = FindByPoint(x, y);

            var derStr = "der(" + equation.StrExpression + "," + equation.VariableName + ")";
            var derivativeExpression = new Expression(derStr);

            var argument = new Argument(equation.VariableName, x);
            derivativeExpression.addArguments(argument);

            var derivativeResult = derivativeExpression.calculate();

            var tangentumEqStr = y.ToString(CultureInfo.InvariantCulture) + "+(" + derivativeResult.ToString(CultureInfo.InvariantCulture)
                + ")*(x-(" + x.ToString(CultureInfo.InvariantCulture) + "))";

            var tangentumEqModel = CreateEquation(tangentumEqStr);
            Equations.Add(tangentumEqModel);
        }

        public void CreateNormalFromPoint(double x, double y)
        {
            var equation = FindByPoint(x, y);

            var derStr = "der(" + equation.StrExpression + "," + equation.VariableName + ")";
            var derivativeExpression = new Expression(derStr);

            var argument = new Argument(equation.VariableName, x);
            derivativeExpression.addArguments(argument);

            var derivativeResult = derivativeExpression.calculate();

            var tangentumEqStr = y.ToString(CultureInfo.InvariantCulture) + "+(-1/" + derivativeResult.ToString(CultureInfo.InvariantCulture)
                                 + ")*(x-(" + x.ToString(CultureInfo.InvariantCulture) + "))";

            var tangentumEqModel = CreateEquation(tangentumEqStr);
            Equations.Add(tangentumEqModel);

        }
        #endregion


        #region Private methods

        private IEnumerable<EquationModel> Load(string patch)
        {
            var resultList = new List<EquationModel>();

            using (var sr = new StreamReader(patch))
            {
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    var equastion = CreateEquation(str);
                    resultList.Add(equastion);
                }
            }
            return resultList;

        }

        private static EquationModel CreateEquation(string equationStr)
        {
            var expression = new Expression(equationStr);

            var equastion = new EquationModel(expression, Colors.GetNext(), StepMult);

            return equastion;
        }

        private void UppdateSeries(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var equation = item as EquationModel;
                    var series = GetSeries(equation);
                    Series.Add(series);
                }
            }
        }

        private LineSeries GetSeries(EquationModel equation)
        {
            var lineSeries = equation.GetSeriesInRange(_currentRange);
            lineSeries.Fill = Brushes.Transparent;
            lineSeries.Stroke = equation.Brush;
            lineSeries.PointGeometrySize = equation.LineWidth;
            lineSeries.Tag = equation;

            return lineSeries;
        }

        private EquationModel FindByPoint(double x, double y)
        {
            foreach (var equation in Equations)
            {
                var result = equation.CalculateInPoint(x);

                if (y.Equals(result.Y))
                    return equation;
            }

            return null;
        }
        #endregion
    }

}
