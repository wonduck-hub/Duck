#pragma once

#include <Windows.h>
#include <d2d1.h>
#include <dwrite.h>

#include <cassert>
#include <cstdint>

#include "pch.h"
#include "BinaryDocument.h"

namespace DuckHex
{
	class HexViewer
	{
	private:
		BinaryDocument* mDocument;
		uint64_t mCurrentSeekPosition;
		uint64_t mCurrentBytesPerLine;
		uint64_t mTotalLines;

		static const uint32_t DEFAULT_BYTES_PER_LINE = 16;
		static const uint32_t DEFAULT_FONT_SIZE = 10;

		HWND mWindowHandle;
		ID2D1Factory* mD2DFactory;
		ID2D1HwndRenderTarget* mRenderTarget;
		ID2D1SolidColorBrush* mTextBrush;

		IDWriteFactory* mDWriteFactory;
		IDWriteTextFormat* mTextFormat;

		float mWindowWidth;
		float mWindowHeight;
		float mLineHeight;
		float mCharacterWidth;

	private:
		bool CreateDeviceResources();
		void ReleaseDeviceResources();

	public:
		HexViewer();
		HexViewer(BinaryDocument* document);
		~HexViewer();

		bool LoadDocument(BinaryDocument* document);
		void UnloadDocument();

		bool Initialize(HWND windowHandle);
		void Shutdown();

		bool SetCurrentSeekPosition(uint64_t position);
		void SetViewPort(float windowWidth, float windowHeight);

		uint64_t GetCurrentSeekPosition() const;
		uint64_t GetBytePerLine() const;
		uint64_t GetTotalLines() const;
		uint64_t GetDocumentSize() const;

		bool HasDocument() const { return mDocument != nullptr; }
		void Render();
	};
}