using Microsoft.ML.OnnxRuntime;

namespace DAI
{
    internal class Program
    {
        private static string mModelPath = Path.Combine(AppContext.BaseDirectory, "AiModels", "Mobilenet.onnx");
        static void Main(string[] args)
        {
            if (!File.Exists(mModelPath))
            {
                Console.WriteLine("모델 파일을 찾을 수 없습니다.");
                return;
            }

            InferenceSession session = new InferenceSession(mModelPath);
        }
    }
}
