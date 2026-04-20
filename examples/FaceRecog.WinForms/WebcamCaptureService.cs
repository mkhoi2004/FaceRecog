using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace FaceRecog.WinForms
{
    internal sealed class WebcamCaptureService : IDisposable
    {
        private VideoCapture _Capture;
        private bool _IsStarted;

        public bool IsStarted => this._IsStarted;

        public void Start(int deviceIndex = 0)
        {
            this.Stop();

            this._Capture = new VideoCapture(deviceIndex);
            if (!this._Capture.IsOpened())
            {
                this._Capture.Dispose();
                this._Capture = null;
                throw new InvalidOperationException("Không mở được webcam mặc định.");
            }

            this._IsStarted = true;
        }

        public Bitmap CaptureFrame()
        {
            if (!this._IsStarted || this._Capture == null)
                return null;

            using (var mat = new Mat())
            {
                if (!this._Capture.Read(mat) || mat.Empty())
                    return null;

                return this.ConvertMatToBitmap(mat);
            }
        }

        private Bitmap ConvertMatToBitmap(Mat mat)
        {
            if (mat == null)
                throw new ArgumentNullException(nameof(mat));

            Mat source = mat;
            Mat converted = null;

            if (mat.Channels() == 1)
            {
                converted = new Mat();
                Cv2.CvtColor(mat, converted, ColorConversionCodes.GRAY2BGR);
                source = converted;
            }
            else if (mat.Channels() == 4)
            {
                converted = new Mat();
                Cv2.CvtColor(mat, converted, ColorConversionCodes.BGRA2BGR);
                source = converted;
            }

            using (converted)
            using (var continuous = source.IsContinuous() ? source : source.Clone())
            {
                var bitmap = new Bitmap(continuous.Width, continuous.Height, PixelFormat.Format24bppRgb);
                var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                try
                {
                    var rowLength = checked(continuous.Width * (int)continuous.ElemSize());
                    var bufferLength = checked((int)(continuous.Total() * (long)continuous.ElemSize()));
                    var buffer = new byte[bufferLength];
                    Marshal.Copy(continuous.Data, buffer, 0, buffer.Length);

                    for (var y = 0; y < continuous.Height; y++)
                    {
                        var sourceOffset = y * rowLength;
                        var destinationOffset = IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride);
                        Marshal.Copy(buffer, sourceOffset, destinationOffset, rowLength);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                return bitmap;
            }
        }

        public void Stop()
        {
            if (this._Capture != null)
            {
                this._Capture.Release();
                this._Capture.Dispose();
                this._Capture = null;
            }

            this._IsStarted = false;
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}