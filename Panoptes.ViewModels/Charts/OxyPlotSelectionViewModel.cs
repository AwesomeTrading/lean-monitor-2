﻿using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Panoptes.Model.Charting;
using Panoptes.Model.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Result = Panoptes.Model.Result;
using ScatterMarkerSymbol = QuantConnect.ScatterMarkerSymbol;
using SeriesType = QuantConnect.SeriesType;

namespace Panoptes.ViewModels.Charts
{
    public sealed class OxyPlotSelectionViewModel : ToolPaneViewModel
    {
        #region Colors (Should not be here, let's try to put it in xaml) 
        internal readonly static OxyColor SciChartBackgroungOxy = OxyColor.FromArgb(255, 28, 28, 30);

        internal readonly static OxyColor SciChartMajorGridLineOxy = OxyColor.FromArgb(255, 50, 53, 57);

        internal readonly static OxyColor SciChartMinorGridLineOxy = OxyColor.FromArgb(255, 35, 36, 38);

        internal readonly static OxyColor SciChartTextOxy = OxyColor.FromArgb(255, 166, 167, 172);

        internal readonly static OxyColor SciChartCandleStickIncreasingOxy = OxyColor.FromArgb(255, 82, 204, 84);

        internal readonly static OxyColor SciChartCandleStickDecreasingOxy = OxyColor.FromArgb(255, 226, 101, 101);

        internal readonly static OxyColor SciChartLegendTextOxy = OxyColor.FromArgb(255, 198, 230, 235);
        #endregion

        private readonly IMessenger _messenger;

        private readonly BackgroundWorker _resultBgWorker;

        private readonly BlockingCollection<Result> _resultsQueue = new BlockingCollection<Result>();

        private readonly Dictionary<string, PlotModel> _plotModelsDict = new Dictionary<string, PlotModel>();

        public AsyncRelayCommand BarsAll { get; }
        public AsyncRelayCommand Bars1m { get; }
        public AsyncRelayCommand Bars5m { get; }
        public AsyncRelayCommand Bars1h { get; }
        public AsyncRelayCommand Bars1d { get; }

        public OxyPlotSelectionViewModel()
        {
            Name = "Charts";
            BarsAll = new AsyncRelayCommand(DoBarsAll, CanDoBarsAll);
            Bars1m = new AsyncRelayCommand(DoBars1m, CanDoBars1m);
            Bars5m = new AsyncRelayCommand(DoBars5m, CanDoBars5min);
            Bars1h = new AsyncRelayCommand(DoBars1h, CanDoBars1h);
            Bars1d = new AsyncRelayCommand(DoBars1d, CanDoBars1d);
        }

        public Task DoBarsAll(CancellationToken cancelationToken)
        {
            return Task.Run(() =>
            {
                Trace.WriteLine("OxyPlotSelectionViewModel: Start DoBarsAll...");
                lock (SelectedSeries.SyncRoot)
                {
                    foreach (var serie in SelectedSeries.Series)
                    {
                        if (serie is LineCandleStickSeries candleStickSeries)
                        {
                            candleStickSeries.SerieType = LineCandleStickSeries.SerieTypes.Line;
                        }
                    }
                }
                InvalidatePlotThreadUI();
                Trace.WriteLine("OxyPlotSelectionViewModel: Done DoBarsAll.");
            }, cancelationToken);
        }

        public bool CanDoBarsAll()
        {
            return true;
        }

        public Task DoBars1m(CancellationToken cancelationToken)
        {
            return Task.Run(() =>
            {
                Trace.WriteLine("OxyPlotSelectionViewModel: Start DoBars1m...");
                lock (SelectedSeries.SyncRoot)
                {
                    foreach (var serie in SelectedSeries.Series)
                    {
                        if (serie is LineCandleStickSeries candleStickSeries)
                        {
                            candleStickSeries.SerieType = LineCandleStickSeries.SerieTypes.Candles;
                            candleStickSeries.SetPeriod(TimeSpan.FromMinutes(1));
                        }
                    }
                }
                InvalidatePlotThreadUI();
                Trace.WriteLine("OxyPlotSelectionViewModel: Done DoBars1m.");
            }, cancelationToken);
        }

