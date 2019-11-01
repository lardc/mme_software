using System;
using SCME.Types;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class NumericUpDown : MahApps.Metro.Controls.NumericUpDown
    {
        public ConstantsMinMax.MinMaxInterval MinMaxInterval
        {
            set
            {
                Minimum = value.Minimum;
                Maximum = value.Maximum;
                Interval = value.Interval;
            }
        }
//        public new float Minimum
//        {
//            get => Convert.ToSingle(base.Minimum);
//            set => base.Minimum = Convert.ToSingle(value);
//        }
    }
}