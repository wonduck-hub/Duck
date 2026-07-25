#define WIN32_LEAN_AND_MEAN

#include "pch.h"

// D3D관련 라이브러리를 링커에게 자동으로 추가
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")

namespace // static 처럼 접근 권한을 이 파일로 한정하기 위한 것
{
	ID3D11Device* gD3DDevice = nullptr;
	ID3D11DeviceContext* gD3DDeviceContext = nullptr;
	IDXGISwapChain* gSwapChain = nullptr;
	ID3D11RenderTargetView* gRenderTargetView = nullptr;

	bool CreateD3DDevice(HWND windowHandle);
	void CleanupD3DDevice();

	bool CreateRenderTarget();
	void CleanupRenderTarget();

	void DrawMainUI();

	LRESULT CALLBACK WindowProcedure(
		HWND windowHandle,
		UINT message,
		WPARAM wParam,
		LPARAM lParam);
}

// 일부 버전에서는 imgui_impl_win32.h에 이 함수 선언이 노출되지 않아
// 직접 선언해야 한다.
extern IMGUI_IMPL_API LRESULT ImGui_ImplWin32_WndProcHandler(
	HWND windowHandle,
	UINT message,
	WPARAM wParam,
	LPARAM lParam);

int WINAPI wWinMain(
	HINSTANCE instance,
	HINSTANCE,
	PWSTR,
	int showCommand)
{
	constexpr wchar_t windowClassName[] =
		L"DHexEditorWindowClass";

	WNDCLASSEXW windowClass{};
	windowClass.cbSize = sizeof(windowClass);
	windowClass.style = CS_CLASSDC;
	windowClass.lpfnWndProc = WindowProcedure;
	windowClass.hInstance = instance;
	windowClass.lpszClassName = windowClassName;

	if (RegisterClassExW(&windowClass) == 0)
		return 1;

	constexpr DWORD windowStyle =
		WS_OVERLAPPED |
		WS_CAPTION |
		WS_SYSMENU |
		WS_MINIMIZEBOX;

	RECT windowRectangle
	{
		0,
		0,
		1280,
		720
	};

	AdjustWindowRect(
		&windowRectangle,
		windowStyle,
		FALSE);

	HWND windowHandle = CreateWindowExW(
		0,
		windowClassName,
		L"DuckHex",
		windowStyle,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		windowRectangle.right - windowRectangle.left,
		windowRectangle.bottom - windowRectangle.top,
		nullptr,
		nullptr,
		instance,
		nullptr);

	if (windowHandle == nullptr)
	{
		UnregisterClassW(windowClassName, instance);
		return 1;
	}

	if (!CreateD3DDevice(windowHandle))
	{
		CleanupD3DDevice();
		DestroyWindow(windowHandle);
		UnregisterClassW(windowClassName, instance);
		return 1;
	}

	ShowWindow(windowHandle, showCommand);
	UpdateWindow(windowHandle);

	// Dear ImGui Context 생성
	IMGUI_CHECKVERSION();
	ImGui::CreateContext();

	ImGuiIO& io = ImGui::GetIO();

	// 키보드 탐색 활성화
	io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;

	// 기본 어두운 테마
	ImGui::StyleColorsLight();

	// Win32 입력 처리 백엔드 초기화
	ImGui_ImplWin32_Init(windowHandle);

	// Direct3D 11 렌더링 백엔드 초기화
	ImGui_ImplDX11_Init(
		gD3DDevice,
		gD3DDeviceContext);

	bool applicationRunning = true;

	while (applicationRunning)
	{
		MSG message{};

		while (PeekMessageW(
			&message,
			nullptr,
			0,
			0,
			PM_REMOVE))
		{
			TranslateMessage(&message);
			DispatchMessageW(&message);

			if (message.message == WM_QUIT)
				applicationRunning = false;
		}

		if (!applicationRunning)
			break;

		// ImGui 프레임 시작
		ImGui_ImplDX11_NewFrame();
		ImGui_ImplWin32_NewFrame();
		ImGui::NewFrame();

		// 애플리케이션 UI 구성
		DrawMainUI();

		// ImGui DrawData 생성
		ImGui::Render();

		constexpr float clearColor[4]
		{
			244 / 255.0f,
			233 / 255.0f,
			221 / 255.0f,
			1.0f
		};

		// 현재 Back Buffer를 렌더링 대상으로 설정
		gD3DDeviceContext->OMSetRenderTargets(
			1,
			&gRenderTargetView,
			nullptr);

		// 배경 지우기
		gD3DDeviceContext->ClearRenderTargetView(
			gRenderTargetView,
			clearColor);

		// ImGui가 만든 DrawData를 실제 D3D11로 렌더링
		ImGui_ImplDX11_RenderDrawData(
			ImGui::GetDrawData());

		// Back Buffer를 화면에 표시
		// 1은 수직 동기화 활성화
		gSwapChain->Present(1, 0);
	}

	// ImGui 종료
	ImGui_ImplDX11_Shutdown();
	ImGui_ImplWin32_Shutdown();
	ImGui::DestroyContext();

	// Direct3D 및 Win32 리소스 정리
	CleanupD3DDevice();

	DestroyWindow(windowHandle);
	UnregisterClassW(windowClassName, instance);

	return 0;
}

