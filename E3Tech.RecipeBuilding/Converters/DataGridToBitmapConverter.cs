using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace E3Tech.RecipeBuilding.Converters
{
    public class DataGridToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertDataGridToBitmap(value as DataGrid);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as RenderTargetBitmap;
        }

        private RenderTargetBitmap ConvertDataGridToBitmap(DataGrid dataGrid)
        {
            if (dataGrid != null && dataGrid.ActualHeight > 0 && dataGrid.ActualWidth > 0)
            {
                double width = dataGrid.ActualWidth;
                double height = dataGrid.ActualHeight;
                RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 100, 100, PixelFormats.Default);
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(dataGrid);
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), new Size(width, height)));
                }
                bmpCopied.Render(drawingVisual);

                return bmpCopied;
            }

            return default;
        }
    }
}
