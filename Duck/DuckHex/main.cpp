#define WIN32_LEAN_AND_MEAN

#include "pch.h"

#include "HexViewer.h"
#include "BinaryDocument.h"

// D3D관련 라이브러리를 링커에게 자동으로 추가
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "Comdlg32.lib")

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
	void DrawMenuWindow();

	LRESULT CALLBACK WindowProcedure(
		HWND windowHandle,
		UINT message,
		WPARAM wParam,
		LPARAM lParam);

	void BuildDefaultDockLayout(ImGuiID dockspaceId);

	DuckHex::HexViewer* gHexViewer = nullptr;
	DuckHex::BinaryDocument* gBinaryDocument = nullptr;
	ImGuiID gSearchDockId = 0;

	bool CreateDuckHexResource()
	{
		if (gHexViewer == nullptr)
		{
			gHexViewer = new DuckHex::HexViewer();
		}
		if (gBinaryDocument == nullptr)
		{
			gBinaryDocument = new DuckHex::BinaryDocument();
		}
		return true;
	}
	void CleanupDuckHexResources()
	{
		if (gHexViewer != nullptr)
		{
			delete gHexViewer;
			gHexViewer = nullptr;
		}
		if (gBinaryDocument != nullptr)
		{
			delete gBinaryDocument;
			gBinaryDocument = nullptr;
		}
	}
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
	io.ConfigFlags |= ImGuiConfigFlags_DockingEnable;

	// 기본 밝은 테마
	ImGui::StyleColorsLight();

	// Win32 입력 처리 백엔드 초기화
	ImGui_ImplWin32_Init(windowHandle);

	// Direct3D 11 렌더링 백엔드 초기화
	ImGui_ImplDX11_Init(
		gD3DDevice,
		gD3DDeviceContext);

	// DuckHex 리소스 초기화
	CreateDuckHexResource();

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

		// HexViewer 렌더링 영역 업데이트
		if (gHexViewer != nullptr)
		{
			RECT clientRect{};
			GetClientRect(windowHandle, &clientRect);
			const float windowWidth =
				static_cast<float>(clientRect.right - clientRect.left);
			const float windowHeight =
				static_cast<float>(clientRect.bottom - clientRect.top);
			gHexViewer->SetViewPort(
				windowWidth,
				windowHeight);
		}

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

	CleanupDuckHexResources();

	return 0;
}

namespace
{
	void DrawMainUI()
	{
		ImGuiViewport* viewport = ImGui::GetMainViewport();

		ImGui::SetNextWindowPos(viewport->WorkPos);
		ImGui::SetNextWindowSize(viewport->WorkSize);
		ImGui::SetNextWindowViewport(viewport->ID);

		constexpr ImGuiWindowFlags dockHostFlags =
			ImGuiWindowFlags_NoTitleBar |
			ImGuiWindowFlags_NoCollapse |
			ImGuiWindowFlags_NoResize |
			ImGuiWindowFlags_NoMove |
			ImGuiWindowFlags_NoBringToFrontOnFocus |
			ImGuiWindowFlags_NoNavFocus |
			ImGuiWindowFlags_NoDocking;

		ImGui::PushStyleVar(
			ImGuiStyleVar_WindowRounding,
			0.0f
		);

		ImGui::PushStyleVar(
			ImGuiStyleVar_WindowBorderSize,
			0.0f
		);

		ImGui::PushStyleVar(
			ImGuiStyleVar_WindowPadding,
			ImVec2(0.0f, 0.0f)
		);

		// DockSpace를 소유하는 숨은 부모 창
		ImGui::Begin(
			"DuckHexDockHost",
			nullptr,
			dockHostFlags
		);

		ImGui::PopStyleVar(3);

		const ImGuiID dockspaceId =
			ImGui::GetID("DuckHexDockSpace");

		if (ImGui::DockBuilderGetNode(dockspaceId) == nullptr)
		{
			BuildDefaultDockLayout(dockspaceId);
		}

		// 실제 DockSpace 제출
		ImGui::DockSpace(
			dockspaceId,
			ImVec2(0.0f, 0.0f),
			ImGuiDockNodeFlags_None
		);

		ImGui::End();

		// DockSpace 안에 들어갈 실제 창들
		DrawMenuWindow();

		// 아직 구현 전이라면 일단 빈 창이라도 만들어야 함
		if (gSearchDockId != 0)
		{
			ImGui::SetNextWindowDockID(
				 gSearchDockId,
			ImGuiCond_FirstUseEver);
		}

		gHexViewer->Render();
	}

