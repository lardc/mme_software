﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCME.UI.ModelViews
{
    public class TOUPageVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public Types.TOU.TestParameters Input { get; set; } = new Types.TOU.TestParameters() { IsEnabled = true };

        public string State { get; set; }

        public float ITM { get; set; }

        public float TGD { get; set; }

        public float TGT { get; set; }


        public bool IsRunning { get; set; }

    }
}
