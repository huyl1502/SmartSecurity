using System;
using System.Collections.Generic;
namespace Xamarin.Forms
{
    public class MyMenuItemInfo
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public string ClassName { get; set; }
        public string IconName { get; set; }
        public Rgb IconColor { get; set; }
        public string Description { get; set; }

        public string Name { get; set; }
        public bool BeginGroup { get; set; }
        public bool Disabled { get; set; }
        public bool IsActive { get; set; }

        public List<MyMenuItemInfo> Childs { get; set; }
    }

    public class MyMenuItemView : MyTableLayout
    {
        MyIcon _icon;
        public MyIcon Icon => _icon;

        Label _label;
        public Label Label => _label;

        public string Url { get; set; }

        public MyMenuItemView()
            : base(2, 2)
        {
            this.SetWidths(MyMenuView.IconSize, 0);
            this.SetHeights(MyMenuView.IconSize >> 1, 0);

            _icon = new MyIcon {  };
            _icon.SetValue(RowSpanProperty, 2);

            _label = new Label { VerticalOptions = LayoutOptions.Center };
            _label.SetValue(RowSpanProperty, 2);

            this.Add(_icon);
            this.Add(0, 1, _label);
        }
        public MyMenuItemView(MyMenuItemInfo info)
            : this()
        {
            Icon.Key = info.IconName;
            Icon.Foreground = info.IconColor;

            Url = info.Url;
            _label.Text = info.Text;

            SetSubcontent(info.Description);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            //if (Url != null || Clicked != null)
            {
                var btn = new Button { BackgroundColor = Color.Transparent };
                btn.SetValue(RowSpanProperty, 2);
                btn.SetValue(ColumnSpanProperty, 2);

                btn.Clicked += delegate {

                    if (Clicked != null)
                    {
                        var e = new MyButtonEventArgs();
                        Clicked.Invoke(this, e);

                        if (e.Handled) return;
                    }

                    if (Url != null)
                    {
                        System.Mvc.Engine.Execute(Url);
                    }
                };

                this.Add(btn);
            }
        }

        public void SetSubcontent(View subcontent)
        {
            _label.Margin = new Thickness(0, 10, 0, 0);
            _label.SetValue(RowSpanProperty, 1);
            _label.VerticalOptions = LayoutOptions.End;

            subcontent.VerticalOptions = LayoutOptions.Start;
            subcontent.Margin = new Thickness(0, -5, 0, 15);

            this.Add(1, 1, subcontent);
        }
        public void SetSubcontent(string text)
        {
            if (text != null)
            {
                this.SetSubcontent(new Label
                {
                    Text = text,
                    FontSize = 13,
                    TextColor = Color.Gray,
                });
            }
        }

        public void SetExtendedContent(View extendedContent)
        {
            if (this.ColumnDefinitions.Count < 3)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            }
            this.Add(0, 2, extendedContent);
            extendedContent.SetValue(RowSpanProperty, 2);
            extendedContent.VerticalOptions = LayoutOptions.Center;
            extendedContent.HorizontalOptions = LayoutOptions.Center;
        }

        public event Action<MyMenuItemView, MyButtonEventArgs> Clicked;
    }

    public class MyMenuView : StackLayout
    {
        public const int IconSize = 80;
        
        public MyMenuView()
        {
            this.Spacing = 0;
        }

        public void Separate(int height, string top, string bottom)
        {
            var box = new Grid {
                HeightRequest = height,
                BackgroundColor = Color.WhiteSmoke
            };

            if (top != null)
            {
                box.Children.Add(new Label {
                    Text = top,
                    TextColor = Color.Gray,
                    VerticalOptions = LayoutOptions.Start,
                    FontSize = 14,
                    Margin = new Thickness(IconSize, 5, 5, 5),
                });
            }

            if (bottom != null)
            {
                box.Children.Add(new Label
                {
                    Text = bottom,
                    TextColor = Color.Gray,
                    VerticalOptions = LayoutOptions.End,
                    FontSize = 14,
                    Margin = new Thickness(IconSize, 5, 5, 5)
                });
            }

            this.Children.Add(box);
        }

        public MyMenuItemView Add(MyMenuItemView item)
        {
            var hr = new BoxView {
                HeightRequest = 1,
                Color = Color.WhiteSmoke,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(IconSize, 0, 0, 0),
            };
            var grid = new Grid {
                Children = { item, hr },
            };
            this.Children.Add(grid);

            ItemAdded?.Invoke(item);

            return item;
        }

        public MyMenuItemView Add(MyMenuItemInfo info)
        {
            var item = new MyMenuItemView(info);
            return Add(item);
        }

        public event Action<MyMenuItemView> ItemAdded;

        System.Collections.IEnumerable _itemsSource;
        public System.Collections.IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource == value) return;
                this.Children.Clear();

                if((_itemsSource = value)!= null)
                {
                    foreach(MyMenuItemInfo info in value)
                    {
                        this.Add(info);
                    }
                }
            }
        }
    }
}
