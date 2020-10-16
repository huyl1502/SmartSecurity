using System;
namespace Xamarin.Forms
{
    public class MyButtonEventArgs : EventArgs
    {
        public bool Handled;
    }
    public class MyButton : MyLabel
    {
        public string Url { get; set; }
        public event Action<MyButtonEventArgs> Clicked;

        public MyButton()
        {
            var btn = (Button)base.Insert(new Button {
                BackgroundColor = Color.Transparent,
            });

            btn.Clicked += (s, e) => {

                var arg = new MyButtonEventArgs();
                Clicked?.Invoke(arg);

                if (arg.Handled == false && Url != null)
                {
                    System.Mvc.Engine.Execute(Url);
                }
            };
        }

        new public double BorderSize
        {
            get { return base.BorderSize; }
            set
            {
                base.BorderSize = value;
                if (value > 1)
                {
                    Outer.BackgroundColor = Inner.BackgroundColor;
                    Outer.Opacity = 0.8;
                }
            }
        }
    }
}
