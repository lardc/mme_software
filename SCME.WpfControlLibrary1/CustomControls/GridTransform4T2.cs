using System.Windows;
using System.Windows.Controls;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class GridTransform4T2 : Grid
    {
        public bool NeedTransform { get; set; }

        protected void Transform4T2()
        {
            if (NeedTransform == false)
                return;
            
            foreach (UIElement uiElement in Children)
            {
                var oldColumn = (int)uiElement.GetValue(Grid.ColumnProperty);
                int newColumn;
                switch (oldColumn)
                {
                    case 2:
                        newColumn = 0;
                        break;
                    case 3:
                        newColumn = 1;
                        break;
                    default:
                        newColumn = oldColumn;
                        break;
                }

                if (oldColumn != newColumn)
                    uiElement.SetValue(Grid.ColumnProperty, newColumn);
            }
        }
        
    }
}