        public bool CanDoBars1m()
        {
            if (SelectedSeries == null) return true;
            foreach (var serie in SelectedSeries.Series)
            {
                if (serie is LineCandleStickSeries candleStickSeries && 
                    candleStickSeries.CanDoTimeSpan(TimeSpan.FromMinutes(1)))
                {
                    return true;
                }
            }
            return false;
        }

        public Task DoBars5m(CancellationToken cancelationToken)
        {
            return Task.Run(() =>
            {
                Trace.WriteLine("OxyPlotSelectionViewModel: Start DoBars5m...");
                lock (SelectedSeries.SyncRoot)
                {
                    foreach (var serie in SelectedSeries.Series)
                    {
                        if (serie is LineCandleStickSeries candleStickSeries)
                        {
                            candleStickSeries.SerieType = LineCandleStickSeries.SerieTypes.Candles;
                            candleStickSeries.SetPeriod(TimeSpan.FromMinutes(5));
                        }
                    }
                }
                InvalidatePlotThreadUI();
                Trace.WriteLine("OxyPlotSelectionViewModel: Done DoBars5m.");
            }, cancelationToken);
        }

        public bool CanDoBars5min()
        {
            if (SelectedSeries == null) return true;
            foreach (var serie in SelectedSeries.Series)
            {
                if (serie is LineCandleStickSeries candleStickSeries &&
                    candleStickSeries.CanDoTimeSpan(TimeSpan.FromMinutes(5)))
                {
                    return true;
                }
            }
            return false;
        }

        public Task DoBars1h(CancellationToken cancelationToken)
        {
            return Task.Run(() =>
            {
                Trace.WriteLine("OxyPlotSelectionViewModel: Start DoBars1h...");
                lock (SelectedSeries.SyncRoot)
                {
                    foreach (var serie in SelectedSeries.Series)
                    {
                        if (serie is LineCandleStickSeries candleStickSeries)
                        {
                            candleStickSeries.SerieType = LineCandleStickSeries.SerieTypes.Candles;
                            candleStickSeries.SetPeriod(TimeSpan.FromHours(1));
                        }
                    }
                }
                InvalidatePlotThreadUI();
                Trace.WriteLine("OxyPlotSelectionViewModel: Done DoBars1h.");
            }, cancelationToken);
        }

        public bool CanDoBars1h()
        {
            if (SelectedSeries == null) return true;
            foreach (var serie in SelectedSeries.Series)
            {
                if (serie is LineCandleStickSeries candleStickSeries &&
                    candleStickSeries.CanDoTimeSpan(TimeSpan.FromHours(1)))
                {
                    return true;
                }
            }
            return false;
        }

        public Task DoBars1d(CancellationToken cancelationToken)
        {
            return Task.Run(() =>
            {
                lock (SelectedSeries.SyncRoot)
                {
                    foreach (var serie in SelectedSeries.Series)
                    {
                        if (serie is LineCandleStickSeries candleStickSeries)
                        {
                            candleStickSeries.SerieType = LineCandleStickSeries.SerieTypes.Candles;
                            candleStickSeries.SetPeriod(TimeSpan.FromDays(1));
                        }
                    }
                }
                InvalidatePlotThreadUI();
            }, cancelationToken);
        }

        public bool CanDoBars1d()
        {
            if (SelectedSeries == null) return true;
            foreach (var serie in SelectedSeries.Series)
            {
                if (serie is LineCandleStickSeries candleStickSeries &&
                    candleStickSeries.CanDoTimeSpan(TimeSpan.FromDays(1)))
                {
                    return true;
                }
            }
            return false;
        }

