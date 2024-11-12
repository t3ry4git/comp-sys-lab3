namespace comp_sys_lab3
{
    public class Neuron
    {
        public List<double> Weights { get; }
        public double Bias { get; }
        public double Output { get; private set; }

        public Neuron(int inputCount)
        {
            var random = new Random();
            Weights = [];

            // Randomly generates weights and bias
            for (int i = 0; i < inputCount; i++)
                Weights.Add((random.NextDouble() * 2) - 1);

            Bias = (random.NextDouble() * 2) - 1 + 0.1;
        }

        public override string ToString() => $"Weights: {string.Join(", ", Weights)}\nBias: {Bias}";

        public double Activate(List<double> inputs)
        {
            if (inputs.Count != Weights.Count)
                throw new ArgumentException("Inputs count have to be equal to weights count");

            double sum = 0.0;

            for (int i = 0; i < inputs.Count; i++)
                sum += inputs[i] * Weights[i];

            sum += Bias;
            Output = LeakyReLU(sum);
            return Output;
        }

        private static double LeakyReLU(double x) => x > 0 ? x : 0.01 * x;

    }

    public class Layer
    {
        public List<Neuron> Neurons { get; }

        public Layer(int neuronCount, int inputsPerNeuron)
        {
            Neurons = [];

            for (int i = 0; i < neuronCount; i++)
                Neurons.Add(new Neuron(inputsPerNeuron));
        }

        public List<double> Activate(List<double> inputs) => (from neuron in Neurons
                                                              select neuron.Activate(inputs)).ToList();
    }

    public class NeuralNetwork
    {
        public List<Layer> Layers { get; }
        public int iter = 0;


        public NeuralNetwork(int[] layersStructure)
        {
            Layers = [];

            for (int i = 0; i < layersStructure.Length - 1; i++)
                Layers.Add(new Layer(layersStructure[i + 1], layersStructure[i]));
        }

        public List<Neuron> GetAllNeurons()
        {
            List<Neuron> allNeurons = [];

            foreach (var layer in Layers)
                allNeurons.AddRange(layer.Neurons);

            return allNeurons;
        }

        public List<List<double>> FeedForwardWithLayerOutputs(List<double> inputs)
        {
            List<List<double>> outputsPerLayer = [];
            List<double> outputs = inputs;
            iter++;

            foreach (var layer in Layers)
            {
                outputs = layer.Activate(outputs);
                outputsPerLayer.Add(new List<double>(outputs));
            }

            return outputsPerLayer;
        }

    }
}