namespace
{
	void DrawMainUI()
	{
		static bool showDemoWindow = false;

		ImGui::Begin("DuckHex");

		ImGui::TextUnformatted(
			"Win32 + Direct3D 11 + Dear ImGui");

		ImGui::Separator();

		if (ImGui::Button("Open File"))
		{
			// TODO:
			// 파일 선택 대화상자 호출
			// HexDocument 생성
		}

		ImGui::SameLine();

		if (ImGui::Button("Save"))
		{
			// TODO:
			// 현재 문서 저장
		}

		ImGui::SameLine();

		if (ImGui::Button("Close"))
		{
			// TODO:
			// 현재 문서 닫기
		}

		ImGui::Separator();

		ImGui::Checkbox(
			"Show ImGui Demo",
			&showDemoWindow);

		ImGui::Separator();

		ImGui::End();

		if (showDemoWindow)
		{
			ImGui::ShowDemoWindow(
				&showDemoWindow);
		}
	}

	bool CreateD3DDevice(HWND windowHandle)
	{
		DXGI_SWAP_CHAIN_DESC swapChainDescription{};

		swapChainDescription.BufferCount = 2;

		swapChainDescription.BufferDesc.Format =
			DXGI_FORMAT_R8G8B8A8_UNORM;

		swapChainDescription.BufferUsage =
			DXGI_USAGE_RENDER_TARGET_OUTPUT;

		swapChainDescription.OutputWindow =
			windowHandle;

		swapChainDescription.SampleDesc.Count = 1;
		swapChainDescription.SampleDesc.Quality = 0;

		swapChainDescription.Windowed = TRUE;

		swapChainDescription.SwapEffect =
			DXGI_SWAP_EFFECT_DISCARD;

		constexpr D3D_FEATURE_LEVEL requestedFeatureLevels[]
		{
			D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL_10_0
		};

		D3D_FEATURE_LEVEL selectedFeatureLevel{};

		HRESULT result = D3D11CreateDeviceAndSwapChain(
			nullptr,
			D3D_DRIVER_TYPE_HARDWARE,
			nullptr,
			0,
			requestedFeatureLevels,
			static_cast<UINT>(
				std::size(requestedFeatureLevels)),
			D3D11_SDK_VERSION,
			&swapChainDescription,
			&gSwapChain,
			&gD3DDevice,
			&selectedFeatureLevel,
			&gD3DDeviceContext);

		if (FAILED(result))
			return false;

		return CreateRenderTarget();
	}

	void CleanupD3DDevice()
	{
		CleanupRenderTarget();

		if (gSwapChain != nullptr)
		{
			gSwapChain->Release();
			gSwapChain = nullptr;
		}

		if (gD3DDeviceContext != nullptr)
		{
			gD3DDeviceContext->Release();
			gD3DDeviceContext = nullptr;
		}

		if (gD3DDevice != nullptr)
		{
			gD3DDevice->Release();
			gD3DDevice = nullptr;
		}
	}

	bool CreateRenderTarget()
	{
		if (gSwapChain == nullptr ||
			gD3DDevice == nullptr)
		{
			return false;
		}

		ID3D11Texture2D* backBuffer = nullptr;

		HRESULT result = gSwapChain->GetBuffer(
			0,
			IID_PPV_ARGS(&backBuffer));

		if (FAILED(result))
			return false;

		result = gD3DDevice->CreateRenderTargetView(
			backBuffer,
			nullptr,
			&gRenderTargetView);

		backBuffer->Release();

		return SUCCEEDED(result);
	}

	void CleanupRenderTarget()
	{
		if (gRenderTargetView != nullptr)
		{
			gRenderTargetView->Release();
			gRenderTargetView = nullptr;
		}
	}

	LRESULT CALLBACK WindowProcedure(
		HWND windowHandle,
		UINT message,
		WPARAM wParam,
		LPARAM lParam)
	{
		// ImGui에 마우스와 키보드 입력 전달
		if (ImGui_ImplWin32_WndProcHandler(
			windowHandle,
			message,
			wParam,
			lParam))
		{
			return TRUE;
		}

		switch (message)
		{
		case WM_SIZE:
			if (gD3DDevice != nullptr &&
				wParam != SIZE_MINIMIZED)
			{
				CleanupRenderTarget();

				HRESULT result =
					gSwapChain->ResizeBuffers(
						0,
						static_cast<UINT>(LOWORD(lParam)),
						static_cast<UINT>(HIWORD(lParam)),
						DXGI_FORMAT_UNKNOWN,
						0);

				if (SUCCEEDED(result))
					CreateRenderTarget();
			}

			return 0;

		case WM_SYSCOMMAND:
			// Alt 키를 눌렀을 때 나타나는 기본 메뉴 방지
			if ((wParam & 0xFFF0) == SC_KEYMENU)
				return 0;

			break;

		case WM_DESTROY:
			PostQuitMessage(0);
			return 0;

		default:
			break;
		}

		return DefWindowProcW(
			windowHandle,
			message,
			wParam,
			lParam);
	}
}