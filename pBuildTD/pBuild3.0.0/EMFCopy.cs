using BCDev.XamlToys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace pBuild
{
    public class EMFCopy
    {
        public static void CopyUIElementToClipboard(FrameworkElement element)
        {
            ////data object to hold our different formats representing the element
            //DataObject dataObject = new DataObject();

            ////lets start with the text representation
            ////to make is easy we will just assume the object set as the DataContext has the ToString method overrideen and we use that as the text
            //dataObject.SetData(DataFormats.Text, element.DataContext.ToString(), true);

            ////now lets do the image representation
            //double width = element.ActualWidth;
            //double height = element.ActualHeight;
            //RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            //DrawingVisual dv = new DrawingVisual();
            //using (DrawingContext dc = dv.RenderOpen())
            //{
            //    VisualBrush vb = new VisualBrush(element);
            //    dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            //}
            //bmpCopied.Render(dv);
            //dataObject.SetData(DataFormats.Bitmap, bmpCopied, true);

            ////now place our object in the clipboard
            //Clipboard.SetDataObject(dataObject, true);


            double width = element.ActualWidth;
            double height = element.ActualHeight;
            RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(element);
                dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            }
            bmpCopied.Render(dv);
            Clipboard.SetImage(bmpCopied);
        }

        public static void CopyVisualToWmfClipboard(Visual visual, Window clipboardOwnerWindow)
        {
            CopyXAMLStreamToWmfClipBoard(visual, clipboardOwnerWindow);
            return;
        }

        public static object LoadXamlFromStream(Stream stream)
        {
            using (Stream s = stream)
                return XamlReader.Load(s);
        }

        public static System.Drawing.Graphics CreateEmf(Stream wmfStream, Rect bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0) bounds = new Rect(0, 0, 1, 1);
            using (System.Drawing.Graphics refDC = System.Drawing.Graphics.FromImage(new System.Drawing.Bitmap(1, 1)))
            {
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new System.Drawing.Imaging.Metafile(wmfStream, refDC.GetHdc(), bounds.ToGdiPlus(), System.Drawing.Imaging.MetafileFrameUnit.Pixel, System.Drawing.Imaging.EmfType.EmfPlusDual));
                return graphics;
            }
        }

        public static T GetDependencyObjectFromVisualTree<T>(DependencyObject startObject)
            // don't restrict to DependencyObject items, to allow retrieval of interfaces
            //where T : DependencyObject
            where T : class
        {
            //Walk the visual tree to get the parent(ItemsControl) 
            //of this control
            DependencyObject parent = startObject;
            while (parent != null)
            {
                T pt = parent as T;
                if (pt != null)
                    return pt;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        public static void CopyXAMLStreamToWmfClipBoard(Visual visual, Window clipboardOwnerWindow)
        {
            // http://xamltoys.codeplex.com/
            try
            {
                var drawing = Utility.GetDrawingFromXaml(visual);

                var bounds = drawing.Bounds;
                Console.WriteLine("Drawing Bounds: {0}", bounds);

                MemoryStream wmfStream = new MemoryStream();

                using (var g = CreateEmf(wmfStream, bounds))
                    Utility.RenderDrawingToGraphics(drawing, g);

                wmfStream.Position = 0;

                System.Drawing.Imaging.Metafile metafile = new System.Drawing.Imaging.Metafile(wmfStream);

                IntPtr hEMF, hEMF2;
                hEMF = metafile.GetHenhmetafile(); // invalidates mf
                if (!hEMF.Equals(new IntPtr(0)))
                {
                    hEMF2 = NativeMethods.CopyEnhMetaFile(hEMF, new IntPtr(0));
                    if (!hEMF2.Equals(new IntPtr(0)))
                    {
                        if (NativeMethods.OpenClipboard(((IWin32Window)clipboardOwnerWindow.OwnerAsWin32()).Handle))
                        {
                            if (NativeMethods.EmptyClipboard())
                            {
                                NativeMethods.SetClipboardData(14 /*CF_ENHMETAFILE*/, hEMF2);
                                NativeMethods.CloseClipboard();
                            }
                        }
                    }
                    NativeMethods.DeleteEnhMetaFile(hEMF);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
