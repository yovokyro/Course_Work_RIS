using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace OBRLibrary
{
    public class Canny
    {
        private const float HIGHT_HRESHOLD = 80;
        private const float LOW_HRESHOLD = -65;

        private static float[,] _gY = {{1, 2, 1},
                        {0, 0, 0},
                        {-1, -2, -1}};

        private static float[,] _gX = {{-1, 0, 1},
                        {-2, 0, 2},
                        {-1, 0, 1}};

        public Canny()
        { }

        public byte[,] Calculation(Bitmap bitmap)
        {
            byte[,,] pixel_values = GetPixelValues(bitmap);
            float[,] data = GetGrayImage(pixel_values);
            pixel_values = null;

            data = GetConv(data, GetGauseMask());
            Gradients(data);
            SuppressionNoMaximum(data);

            (bool[,] hightMask, bool[,] mainMask) = GetDoubleThresholdFiltering(data, LOW_HRESHOLD, HIGHT_HRESHOLD);
            data = null;

            byte[,] result = GetTracking(hightMask, mainMask);
            return result;
        }

        public byte[,] CalculationMulty(Bitmap bitmap, int threadCount)
        {
            byte[,,] pixel_values = GetPixelValues(bitmap);
            float[,] data = GetGrayImage(pixel_values);
            pixel_values = null;

            CreateConvThreadPool(data, GetGauseMask(), threadCount);
            GradientsThread(data, threadCount);
            SuppressionNoMaximum(data);

            (bool[,] hightMask, bool[,] mainMask) = GetDoubleThresholdFiltering(data, LOW_HRESHOLD, HIGHT_HRESHOLD);
            byte[,] result = GetTracking(hightMask, mainMask);

            return result;
        }




        private float[,] GetGauseMask()
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

        public byte[,,] GetPixelValues(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[,,] data = new byte[width, height, 3];

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
                    data[x, y, 0] = values[idx + 2];
                    data[x, y, 1] = values[idx + 1];
                    data[x, y, 2] = values[idx];
                }
            }

            bitmap.UnlockBits(bitmapData);
            bitmapData = null;
            bitmap.Dispose();

            return data;
        }

        private float[,] GetGrayImage(byte[,,] pixelValues)
        {
            int width = pixelValues.GetLength(0);
            int height = pixelValues.GetLength(1);

            float[,] grayValues = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grayValues[x, y] = (float)Math.Round(0.2989 * pixelValues[x, y, 0] + 0.5870 * pixelValues[x, y, 1] + 0.1140 * pixelValues[x, y, 2]);
                }
            }

            return grayValues;
        }

        private float[,] GetConv(float[,] data, float[,] kernel)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);
            int kernelSize = kernel.GetLength(0);
            int padSize = kernelSize / 2;
            float[,] paddedImage = PadImage(data, padSize);
            float[,] result = new float[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int m = 0; m < kernelSize; m++)
                    {
                        for (int n = 0; n < kernelSize; n++)
                        {
                            result[i, j] += paddedImage[i + m, j + n] * kernel[m, n];
                        }
                    }
                }
            }

            paddedImage = null;

            return result;
        }

        private void Gradients(float[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);

            float[,] gradientY = GetConv(data, _gY);
            float[,] gradientX = GetConv(data, _gX);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    data[i, j] = (float)Math.Sqrt(Math.Pow(gradientX[i, j], 2) + Math.Pow(gradientY[i, j], 2));
                }
            }

            gradientY = null;
            gradientX = null;
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

        private byte[,] GetTracking(bool[,] highMask, bool[,] mainMask)
        {
            int height = highMask.GetLength(0);
            int width = highMask.GetLength(1);
            byte[,] result = new byte[height, width];

            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    if (mainMask[i, j] == false)
                    {
                        result[i, j] = (byte)(ContainsTrueNeighbor(highMask, i, j) ? 255 : 0);
                    }
                }
            }

            highMask = null;
            mainMask = null;

            return result;
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
        private void CreateConvThreadPool(float[,] pixelValues, float[,] kernel, int threadCount)
        {
            int workerThreads = 0;

            ThreadPool.SetMaxThreads(threadCount, 0);
            ThreadPool.GetMaxThreads(out workerThreads, out int _);

            int width = pixelValues.GetLength(0);
            int height = pixelValues.GetLength(1);

            int pixelCount = width * height;

            int iteration = workerThreads;
            int whole = pixelCount / workerThreads;
            int remainder = pixelCount % workerThreads;

            int kernelSize = kernel.GetLength(0);
            int padSize = kernelSize / 2;
            float[,] paddedImage = PadImage(pixelValues, padSize);

            if (whole > 4096)
            {
                whole = pixelCount / 4096;
                remainder = pixelCount % 4096;
                iteration = whole;
            }

            CountdownEvent countdownEvent = new CountdownEvent(iteration);
            List<ConvMultyParam> paramThreads = new List<ConvMultyParam>();
            for (int i = 0, countPrev = 0; i < iteration; i++)
            {
                int pixelsPerThread = whole + (i < remainder ? 1 : 0);
                paramThreads.Add(new ConvMultyParam(paddedImage, countPrev, pixelsPerThread, kernel, countdownEvent));
                ThreadPool.QueueUserWorkItem(new WaitCallback(СonvThread), paramThreads[i]);
                countPrev += pixelsPerThread;

            }

            countdownEvent.Wait();
            countdownEvent.Dispose();

            for (int i = 0; i < iteration; i++)
            {
                AddResultThread(paramThreads[i], pixelValues);
            }

            paramThreads.Clear();
        }

        private void СonvThread(object obj)
        {
            ConvMultyParam param = (ConvMultyParam)obj;

            int kernelSize = param.kernel.GetLength(0);
            int padSize = param.kernel.GetLength(0) / 2;

            int width = param.paddedImage.GetLength(0) - 2 * padSize;
            int height = param.paddedImage.GetLength(1) - 2 * padSize;

            int positionX = param.countPrev / height;
            int positionY = param.countPrev % height;

            int x = 0;
            for (int i = positionX; i < width && x != param.countThis; i++, positionY = 0)
            {
                for (int j = positionY; j < height && x != param.countThis; j++, x++)
                {
                    for (int m = 0; m < kernelSize; m++)
                    {
                        for (int n = 0; n < kernelSize; n++)
                        {
                            param.output[x] += param.paddedImage[i + m, j + n] * param.kernel[m, n];
                        }
                    }
                }
            }

            param.countdownEvent.Signal();
        }

        private void GradientsThread(float[,] data, int threadCount)
        {
            float[,] gradientY = new float[data.GetLength(0), data.GetLength(1)];
            float[,] gradientX = new float[data.GetLength(0), data.GetLength(1)];

            CopyArray(gradientX, data);
            CopyArray(gradientY, data);

            CreateConvThreadPool(gradientX, _gX, threadCount);
            CreateConvThreadPool(gradientY, _gY, threadCount);

            int width = data.GetLength(0);
            int height = data.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    data[i, j] = (float)Math.Sqrt(Math.Pow(gradientX[i, j], 2) + Math.Pow(gradientY[i, j], 2));
                }
            }

            gradientY = null;
            gradientX = null;
        }

        private void AddResultThread(ConvMultyParam param, float[,] data)
        {
            int positionX = param.countPrev / data.GetLength(1);
            int positionY = param.countPrev % data.GetLength(1);

            int x = 0;
            for (int i = positionX; i < data.GetLength(0) && x != param.countThis; i++, positionY = 0)
            {
                for (int j = positionY; j < data.GetLength(1) && x != param.countThis; j++, x++)
                {
                    data[i, j] = param.output[x];
                }
            }
        }

        ///FUNCTIONALNO
        private void CopyArray(float[,] output, float[,] input)
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