	void DrawMenuWindow()
	{
		ImGui::Begin("Menu");

		ImGui::TextUnformatted(
			"Win32 + Direct3D 11 + Dear ImGui");

		ImGui::Separator();

		if (ImGui::Button("Open File"))
		{
			OPENFILENAMEW fileDialog{};

			wchar_t filePath[MAX_PATH]{};

			fileDialog.lStructSize = sizeof(fileDialog);
			fileDialog.hwndOwner = nullptr;
			fileDialog.lpstrFile = filePath;
			fileDialog.nMaxFile = MAX_PATH;

			fileDialog.lpstrFilter =
				L"All Files\0*.*\0"
				L"Executable Files\0*.exe\0"
				L"Binary Files\0*.bin\0\0";

			fileDialog.Flags =
				OFN_FILEMUSTEXIST |
				OFN_PATHMUSTEXIST;

			if (GetOpenFileNameW(&fileDialog))
			{
				if (gBinaryDocument->LoadFile(filePath))
				{
					gHexViewer->LoadDocument(gBinaryDocument);
				}
			}
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
			gHexViewer->UnloadDocument();
			gBinaryDocument->Unload();
		}

		ImGui::End();
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

	void BuildDefaultDockLayout(ImGuiID dockspaceId)
	{
		ImGuiViewport* viewport = ImGui::GetMainViewport();

		// 기존 DockSpace 설정 제거
		ImGui::DockBuilderRemoveNode(dockspaceId);

		// 새로운 DockSpace 생성
		ImGui::DockBuilderAddNode(
			dockspaceId,
			ImGuiDockNodeFlags_DockSpace
		);

		ImGui::DockBuilderSetNodeSize(
			dockspaceId,
			viewport->WorkSize
		);

		// 처음에는 전체 영역이 mainDockId 하나다.
		ImGuiID mainDockId = dockspaceId;
		ImGuiID leftDockId = 0;
		ImGuiID rightDockId = 0;
		ImGuiID bottomDockId = 0;
		ImGuiID rightBottomDockId = 0;

		// 전체 영역에서 왼쪽 25% 분리
		leftDockId = ImGui::DockBuilderSplitNode(
			mainDockId,
			ImGuiDir_Left,
			0.20f,
			nullptr,
			&mainDockId
		);

		// 전체 영역에서 오른쪽 25% 분리
		rightDockId = ImGui::DockBuilderSplitNode(
			mainDockId,
			ImGuiDir_Right,
			0.25f,
			nullptr,
			&mainDockId
		);

		// 남은 메인 영역에서 아래쪽 20% 분리
		bottomDockId = ImGui::DockBuilderSplitNode(
			mainDockId,
			ImGuiDir_Down,
			0.20f,
			nullptr,
			&mainDockId
		);

		// 오른쪽 영역을 위아래로 50% 분리
		rightBottomDockId = ImGui::DockBuilderSplitNode(
			rightDockId,
			ImGuiDir_Down,
			0.50f,
			nullptr,
			&rightDockId
		);

		gSearchDockId = rightDockId;

		// 남아 있는 mainDockId가 중앙 영역
		const ImGuiID centerDockId = mainDockId;

		// 각각의 Window를 DockNode에 배치
		ImGui::DockBuilderDockWindow(
			"Menu",
			leftDockId
		);

		ImGui::DockBuilderDockWindow(
			"Hex Viewer",
			centerDockId
		);

		ImGui::DockBuilderDockWindow(
			"Inspector",
			rightDockId
		);

		ImGui::DockBuilderDockWindow(
			"Bookmarks",
			rightBottomDockId
		);

		ImGui::DockBuilderDockWindow(
			"Output",
			bottomDockId
		);

		// 레이아웃 생성 완료
		ImGui::DockBuilderFinish(dockspaceId);
	}
}

