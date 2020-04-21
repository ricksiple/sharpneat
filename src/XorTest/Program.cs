using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int specieCount = 10;
            const int inputCount = 2;
            const int outputCount = 1;
            const int populationSize = 150;
            const string complexityRegulationStr = "Absolute";
            const int complexityThreshold = 10;

            var activationScheme = SharpNeat.Decoders.NetworkActivationScheme.CreateAcyclicScheme();

            var eaParams = new SharpNeat.EvolutionAlgorithms.NeatEvolutionAlgorithmParameters();
            eaParams.SpecieCount = specieCount;

            var neatGenomeParams = new SharpNeat.Genomes.Neat.NeatGenomeParameters();
            neatGenomeParams.FeedforwardOnly = activationScheme.AcyclicNetwork;
            neatGenomeParams.ActivationFn = SharpNeat.Network.LeakyReLU.__DefaultInstance;

            var genomeFactory = new SharpNeat.Genomes.Neat.NeatGenomeFactory(inputCount, outputCount, neatGenomeParams);

            // Create an initial population of randomly generated genomes.
            var genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

            //_ea = experiment.CreateEvolutionAlgorithm(_genomeFactory, _genomeList);

            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weight difference.
            SharpNeat.Core.IDistanceMetric distanceMetric = new SharpNeat.DistanceMetrics.ManhattanDistanceMetric(1.0, 0.0, 10.0);

            var parallelOptions = new ParallelOptions();
            var speciationStrategy = new SharpNeat.SpeciationStrategies.ParallelKMeansClusteringStrategy<SharpNeat.Genomes.Neat.NeatGenome>(distanceMetric, parallelOptions);

            // Create complexity regulation strategy.
            var complexityRegulationStrategy = SharpNeat.Domains.ExperimentUtils.CreateComplexityRegulationStrategy(complexityRegulationStr, complexityThreshold);

            // Create the evolution algorithm.
            var ea = new SharpNeat.EvolutionAlgorithms.NeatEvolutionAlgorithm<SharpNeat.Genomes.Neat.NeatGenome>(eaParams, speciationStrategy, complexityRegulationStrategy);

            // Create IBlackBox evaluator.
            var evaluator = new SharpNeat.Domains.XorBlackBoxEvaluator();
            
            // Create genome decoder.
            var genomeDecoder = new SharpNeat.Decoders.Neat.NeatGenomeDecoder(activationScheme);

            // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
            var innerEvaluator = new SharpNeat.Core.ParallelGenomeListEvaluator<SharpNeat.Genomes.Neat.NeatGenome, SharpNeat.Phenomes.IBlackBox>(genomeDecoder, evaluator, parallelOptions);

            // Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
            // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
            var selectiveEvaluator = new SharpNeat.Core.SelectiveGenomeListEvaluator<SharpNeat.Genomes.Neat.NeatGenome>(
                                                                                    innerEvaluator,
                                                                                    SharpNeat.Core.SelectiveGenomeListEvaluator<SharpNeat.Genomes.Neat.NeatGenome>.CreatePredicate_OnceOnly());
            // Initialize the evolution algorithm.
            ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);

            ea.UpdateEvent += (object sender, EventArgs e) =>
            {
                Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N6}", ea.CurrentGeneration, ea.Statistics._maxFitness));
            };
            ea.StartContinue();

            Console.WriteLine("Press ENTER to exit the application.");
            Console.ReadLine();
        }
    }
}
