using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Xamarin.Forms
{
    public class Rgb
    {
        int _value;

        public byte B
        {
            get => (byte)(_value);
            set => _value = (_value & 0xFFFF00) | value;
        }
        public byte G
        {
            get => (byte)(_value >> 8);
            set => _value = (_value & 0xFF00FF) | (value << 8);
        }
        public byte R
        {
            get => (byte)(_value >> 16);
            set => _value = (_value & 0xFFFF) | (value << 16);
        }

        public Rgb() { }
        public Rgb(byte r, byte g, byte b)
        {
            _value = (r << 16) | (g << 8) | b;
        }
        public Rgb(long color)
        {
            _value = (int)color;
        }
        public Rgb(string hex)
        {
            byte[] v = new byte[6];
            int i;
            for (i = 0; i < hex.Length && i < v.Length; i++)
            {
                char c = hex[i];
                if (c >= 'a') { c -= ' '; }

                byte b = (byte)(c >= 'A' ? c - 55 : (c & 15));
                v[i] = b;
            }

            if (i < 4)
            {
                for (--i; i >= 0; i--)
                {
                    int k = i << 1;
                    v[k + 1] = v[k] = v[i];
                }
            }

            _value = 0;
            foreach (var n in v)
            {
                _value = (_value << 4) | n;
            }
        }
        public Rgb(Color color)
        {
            _value = ((int)(color.R * 255) << 16) | ((int)(color.G * 255) << 8) | (int)(color.B * 255);
        }

        public static explicit operator Color(Rgb rgb)
        {
            if (rgb._value < 0) { return Color.Transparent; }
            return Color.FromRgb(rgb.R / 255.0, rgb.G / 255.0, rgb.B / 255.0);
        }

        public static implicit operator Rgb(Color color)
        {
            return new Rgb(color);
        }
        public static implicit operator Rgb(long color)
        {
            return new Rgb(color);
        }
        public static implicit operator Rgb(string color)
        {
            return new Rgb(color);
        }
    }

    public class Bound
    {
        public int Left;
        public int Top;
        public int Bottom;
        public int Right;

        public Bound() { }
        public Bound(double all)
        {
            Left = Top = Bottom = Right = (int)all;
        }
        public Bound(double left, double top, double right, double bottom)
        {
            Left = (int)left;
            Top = (int)top;
            Bottom = (int)bottom;
            Right = (int)right;
        }

        public Bound(string value)
        {
            int[] v = new int[4];
            int a = 0;
            int i = 0;
            bool minus = false;
            char last = ' ';
            foreach (var c in value)
            {
                switch (c)
                {
                    case '-': minus = true; continue;
                    case ' ': case ',':
                        if (last == ' ') continue;
                        v[i++] = (minus ? -a : a);

                        a = 0; minus = false;
                        last = ' ';
                        break;

                    default:
                        if (c >= '0' && '9' >= c)
                        {
                            a = (a << 1) + (a << 3) + (c & 15);
                        }
                        last = c;
                        break;
                }
                if (i >= v.Length) { break; }
            }

            if (i < v.Length)
            {
                v[i] = (minus ? -a : a);
            }
            if (i == 1)
            {
                v[2] = v[0];
                v[3] = v[1];
            }
            Left = v[0]; Right = v[2];
            Top = v[1]; Bottom = v[3];
        }

        public Bound Inflate(int x, int y)
        {
            Left -= x; Right += x;
            Top -= y; Bottom += y;

            return this;
        }
        public Bound Inflate(int size)
        {
            Left += size;
            Top += size;
            Right += size;
            Bottom += size;

            return this;
        }

        public static implicit operator Bound(long all)
        {
            return new Bound((int)all);
        }
        public static implicit operator Bound(string value)
        {
            return new Bound(value);
        }
        public static implicit operator Bound(Thickness v)
        {
            return new Bound(v.Left, v.Top, v.Right, v.Bottom);
        }
        public static implicit operator Bound(CornerRadius v)
        {
            return new Bound(v.TopLeft, v.TopRight, v.BottomRight, v.BottomLeft);
        }

        public static explicit operator Thickness(Bound b)
        {
            return new Thickness(b.Left, b.Top, b.Right, b.Bottom);
        }
        public static explicit operator CornerRadius(Bound b)
        {
            return new CornerRadius(b.Left, b.Top, b.Right, b.Bottom);
        }
    }

    public class StyleSheetMap : Dictionary<string, StyleSheet>
    {
        static StyleSheetMap _root;
        public static void SetClass(string name, StyleSheet item)
        {
            name = name.ToLower();
            if (_root == null) {
                _root = new StyleSheetMap();
            }
            _root.Add(name, item);
        }
        public static void SetClass<T>(StyleSheet item)
        {
            SetClass(typeof(T).Name, item);
        }
        public static StyleSheet GetClass(string name)
        {
            StyleSheet t;
            if (_root == null || _root.TryGetValue(name, out t) == false)
            {
                t = new StyleSheet();
            }
            return t;
        }

        public static void ApplyTo(View view, string css)
        {
            if (_root == null) return;
            foreach (var name in css.Split(' '))
            {
                if (name != string.Empty)
                {
                    StyleSheet ss;
                    if (_root.TryGetValue(name.ToLower(), out ss))
                    {
                        ss.SetStyle(view);
                    }
                }
            }
        }

        public static void Load(string filename)
        {
            try
            {
                var items = Vst.Json.Read<Dictionary<string, StyleSheet>>(filename);
                if (_root == null)
                {
                    _root = new StyleSheetMap();
                    foreach (var p in items)
                    {
                        SetClass(p.Key, p.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class StyleSheet
    {
        public Rgb Foreground { get; set; }
        public Rgb Background { get; set; }
        public Rgb BorderColor { get; set; }
        public double? BorderSize { get; set; }
        public Bound Padding { get; set; }
        public Bound Margin { get; set; }
        public Bound CornerRadius { get; set; }
        public double? Opacity { get; set; } = 1;
        public double? FontSize { get; set; }
        public int? FontWeight { get; set; }
        public LayoutOptions? HAlign { get; set; }
        public LayoutOptions? VAlign { get; set; }

        StyleSheetMap _childs;
        public StyleSheetMap Childs
        {
            get
            {
                if (_childs == null)
                {
                    _childs = new StyleSheetMap();
                }
                return _childs;
            }
        }
        public bool HasChilds => _childs != null && _childs.Count > 0;

        public StyleSheet Clone()
        {
            return (StyleSheet)MemberwiseClone();
        }

        public static implicit operator StyleSheet(string text)
        {
            return StyleSheetMap.GetClass(text).Clone();
        }

        public void SetStyle(View elem)
        {
            var type = elem.GetType();
            foreach (var p in this.GetType().GetProperties())
            {
                if (p.Name == "Childs")
                {
                    continue;
                }

                var v = p.GetValue(this);
                if (v != null)
                {
                    var q = type.GetProperty(p.Name);
                    if (q != null)
                    {
                        try
                        {
                            q.SetValue(elem, v);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }

    public class StyleSheetList : List<StyleSheet>
    {
        public static implicit operator StyleSheetList(string text)
        {
            var lst = new StyleSheetList();
            foreach (var name in text.Trim().Split(' '))
            {
                if (name != string.Empty)
                {
                    lst.Add(StyleSheetMap.GetClass(name));
                }
            }
            return lst;
        }
    }
}
