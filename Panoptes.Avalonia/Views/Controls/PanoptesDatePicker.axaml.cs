using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;

namespace Panoptes.Avalonia.Views.Controls
{
    public partial class PanoptesDatePicker : UserControl
    {
        public PanoptesDatePicker()
        {
            InitializeComponent();
            _textBoxDate = this.Get<TextBox>("_textBoxDate");
            _textBoxDate.DataContext = this;
            _textBoxDate.AddHandler(TextInputEvent, _textBoxFrom_TextInput, RoutingStrategies.Tunnel);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly DirectProperty<PanoptesDatePicker, DateTime?> SelectedDateProperty = AvaloniaProperty.RegisterDirect<PanoptesDatePicker, DateTime?>(nameof(SelectedDate), o => o.SelectedDate, (o, v) => o.SelectedDate = v, defaultBindingMode: BindingMode.OneWayToSource);

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get
            {
                return _selectedDate;
            }

            private set
            {
                if (SetAndRaise(SelectedDateProperty, ref _selectedDate, value))
                {
                    //
                }
            }
        }

        public string Text => _textBoxDate.Text;

        private readonly TextBox _textBoxDate;

        private string _error;
        private string _dateStr;
        public string DateStr
        {
            get
            {
                return _dateStr;
            }
            set
            {
                _dateStr = value;
                if (_dateStr.Length != 10)
                {
                    SelectedDate = null;
                }

                if (!string.IsNullOrEmpty(_error))
                {
                    SelectedDate = null;
                    throw new DataValidationException(_error);
                }
            }
        }

        private readonly string[] _separators = new[] { "/", " ", "-" };
        private readonly string[] _day = new string[2];
        private readonly string[] _month = new string[2];
        private readonly string[] _year = new string[4];

        private int Day { get; set; }
        private int Month { get; set; }
        private int Year { get; set; }

        private void _textBoxFrom_TextInput(object? sender, global::Avalonia.Input.TextInputEventArgs e)
        {
            SelectedDate = null;
            if (sender is not TextBox tb)
            {
                return;
            }

            var index = tb.CaretIndex;
            if (!string.IsNullOrEmpty(tb.Text))
            {
                tb.Text = string.Join("", tb.Text.Take(index));
            }

            int num = -1;
            if (index == 2 || index == 5)
            {
                if (!_separators.Contains(e.Text))
                {
                    // check if separator
                    e.Handled = true;
                    return;
                }
            }
            else if (!int.TryParse(e.Text, out num))
            {
                e.Handled = true;
                return;
            }

            switch (index)
            {
                case 0:
                    if (num > 3)
                    {
                        e.Handled = true;
                        return;
                    }
                    _day[0] = e.Text;
                    return;

                case 1:
                    _day[1] = e.Text;
                    Day = int.Parse(string.Concat(_day));
                    if (Day > 31)
                    {
                        _error = "Day is not valid.";
                    }
                    else
                    {
                        _error = null;
                    }
                    return;

                case 2:
                    return;

                case 3:
                    if (num > 1)
                    {
                        e.Handled = true;
                        return;
                    }
                    _month[0] = e.Text;
                    return;

                case 4:
                    _month[1] = e.Text;
                    Month = int.Parse(string.Concat(_month));
                    if (Month > 12)
                    {
                        _error = "Month is not valid.";
                    }
                    else if (Day > 31)
                    {
                        _error = "Day is not valid.";
                    }
                    else
                    {
                        _error = null;
                    }
                    return;

                case 5:
                    return;

                case 6:
                case 7:
                case 8:
                    _year[index - 6] = e.Text;
                    break;

                case 9:
                    _year[3] = e.Text;
                    Year = int.Parse(string.Concat(_year));
                    try
                    {
                        _error = null;
                        SelectedDate = new DateTime(Year, Month, Day);
                    }
                    catch (Exception ex)
                    {
                        _error = ex.Message;
                    }
                    return;

                default:
                    e.Handled = true;
                    return;
            }
        }
    }
}
