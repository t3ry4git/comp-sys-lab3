using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace comp_sys_lab3
{
    public partial class MainWindow : Window
    {
        private NeuralNetwork network;
        private readonly Random random;
        private readonly List<int> layerStructure;

        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            layerStructure = [3, 2, 1];
            network = new NeuralNetwork([.. layerStructure]);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => DrawNetwork(network.GetAllNeurons());

        private void InitializeNetwork() => network = new NeuralNetwork([.. layerStructure]);

        private void DrawNetwork(List<Neuron> neurons, List<double>? inputs = null)
        {
            NetworkCanvas.Children.Clear();

            double canvasWidth = NetworkCanvas.ActualWidth;
            double canvasHeight = NetworkCanvas.ActualHeight;
            double layerSpacing = canvasWidth / (layerStructure.Count + 1);
            double nodeRadius = 15;

            int neuronIndex = 0;
            int inputIndex = 0;
            List<Point>? previousLayerPositions = null;

            for (int i = 0; i < layerStructure.Count; i++)
            {
                int nodeCount = layerStructure[i];
                double nodeSpacing = canvasHeight / (nodeCount + 1);

                List<Point> currentLayerPositions = [];

                for (int j = 0; j < nodeCount; j++)
                {
                    double x = layerSpacing * (i + 1);
                    double y = nodeSpacing * (j + 1);

                    Ellipse node = new()
                    {
                        Width = nodeRadius * 2,
                        Height = nodeRadius * 2,
                        Fill = Brushes.Gray,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };

                    if (neuronIndex < neurons.Count && previousLayerPositions != null)
                    {
                        node.ToolTip = neurons[neuronIndex].ToString();
                        neuronIndex++;
                    }
                    else
                    {
                        if (previousLayerPositions == null)
                        {
                            if (inputs != null)
                            {
                                node.ToolTip = $"Input{inputIndex}: {inputs[inputIndex]}";
                                inputIndex++;
                            }
                            else
                            {
                                node.ToolTip = "Input empty";
                            }
                        }
                    }

                    Canvas.SetLeft(node, x - nodeRadius);
                    Canvas.SetTop(node, y - nodeRadius);
                    NetworkCanvas.Children.Add(node);

                    currentLayerPositions.Add(new Point(x, y));

                }

                if (previousLayerPositions != null)
                {
                    foreach (var previousPos in previousLayerPositions)
                    {
                        foreach (var currentPos in currentLayerPositions)
                        {
                            Line connection = new()
                            {
                                X1 = previousPos.X,
                                Y1 = previousPos.Y,
                                X2 = currentPos.X,
                                Y2 = currentPos.Y,
                                Stroke = Brushes.LightGray,
                                StrokeThickness = 1
                            };
                            NetworkCanvas.Children.Add(connection);
                        }
                    }
                }

                previousLayerPositions = currentLayerPositions;
            }
        }

        private void AddLayers(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < layerStructure.Count; i++)
                layerStructure[i] = Math.Min((layerStructure.Count - i)*3, layerStructure[i]*2);
            InitializeNetwork();
            DrawNetwork(network.GetAllNeurons(), null);
        }

        private void RemoveLayers(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < layerStructure.Count; i++)
                layerStructure[i] = Math.Max(layerStructure.Count - i, layerStructure[i] / 2);
            InitializeNetwork();
            DrawNetwork(network.GetAllNeurons());
        }

        private void SendInputs(object sender, RoutedEventArgs e)
        {
            var inputs = new List<double>();
            for (int i = 0; i < layerStructure[0]; i++)
                inputs.Add(0.1 + (random.NextDouble() * 0.8));

            var outputsPerLayer = network.FeedForwardWithLayerOutputs(inputs);


            MyTextBox.AppendText($"\nData sent: {network.iter}");
            MyTextBox.AppendText($"\nLayer 0: " + string.Join(", ", inputs));
            for (int layerIndex = 0; layerIndex < outputsPerLayer.Count; layerIndex++)
                MyTextBox.AppendText($"\nLayer {layerIndex + 1}: " + string.Join(", ", outputsPerLayer[layerIndex]));
            MyTextBox.ScrollToEnd();

            List<double> allValues = [.. inputs, .. outputsPerLayer.SelectMany(layer => layer)];
            var myNeurons = FindEllipsesInCanvas(NetworkCanvas);
            int neuronIndex = 0;

            foreach (var input in inputs)
                if (neuronIndex < myNeurons.Count)
                {
                    myNeurons[neuronIndex].Fill = OutputToColor(input, allValues, 0);
                    neuronIndex++;
                }

            for (int layerIndex = 0; layerIndex < outputsPerLayer.Count; layerIndex++)
                foreach (var output in outputsPerLayer[layerIndex])
                    if (neuronIndex < myNeurons.Count)
                    {
                        myNeurons[neuronIndex].Fill = OutputToColor(output, allValues, layerIndex + 1);
                        neuronIndex++;
                    }
        }
        private static List<Ellipse> FindEllipsesInCanvas(Canvas canvas)
        {
            List<Ellipse> ellipses = [];

            foreach (var child in canvas.Children)
                if (child is Ellipse ellipse)
                    ellipses.Add(ellipse);

            return ellipses;
        }

        private static SolidColorBrush OutputToColor(double value, List<double> allValues, int layer)
        {
            double min = allValues.Min();
            double max = allValues.Max();

            max = (max - min == 0) ? min + 1 : max;

            double normalizedValue = (value - min) / (max - min);
            double layerFactor = 1 + (layer * 0.3);

            normalizedValue = Math.Min(1, Math.Max(0, normalizedValue * layerFactor));

            byte red = (byte)(normalizedValue * 255);
            byte blue = (byte)((1 - normalizedValue) * 255);

            return new SolidColorBrush(Color.FromRgb(red, 0, blue));
        }


    }
}
