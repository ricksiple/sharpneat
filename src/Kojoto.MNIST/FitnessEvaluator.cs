using System;
using System.IO;
using System.Collections.Generic;

using SharpNeat.Core;
using SharpNeat.Phenomes;

using Kojoto.MNIST.Database;

namespace Kojoto.MNIST
{
    public class FitnessEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        private ulong _EvalCount = 0;

        private double[,] _expected;
        private byte[,] _images;
        private int _sampleCount = 0;
        private int _imageSize = 0;

        public FitnessEvaluator(double[,] expected, byte[,] images)
        {
            _expected = expected;
            _images = images;

            if (_expected.GetLength(0) != _images.GetLength(0))
            {
                throw new Exception(string.Format("Image count {0} does not match label count {1}.", _expected.GetLength(0), _images.GetLength(0)));
            }

            _sampleCount = _expected.GetLength(0);
            _imageSize = _images.GetLength(1);
        }

        ulong IPhenomeEvaluator<IBlackBox>.EvaluationCount { get { return _EvalCount; } }

        bool IPhenomeEvaluator<IBlackBox>.StopConditionSatisfied { get { return false; } }

        void IPhenomeEvaluator<IBlackBox>.Reset() { return; }

        FitnessInfo IPhenomeEvaluator<IBlackBox>.Evaluate(IBlackBox phenome)
        {
            _EvalCount += 1;

            double totalScore = 0;

            ISignalArray inputArr = phenome.InputSignalArray;
            ISignalArray outputArr = phenome.OutputSignalArray;

            for (int sample = 0; sample < _sampleCount; sample += 1)
            {
                phenome.ResetState();

                for (int i = 0; i < _imageSize; i += 1)
                {
                    inputArr[i] = _images[sample, i];
                }

                phenome.Activate();

                if (!phenome.IsStateValid) { return FitnessInfo.Zero; }

                for (int o = 0; o < 10; o += 1)
                {
                    if (outputArr[o] < 0) { throw new Exception("Unexpected negative output."); }
                    totalScore += (1.0 - Math.Pow(_expected[sample, o] - outputArr[o], 2));
                }

            }

            return new FitnessInfo(totalScore, totalScore);
        }

        public static FitnessEvaluator Create()
        {
            byte[,] images;
            double[,] expected;

            var labelFile = new FileInfo("t10k-labels.idx1-ubyte");
            var imageFile = new FileInfo("t10k-images.idx3-ubyte");

            var src = new Database.Db(imageFile, labelFile);
            images = new byte[src.ImageCount, src.PixelCount];
            expected = new double[src.ImageCount, 10];

            using (src)
            {
                int i = -1;
                foreach (IRecord r in src)
                {
                    i += 1;
                    
                    expected[i, r.Label] = 1;
                    
                    for (int pxl=0; pxl<src.PixelCount; pxl+=1)
                    {
                        images[i, pxl] = r.Image[pxl];
                    }
                }
            }

            return new FitnessEvaluator(expected, images);
        }
    }

}
