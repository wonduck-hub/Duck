cmake_minimum_required(VERSION 3.24)

project(DuckLLM VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

option(DUCKLLM_ENABLE_FTXUI "Prepare FTXUI for the future terminal UI" OFF)

set(ONNX_RUNTIME_GENAI_ROOT "" CACHE PATH
    "ONNX Runtime GenAI SDK root containing include/ and lib/")

if(NOT ONNX_RUNTIME_GENAI_ROOT)
    set(ONNX_RUNTIME_GENAI_ROOT "${CMAKE_SOURCE_DIR}/third_party/onnxruntime-genai")
endif()

set(ONNX_RUNTIME_GENAI_INCLUDE_DIR "${ONNX_RUNTIME_GENAI_ROOT}/include")
set(ONNX_RUNTIME_GENAI_LIBRARY_DIR "${ONNX_RUNTIME_GENAI_ROOT}/lib")

if(NOT EXISTS "${ONNX_RUNTIME_GENAI_INCLUDE_DIR}/ort_genai.h")
    message(FATAL_ERROR
        "ort_genai.h was not found. Set ONNX_RUNTIME_GENAI_ROOT to the installed ONNX Runtime GenAI SDK.")
endif()

if(WIN32)
    set(ONNX_RUNTIME_LIBRARY_NAME "onnxruntime.lib")
    set(ONNX_RUNTIME_GENAI_LIBRARY_NAME "onnxruntime-genai.lib")
elseif(APPLE)
    set(ONNX_RUNTIME_LIBRARY_NAME "libonnxruntime.dylib")
    set(ONNX_RUNTIME_GENAI_LIBRARY_NAME "libonnxruntime-genai.dylib")
else()
    set(ONNX_RUNTIME_LIBRARY_NAME "libonnxruntime.so")
    set(ONNX_RUNTIME_GENAI_LIBRARY_NAME "libonnxruntime-genai.so")
endif()

find_library(ONNX_RUNTIME_LIBRARY
    NAMES onnxruntime
    PATHS "${ONNX_RUNTIME_GENAI_LIBRARY_DIR}"
    NO_DEFAULT_PATH
    REQUIRED)
find_library(ONNX_RUNTIME_GENAI_LIBRARY
    NAMES onnxruntime-genai
    PATHS "${ONNX_RUNTIME_GENAI_LIBRARY_DIR}"
    NO_DEFAULT_PATH
    REQUIRED)

add_executable(DuckLLM
    src/main.cpp)

target_include_directories(DuckLLM PRIVATE
    "${ONNX_RUNTIME_GENAI_INCLUDE_DIR}")
target_link_libraries(DuckLLM PRIVATE
    "${ONNX_RUNTIME_LIBRARY}"
    "${ONNX_RUNTIME_GENAI_LIBRARY}")

if(UNIX)
    set_target_properties(DuckLLM PROPERTIES
        BUILD_RPATH "${ONNX_RUNTIME_GENAI_LIBRARY_DIR}")
endif()

if(WIN32)
    foreach(runtime_library IN ITEMS onnxruntime.dll onnxruntime-genai.dll)
        if(EXISTS "${ONNX_RUNTIME_GENAI_LIBRARY_DIR}/${runtime_library}")
            add_custom_command(TARGET DuckLLM POST_BUILD
                COMMAND ${CMAKE_COMMAND} -E copy_if_different
                    "${ONNX_RUNTIME_GENAI_LIBRARY_DIR}/${runtime_library}"
                    "$<TARGET_FILE_DIR:DuckLLM>")
        endif()
    endforeach()
endif()

if(DUCKLLM_ENABLE_FTXUI)
    include(FetchContent)
    FetchContent_Declare(ftxui
        GIT_REPOSITORY https://github.com/ArthurSonzogni/ftxui.git
        GIT_TAG v6.1.9)
    FetchContent_MakeAvailable(ftxui)
    target_link_libraries(DuckLLM PRIVATE ftxui::component)
    target_compile_definitions(DuckLLM PRIVATE DUCKLLM_ENABLE_FTXUI=1)
endif()
