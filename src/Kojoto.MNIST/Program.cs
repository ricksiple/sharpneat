using System;
using System.Threading.Tasks;
using System.IO;

namespace Kojoto.MNIST
{
    class Program
    {

        static void Main(string[] args)
        {
            MNISTExperiment exp = new MNISTExperiment();
            
            exp.LoadPopulation();
            exp.StartContinue();

            Console.WriteLine("Press <ENTER> to stop...");
            Console.ReadLine();

            exp.RequestPauseAndWait();
            exp.SavePopulation();
            
            Console.WriteLine("Press <ENTER> to exit...");
            Console.ReadLine();
        }

        static void MainOld(string[] args)
        {
            const int specieCount = 10;
            const int inputCount = 784;
            const int outputCount = 10;
            const int populationSize = 150;
            const string complexityRegulationStr = "Absolute";
            const int complexityThreshold = 10;

            var labelFile = new FileInfo("t10k-labels.idx1-ubyte");
            var imageFile = new FileInfo("t10k-images.idx3-ubyte");
            var evaluator = FitnessEvaluator.Create(labelFile, imageFile);
            // NOTE:
            // maximum achievable fitness score on any given sample is 10.
            // the maximum overall fitness score is, therefore, 10 times the number of samples.

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
