using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Encog_NEAT_TicTacToe_Experiment
{
    /// <summary>
    /// Interaction logic for NeuralNetworkVisualizer.xaml
    /// </summary>
    public partial class NeuralNetworkVisualizer : Window
    {
        /// <summary>
        /// Initialize the NeuralNetworkVisualizer with the layer and synapses info
        /// </summary>
        /// <param name="layers"> { 10, 18, 2 } => The number of neurons per layer of the Neural Network. </param>
        /// <param name="synapses"> { { 0, 16, 0.2546 }, { 1, 24, -1.3584 } } => An array of neuronID`s and weights of the synapses of the Neural Netowork meaning the neuroniDIn, neuroniDOut and weight of the synapses of the Neural Network. </param>
        public NeuralNetworkVisualizer(int[] layers, double[][] synapses)
        {
            InitializeComponent();

            this.NeuralNetworkCanvas.Width = 600;
            this.NeuralNetworkCanvas.Height = 300;

            int numberOfHiddens = layers.Where((l, i) => i != 0 && i != layers.Length).Sum();

            int biggestLayer = layers.Max();
            int neuronRadius = (int)Math.Floor((this.NeuralNetworkCanvas.Height - 5 * (biggestLayer - 1)) / biggestLayer);

            // Draw input neurons and output neurons vertically and hidden neurons in a circle in the middle and keep track of the coordinates
            // where the synapses will connect to the left side of the neurons in a 2D-array
            int[][] synapseCoordinates = new int[layers.Sum()][];
            int neuronID = 0;
            for (int layerID = 0; layerID < layers.Length; layerID++)
            {
                for (int layerNeuronID = 0; layerNeuronID < layers[layerID]; layerNeuronID++)
                {
                    int neuronX = layerID == 0 ? 0 : layerID == layers.Length - 1 ? (int)Math.Round(this.NeuralNetworkCanvas.Width) - neuronRadius : (int)Math.Round((this.NeuralNetworkCanvas.Width - neuronRadius) / 2 + (this.NeuralNetworkCanvas.Height - neuronRadius) / 2 * Math.Cos(Math.PI * 2 / numberOfHiddens) - neuronRadius / 2);
                    int neuronY = layerID == 0 || layerID == layers.Length - 1 ? layerNeuronID * (neuronRadius + 5) : (int)Math.Round((this.NeuralNetworkCanvas.Height - neuronRadius) / 2 + (this.NeuralNetworkCanvas.Height - neuronRadius) / 2 * Math.Sin(Math.PI * 2 / numberOfHiddens));
                    synapseCoordinates[neuronID] = new int[] { neuronX, (int)Math.Round((double)neuronY + neuronRadius / 2) };

                    // Draw neuron
                    Ellipse neuron = new Ellipse
                    {
                        Width = neuronRadius,
                        Height = neuronRadius,
                        Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0))
                    };
                    Canvas.SetLeft(neuron, neuronX);
                    Canvas.SetTop(neuron, neuronY);
                    this.NeuralNetworkCanvas.Children.Add(neuron);

                    neuronID++;
                }
            }
            
            // Draw synapses with the weight represented as the line color and as text
            foreach(double[] synapse in synapses)
            {
                int neuronIDIn = (int)Math.Round(synapse[0]);
                neuronIDIn = neuronIDIn >= synapseCoordinates.Length ? synapseCoordinates.Length - 1 : neuronIDIn;
                int neuronIDOut = (int)Math.Round(synapse[1]);
                neuronIDOut = neuronIDOut >= synapseCoordinates.Length ? synapseCoordinates.Length - 1 : neuronIDOut;
                double weight = synapse[2];

                // Draw synapse line with the weight of the synapse represented as the line color
                Line synapseLine = new Line()
                {
                    X1 = synapseCoordinates[neuronIDIn][0] + neuronRadius,
                    Y1 = synapseCoordinates[neuronIDIn][1],
                    X2 = synapseCoordinates[neuronIDOut][0],
                    Y2 = synapseCoordinates[neuronIDOut][1],
                    Stroke = new SolidColorBrush(Color.FromScRgb(weight >= 1 || weight <= -1 ? 1 : (float)Math.Abs(weight), weight < 0 ? 1 : 0, weight > 0 ? 1 : 0, 0))
                };
                this.NeuralNetworkCanvas.Children.Add(synapseLine);

                // Draw weight text of synapse up to 4 decimal places
                TextBlock weightText = new TextBlock()
                {
                    Text = "" + Math.Round(weight, 4),
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
                };
                Canvas.SetLeft(weightText, (synapseLine.X1 + synapseLine.X2) / 2);
                Canvas.SetTop(weightText, (synapseLine.Y1 + synapseLine.Y2) / 2);
                this.NeuralNetworkCanvas.Children.Add(weightText);
            }
        }
    }
}