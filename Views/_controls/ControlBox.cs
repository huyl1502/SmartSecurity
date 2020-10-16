using System;
namespace Xamarin.Forms
{
    public class ControlBox : MyTableLayout
    {
        protected IMyInput CreateInput(BindingInfo info)
        {
            if (info.Input != null)
            {
                if (info.Input == "none") return null;
                switch (info.Input[0])
                {
                    case 'd': return new MyDateBox();
                    case 't': return new MyTimeBox();
                    case 'm': return new MyMultyLineBox();
                    case 'p': return new MyPasswordBox();
                    case 'c': return new MyComboBox();
                }                    
            }                
            return new MyTextBox();
        }

        BindingInfoCollection _binding;
        public BindingInfoCollection Binding
        {
            get => _binding;
            set
            {
                if (_binding == value) return;

                foreach (var p in (_binding = value))
                {
                    var info = p.Value;
                    var input = CreateInput(info);

                    if (input == null) continue;

                    input.BindingName = info.BindingName ?? p.Key;
                    input.Required = (info.AllowNull == false);

                    this.AddInput(info.Caption ?? p.Key, input);
                }
            }
        }

        public IMyInput this[string name]
        {
            get
            {
                foreach (var e in this.Children)
                {
                    var input = e as IMyInput;
                    if (input != null && input.BindingName == name)
                    {
                        return input;
                    }
                }
                return null;
            }
        }

        public virtual void AddInput(string caption, IMyInput input)
        {
            this.AddRow(new Label {
                Text = caption,
                TextColor = Color.Gray,
                Margin = new Thickness(0, 10, 0, 0),

            });
            this.AddRow((View)input);
        }

        public void SetValue(object value)
        {
            if (value == null)
            {
                return;
            }
            var type = value.GetType();
            foreach (var e in this.Children)
            {
                var input = e as IMyInput;
                if (input != null)
                {
                    var p = type.GetProperty(input.BindingName);
                    if (p != null)
                    {
                        input.Value = p.GetValue(value);
                    }
                }
            }
        }

        public string GetJsonString()
        {
            var lst = new System.Collections.Generic.List<string>();
            foreach (var e in this.Children)
            {
                var input = e as IMyInput;
                if (input != null)
                {
                    var v = input.Value;
                    if (v == null && input.Required)
                    {
                        return null;
                    }
                    lst.Add(string.Format("\"{0}\":\"{1}\"", input.BindingName, v));
                }
            }

            return "{" + string.Join(",", lst) + "}";
        }

        public T GetValue<T>()
        {
            var json = GetJsonString();
            if (json == null)
            {
                return default(T);
            }
            return Vst.Json.GetObject<T>(json);
        }
    }

    public class HorizontalControlBox : ControlBox
    {
        public HorizontalControlBox()
        {
            this.AddColumn();
            this.AddColumn();
        }
        public override void AddInput(string caption, IMyInput input)
        {
            var r = this.AddRow();
            this.Add(r, 0, new Label { Text = caption });
            this.Add(r, 1, (View)input);
        }
    }
}
