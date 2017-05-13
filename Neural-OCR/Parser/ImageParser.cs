﻿using AForge.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Neural_OCR.Network;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Neural_OCR.Parser
{
    public class ImageParser
    {

        private Mat _processedImage;
        private List<double> _pixelValues;



        public ImageParser()
        {
            _pixelValues = new List<double>();
        }

        public TeachingElement CreateTeachingElementFromImage(Bitmap image, int expectedDigit = 0)
        {
            _pixelValues.Clear();

            Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(image);
            _processedImage = imageCV.Mat;
            preprocessImage();
            removeBlankPlaces();
            resizeImage();
            addPixelValues();

            return new TeachingElement
            {
                Inputs = _pixelValues,
                ExpectedOutputs = ExpectedOutputFactory.getExpectedOutput(expectedDigit)
            };
        }


        public TeachingElement CreateTeachingElementFromImage(string path, int expectedDigit)
        {
            _pixelValues.Clear();

            loadImageFromFile(path);
            preprocessImage();
            removeBlankPlaces();
            resizeImage();
            addPixelValues();


            return new TeachingElement
            {
                Inputs = _pixelValues,
                ExpectedOutputs = ExpectedOutputFactory.getExpectedOutput(expectedDigit)
            };
        }


        private void loadImageFromFile(string path)
        {
            _processedImage = CvInvoke.Imread(path, LoadImageType.Grayscale);
        }


        private void preprocessImage()
        {
            CvInvoke.Threshold(_processedImage, _processedImage, 100, 255, ThresholdType.Binary);
        }

        private void removeBlankPlaces()
        {
            Shrink shrinkFilter = new Shrink(Color.FromArgb(255, 255, 255));
            ResizeNearestNeighbor resizeFilter = new ResizeNearestNeighbor(0, 0);

            Bitmap tempImage = shrinkFilter.Apply(_processedImage.Bitmap);

            // image dimenstoin
            int width = _processedImage.Width;
            int height = _processedImage.Height;
            // shrinked image dimension
            int tw = tempImage.Width;
            int th = tempImage.Height;
            // resize factors
            float fx = (float)width / (float)tw;
            float fy = (float)height / (float)th;

            if (fx > fy)
                fx = fy;
            // set new size of shrinked image
            int nw = (int)Math.Round(fx * tw);
            int nh = (int)Math.Round(fy * th);
            resizeFilter.NewWidth = nw;
            resizeFilter.NewHeight = nh;

            // resize image
            Bitmap tempImage2 = resizeFilter.Apply(tempImage);

            Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(tempImage2);

            _processedImage = imageCV.Mat;
        }


        private void resizeImage()
        {
            CvInvoke.Resize(_processedImage, _processedImage, new Size(7, 10), 0, 0, Inter.Linear);
            removeBlankPlaces();
            Emgu.CV.UI.ImageViewer.Show(_processedImage);
        }


        private void addPixelValues()
        {
            double value = 0;

            for (int i = 0; i < _processedImage.Bitmap.Height; i++)
            {
                for (int j = 0; j < _processedImage.Bitmap.Width; j++)
                {
                    value = _processedImage.Bitmap.GetPixel(j, i).B;

                    if (value > 200)
                    {
                        value = -1;
                    }
                    else
                    {
                        value = 1;
                    }
                    _pixelValues.Add(value);
                    value = 0;
                }
            }
        }

    }
}
