using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Examples
{
    public class DataGridBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.CurrentCellChanged += AssociatedObject_CurrentCellChanged;
            //AssociatedObject.CellEditEnding += AssociatedObject_CellEditEnding;
        }

        private void AssociatedObject_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.GetType() == typeof(DataGridTemplateColumn))
            {
                var popup = GetVisualChild<Popup>(e.EditingElement);
                if (popup != null && popup.IsOpen && popup.Name.Equals("IntelliboxPopup1"))
                {
                    e.Cancel = true;
                }
            }
        }

        private static T GetVisualChild<T>(DependencyObject visual) where T : DependencyObject
        {
            if (visual == null)
                return null;

            var count = VisualTreeHelper.GetChildrenCount(visual);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(visual, i);

                var childOfTypeT = child as T ?? GetVisualChild<T>(child);
                if (childOfTypeT != null)
                    return childOfTypeT;
            }

            return null;
        }

        private void AssociatedObject_CurrentCellChanged(object sender, EventArgs e)
        {
            AssociatedObject.BeginEdit();
        }
    }
}
