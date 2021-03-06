﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.VirtualVacation
{
    // Based on the algorithms defined in: http://www.cs.bris.ac.uk/~reinhard/papers/colourtransfer.pdf

    class ColorTransfer
    {
        public class Point3D
        {
            public float l = 0.0f;
            public float m = 0.0f;
            public float s = 0.0f;
        };

        static float Sqrt3 = 1.73205080757f;
        static float Sqrt6 = 2.44948974278f;
        static float Sqrt2 = 1.41421356237f;
        static float OneOver255 = 1.0f / 255.0f;

        public static void UnitTest()
        {
            byte[] testPixels = new byte[6 * 4];
            testPixels[0] = 255;
            testPixels[1] = 255;
            testPixels[2] = 255;
            testPixels[3] = 255;

            testPixels[4] = 255;
            testPixels[5] = 0;
            testPixels[6] = 0;
            testPixels[7] = 255;

            testPixels[8] = 0;
            testPixels[9] = 255;
            testPixels[10] = 0;
            testPixels[11] = 255;

            testPixels[12] = 0;
            testPixels[13] = 0;
            testPixels[14] = 255;
            testPixels[15] = 255;

            testPixels[16] = 0;
            testPixels[17] = 0;
            testPixels[18] = 255;
            testPixels[19] = 255;

            testPixels[20] = 0;
            testPixels[21] = 0;
            testPixels[22] = 0;
            testPixels[23] = 255;

            for (int i = 0; i < testPixels.Length; )
            {
                byte r, g, b, r2,g2,b2;
                float l, alpha, beta;
                Point3D lms = new Point3D();
                Point3D lms2 = new Point3D();
                RGBtoLMS(testPixels[i + 2], testPixels[i + 1], testPixels[i], ref lms);
                LMSToDecorrellated(ref lms, out l, out alpha, out beta);
                DecorrellatedToLMS(l, alpha, beta, ref lms2);
                LMSToRGB(ref lms, out r, out g, out b);
                LMSToRGB(ref lms2, out r2, out g2, out b2);

                if (!(r == testPixels[i + 2] && g == testPixels[i + 1] && b == testPixels[i]))
                    System.Diagnostics.Debug.WriteLine("Failed RGBtoLMStoRGB" + i);
                if (!(r2 == testPixels[i + 2] && g2 == testPixels[i + 1] && b2 == testPixels[i]))
                    System.Diagnostics.Debug.WriteLine("Failed RGBtoLMStoDecorrToLMStoRGB" + i);

                float distanceLMS = Math.Abs(lms.l - lms2.l) + Math.Abs(lms.m - lms2.m) + Math.Abs(lms.s - lms2.s);
                if (!(distanceLMS < 0.001f))
                    System.Diagnostics.Debug.WriteLine("Failed LMStoDecorrToLMS" + i);
                i += 4;
            }

            byte[] copyPixels = new byte[testPixels.Length];
            Array.Copy(testPixels, copyPixels, testPixels.Length);

            Point3D[] decorrellatedPixels = new Point3D[testPixels.Length / 4];
            for (int i = 0; i < decorrellatedPixels.Length; i++)
                decorrellatedPixels[i] = new Point3D();
            Point3D mean;
            Point3D stdDev;
            ComputeDecorrelation(testPixels, 4, ref decorrellatedPixels, out mean, out stdDev);
            TransferColor(mean, stdDev, mean, stdDev, ref copyPixels, 4, ref decorrellatedPixels);

            for (int i = 0; i < testPixels.Length; i++)
            {
                if (!(testPixels[i] == copyPixels[i]))
                    System.Diagnostics.Debug.WriteLine("Failed array copy " + i);
            }
        }

        public static void TransferColor(Point3D oldMean, Point3D oldStdDev, Point3D newMean, Point3D newStdDev, ref byte[] pixels, int bpp, ref Point3D[] decorrelatedValues)
        {
            float lScale = newStdDev.l / oldStdDev.l;
            float alphaScale = newStdDev.m / oldStdDev.m;
            float betaScale = newStdDev.s / oldStdDev.s;

            int index = 0;
            Point3D decorrelatedValue = new Point3D();
            Point3D lms = new Point3D();
            for (int i = 0; i < pixels.Length; )
            {
                byte b = pixels[i + 0];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                if (a == 0)
                {
                    i += bpp;
                    index++;
                    continue;
                }

                decorrelatedValue.l = decorrelatedValues[index].l;
                decorrelatedValue.m = decorrelatedValues[index].m;
                decorrelatedValue.s = decorrelatedValues[index].s;
                decorrelatedValue.l -= oldMean.l;
                decorrelatedValue.m -= oldMean.m;
                decorrelatedValue.s -= oldMean.s;
                decorrelatedValue.l *= lScale;
                decorrelatedValue.m *= alphaScale;
                decorrelatedValue.s *= betaScale;
                decorrelatedValue.l += newMean.l;
                decorrelatedValue.m += newMean.m;
                decorrelatedValue.s += newMean.s;

                DecorrellatedToLMS(decorrelatedValue.l, decorrelatedValue.m, decorrelatedValue.s, ref lms);
                LMSToRGB(ref lms, out r, out g, out b);

                pixels[i + 2] = r;
                pixels[i + 1] = g;
                pixels[i + 0] = b;
                i += bpp;
                index++;
            }

        }

        public static void ComputeDecorrelation(byte[] pixels, int bpp, ref Point3D[] decorrelatedPixels, out Point3D mean, out Point3D stdDev)
        {
            mean = new Point3D();
            mean.l = 0.0f;
            mean.m = 0.0f;
            mean.s = 0.0f;

            stdDev = new Point3D();
            stdDev.l = 0.0f;
            stdDev.m = 0.0f;
            stdDev.s = 0.0f;

            float l, alpha, beta;

            float weightSum = 0.0f;

            int numDecorrelatedPixels = pixels.Length / bpp;
            
            // First, compute the mean
            int index = 0;
            float weight = 0.0f;
            Point3D tempLMS = new Point3D();
            for (int i = 0; i < pixels.Length; )
            {
                byte b = pixels[i + 0];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                if (a == 0)
                {
                    i+=bpp;
                    decorrelatedPixels[index].l = 0.0f;
                    decorrelatedPixels[index].m = 0.0f;
                    decorrelatedPixels[index].s = 0.0f;
                    index++;
                    continue;
                }

                RGBtoLMS(r, g, b, ref tempLMS);
                LMSToDecorrellated(ref tempLMS, out l, out alpha, out beta);
                weight = 1.0f;//OneOver255*(float)a;
                decorrelatedPixels[index].l = l;
                decorrelatedPixels[index].m = alpha;
                decorrelatedPixels[index].s = beta;

                mean.l += l * weight;
                mean.m += alpha * weight;
                mean.s += beta * weight;

                weightSum += weight;
                i += bpp;
                index++;
            }

            if (weightSum > 0.0f)
            {
                mean.l /= weightSum;
                mean.m /= weightSum;
                mean.s /= weightSum;
            }

            // Now compute std deviation
            index = 0;
            for (int i = 0; i < pixels.Length; )
            {
                byte a = pixels[i + 3];

                if (a == 0)
                {
                    i += bpp;
                    index++;
                    continue;
                }

                l = decorrelatedPixels[index].l;
                alpha = decorrelatedPixels[index].m;
                beta = decorrelatedPixels[index].s;

                weight = 1.0f;//OneOver255 * (float)a;

                l -= mean.l;
                alpha -= mean.m;
                beta -= mean.s;

                stdDev.l += l * l * weight;
                stdDev.m += alpha * alpha * weight;
                stdDev.s += beta * beta * weight;

                i += bpp;
                index++;
            }

            if (weightSum > 0.0f)
            {
                stdDev.l /= weightSum;
                stdDev.m /= weightSum;
                stdDev.s /= weightSum;

                stdDev.l = (float)Math.Sqrt(stdDev.l);
                stdDev.m = (float)Math.Sqrt(stdDev.m);
                stdDev.s = (float)Math.Sqrt(stdDev.s);
            }

        }

        static void RGBtoLMS(byte r, byte g, byte b, ref Point3D lms)
        {
            float rf = (float)Math.Max(r * OneOver255,0.0000001f);
            float gf = (float)Math.Max(g * OneOver255,0.0000001f);
            float bf = (float)Math.Max(b * OneOver255, 0.0000001f);

            lms.l = (float)Math.Log10((double)/*Math.Max(0.1f,*/(0.3811f * rf + 0.5783f * gf + 0.0402f * bf));
            lms.m = (float)Math.Log10((double)/*Math.Max(0.1f,*/(0.1967f * rf + 0.7244f * gf + 0.0782f * bf));
            lms.s = (float)Math.Log10((double)/*Math.Max(0.1f,*/(0.0241f * rf + 0.1288f * gf + 0.8444f * bf));
        }

        static void LMSToDecorrellated(ref Point3D lms, out float l, out float alpha, out float beta)
        {
            l = (lms.l + lms.m + lms.s) / Sqrt3;
            alpha = (lms.l + lms.m - 2.0f * lms.s) / Sqrt6;
            beta = (lms.l - lms.m) / Sqrt2;
        }

        static void DecorrellatedToLMS(float l, float alpha, float beta, ref Point3D lms)
        {
            float l2 = l * (Sqrt3/3.0f);
            float alpha2 = alpha * (Sqrt6/6.0f);
            float beta2 = beta * (Sqrt2/2.0f);

            lms.l = l2 + alpha2 + beta2;
            lms.m = l2 + alpha2 - beta2;
            lms.s = l2 - 2.0f * alpha2;
        }

        static float Clamp(float value, float min, float max)
        {
            return value > max ? max : (value < min ? min : value);
        }

        static void LMSToRGB(ref Point3D lms, out byte r, out byte g, out byte b)
        {
            float l2 = (float)Math.Pow(10.0f, lms.l);
            float m2 = (float)Math.Pow(10.0f, lms.m);
            float s2 = (float)Math.Pow(10.0f, lms.s);

            r = (byte)Clamp(255.0f * (4.46790f * l2 - 3.5873f * m2 + 0.1193f * s2) + 0.5f, 0.0f, 255.0f);
            g = (byte)Clamp(255.0f * (-1.2186f * l2 + 2.3809f * m2 - 0.1624f * s2) + 0.5f, 0.0f, 255.0f);
            b = (byte)Clamp(255.0f * (0.04970f * l2 - 0.2439f * m2 + 1.2045f * s2) + 0.5f, 0.0f, 255.0f);
        }
    }
}
