using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace OBRLibrary
{
    public class Canny
    {
        private const float HIGHT_HRESHOLD = 50;
        private const float LOW_HRESHOLD = -50;

        private static float[,] _gY = {{1, 2, 1},
                        {0, 0, 0},
                        {-1, -2, -1}};

        private static float[,] _gX = {{-1, 0, 1},
                        {-2, 0, 2},
                        {-1, 0, 1}};

        public Canny()
        { }

        public float[,] Calculation(Bitmap bitmap)
        {
            float[,] paddedImage = null;
            float[,] data = GetGrayImage(bitmap);

            Gaussian(data, paddedImage, GetGauseMask());
            Gradients(data, paddedImage);
            SuppressionNoMaximum(data);

            (bool[,] hightMask, bool[,] mainMask) = GetDoubleThresholdFiltering(data, LOW_HRESHOLD, HIGHT_HRESHOLD);
            GetTracking(data, hightMask, mainMask);
            hightMask = null;
            mainMask = null;

            return data;
        }

        public float[,] CalculationMulty(Bitmap bitmap, int threadCount = 8)
        {
            float[,] paddedImage = null;
            float[,] data = GetGrayImage(bitmap);

            CreateConvThreadPool(data, paddedImage, GetGauseMask(), threadCount);
            GradientsThread(data, paddedImage, threadCount);
            SuppressionNoMaximum(data);

            (bool[,] hightMask, bool[,] mainMask) = GetDoubleThresholdFiltering(data, LOW_HRESHOLD, HIGHT_HRESHOLD);
            GetTracking(data, hightMask, mainMask);

            return data;
        }

        private float[,] GetGrayImage(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            float[,] result = new float[width, height];

            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = bitmap.Palette;
                byte[] grayValues = new byte[256];

                for (int i = 0; i < 256; i++)
                {
                    grayValues[i] = (byte)i;
                }

                if (palette.Entries.Length == 256)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            Color pixelColor = bitmap.GetPixel(x, y);
                            result[x, y] = (float)Math.Round(0.2989 * grayValues[pixelColor.R] + 0.5870 * grayValues[pixelColor.G] + 0.1140 * grayValues[pixelColor.B]);
                        }
                    }
                }
            }
            else
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                IntPtr ptr = bitmapData.Scan0;

                int bytes = Math.Abs(bitmapData.Stride) * height;
                byte[] values = new byte[bytes];
                Marshal.Copy(ptr, values, 0, bytes);
                int bytePerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int idx = y * bitmapData.Stride + x * bytePerPixel;
                        result[x, y] = (float)Math.Round(0.2989 * values[idx + 2] + 0.5870 * values[idx + 1] + 0.1140 * values[idx]);
                    }
                }

                bitmap.UnlockBits(bitmapData);
                bitmapData = null;
                values = null;
            }

            bitmap.Dispose();


            return result;
        }

        private float GetConv(float[,] paddedImage, float[,] kernel, int x, int y)
        {
            int kernelSize = kernel.GetLength(0);
            int padSize = kernelSize / 2;
            float result = 0f;

            for (int m = 0; m < kernelSize; m++)
            {
                for (int n = 0; n < kernelSize; n++)
                {
                    result += paddedImage[x + m, y + n] * kernel[m, n];
                }
            }

            return result;
        }

        private void Gaussian(float[,] data, float[,] paddedImage, float[,] kernel)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);
            int padSize = kernel.GetLength(0) / 2;

            paddedImage = PadImage(data, padSize);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    data[i, j] = GetConv(paddedImage, kernel, i, j);
                }
            }
        }

        private void Gradients(float[,] data, float[,] paddedImage)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            paddedImage = PadImage(data, 1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float gradientY = GetConv(paddedImage, _gY, i, j);
                    float gradientX = GetConv(paddedImage, _gX, i, j);
                    data[i, j] = (float)Math.Sqrt(Math.Pow(gradientX, 2) + Math.Pow(gradientY, 2));
                }
            }
        }

        private void SuppressionNoMaximum(float[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            float[] neighbors = new float[2];
            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    float direction = data[i, j];

                    if ((0 <= direction && direction < Math.PI / 4) || (7 * Math.PI / 4 <= direction && direction <= 2 * Math.PI))
                    {
                        neighbors[0] = data[i, j + 1];
                        neighbors[1] = data[i, j - 1];
                    }
                    else if (Math.PI / 4 <= direction && direction < 3 * Math.PI / 4)
                    {
                        neighbors[0] = data[i - 1, j + 1];
                        neighbors[1] = data[i + 1, j - 1];
                    }
                    else if (3 * Math.PI / 4 <= direction && direction < 5 * Math.PI / 4)
                    {
                        neighbors[0] = data[i - 1, j];
                        neighbors[1] = data[i + 1, j];
                    }
                    else
                    {
                        neighbors[0] = data[i - 1, j - 1];
                        neighbors[1] = data[i + 1, j + 1];
                    }

                    if (data[i, j] < Math.Max(neighbors[0], neighbors[1]))
                    {
                        data[i, j] = 0;
                    }
                }
            }
            neighbors = null;
        }

        private (bool[,], bool[,]) GetDoubleThresholdFiltering(float[,] data, float lowThreshold, float highThreshold)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);

            bool[,] highMask = new bool[height, width];
            bool[,] mainMask = new bool[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (data[i, j] >= highThreshold)
                    {
                        highMask[i, j] = true;
                    }
                    else if (data[i, j] >= lowThreshold && data[i, j] < highThreshold)
                    {
                        mainMask[i, j] = true;
                    }
                }
            }

            return (highMask, mainMask);
        }

        private void GetTracking(float[,] data, bool[,] highMask, bool[,] mainMask)
        {
            int height = highMask.GetLength(0);
            int width = highMask.GetLength(1);

            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    if (mainMask[i, j] == false)
                    {
                        data[i, j] = (byte)(ContainsTrueNeighbor(highMask, i, j) ? 255 : 0);
                    }
                }
            }
        }

        private bool ContainsTrueNeighbor(bool[,] mask, int row, int col) => Enumerable.Range(row - 1, 3).Any(x => Enumerable.Range(col - 1, 3).Any(y => mask[x, y] != false));

        private float[,] PadImage(float[,] image, int padSize)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            float[,] paddedImage = new float[width + 2 * padSize, height + 2 * padSize];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    paddedImage[i + padSize, j + padSize] = image[i, j];
                }
            }

            return paddedImage;
        }



        ////MULTIKI
        /*  private void CreateConvThreadPool(float[,] data, float[,] paddedImage, float[,] kernel, int threadCount)
          {
              int workerThreads = 0;

              ThreadPool.SetMaxThreads(threadCount, 0);
              ThreadPool.GetMaxThreads(out workerThreads, out int _);

              int width = data.GetLength(0);
              int height = data.GetLength(1);

              int pixelCount = width * height;

              int iteration = workerThreads;
              int whole = pixelCount / workerThreads;
              int remainder = pixelCount % workerThreads;

              int kernelSize = kernel.GetLength(0);
              int padSize = kernelSize / 2;
              paddedImage = PadImage(data, padSize);

              if (whole > 4096)
              {
                  whole = pixelCount / 4096;
                  remainder = pixelCount % 4096;
                  iteration = whole;
              }

              CountdownEvent countdownEvent = new CountdownEvent(iteration);

              int positionX = 0;
              int positionY = 0;
              for (int i = 0; i < iteration; i++)
              {
                  int pixelsPerThread = whole + (i < remainder ? 1 : 0);

                  int newY = (pixelsPerThread + (height * positionX + positionY)) % height;
                  int newX = (pixelsPerThread + (height * positionX + positionY)) / height;

                  positionY = newY;
                  positionX = newX;

                  ConvMultyParam convParam = new ConvMultyParam(positionX, positionY, pixelsPerThread);

                  ThreadPool.QueueUserWorkItem(state =>
                  {
                      ConvMultyParam param = (ConvMultyParam)state;

                      int idx = 0;
                      for (int x = param.positionX; x < width && idx != param.countThis; x++, param.positionY = 0)
                      {
                          for (int y = param.positionY; y < height && idx != param.countThis; y++, idx++)
                          {
                              data[x, y] = GetConv(paddedImage, kernel, x, y);
                          }
                      }

                      countdownEvent.Signal();

                  }, convParam);

              }

              countdownEvent.Wait();
              countdownEvent.Dispose();
          }*/

        private void CreateConvThreadPool(float[,] data, float[,] paddedImage, float[,] kernel, int threadCount)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);
            int pixelCount = width * height;

            int kernelSize = kernel.GetLength(0);
            int padSize = kernelSize / 2;
            paddedImage = PadImage(data, padSize);

            int pixelsPerThread = pixelCount / threadCount;
            int remainingPixels = pixelCount % threadCount;

            ManualResetEvent[] resetEvents = new ManualResetEvent[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int startPixel = i * pixelsPerThread;
                int endPixel = (i == threadCount - 1) ? pixelCount : startPixel + pixelsPerThread + ((remainingPixels > 0) ? 1 : 0);
                remainingPixels = Math.Max(0, remainingPixels - 1);

                resetEvents[i] = new ManualResetEvent(false);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    ConvPixels(data, paddedImage, kernel, width, height, startPixel, endPixel);
                    resetEvents[(int)state].Set();
                }, i);
            }

            WaitHandle.WaitAll(resetEvents);
        }

        private void ConvPixels(float[,] data, float[,] paddedImage, float[,] kernel, int width, int height, int startPixel, int endPixel)
        {
            int kernelSize = kernel.GetLength(0);
            int padSize = kernelSize / 2;

            for (int i = startPixel; i < endPixel; i++)
            {
                int x = i / height;
                int y = i % height;
                data[x, y] = GetConv(paddedImage, kernel, x, y);
            }
        }

        private void GradientsThread(float[,] data, float[,] paddedImage, int threadCount)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            float[,] byffer = new float[width, height];
            CopyArray(byffer, data);

            CreateConvThreadPool(byffer, paddedImage, _gX, threadCount);
            CreateConvThreadPool(data, paddedImage, _gY, threadCount);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    data[i, j] = (float)Math.Sqrt(Math.Pow(byffer[i, j], 2) + Math.Pow(data[i, j], 2));
                }
            }

            byffer = null;
        }



        ////FUNCTIONALNO

        private static float[,] GetGauseMask()
        {
            float[,] gaussMask = {{2, 4, 5, 4, 2},
                                   {4, 9, 12, 9, 4},
                                   {5, 12, 15, 12, 5},
                                   {4, 9, 12, 9, 4},
                                   {2, 4, 5, 4, 2}};

            for (int i = 0; i < gaussMask.GetLength(0); i++)
            {
                for (int j = 0; j < gaussMask.GetLength(1); j++)
                {
                    gaussMask[i, j] /= 159;
                }
            }

            return gaussMask;
        }
        private static void CopyArray(float[,] output, float[,] input)
        {
            if (output.GetLength(0) != input.GetLength(0) || output.GetLength(1) != input.GetLength(1))
            {
                return;
            }

            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    output[i, j] = input[i, j];
                }
            }
        }
    }
}
