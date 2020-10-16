using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
    using Xamarin.Forms.Internals;

    public class StyleView<T> : ContentView
        where T: View, new()
    {
        new public T Content => (T)base.Content;

        public StyleView()
        {
            base.Content = new T();
        }
        public StyleView(T view)
        {
            base.Content = view;
        }

        public LayoutOptions HAlign
        {
            get => (LayoutOptions)GetValue(HorizontalOptionsProperty);
            set => SetValue(HorizontalOptionsProperty, (LayoutOptions)value);
        }
        public LayoutOptions VAlign
        {
            get => (LayoutOptions)GetValue(VerticalOptionsProperty);
            set => SetValue(VerticalOptionsProperty, (LayoutOptions)value);
        }

        string _css;
        public string Css
        {
            get => _css;
            set
            {
                if (_css != value)
                {
                    _css = value;
                    if (this.Parent != null)
                    {
                        StyleSheetMap.ApplyTo(this, _css + ' ' + this.GetType().Name);
                    }
                }
            }
        }

        public virtual void Refresh() { }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            StyleSheetMap.ApplyTo(this, _css + ' ' + this.GetType().Name);
        }
    }
    public class PanelView<T> : StyleView<T>
        where T : Layout, new()
    {
        public void AddRange(params View[] items)
        {
            var children = (System.Collections.IList)this.Content.Children;
            foreach (var e in items) { children.Add(e); }
        }

        public void Add(View item)
        {
            var children = (System.Collections.IList)this.Content.Children;
            children.Add(item);
        }
        public void Clear()
        {
            var children = (System.Collections.IList)this.Content.Children;
            children.Clear();
        }
    }

    public class BorderView : StyleView<Grid>
    {
        protected BoxView Outer => (BoxView)base.Content.Children[0];
        protected BoxView Inner => (BoxView)base.Content.Children[1];

        public BorderView()
        {
            base.Content.Children.Add(new BoxView());
            base.Content.Children.Add(new BoxView());
            Inner.Margin = new Thickness(1);
        }
        public Rgb Background
        {
            get => (Rgb)Inner.BackgroundColor;
            set => Inner.BackgroundColor = (Color)value;
        }
        public Rgb BorderColor
        {
            get => (Rgb)Outer.Color;
            set
            {
                Outer.Color = (Color)value;
            }
        }

        public double BorderSize
        {
            get => Inner.Margin.Left;
            set => Inner.Margin = new Thickness(value);
        }
        public Bound CornerRadius
        {
            get => (Bound)Outer.CornerRadius;
            set
            {
                Outer.CornerRadius = (CornerRadius)value;

                var size = (int)Inner.Margin.Left;
                Inner.CornerRadius = (CornerRadius)value.Inflate(-size);
            }
        }
    }

    public class BorderView<T> : BorderView
        where T: View, new()
    {

        new public T Content => (T)base.Content.Children[2];


        public BorderView()
            : this(new T())
        {
        }
        public BorderView(T view)
        {
            base.Content.Children.Add(new T());
        }

        new public Bound Padding
        {
            get => Content.Margin;
            set => Content.Margin = (Thickness)value;
        }
        public View Insert(View item)
        {
            base.Content.Children.Add(item);
            return item;
        }

        public View Insert(int index, View item)
        {
            base.Content.Children.Insert(index + 2, item);
            return item;
        }
    }

    public class TextView<T> : BorderView<T>
        where T : View, new()
    {
        protected virtual void SetContentValue(string name, object value)
        {
            typeof(T).GetProperty(name).SetValue(Content, value);
        }
        protected virtual TValue GetContentValue<TValue>(string name)
        {
            return (TValue)typeof(T).GetProperty(name).GetValue(Content);
        }

        public Rgb Foreground
        {
            get => GetContentValue<Rgb>("TextColor");
            set => SetContentValue("TextColor", (Color)value);
        }
        public double FontSize
        {
            get => GetContentValue<double>("FontSize");
            set => SetContentValue("FontSize", value);
        }

        new public bool IsFocused => Content.IsFocused;
        public virtual string Text { get; set; }
    }

    public class MyLabel : TextView<Label>
    {
        public MyLabel()
        {
            Content.HorizontalOptions = LayoutOptions.Center;
            Content.VerticalOptions = LayoutOptions.Center;
        }
        public override string Text
        {
            get => (string)Content.GetValue(Label.TextProperty);
            set => Content.SetValue(Label.TextProperty, value);
        }
    }

    public class MyCircle : BorderView
    {
        public MyCircle()
        {
            this.VerticalOptions = LayoutOptions.Center;
        }
        public double Size
        {
            get => this.HeightRequest;
            set
            {
                this.WidthRequest = this.HeightRequest = value;
                CornerRadius = (int)(value / 2);
            }
        }
    }
}