        public OxyPlotSelectionViewModel(IMessenger messenger) : this()
        {
            _messenger = messenger;

            _messenger.Register<OxyPlotSelectionViewModel, SessionUpdateMessage>(this, (r, m) =>
            {
                if (m.ResultContext.Result.Charts.Count == 0) return;
                r._resultsQueue.Add(m.ResultContext.Result);
            });
            _messenger.Register<OxyPlotSelectionViewModel, SessionClosedMessage>(this, (r, _) => r.Clear());

            _resultBgWorker = new BackgroundWorker() { WorkerReportsProgress = true };
            _resultBgWorker.DoWork += ResultQueueReader;
            _resultBgWorker.ProgressChanged += (s, e) =>
            {
                switch (e.ProgressPercentage)
                {
                    case 0:
                        lock (PlotModels)
                        {
                            PlotModels.Add((PlotModel)e.UserState);
                            if (PlotModels.Count == 1)
                            {
                                SelectedSeries = PlotModels.FirstOrDefault();
                            }
                        }
                        break;

                    case 1:
                        SelectedSeries?.InvalidatePlot(true);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(e), "Unknown 'ProgressPercentage' passed.");
                }
            };

            _resultBgWorker.RunWorkerCompleted += (s, e) => { /*do anything here*/ };
            _resultBgWorker.RunWorkerAsync();
        }

        private void ResultQueueReader(object sender, DoWorkEventArgs e)
        {
            while (!_resultBgWorker.CancellationPending)
            {
                var result = _resultsQueue.Take(); // Need cancelation token
                if (result.Charts.Count == 0) continue;
                ParseResult(result);
            }
        }

        private ObservableCollection<PlotModel> _plotModels = new ObservableCollection<PlotModel>();
        public ObservableCollection<PlotModel> PlotModels
        {
            get { return _plotModels; }
            set
            {
                _plotModels = value;
                OnPropertyChanged();
            }
        }

