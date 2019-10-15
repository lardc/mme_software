namespace SCME.Types
{
    public class ConstantsMinMax
    {
        public struct MinMaxInterval
        {
            public MinMaxInterval(float minimum, float maximum, float interval = 1)
            {
                Minimum = minimum;
                Maximum = maximum;
                Interval = interval;
            }

            public float Minimum { get; private set; }
            public float Maximum { get; private set; }
            public float Interval { get; private set; }
            
            
        }
        
        public class Tou
        {
            public static MinMaxInterval CurrentAmplitude { get; set; } = new MinMaxInterval(160,1250,5);
        }
    }
}