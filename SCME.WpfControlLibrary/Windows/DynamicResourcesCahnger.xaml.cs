using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;
using PropertyChanged;

namespace SCME.WpfControlLibrary.Windows
{
    public partial class DynamicResourcesChanger : Window
    {
        [AddINotifyPropertyChangedInterface]
        public abstract class SliderValue<T> 
        {
            protected SliderValue(string name, T data)
            {
                Name = name;
                Data = data;
            }

            public string Name { get; set; }
            protected T Data;

            protected abstract void SetValue();

        }

        // ReSharper disable once MemberCanBePrivate.Global
        [AddINotifyPropertyChangedInterface]
        public class SliderValueInt : SliderValue<int>  
        {
            public int Value
            {
                get => Data;
                set { Data = value;
                    SetValue();
                }
            }
            public SliderValueInt(string name, int data) : base(name, data)
            {
            }

            protected override void SetValue()
            {
                Application.Current.Resources[Name] = (double)Data;
            }
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        [AddINotifyPropertyChangedInterface]
        public class SliderValueIntThickness : SliderValue<Thickness>
        {
            public int Left
            {
                get => (int)Data.Left;
                set
                {
                    Data.Left = value;
                    SetValue();
                }
            }
            public int Top
            {
                get => (int)Data.Top;
                set { Data.Top = value;
                    SetValue();
                }
            }
            public int Right
            {
                get => (int)Data.Right;
                set { Data.Right = value;
                    SetValue();
                }
            }
            public int Bottom
            {
                get =>(int) Data.Bottom;
                set { Data.Bottom = value;
                    SetValue();
                }
            }
            
            public SliderValueIntThickness(string name, Thickness data) : base(name, data)
            {
            }

            protected override void SetValue()
            {
                Application.Current.Resources[Name] = Data;
            }
        }

        
        public ObservableCollection<object> ProgressBarValues { get; set; } = new ObservableCollection<object>();


        public DynamicResourcesChanger()
        {
            InitializeComponent();
            Topmost = true;
            var q = Application.Current.Resources.Keys.OfType<string>().Where(m => m.Substring(0, "SCME.".Length) == "SCME.").OrderBy(m=> m);
            foreach (var i in q)
            {
                var obj = Application.Current.Resources[i];
                switch (obj)
                {
                    case double d:
                        ProgressBarValues.Add(new SliderValueInt(i, (int) d));
                    break;;
                    case Thickness thickness:
                        ProgressBarValues.Add(new SliderValueIntThickness(i, thickness));
                        break;;
                }
            }
        }
    }
}