        private PlotModel _selectedSeries;
        public PlotModel SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                _selectedSeries = value;
                OnPropertyChanged();
            }
        }

        private static string GetUnit(ChartDefinition chartDefinition)
        {
            if (chartDefinition.Series == null || chartDefinition.Series.Count == 0)
            {
                return null;
            }
            else if (chartDefinition.Series.Count == 1)
            {
                return chartDefinition.Series?.Values?.First().Unit;
            }

            return string.Join(",", chartDefinition.Series?.Values?.Select(s => s.Unit).Distinct());
        }

        private void ParseResult(Result result)
        {
            foreach (var chart in result.Charts.OrderBy(x => x.Key))
            {
                PlotModel plot;
                if (_plotModelsDict.ContainsKey(chart.Key))
                {
                    plot = _plotModelsDict[chart.Key];
                }
                else
                {
                    // Create Plot
                    plot = new PlotModel()
                    {
                        Title = chart.Key,
                        TitleFontSize = 0,
                        TextColor = SciChartTextOxy,
                        PlotAreaBorderColor = SciChartMajorGridLineOxy,
                        TitleColor = SciChartTextOxy,
                        SubtitleColor = SciChartTextOxy
                    };

                    // Keep axis simple for the moment
                    var timeSpanAxis1 = new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        Selectable = false,
                        IntervalType = DateTimeIntervalType.Auto,
                        AxisDistance = 30,
                        ExtraGridlineStyle = LineStyle.DashDot,
                        AxislineColor = SciChartMajorGridLineOxy,
                        ExtraGridlineColor = SciChartMajorGridLineOxy,
                        TicklineColor = SciChartTextOxy
                    };

                    plot.Axes.Add(timeSpanAxis1);
                    var linearAxis1 = new LinearAxis
                    {
                        Position = AxisPosition.Right,
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Solid,
                        TickStyle = TickStyle.Outside,
                        AxislineColor = SciChartMajorGridLineOxy,
                        ExtraGridlineColor = SciChartMajorGridLineOxy,
                        MajorGridlineColor = SciChartMajorGridLineOxy,
                        TicklineColor = SciChartMajorGridLineOxy,
                        MinorGridlineColor = SciChartMinorGridLineOxy,
                        MinorTicklineColor = SciChartMinorGridLineOxy,
                        TextColor = SciChartTextOxy,
                        TitleColor = SciChartTextOxy,
                        Unit = GetUnit(chart.Value)
                    };
                    plot.Axes.Add(linearAxis1);

                    _plotModelsDict[chart.Key] = plot;
                    AddPlotThreadUI(plot);
                }

                lock (plot.SyncRoot)
                {
                    foreach (var serie in chart.Value.Series.OrderBy(x => x.Key))
                    {
                        if (serie.Value.Values.Count == 0) continue;
                        var s = plot.Series.FirstOrDefault(k => (string)k.Tag == serie.Value.Name);

                        // Create Series
                        if (s == null)
                        {
                            //serie.Value.Unit
                            switch (serie.Value.SeriesType)
                            {
                                // Handle candle and line series the same way, choice is done in UI
                                case SeriesType.Candle:
                                case SeriesType.Line:
                                    s = new LineCandleStickSeries()
                                    {
                                        LineColor = serie.Value.Color.ToOxyColor().Negative(),
                                        Tag = serie.Value.Name,
                                        Title = serie.Value.Name,
                                        SerieType = LineCandleStickSeries.SerieTypes.Line
                                    };
                                    plot.Series.Add(s);
                                    break;

                                case SeriesType.Bar:
                                    s = new LineSeries()
                                    {
                                        Color = serie.Value.Color.ToOxyColor().Negative(),
                                        Tag = serie.Value.Name,
                                        Title = serie.Value.Name,
                                        MarkerType = GetMarkerType(serie.Value.ScatterMarkerSymbol),
                                        CanTrackerInterpolatePoints = false
                                    };
                                    plot.Series.Add(s);
                                    break;

                                case SeriesType.Treemap: // todo
                                case SeriesType.Scatter:
                                    s = new ScatterSeries()
                                    {
                                        MarkerFill = serie.Value.Color.ToOxyColor().Negative(),
                                        Tag = serie.Value.Name,
                                        Title = serie.Value.Name,
                                        MarkerType = GetMarkerType(serie.Value.ScatterMarkerSymbol),
                                        MarkerOutline = null,
                                    };
                                    plot.Series.Add(s);
                                    break;

                                /*
                            case SeriesType.Bar:
                                s = new RectangleSeries()
                                {
                                    Tag = serie.Value.Name,
                                    Title = serie.Value.Name
                                };
                                plot.Series.Add(s);
                                break;
                                */

                                default:
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
                                    throw new NotImplementedException($"Chart type '{serie.Value.SeriesType}' is not implemented.");
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
                            }
                        }

                        switch (serie.Value.SeriesType)
                        {
                            case SeriesType.Candle:
                            case SeriesType.Line:
                                ((LineCandleStickSeries)s).AddRange(serie.Value.Values.Select(p =>
                                            DateTimeAxis.CreateDataPoint(p.X.ToDateTimeUtc(), (double)p.Y)));
                                break;

                            case SeriesType.Bar:
                                // Handle candle and line series the same way, choice is done in UI
                                var lineSeriesBar = (LineSeries)s;
                                var newLinePointsBar = serie.Value.Values.Select(p => DateTimeAxis.CreateDataPoint(p.X.ToDateTimeUtc(), (double)p.Y));
                                var currentLineBar = lineSeriesBar.Points;
                                var filteredLineBar = newLinePointsBar.Except(currentLineBar).ToList();
                                if (filteredLineBar.Count == 0) break;
                                lineSeriesBar.Points.AddRange(filteredLineBar);
                                break;

                            case SeriesType.Scatter:
                                var scatterSeries = (ScatterSeries)s;
                                var newScatterSeries = serie.Value.Values.Select(p => new ScatterPoint(DateTimeAxis.ToDouble(p.X.ToDateTimeUtc()), (double)p.Y));
                                var currentScatter = scatterSeries.Points;
                                var filteredScatter = newScatterSeries.Except(currentScatter, ScatterPointComparer).ToList();
                                if (filteredScatter.Count == 0) break;
                                scatterSeries.Points.AddRange(filteredScatter);
                                break;
                            /*
                        case SeriesType.Bar:
                            var barSeries = (RectangleSeries)s;
                            var newBarSeries = serie.Value.Values.Select(p =>
                                new RectangleItem(DateTimeAxis.ToDouble(p.X.ToDateTimeUtc()),
                                                  DateTimeAxis.ToDouble(p.X.ToDateTimeUtc().AddDays(1)),
                                                  (double)p.Y, (double)p.Y, (double)p.Y));
                            var currentBar = barSeries.Items;
                            var filteredBar = newBarSeries.Except(currentBar).ToList();
                            if (filteredBar.Count == 0) break;
                            barSeries.Items.AddRange(filteredBar);
                            break;
                            */

                            case SeriesType.Pie:
                            case SeriesType.StackedArea:
                            default:
                                continue; // TODO
                                //throw new NotImplementedException();
                        }
                    }
                }
            }

            InvalidatePlotThreadUI();

            //}
            /*
            ProfitLoss = new ObservableCollection<ProfitLossItemViewModel>(result.ProfitLoss.OrderBy(o => o.Key).Select(p => new ProfitLossItemViewModel
            {
                DateTime = p.Key,
                Profit = p.Value,
                IsNegative = p.Value < 0
            }));
            */
        }

        private void AddPlotThreadUI(PlotModel plot)
        {
            _resultBgWorker.ReportProgress(0, plot);
        }

        private bool _canInvalidatePlot = true;
        private readonly object _canInvalidatePlotLock = new object();
        public bool CanInvalidatePlot
        {
            get
            {
                lock (_canInvalidatePlotLock)
                {
                    return _canInvalidatePlot;
                }
            }

            set
            {
                lock (_canInvalidatePlotLock)
                {
                    _canInvalidatePlot = value;
                }
            }
        }

        private DateTime _lastInvalidatePlot = DateTime.MinValue;

        private void InvalidatePlotThreadUI()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastInvalidatePlot).TotalMilliseconds > 250 && CanInvalidatePlot)
            {
                _lastInvalidatePlot = now;
                _resultBgWorker.ReportProgress(1);
            }
        }

        private void Clear()
        {
            _plotModelsDict.Clear();
            _plotModels.Clear();
        }

        private static MarkerType GetMarkerType(ScatterMarkerSymbol scatterMarkerSymbol)
        {
            switch (scatterMarkerSymbol)
            {
                case ScatterMarkerSymbol.None:
                    return MarkerType.None;

                case ScatterMarkerSymbol.Circle:
                    return MarkerType.Circle;

                case ScatterMarkerSymbol.Square:
                    return MarkerType.Square;

                case ScatterMarkerSymbol.Diamond:
                    return MarkerType.Diamond;

                case ScatterMarkerSymbol.Triangle:
                    return MarkerType.Triangle;

                case ScatterMarkerSymbol.TriangleDown:
                    return MarkerType.Custom;

                default:
                    throw new ArgumentException($"Unknown ScatterMarkerSymbol type '{scatterMarkerSymbol}'", nameof(scatterMarkerSymbol));
            }
        }

        private readonly ScatterPointComparer ScatterPointComparer = new ScatterPointComparer();
    }

    internal sealed class ScatterPointComparer : IEqualityComparer<ScatterPoint>
    {
        public bool Equals(ScatterPoint p1, ScatterPoint p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y && p1.Size == p2.Size;
        }

        public int GetHashCode([DisallowNull] ScatterPoint pt)
        {
            return (pt.X, pt.Y, pt.Size).GetHashCode();
        }
    }
}
