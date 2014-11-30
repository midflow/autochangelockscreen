using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using System.Windows.Media.Imaging;


namespace AutoChangeLockScreen
{
    public class NokiaTool : ImageEditorTool
    {
        int Value = 0;
        public NokiaTool()
        {
            this.Value = 1;
        }

        // Inherited from ImageEditorTool
        public override string Icon
        {
            get
            {
                // The icon must be added as a Resource to the project under the Assets folder for this path to work.
                return "/Assets/nokiaimg.png";
            }
        }

        // Inherited from ImageEditorTool
        public override string Name
        {
            get
            {
                return "NOKIA";
            }
        }

        

        //// Inherited from RangeTool
        //protected override void OnValueChanged(double newValue, double oldValue)
        //{
        //    if (this.PreviewImage == null)
        //    {
        //        return;
        //    }

        //    // Sets the working bitmap to have the same pixels as the original image.
        //    this.ResetWorkingBitmap();

        //    // Apply algorithm to the temporary working bitmap.
        //    this.ApplyRedness(newValue, this.workingBitmap.Pixels);

        //    // Set the result image so that the view can be updated.
        //    this.ModifiedImage = this.workingBitmap;
        //}

        // Inherited from ImageEditorTool
        protected async override System.Threading.Tasks.Task<WriteableBitmap> ApplyCore(WriteableBitmap actualImage)
        {
            int[] pixels = actualImage.Pixels;
            double value = this.Value;

            // Algorithms are computed on a separate thread so that the UI thread remains responsive.
            await Task.Factory.StartNew(() =>
            {
                this.ApplyNokia(value, pixels);
            });

            return actualImage;
        }

        private void ApplyNokia(double redness, int[] pixels)
        {
            for (int i = 0; i < pixels.Length; ++i)
            {
                byte[] bgra = BitConverter.GetBytes(pixels[i]);

                int r = bgra[2];
                r = (int)Math.Round(r * redness);
                if (r < 0)
                {
                    r = 0;
                }

                if (r > 255)
                {
                    r = 255;
                }

                bgra[2] = (byte)r;
                pixels[i] = BitConverter.ToInt32(bgra, 0);
            }
        }

    }
}
