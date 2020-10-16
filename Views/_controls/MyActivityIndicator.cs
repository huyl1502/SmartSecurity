using System;
using Xamarin;

namespace Xamarin.Forms
{
    public class MyActivityIndicator : ActivityIndicator
    {
        BoxView _backGround;
        
        public MyActivityIndicator()
        {
            this.WidthRequest = this.HeightRequest = 50;
            this.HorizontalOptions = LayoutOptions.Center;
            this.VerticalOptions = LayoutOptions.Center;
            //this.Color = Color.White;
        }

        public MyActivityIndicator SetBlending(double opacity, Color color)
        {
            if (_backGround == null)
                _backGround = new BoxView();

            _backGround.BackgroundColor = color;
            _backGround.Opacity = opacity;
            
            return this;
        }

        public MyActivityIndicator Show(Grid grid)
        {
            if (_backGround != null)
            {
                grid.Children.Add(_backGround);
            }
            grid.Children.Add(this);
            this.IsRunning = true;
            return this;
        }

        public MyActivityIndicator Show(ContentPage page)
        {
            var mainContent = page.Content;
            var grid = new Grid {
                Children = { mainContent }
            };
            page.Content = grid;

            return this.Show(grid);
        }

        public void Hide()
        {
            var grid = (Grid)this.Parent;
            if (grid != null)
            {
                grid.Children.Remove(this);
                if (_backGround != null)
                {
                    grid.Children.Remove(_backGround);
                }
            }
        }
    }
}
