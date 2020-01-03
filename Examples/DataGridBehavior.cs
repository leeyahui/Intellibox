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
    public static class OrderDataGridHelper
    {
        /// <summary>         
        /// Gets the visual child of an element         
        /// </summary>         
        /// <typeparam name="T">Expected type</typeparam>         
        /// <param name="parent">The parent of the expected element</param>         
        /// <returns>A visual child</returns>         
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        /// <summary>  
        /// 获取DataGrid的行  
        /// </summary>  
        /// <param name="dataGrid">DataGrid控件</param>  
        /// <param name="rowIndex">DataGrid行号</param>  
        /// <returns>指定的行号</returns>  
        public static DataGridRow GetRow(this DataGrid dataGrid, int rowIndex)
        {
            DataGridRow rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (rowContainer == null)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
                rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            }
            return rowContainer;
        }

        /// <summary>
        /// Gets the specified cell of the DataGrid
        /// </summary>
        /// <param name="grid">The DataGrid instance</param>
        /// <param name="row">The row of the cell</param>
        /// <param name="column">The column index of the cell</param>
        /// <returns>A cell of the DataGrid</returns>
        public static DataGridCell GetCell(this DataGrid grid, int row, int column)
        {
            DataGridRow rowContainer = grid.GetRow(row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    grid.ScrollIntoView(rowContainer, grid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public static DataGridCell GetNextCell(this DataGrid grid, int col)
        {
            if (col + 1 < grid.Columns.Count)
            {
                Int32 row = grid.Items.IndexOf(grid.CurrentItem);
                var cell = GetCell(grid, row, col + 1);
                if (cell == null)
                {
                    return GetNextCell(grid, col + 1);
                }
                else if (cell.IsReadOnly)
                {
                    return GetNextCell(grid, col + 1);
                }
                else
                {
                    return cell;
                }
            }
            else
            {
                return null;
            }
        }

    }
    public class DataGridBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.CurrentCellChanged += AssociatedObject_CurrentCellChanged;
            AssociatedObject.CellEditEnding += AssociatedObject_CellEditEnding;
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var uie = e.OriginalSource as System.Windows.UIElement;
            switch (e.Key)
            {
                case Key.Enter:
                    var popup = GetVisualBrother<Popup>(uie);
                    if (popup != null && popup.IsOpen && popup.Name.Equals("IntelliboxPopup1"))
                    {
                        return;
                    }

                    e.Handled = true;
                    var Col = AssociatedObject.Columns.IndexOf(AssociatedObject.CurrentColumn);
                    var cell = AssociatedObject.GetNextCell(Col);
                    if (cell != null)
                    {
                        cell.IsSelected = true;
                        cell.Focus();
                        AssociatedObject.BeginEdit();
                    }
                    break;
                default:
                    break;
            }
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

        public static T GetVisualBrother<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            return GetVisualChild<T>(parent);
        }

        private void AssociatedObject_CurrentCellChanged(object sender, EventArgs e)
        {
            AssociatedObject.BeginEdit();
        }
    }
}
