using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Plugins.SoundCloud.Drawing;
using SkiaSharp;

namespace MediaBrowser.Plugins.SoundCloud.ImageProcessing
{
    public static class OverlayHelper
    {
        public static void DrawImage(SKBitmap destination, SKBitmap source, Rectangle destinationRect,
                                     Rectangle sourceRect)
        {
            source.CurrentImage.CropImage(sourceRect.Width, sourceRect.Height, sourceRect.X, sourceRect.Y);
            OverlayHelper.DrawImage(destination, source, destinationRect);
        }

        public static void DrawImage(SKBitmap destination, SKBitmap source, Rectangle rect)
        {
            OverlayHelper.DrawImage(destination, source, (float)rect.X, (float)rect.Y, (float)rect.Width,
                                    (float)rect.Height);
        }

        private static void DrawImage(SKBitmap destination, SKBitmap source,
                                      float x, float y, float width, float height)
        {
            source.CurrentImage.ResizeImage(Convert.ToInt32(width), Convert.ToInt32(height));
            OverlayHelper.DrawImage(destination, source, x, y);
        }

        public static void DrawImage(SKBitmap destination, SKBitmap source, float x, float y)
        {
            destination.CurrentImage.CompositeImage(source, CompositeOperator.OverCompositeOp,
                                                    Convert.ToInt32(x), Convert.ToInt32(y));
        }

        public static SKBitmap GetNewTransparentImage(int width, int height)
        {
            SKBitmap ret = new SKBitmap(width, height);
            new SKCanvas(ret).Clear(SKColor.Transparent);
            return ret;
        }

        public static SKBitmap GetNewColorImage(string color, int width, int height)
        {
            SKBitmap ret = new SKBitmap(width, height);
            new SKCanvas(ret).Clear(SKColor.Parse(color));
            return ret;
        }
    }
}
