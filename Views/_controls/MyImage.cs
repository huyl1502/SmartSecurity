using System;
namespace Xamarin.Forms
{
    public class MyIcon : View
    {
        public MyIcon(string key)
        {
            this.Key = key;
        }
        public MyIcon()
        {
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            Size = 30;
        }

        public static readonly BindableProperty ForegroundProperty = BindableProperty.Create(
                                                                 propertyName: "Foreground",
                                                                 returnType: typeof(Rgb),
                                                                 declaringType: typeof(MyIcon),                                                                 
                                                                 defaultBindingMode: BindingMode.TwoWay);

        public Rgb Foreground
        {
            get { return (Rgb)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }


        public static readonly BindableProperty KeyProperty = BindableProperty.Create(
                                                                 propertyName: "Key",
                                                                 returnType: typeof(string),
                                                                 declaringType: typeof(MyIcon),
                                                                 defaultBindingMode: BindingMode.TwoWay);

        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value?.ToLower()); }
        }
        public double Size
        {
            get
            {
                return HeightRequest;
            }
            set
            {
                HeightRequest = WidthRequest = value;
            }
        }
    }

    public class MyImage : Image
    {
        static public string GetImageFilePath(string name)
        {
            var imgFileName = name + ".png";
            if (Device.RuntimePlatform == Device.iOS)
            {
                imgFileName = "Images/" + imgFileName;
            }
            return imgFileName;
        }
        public MyImage()
        {
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
        }
        public MyImage(string name)
            : this()
        {
            this.Source = ImageSource.FromFile(GetImageFilePath(name));
        }

        public double SquareSize
        {
            get { return this.Width; }
            set { this.WidthRequest = this.HeightRequest = value; }
        }
    }
}
