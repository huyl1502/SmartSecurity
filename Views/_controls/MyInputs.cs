using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin;

namespace Xamarin.Forms
{
    public interface IMyInput
    {
        object Value { get; set; }
        string BindingName { get; set; }
        string Error { get; set; }
        string Caption { get; set; }
        bool Required { get; set; }
    }
    public abstract class MyInput<T> : TextView<T>, IMyInput
        where T : View, new()
    {
        public string BindingName { get; set; }
        public string Error { get; set; }
        public bool Required { get; set; }

        public string Caption { get; set; }

        public MyInput()
        {
            CornerRadius = new CornerRadius(4);
            Padding = "10, 0";
            BorderColor = "ccc";
            Background = "fff";
        }

        public abstract object Value { get; set; }
    }

    public class MyTextBox : MyInput<Entry>
    {
        public MyTextBox()
        {
            FontSize = 18;
        }
        public override object Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Content.Text))
                    return null;
                return Content.Text;
            }

            set
            {
                Content.Text = value?.ToString();
            }
        }
    }
    public class MyMultyLineBox : MyInput<Editor>
    {
        public MyMultyLineBox()
        {
            Content.HeightRequest = 100;
        }
        public override object Value
        {
            get => Content.Text;
            set => Content.Text = value?.ToString();
        }
    }

    public class MyPasswordBox : MyTextBox
    {
        public MyPasswordBox()
        {
            Content.IsPassword = true;
        }
    }

    public class MyDateBox : MyInput<DatePicker>
    {
        public MyDateBox()
        {
            Content.Format = "dd/MM/yyyy";
        }
        public override object Value
        {
            get => Content.Date;
            set
            {
                object v = null;
                try
                {
                    v = Convert.ChangeType(value, typeof(DateTime));
                }
                catch
                {
                }
                if (v == null)
                {
                    v = DateTime.Today;
                }
                Content.Date = (DateTime)v;
            }
        }
    }

    public class MyTimeBox : MyInput<TimePicker>
    {
        public MyTimeBox()
        {
            Content.Format = "HH:mm";
        }
        public override object Value
        {
            get => Content.Time;
            set
            {
                object v = null;
                try
                {
                    v = Convert.ChangeType(value, typeof(TimeSpan));
                }
                catch
                {
                }
                if (v == null)
                {
                    v = new TimeSpan(DateTime.Now.Ticks);
                }
                Content.Time = (TimeSpan)v;
            }
        }
    }

    public class MyComboBox : MyInput<Picker>
    {
        public override object Value
        {
            get => Content.SelectedItem;
            set => Content.SelectedItem = value;
        }

        public object ItemsSource
        {
            get => Content.ItemsSource;
            set
            {
                if (value is IList)
                {
                    Content.ItemsSource = (IList)value;
                    return;
                }

                var lst = new List<object>();
                foreach (var e in (IEnumerable)value)
                {
                    lst.Add(e);
                }
                Content.ItemsSource = lst;
            }
        }
    }

    public class MyPinCodeInput : Grid
    {
        static double CellSize => Screen.Width / 6;

        class KeyPad : MyTableLayout
        {
            public event Action<char> KeyPressed;

            public void CreateNumber(int r, int c, int num)
            {
                var w = CellSize;               
                var boxView = new BoxView {
                    Color = Color.Gray,
                    Opacity = 0.5,
                    WidthRequest = w,
                    HeightRequest = w,
                    CornerRadius = w / 2,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var label = new Label {
                    Text = num.ToString(),
                    FontSize = w / 2,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Color.White,
                };
                var btn = new Button {
                    BackgroundColor = Color.Transparent,
                    WidthRequest = w,
                    HeightRequest = w,
                    CornerRadius = (int)(w / 2),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };

                this.Add(r, c, boxView);
                this.Add(r, c, label);
                this.Add(r, c, btn);

                btn.Clicked += (s, e) => KeyPressed?.Invoke((char)(num + 48));
            }

            public void CreateIcon(int r, int c, string iconName, char key)
            {
                var icon = new MyIcon
                {
                    Key = iconName,
                    Foreground = Color.White,
                };
                var btn = new Button
                {
                    BackgroundColor = Color.Transparent,
                };

                this.Add(r, c, icon);
                this.Add(r, c, btn);

                btn.Clicked += (s, e) => KeyPressed?.Invoke(key);
            }

            public void CreateText(int r, int c, string text, char key)
            {
                var label = new Label
                {
                    Text = text,
                    TextColor = Color.White,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var btn = new Button
                {
                    BackgroundColor = Color.Transparent,
                };

                this.Add(r, c, label);
                this.Add(r, c, btn);

                btn.Clicked += (s, e) => KeyPressed?.Invoke(key);
            }

            public KeyPad() : base(4, 3)
            {
                for (int r = 0, k = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                        CreateNumber(r, c, ++k);
                }
                CreateNumber(3, 1, 0);
                CreateIcon(3, 2, "pcdel", (char)8);
                

                var h = (int)Screen.Width / 4;
                this.SetHeights(h, h, h, h);
            }
        }

        class Digit : BorderView<Label>
        {
            bool _isCircle;
            public Digit(bool circle)
            {
                this.HorizontalOptions = LayoutOptions.Start;
                this.VerticalOptions = LayoutOptions.Start;

                BorderColor = Color.White;
                BorderSize = 1;

                var w = (int)CellSize;
                if (_isCircle = circle)
                {
                    w = w >> 2;
                    CornerRadius = w / 2;
                }
                else
                {
                    w = (w << 1) / 3;
                    Content.FontSize = w / 2;
                    Content.TextColor = Color.White;

                    Content.HorizontalOptions = LayoutOptions.Center;
                    Content.VerticalOptions = LayoutOptions.Center;
                }
                this.WidthRequest = this.HeightRequest = w;

                Inner.BackgroundColor = Color.Black;
                Inner.Opacity = 0.83;
                Margin = new Thickness(5);
            }

            public void Fill(char c)
            {
                if (_isCircle)
                {
                    Inner.BackgroundColor = c != '\0' ? Color.WhiteSmoke : Color.Black;
                }
                else
                {
                    Content.Text = new string(c, 1);
                }
            }
        }

        MyLabel _caption;
        StackLayout _resultContent;
        KeyPad _keyPad;

        char[] _digits;
        public string Value => new string(_digits);

        public string Caption
        {
            get => _caption.Text;
            set => _caption.Text = value;
        }

        public MyPinCodeInput()
        {
            var bg = new BorderView {
                BackgroundColor = Color.Black,
                Opacity = 0.83,
            };

            var logo = new MyImage("logo");

            this.Children.Add(logo);
            this.Children.Add(bg);

            var content = new MyTableLayout {
                VerticalOptions = LayoutOptions.Center,
            };
            content.AddRow(_caption = new MyLabel {
                Margin = new Thickness(10),
                Foreground = Color.White,
            });

            content.AddRow(_resultContent = new StackLayout {
                HeightRequest = CellSize,
                HorizontalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0, CellSize / 2),
            });

            var keypad = new KeyPad();
            content.AddRow(_keyPad = keypad);

            this.Children.Add(content);
        }

        public MyPinCodeInput Start(int length)
        {
            int count = 0;
            _digits = new char[length];
            _keyPad.KeyPressed += (c) => {

                switch ((int)c)
                {
                    case 8:
                        if (count > 0)
                        {
                            _digits[--count] = '\0';
                        }
                        break;

                    case 13:
                        if (count == length)
                        {
                            Completed?.Invoke(Value);
                        }
                        return;

                    default:
                        if (count < length)
                        {
                            _digits[count++] = c;
                        }
                        break;
                }

                int i = 0;
                foreach (Digit d in _resultContent.Children)
                {
                    d.Fill(_digits[i++]);
                }
            };

            return this;
        }

        public Action<string> Completed;

        public MyPinCodeInput ShowCode()
        {
            for (int i = 0; i < _digits.Length; i++)
            {
                var d = new Digit(false);
                _resultContent.Children.Add(d);
            }

            _keyPad.CreateText(3, 0, "ENTER", (char)13);

            return this;
        }
        public MyPinCodeInput HideCode(bool wrong)
        {
            var len = _digits.Length;
            for (int i = 0; i < len; i++)
            {
                var d = new Digit(true);
                _resultContent.Children.Add(d);
            }

            _keyPad.KeyPressed += (c) => {
                if (_digits[len - 1] != '\0')
                {
                    Completed?.Invoke(Value);
                }
            };
            return this;
        }
    }
}
