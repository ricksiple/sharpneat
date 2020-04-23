using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Domains;

namespace Kojoto.MNIST
{
    public abstract class Experiment
    {
        // settings
        //private const int _SpecieCount = 10;
        private const int _SpecieCount = 2;
        private const int _InputCount = 784;
        private const int _OutputCount = 10;
        //private const int _PopulationSize = 150;
        private const int _PopulationSize = 20;
        private const string _ComplexityRegulationStr = "Absolute";
        private const int _ComplexityThreshold = 10;


        private NetworkActivationScheme _NetworkActivationScheme;
        private NeatGenomeDecoder _GenomeDecoder;
        private NeatGenomeParameters _GenomeParameters;
        private NeatGenomeFactory _GenomeFactory;
        private List<NeatGenome> _GenomeList;
        private ParallelOptions _ParallelOptions;
        private IPhenomeEvaluator<IBlackBox> _PhenomeEvaluator;
        private SelectiveGenomeListEvaluator<NeatGenome> _GenomeListEvaluator;
        private ISpeciationStrategy<NeatGenome> _SpeciationStrategy;
        private IComplexityRegulationStrategy _ComplexityRegulationStrategy;
        private NeatEvolutionAlgorithmParameters _EvolutionAlgorithmParameters;
        private NeatEvolutionAlgorithm<NeatGenome> _EvolutionAlgorithm;

        public void StartContinue()
        {
            _EvolutionAlgorithm.StartContinue();
        }

        public void RequestPauseAndWait()
        {
            _EvolutionAlgorithm.RequestPauseAndWait();
        }

        #region Overrides

        protected abstract NetworkActivationScheme ActivationScheme();

        protected abstract IActivationFunction ActivationFunction();

        protected abstract ParallelOptions ParallelOptions();

        protected abstract IPhenomeEvaluator<IBlackBox> PhenomeEvaluator();

        protected abstract ISpeciationStrategy<NeatGenome> SpeciationStrategy();

        protected abstract void UpdateEvent(uint CurrentGeneration, double maxFitness);

        #endregion

        #region Constructors

        public Experiment()
        {
            GetPhenomeEvaluator();
            GetActivationScheme();
            GetGenomeDecoder();
            GetGenomeParameters();
            GetGenomeFactory();
            GetGenomeList();
            GetParallelOptions();
            GetGenomeListEvaluator();
            GetSpeciationStrategy();
            GetComplexityRegulationStrategy();
            GetEvolutionAlgorithmParameters();
            GetEvolutionAlgorithm();
        }

        #endregion

        private void GetPhenomeEvaluator()
        {
            _PhenomeEvaluator = PhenomeEvaluator();
        }

        private void GetActivationScheme()
        {
            _NetworkActivationScheme = ActivationScheme();
        }

        private void GetGenomeDecoder()
        {
            _GenomeDecoder = new SharpNeat.Decoders.Neat.NeatGenomeDecoder(_NetworkActivationScheme);
        }

        private void GetGenomeParameters()
        {
            _GenomeParameters = new NeatGenomeParameters();
            _GenomeParameters.FeedforwardOnly = _NetworkActivationScheme.AcyclicNetwork;
            _GenomeParameters.ActivationFn = ActivationFunction();
        }

        private void GetGenomeFactory()
        {
            _GenomeFactory = new NeatGenomeFactory(_InputCount, _OutputCount, _GenomeParameters);
        }

        private void GetGenomeList()
        {
            _GenomeList = _GenomeFactory.CreateGenomeList(_PopulationSize, 0);
        }

        private void GetParallelOptions()
        {
            _ParallelOptions = ParallelOptions();
        }

        private void GetGenomeListEvaluator()
        {
            var innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(_GenomeDecoder, _PhenomeEvaluator, _ParallelOptions);
            _GenomeListEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                        innerEvaluator,
                                                                        SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
        }

        private void GetSpeciationStrategy()
        {
            _SpeciationStrategy = SpeciationStrategy();
        }

        private void GetComplexityRegulationStrategy()
        {
            _ComplexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_ComplexityRegulationStr, _ComplexityThreshold);
        }

        private void GetEvolutionAlgorithmParameters()
        {
            _EvolutionAlgorithmParameters = new SharpNeat.EvolutionAlgorithms.NeatEvolutionAlgorithmParameters();
            _EvolutionAlgorithmParameters.SpecieCount = _SpecieCount;
        }

        private void GetEvolutionAlgorithm()
        {
            _EvolutionAlgorithm = new NeatEvolutionAlgorithm<NeatGenome>(_EvolutionAlgorithmParameters, _SpeciationStrategy, _ComplexityRegulationStrategy);
            _EvolutionAlgorithm.Initialize(_GenomeListEvaluator, _GenomeFactory, _GenomeList);

            _EvolutionAlgorithm.UpdateEvent += (object sender, EventArgs e) => 
            {
                UpdateEvent(_EvolutionAlgorithm.CurrentGeneration, _EvolutionAlgorithm.Statistics._maxFitness);
            };

        }
    }
}
