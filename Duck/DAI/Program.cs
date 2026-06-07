using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace DAI
{
    internal class Program
    {
        private static readonly string mModelPath = Path.Combine(AppContext.BaseDirectory, "AiModels", "Mobilenet.onnx");
        private static readonly string mSampleImagePath = Path.Combine(AppContext.BaseDirectory, "SampleImages", "sample.png");
        private static readonly float[] mImageNetMean = { 0.485f, 0.456f, 0.406f };
        private static readonly float[] mImageNetStd = { 0.229f, 0.224f, 0.225f };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (!File.Exists(mSampleImagePath))
            {
                Console.WriteLine("샘플 이미지 파일을 찾을 수 없습니다: " + mSampleImagePath);
                return;
            }

            using OnnxEngine engine = new OnnxEngine(mModelPath);
            if (!engine.IsLoaded)
            {
                Console.WriteLine("모델을 로드하지 못했습니다: " + engine.LastError);
                return;
            }

            string inputName = engine.InputNames.First();
            int[] inputDimensions = FixInputDimensions(engine.GetInputDimensions(inputName));
            float[] inputData = LoadImageAsTensorData(mSampleImagePath, inputDimensions);
            IDictionary<string, Tensor<float>> outputs = engine.Run(inputData, inputDimensions, inputName);

            if (outputs.Count == 0)
            {
                Console.WriteLine("추론 결과가 없습니다: " + engine.LastError);
                return;
            }

            KeyValuePair<string, Tensor<float>> firstOutput = outputs.First();
            float[] probabilities = Softmax(firstOutput.Value.ToArray());

            Console.WriteLine("MobileNet ONNX 샘플 이미지 분류 예제");
            Console.WriteLine("Model : " + mModelPath);
            Console.WriteLine("Image : " + mSampleImagePath);
            Console.WriteLine("Input : " + inputName + " [" + string.Join(", ", inputDimensions) + "]");
            Console.WriteLine("Output: " + firstOutput.Key + " [" + string.Join(", ", firstOutput.Value.Dimensions.ToArray()) + "]");
            Console.WriteLine();
            Console.WriteLine("Top 5 예측 결과 (클래스 인덱스 / 확률)");

            foreach ((int classIndex, float probability) in GetTopK(probabilities, 5))
            {
                Console.WriteLine($"{classIndex,5} / {probability:P2}");
            }
        }

        private static int[] FixInputDimensions(IReadOnlyList<int> dimensions)
        {
            int[] fixedDimensions = dimensions.ToArray();

            for (int i = 0; i < fixedDimensions.Length; i++)
            {
                if (fixedDimensions[i] > 0)
                {
                    continue;
                }

                fixedDimensions[i] = i switch
                {
                    0 => 1,
                    1 => 3,
                    2 => 224,
                    3 => 224,
                    _ => 1
                };
            }

            return fixedDimensions;
        }

        private static float[] LoadImageAsTensorData(string imagePath, int[] dimensions)
        {
            if (dimensions.Length != 4)
            {
                throw new NotSupportedException("이 예제는 NCHW 형식의 4차원 입력 텐서를 가정합니다.");
            }

            int batch = dimensions[0];
            int channels = dimensions[1];
            int height = dimensions[2];
            int width = dimensions[3];

            if (batch != 1 || channels != 3)
            {
                throw new NotSupportedException("이 예제는 [1, 3, height, width] 입력 텐서를 가정합니다.");
            }

            using Bitmap source = new Bitmap(imagePath);
            using Bitmap resized = ResizeImage(source, width, height);

            DenseTensor<float> tensor = new DenseTensor<float>(dimensions);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = resized.GetPixel(x, y);
                    tensor[0, 0, y, x] = Normalize(pixel.R, 0);
                    tensor[0, 1, y, x] = Normalize(pixel.G, 1);
                    tensor[0, 2, y, x] = Normalize(pixel.B, 2);
                }
            }

            return tensor.ToArray();
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using Graphics graphics = Graphics.FromImage(resized);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.DrawImage(image, 0, 0, width, height);
            return resized;
        }

        private static float Normalize(byte value, int channel)
        {
            float scaled = value / 255.0f;
            return (scaled - mImageNetMean[channel]) / mImageNetStd[channel];
        }

        private static float[] Softmax(float[] logits)
        {
            float max = logits.Max();
            float sum = 0.0f;
            float[] expValues = new float[logits.Length];

            for (int i = 0; i < logits.Length; i++)
            {
                expValues[i] = MathF.Exp(logits[i] - max);
                sum += expValues[i];
            }

            for (int i = 0; i < expValues.Length; i++)
            {
                expValues[i] /= sum;
            }

            return expValues;
        }

        private static IEnumerable<(int ClassIndex, float Probability)> GetTopK(float[] probabilities, int k)
        {
            return probabilities
                .Select((probability, index) => (ClassIndex: index, Probability: probability))
                .OrderByDescending(item => item.Probability)
                .Take(k);
        }
    }
}
