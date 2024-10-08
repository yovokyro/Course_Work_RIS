using System.Threading;

namespace OBRLibrary
{
    public class ConvMultyParam
    {
        public float[,] paddedImage;
        public int countPrev;
        public int countThis;
        public float[,] kernel;

        public float[] output;
        public CountdownEvent countdownEvent;

        public ConvMultyParam(float[,] paddedImage, int countPrev, int countThis, float[,] kernel, CountdownEvent countdownEvent)
        {
            this.paddedImage = paddedImage;
            this.countPrev = countPrev;
            this.countThis = countThis;
            this.kernel = kernel;
            this.countdownEvent = countdownEvent;

            output = new float[this.countThis];
        }
    }
}
