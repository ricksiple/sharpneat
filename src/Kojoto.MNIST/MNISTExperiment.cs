using System;
using System.Threading.Tasks;
using System.IO;

using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.Genomes.Neat;
using SharpNeat.SpeciationStrategies;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Domains;

namespace Kojoto.MNIST
{
    class MNISTExperiment : Experiment
    {
        private ParallelOptions _ParallelOptions = new ParallelOptions();

        public MNISTExperiment()
        {
            _UpdateEvent = _FirstUpdateEvent;
        }

        protected override IPhenomeEvaluator<IBlackBox> PhenomeEvaluator()
        {
            var labelFile = new FileInfo("train-labels.idx1-ubyte");
            var imageFile = new FileInfo("train-images.idx3-ubyte");

            return FitnessEvaluator.Create(labelFile, imageFile);
        }

        protected override NetworkActivationScheme ActivationScheme()
        {
            return NetworkActivationScheme.CreateAcyclicScheme();
        }

        protected override IActivationFunction ActivationFunction()
        {
            return LeakyReLU.__DefaultInstance;
        }

        protected override ParallelOptions ParallelOptions()
        {
            return _ParallelOptions;
        }

        protected override ISpeciationStrategy<NeatGenome> SpeciationStrategy()
        {
            return new ParallelKMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric(1.0, 0.0, 10.0), _ParallelOptions);

        }

        protected override void UpdateEvent(uint currentGeneration, double maxFitness)
        {
            _UpdateEvent(currentGeneration, maxFitness);
        }

        #region UpdateEvent

        private DateTime _StartTime;
        private DateTime _LastUpdate;
        private double _StartFitness;
        private double _LastFitness;
        private Action<uint, double> _UpdateEvent;

        private void _FirstUpdateEvent(uint currentGeneration, double maxFitness)
        {
            _StartTime = DateTime.Now;
            _LastUpdate = DateTime.Now;

            // don't write out generation 0
            //Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N0}", currentGeneration, maxFitness));

            _UpdateEvent = _SecondUpdateEvent;
        }

        private void _SecondUpdateEvent(uint currentGeneration, double maxFitness)
        {
            _LastUpdate = DateTime.Now;
            _StartFitness = maxFitness;
            _LastFitness = maxFitness;

            Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N0}", currentGeneration, maxFitness));

            _UpdateEvent = _OtherUpdateEvent;
        }

        private void _OtherUpdateEvent(uint currentGeneration, double maxFitness)
        {
            TimeSpan last = DateTime.Now - _LastUpdate;
            TimeSpan total = DateTime.Now - _StartTime;

            double pct = maxFitness / 600000.0;
            double deltaFitness = maxFitness - _StartFitness;
            double remainFitness = 600000 - maxFitness;

            TimeSpan remainTime = new TimeSpan((long)(remainFitness * (double)total.Ticks / deltaFitness));

            Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N0}{6} percent={2:P1} lastTime={3} totalTime={4} remainTime={5}"
                , currentGeneration
                , maxFitness
                , pct
                , last.ToString(@"d\.hh\:mm\:ss")
                , total.ToString(@"d\.hh\:mm\:ss")
                , remainTime.ToString(@"d\.hh\:mm\:ss")
                , (maxFitness - _LastFitness).ToString("+#;-#;+0")
                ));

            _LastUpdate = DateTime.Now;
            _LastFitness = maxFitness;
        }

        #endregion

    }
}
