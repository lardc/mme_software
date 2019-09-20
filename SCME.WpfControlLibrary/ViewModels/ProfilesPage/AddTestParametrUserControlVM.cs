﻿using PropertyChanged;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.WpfControlLibrary.ViewModels.ProfilesPage
{
    [AddINotifyPropertyChangedInterface]
    public class AddTestParametrUserControlVM
    {
        public double Force { get; set; }
        public double Height { get; set; }
        public double Temperature { get; set; }
    }
}
