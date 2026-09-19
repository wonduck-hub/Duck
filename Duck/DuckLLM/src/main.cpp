#include <ort_genai.h>

#include <cstdlib>
#include <cstdint>
#include <exception>
#include <iostream>
#include <stdexcept>
#include <string>

namespace
{
    struct SCommandLineOptions
    {
        std::string modelPath;
        std::string prompt;
        int maxLength = 256;
    };

    void printUsage(const char* executableName)
    {
        std::cout << "Usage: " << executableName << " --model <path> --prompt <text> [--max-length <count>]\n"
                  << "\n"
                  << "The model directory must contain genai_config.json and the ONNX model files.\n";
    }

    SCommandLineOptions parseCommandLine(int argumentCount, char* arguments[])
    {
        SCommandLineOptions options;

        for (int argumentIndex = 1; argumentIndex < argumentCount; ++argumentIndex)
        {
            std::string argument = arguments[argumentIndex];

            if (argument == "--help" || argument == "-h")
            {
                printUsage(arguments[0]);
                std::exit(EXIT_SUCCESS);
            }

            if (argument == "--model")
            {
                if (argumentIndex + 1 >= argumentCount)
                {
                    throw std::invalid_argument("--model requires a path.");
                }

                options.modelPath = arguments[++argumentIndex];
                continue;
            }

            if (argument == "--prompt")
            {
                if (argumentIndex + 1 >= argumentCount)
                {
                    throw std::invalid_argument("--prompt requires text.");
                }

                options.prompt = arguments[++argumentIndex];
                continue;
            }

            if (argument == "--max-length")
            {
                if (argumentIndex + 1 >= argumentCount)
                {
                    throw std::invalid_argument("--max-length requires a number.");
                }

                options.maxLength = std::stoi(arguments[++argumentIndex]);
                continue;
            }

            throw std::invalid_argument("Unknown argument: " + argument);
        }

        if (options.modelPath.empty())
        {
            throw std::invalid_argument("--model is required.");
        }

        if (options.prompt.empty())
        {
            throw std::invalid_argument("--prompt is required.");
        }

        if (options.maxLength <= 0)
        {
            throw std::invalid_argument("--max-length must be greater than zero.");
        }

        return options;
    }

    void generateText(const SCommandLineOptions& options)
    {
        auto model = OgaModel::Create(options.modelPath.c_str());
        auto tokenizer = OgaTokenizer::Create(*model);
        auto tokenizerStream = OgaTokenizerStream::Create(*tokenizer);
        auto sequences = OgaSequences::Create();

        tokenizer->Encode(options.prompt.c_str(), *sequences);

        auto generatorParams = OgaGeneratorParams::Create(*model);
        generatorParams->SetSearchOption("max_length", options.maxLength);
        generatorParams->SetSearchOptionBool("do_sample", false);

        auto generator = OgaGenerator::Create(*model, *generatorParams);
        generator->AppendTokenSequences(*sequences);

        while (!generator->IsDone())
        {
            generator->GenerateNextToken();

            const int32_t* tokenData = generator->GetSequenceData(0);
            size_t tokenCount = generator->GetSequenceCount(0);
            const std::string& decodedToken = tokenizerStream->Decode(tokenData[tokenCount - 1]);

            std::cout << decodedToken << std::flush;
        }

        std::cout << '\n';
    }
}

int main(int argumentCount, char* arguments[])
{
    try
    {
        SCommandLineOptions options = parseCommandLine(argumentCount, arguments);
        generateText(options);
    }
    catch (const std::exception& exception)
    {
        std::cerr << "DuckLLM error: " << exception.what() << '\n';
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}
