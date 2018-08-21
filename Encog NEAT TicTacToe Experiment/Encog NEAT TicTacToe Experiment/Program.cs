using Encog.ML.Data.Basic;
using Encog.ML.EA.Genome;
using Encog.ML.EA.Species;
using Encog.ML.EA.Train;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using System;
using System.Linq;
using Encog.ML;
using Encog.Neural.NEAT.Training;
using Encog.Engine.Network.Activation;
using Encog.Util.Obj;

namespace Encog_NEAT_TicTacToe_Experiment
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            NEATPopulation XORPopulation = new NEATPopulation(2, 1, 1000);
            XORPopulation.Reset();
            XORPopulation.InitialConnectionDensity = 1;

            int generationID = 1;
            while(XORPopulation.Species.SelectMany(s => s.Members).Count(m => m.Score > 0) == 0 || Math.Round(XORPopulation.Species.SelectMany(s => s.Members).Where(m => m.Score > 0).Max(m => m.Score), 4) < 4)
            {
                Console.WriteLine("-----GENERATION " + generationID + "-----");

                foreach(IGenome xorMember in XORPopulation.Species.SelectMany(s => s.Members))
                {
                    double[] xorResults1 = new double[1];
                    double[] xorResults2 = new double[1];
                    double[] xorResults3 = new double[1];
                    double[] xorResults4 = new double[1];
                    ((NEATNetwork)XORPopulation.CODEC.Decode(xorMember)).Compute(new BasicMLData(new double[] { 0, 0 })).CopyTo(xorResults1, 0, 1);
                    ((NEATNetwork)XORPopulation.CODEC.Decode(xorMember)).Compute(new BasicMLData(new double[] { 0, 1 })).CopyTo(xorResults2, 0, 1);
                    ((NEATNetwork)XORPopulation.CODEC.Decode(xorMember)).Compute(new BasicMLData(new double[] { 1, 0 })).CopyTo(xorResults3, 0, 1);
                    ((NEATNetwork)XORPopulation.CODEC.Decode(xorMember)).Compute(new BasicMLData(new double[] { 1, 1 })).CopyTo(xorResults4, 0, 1);
                    double xorResult1 = xorResults1[0] < 0.5 ? 0 : xorResults1[0];
                    double xorResult2 = xorResults2[0] >= 0.5 ? 1 : xorResults2[0];
                    double xorResult3 = xorResults3[0] >= 0.5 ? 1 : xorResults3[0];
                    double xorResult4 = xorResults4[0] < 0.5 ? 0 : xorResults4[0];

                    double fitness = 4 - Math.Abs(0 - xorResult1) - Math.Abs(1 - xorResult2) - Math.Abs(1 - xorResult3) - Math.Abs(0 - xorResult4);
                    xorMember.Score = fitness;
                }

                IGenome[] xorMembers = XORPopulation.Species.SelectMany(s => s.Members).Where(m => m.Score > 0).ToArray();
                Console.WriteLine(" XOR fitness (max): " + Math.Round(xorMembers.Max(m => m.Score), 4));
                Console.WriteLine("     XOR fitness (avg): " + Math.Round(xorMembers.Average(m => m.Score), 4));
                Console.WriteLine("         XOR fitness (min): " + Math.Round(xorMembers.Min(m => m.Score), 4));

                IGenome bestXOR = xorMembers.FirstOrDefault(g => g.Score == xorMembers.Max(s => s.Score));
                if(Math.Round(bestXOR.Score, 4) >= 4.1 && ((NEATGenome)bestXOR).NeuronsChromosome.Count(c => c.NeuronType == NEATNeuronType.Hidden) > 0)
                {
                    NEATGenome bestNetwork = (NEATGenome)bestXOR;
                    int[] layers = new int[] { bestNetwork.InputCount + 1, bestNetwork.NeuronsChromosome.Count(c => c.NeuronType == NEATNeuronType.Hidden), bestNetwork.OutputCount };
                    double[][] synapses = new double[][] { };
                    foreach(NEATLinkGene synapse in bestNetwork.LinksChromosome.Where(s => s.Enabled))
                    {
                        synapses = synapses.Concat(new double[][] { new double[] { synapse.FromNeuronId, synapse.ToNeuronId, synapse.Weight } }).ToArray();
                    }

                    NeuralNetworkVisualizer networkWindow = new NeuralNetworkVisualizer(layers, synapses)
                    {
                        Title = "Best XOR"
                    };
                    networkWindow.ShowDialog();
                }
                
                TrainEA XORTrainer = NEATUtil.ConstructNEATTrainer(XORPopulation, new CalculateScore());
                XORTrainer.Iteration();

                generationID++;
            }

            //// Instantiate NEATPopulations
            //    NEATPopulation PickNumberPopulation = new NEATPopulation(1, 1, 100);
            //    PickNumberPopulation.Reset();
            //    PickNumberPopulation.InitialConnectionDensity = 1;
            //    NEATPopulation GuessNumberPopulation = new NEATPopulation(3, 1, 100);
            //    GuessNumberPopulation.Reset();
            //    GuessNumberPopulation.InitialConnectionDensity = 1;

            //int generationID = 1;
            //while(PickNumberPopulation.Species.SelectMany(s => s.Members).Count(m => m.Score > 0) == 0 || PickNumberPopulation.Species.SelectMany(s => s.Members).Where(m => m.Score > 0).Max(m => m.Score) < 99.9 || GuessNumberPopulation.Species.SelectMany(s => s.Members).Count(m => m.Score > 0) == 0 || GuessNumberPopulation.Species.SelectMany(s => s.Members).Where(m => m.Score > 0).Max(m => m.Score) < 99.9)
            //{
            //    Console.WriteLine("-----GENERATION " + generationID + "-----");

            //    // Calculate fitnesses
            //        foreach (ISpecies pickSpecies in PickNumberPopulation.Species)
            //        {
            //            foreach (IGenome pickMember in pickSpecies.Members)
            //            {
            //                double[] pickNumberResults = new double[1];
            //                ((NEATNetwork)PickNumberPopulation.CODEC.Decode(pickMember)).Compute(new BasicMLData(new double[] { 1 })).CopyTo(pickNumberResults, 0, 1);
            //                double pickNumberResult = Math.Round(pickNumberResults[0], 2);

            //                foreach (ISpecies guessSpecies in GuessNumberPopulation.Species)
            //                {
            //                    foreach (IGenome guessMember in guessSpecies.Members)
            //                    {
            //                        double higher = 0;
            //                        double lower = 0;
            //                        double guessNumberResult = 0;
            //                        int guesses = 1;

            //                        while (Math.Round(guessNumberResult, 2) != pickNumberResult && guesses <= 100)
            //                        {
            //                            double[] guessNumberResults = new double[1];
            //                            ((NEATNetwork)GuessNumberPopulation.CODEC.Decode(guessMember)).Compute(new BasicMLData(new double[] { higher, lower, guessNumberResult })).CopyTo(guessNumberResults, 0, 1);
            //                            guessNumberResult = guessNumberResults[0];
            //                            higher = guessNumberResult < pickNumberResult ? 1 : 0;
            //                            lower = guessNumberResult > pickNumberResult ? 1 : 0;
            //                            guesses++;
            //                        }

            //                        guessMember.Score = -1.01 * guesses + 102.01 + (guessMember.Score > 0 ? guessMember.Score : 0);
            //                        pickMember.Score = 1.01 * guesses - 2.02 + (pickMember.Score > 0 ? pickMember.Score : 0);
            //                    }
            //                }
            //            }
            //        }
            //        foreach (ISpecies guessSpecies in GuessNumberPopulation.Species)
            //        {
            //            foreach (IGenome guessMember in guessSpecies.Members)
            //            {
            //                guessMember.Score /= 100;
            //            }
            //        }
            //        IGenome[] guessMembers = GuessNumberPopulation.Species.SelectMany(s => s.Members).Where(m => m.Score > 0).ToArray();
            //        Console.WriteLine(" Guess fitness (max): " + Math.Round(guessMembers.Max(m => m.Score), 4));
            //        Console.WriteLine("     Guess fitness (avg): " + Math.Round(guessMembers.Average(m => m.Score), 4));
            //        Console.WriteLine("         Guess fitness (min): " + Math.Round(guessMembers.Min(m => m.Score), 4));

            //        IGenome bestGuesser = guessMembers.FirstOrDefault(g => g.Score == guessMembers.Max(s => s.Score));
            //        if(bestGuesser.Score >= 80 && ((NEATGenome)bestGuesser).NeuronsChromosome.Count(c => c.NeuronType == NEATNeuronType.Hidden) > 0)
            //        {
            //            NEATGenome bestNetwork = (NEATGenome)bestGuesser;
            //            int[] layers = new int[] { bestNetwork.InputCount + 1, bestNetwork.NeuronsChromosome.Count(c => c.NeuronType == NEATNeuronType.Hidden), bestNetwork.OutputCount };
            //            double[][] synapses = new double[][] { };
            //            foreach(NEATLinkGene synapse in bestNetwork.LinksChromosome.Where(s => s.Enabled))
            //            {
            //                synapses = synapses.Concat(new double[][] { new double[] { synapse.FromNeuronId, synapse.ToNeuronId, synapse.Weight } }).ToArray();
            //            }

            //            NeuralNetworkVisualizer networkWindow = new NeuralNetworkVisualizer(layers, synapses)
            //            {
            //                Title = "Best Guesser"
            //            };
            //            networkWindow.ShowDialog();
            //        }

            //        foreach (ISpecies pickSpecies in PickNumberPopulation.Species)
            //        {
            //            foreach (IGenome pickMember in pickSpecies.Members)
            //            {
            //                pickMember.Score /= 100;
            //            }
            //        }
            //        IGenome[] pickMembers = PickNumberPopulation.Species.SelectMany(s => s.Members).Where(m => m.Score > 0).ToArray();
            //        Console.WriteLine(" Pick  fitness (max): " + Math.Round(pickMembers.Max(m => m.Score), 4));
            //        Console.WriteLine("     Pick  fitness (avg): " + Math.Round(pickMembers.Average(m => m.Score), 4));
            //        Console.WriteLine("         Pick  fitness (min): " + Math.Round(pickMembers.Min(m => m.Score), 4));

            //        //IGenome bestPicker = pickMembers.FirstOrDefault(p => p.Score == pickMembers.Max(s => s.Score));
            //        //if (bestPicker.Score >= 95)
            //        //{
            //        //    NEATGenome bestNetwork = (NEATGenome)bestPicker;
            //        //    int[] layers = new int[] { bestNetwork.InputCount + 1, bestNetwork.NeuronsChromosome.Count(c => c.NeuronType == NEATNeuronType.Hidden), bestNetwork.OutputCount };
            //        //    double[][] synapses = new double[][] { };
            //        //    foreach (NEATLinkGene synapse in bestNetwork.LinksChromosome.Where(s => s.Enabled))
            //        //    {
            //        //        synapses = synapses.Concat(new double[][] { new double[] { synapse.FromNeuronId, synapse.ToNeuronId, synapse.Weight } }).ToArray();
            //        //    }

            //        //    NeuralNetworkVisualizer networkWindow = new NeuralNetworkVisualizer(layers, synapses)
            //        //    {
            //        //        Title = "Best Picker"
            //        //    };
            //        //    networkWindow.ShowDialog();
            //        //}

            //    // Train NEATPopulations
            //        TrainEA pickNumberTrainer = NEATUtil.ConstructNEATTrainer(PickNumberPopulation, new CalculateScore());
            //        TrainEA guessNumberTrainer = NEATUtil.ConstructNEATTrainer(GuessNumberPopulation, new CalculateScore());
            //        pickNumberTrainer.Iteration();
            //        guessNumberTrainer.Iteration();

            //    generationID++;
            //}

            Console.ReadKey();
        }
    }

    public class CalculateScore : ICalculateScore
    {
        public bool ShouldMinimize
        {
            get
            {
                return true;
            }
        }

        public bool RequireSingleThreaded
        {
            get
            {
                return false;
            }
        }

        double ICalculateScore.CalculateScore(IMLMethod network)
        {
            return 0;
        }
    }
}