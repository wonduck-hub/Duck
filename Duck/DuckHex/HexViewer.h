#pragma once

#include <Windows.h>
#include <d2d1.h>
#include <dwrite.h>

#include <cassert>
#include <cstdint>
#include <string>
#include <vector>

#include "pch.h"
#include "BinaryDocument.h"
#include "ByteSelection.h"
#include "UndoManager.h"

namespace DuckHex
{
	class HexViewer
	{
	private:
		BinaryDocument* mDocument;
		uint64_t mCurrentSeekPosition;
		uint64_t mCurrentBytesPerLine;
		uint64_t mTotalLines;
		bool mShouldResetScroll;
		ByteSelection mSelection;
		UndoManager mUndoManager;
		bool mbIsSelecting;
		uint64_t mSelectionAnchorOffset;
		char mEditBuffer[512];
		bool mbShouldOpenEditPopup;
		bool mbShouldFocusEditInput;
		std::string mEditError;
		char mSearchBuffer[512];
		std::size_t mSearchPatternLength;
		std::vector<uint64_t> mSearchResults;
		std::size_t mCurrentSearchResultIndex;
		bool mbShouldFocusSearchInput;
		bool mbShouldScrollToSelection;
		std::string mSearchError;

		static const uint32_t DEFAULT_BYTES_PER_LINE = 16;
		static const uint32_t DEFAULT_FONT_SIZE = 10;

		float mWindowWidth;
		float mWindowHeight;

	private:
		bool getByteOffsetFromMousePosition(
			const ImVec2& mousePosition,
			const ImVec2& hexStartPosition,
			float byteCellWidth,
			float lineHeight,
			uint64_t firstVisibleLine,
			uint64_t documentLineCount,
			uint64_t* outOffset) const;
		bool parseHexBytes(const char* text, std::vector<uint8_t>* outBytes) const;
		void handleKeyboardInput();
		void openEditPopup();
		void renderEditPopup();
		void renderSearchPanel();
		void searchBytePattern();
		void selectSearchResult(std::size_t resultIndex);
		bool replaceSelection(const std::vector<uint8_t>& newData);
		void applyUndoAction(const UndoManager::UndoAction& action, bool bUseNewData);
		void undo();
		void redo();

	public:
		HexViewer();
		HexViewer(BinaryDocument* document);
		~HexViewer();

		bool LoadDocument(BinaryDocument* document);
		void UnloadDocument();

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
