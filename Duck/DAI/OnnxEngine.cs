using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace DAI
{
    public class OnnxEngine : IDisposable
    {
        private InferenceSession? mSessionOrNull;
        private bool mDisposed;
        public string ModelPath { get; private set; }
        public string LastError { get; private set; }

        public bool IsLoaded
        {
            get { return mSessionOrNull != null && !mDisposed; }
        }

        public IReadOnlyList<string> InputNames
        {
            get
            {
                if (mSessionOrNull == null) return Array.Empty<string>();
                return mSessionOrNull.InputMetadata.Keys.ToList();
            }
        }

        public IReadOnlyList<string> OutputNames
        {
            get
            {
                if (mSessionOrNull == null) return Array.Empty<string>();
                return mSessionOrNull.OutputMetadata.Keys.ToList();
            }
        }

        public IReadOnlyList<int> GetInputDimensions(string inputName)
        {
            if (mSessionOrNull == null)
            {
                LastError = "ONNX session is not available.";
                return Array.Empty<int>();
            }

            if (!mSessionOrNull.InputMetadata.TryGetValue(inputName, out NodeMetadata? metadata))
            {
                LastError = "input name not found: " + inputName;
                return Array.Empty<int>();
            }

            return metadata.Dimensions.ToArray();
        }

        public OnnxEngine(string modelPath, SessionOptions? options)
        {
            ModelPath = string.Empty;
            LastError = string.Empty;
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                LastError = "modelPath is null or empty";
                return;
            }

            if (!File.Exists(modelPath))
            {
                LastError = "model file not found: " + modelPath;
                return;
            }

            try
            {
                if (options == null)
                {
                    mSessionOrNull = new InferenceSession(modelPath);
                }
                else
                {
                    mSessionOrNull = new InferenceSession(modelPath, options);
                }
                ModelPath = modelPath;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        public OnnxEngine(string modelPath) : this(modelPath, null)
        {
        }

        public OnnxEngine(byte[] modelBytes, SessionOptions? optionsOrNull)
        {
            ModelPath = string.Empty;
            LastError = string.Empty;
            if (modelBytes == null || modelBytes.Length == 0)
            {
                LastError = "modelBytes is null or empty";
                return;
            }

            try
            {
                if (optionsOrNull == null)
                {
                    mSessionOrNull = new InferenceSession(modelBytes);
                }
                else
                {
                    mSessionOrNull = new InferenceSession(modelBytes, optionsOrNull);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        public OnnxEngine(byte[] modelBytes) : this(modelBytes, null)
        {
        }

        /// <summary>
        /// Run inference using pre-built NamedOnnxValue inputs. Returns copies of tensor outputs keyed by output name.
        /// </summary>
        public IDictionary<string, Tensor<float>> Run(IEnumerable<NamedOnnxValue> inputs)
        {
            if (mSessionOrNull == null)
            {
                LastError = "ONNX session is not available.";
                return new Dictionary<string, Tensor<float>>();
            }

            if (inputs == null)
            {
                LastError = "inputs is null.";
                return new Dictionary<string, Tensor<float>>();
            }

            List<NamedOnnxValue> inputList = inputs.ToList();
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = mSessionOrNull.Run(inputList);
            try
            {
                Dictionary<string, Tensor<float>> outputs = new Dictionary<string, Tensor<float>>(StringComparer.Ordinal);
                foreach (DisposableNamedOnnxValue res in results)
                {
                    try
                    {
                        string name = res.Name;
                        // copy tensor contents so caller can use them after results disposed
                        try
                        {
                            Tensor<float> t = res.AsTensor<float>();
                            DenseTensor<float> copy = new DenseTensor<float>(t.ToArray(), t.Dimensions.ToArray());
                            outputs[name] = copy;
                        }
                        catch
                        {
                            // ignore non-float tensors for this minimal wrapper
                        }
                    }
                    catch
                    {
                        // ignore any issues with individual outputs
                    }
                }

                LastError = string.Empty;
                return outputs;
            }
            finally
            {
                results.Dispose();
            }
        }

        /// <summary>
        /// Convenience overload: run inference from a single float input tensor and return all outputs.
        /// </summary>
        public IDictionary<string, Tensor<float>> Run(float[] data, int[] dimensions, string inputName)
        {
            if (data == null)
            {
                LastError = "data is null.";
                return new Dictionary<string, Tensor<float>>();
            }

            if (dimensions == null)
            {
                LastError = "dimensions is null.";
                return new Dictionary<string, Tensor<float>>();
            }

            if (string.IsNullOrEmpty(inputName))
            {
                LastError = "inputName is null or empty.";
                return new Dictionary<string, Tensor<float>>();
            }

            DenseTensor<float> tensor = new DenseTensor<float>(data, dimensions);
            NamedOnnxValue input = NamedOnnxValue.CreateFromTensor(inputName, tensor);
            // NamedOnnxValue implements IDisposable but session.Run will consume it; keep simple and wrap in a list
            List<NamedOnnxValue> inputs = new List<NamedOnnxValue> { input };
            try
            {
                return Run(inputs);
            }
            finally
            {
                // NamedOnnxValue does not implement Dispose here; nothing to dispose explicitly
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed) return;
            if (disposing)
            {
                mSessionOrNull?.Dispose();
                mSessionOrNull = null;
            }
            mDisposed = true;
        }

        ~OnnxEngine()
        {
            Dispose(false);
        }
    }
}
