#define NOMINMAX
#include "HexViewer.h"

#include <algorithm>
#include <cctype>
#include <cstring>

DuckHex::HexViewer::HexViewer()
	: mDocument(nullptr), mCurrentSeekPosition(0),
	mCurrentBytesPerLine(DEFAULT_BYTES_PER_LINE), mTotalLines(0),
	mShouldResetScroll(false), mSelection(), mUndoManager(),
	mbIsSelecting(false), mSelectionAnchorOffset(0), mEditBuffer{},
	mbShouldOpenEditPopup(false), mbShouldFocusEditInput(false), mEditError(),
	mSearchBuffer{}, mSearchPatternLength(0), mSearchResults(), mCurrentSearchResultIndex(0),
	mbShouldFocusSearchInput(false), mbShouldScrollToSelection(false), mSearchError(),
	mWindowWidth(0), mWindowHeight(0)
{
}

DuckHex::HexViewer::HexViewer(BinaryDocument* document)
	: mDocument(document), mCurrentSeekPosition(0),
	mCurrentBytesPerLine(DEFAULT_BYTES_PER_LINE), mTotalLines(0),
	mShouldResetScroll(false), mSelection(), mUndoManager(),
	mbIsSelecting(false), mSelectionAnchorOffset(0), mEditBuffer{},
	mbShouldOpenEditPopup(false), mbShouldFocusEditInput(false), mEditError(),
	mSearchBuffer{}, mSearchPatternLength(0), mSearchResults(), mCurrentSearchResultIndex(0),
	mbShouldFocusSearchInput(false), mbShouldScrollToSelection(false), mSearchError(),
	mWindowWidth(0), mWindowHeight(0)
{
}

DuckHex::HexViewer::~HexViewer()
{
	if (mDocument != nullptr)
	{
		mDocument = nullptr;
	}
}

bool DuckHex::HexViewer::LoadDocument(BinaryDocument* document)
{
	assert(document != nullptr && "Document is null. Please provide a valid BinaryDocument.");

	if (mDocument != nullptr)
	{
		UnloadDocument();
	}

	mDocument = document;
	mCurrentSeekPosition = 0;
	mTotalLines = 0;
	mShouldResetScroll = true;
	mSelection.Clear();
	mbIsSelecting = false;
	mSelectionAnchorOffset = 0;
	mEditBuffer[0] = '\0';
	mbShouldOpenEditPopup = false;
	mbShouldFocusEditInput = false;
	mEditError.clear();
	mSearchBuffer[0] = '\0';
	mSearchPatternLength = 0;
	mSearchResults.clear();
	mCurrentSearchResultIndex = 0;
	mbShouldFocusSearchInput = false;
	mbShouldScrollToSelection = false;
	mSearchError.clear();
	return true;
}

void DuckHex::HexViewer::UnloadDocument()
{
	if (mDocument != nullptr)
	{
		mDocument = nullptr;
	}

	mCurrentSeekPosition = 0;
	mTotalLines = 0;
	mShouldResetScroll = true;
	mSelection.Clear();
	mbIsSelecting = false;
	mSelectionAnchorOffset = 0;
	mEditBuffer[0] = '\0';
	mbShouldOpenEditPopup = false;
	mbShouldFocusEditInput = false;
	mEditError.clear();
	mSearchBuffer[0] = '\0';
	mSearchPatternLength = 0;
	mSearchResults.clear();
	mCurrentSearchResultIndex = 0;
	mbShouldFocusSearchInput = false;
	mbShouldScrollToSelection = false;
	mSearchError.clear();
}

bool DuckHex::HexViewer::SetCurrentSeekPosition(uint64_t position)
{
	assert(mDocument != nullptr && "Document is not loaded. Please load a document first.");
	assert(position < mDocument->GetSize() && "Seek position is out of bounds. Please provide a valid position.");

	mCurrentSeekPosition = position;
	return true;
}

void DuckHex::HexViewer::SetViewPort(float windowWidth, float windowHeight)
{
	assert(windowWidth > 0 && windowHeight > 0 && "Bytes per line and total lines must be greater than zero.");

	mWindowWidth = windowWidth;
	mWindowHeight = windowHeight;

	const float offsetWidth = ImGui::CalcTextSize("00000000 ").x;
	const float byteCellWidth = ImGui::CalcTextSize("00 ").x;

	(void)offsetWidth;
	(void)byteCellWidth;

	mCurrentBytesPerLine = DEFAULT_BYTES_PER_LINE;

	const float lineHeight = ImGui::GetTextLineHeight();

	mTotalLines = std::max<uint64_t>(
		1,
		static_cast<uint64_t>(windowHeight / lineHeight));
}

uint64_t DuckHex::HexViewer::GetCurrentSeekPosition() const
{
	return mCurrentSeekPosition;
}

uint64_t DuckHex::HexViewer::GetBytePerLine() const
{
	return mCurrentBytesPerLine;
}

uint64_t DuckHex::HexViewer::GetTotalLines() const
{
	return mTotalLines;
}

uint64_t DuckHex::HexViewer::GetDocumentSize() const
{
	assert(mDocument != nullptr && "Document is not loaded. Please load a document first.");
	return mDocument->GetSize();
}

bool DuckHex::HexViewer::getByteOffsetFromMousePosition(
	const ImVec2& mousePosition,
	const ImVec2& hexStartPosition,
	float byteCellWidth,
	float lineHeight,
	uint64_t firstVisibleLine,
	uint64_t documentLineCount,
	uint64_t* outOffset) const
{
	assert(outOffset != nullptr && "Output offset pointer must not be null.");

	if (mousePosition.x < hexStartPosition.x ||
		mousePosition.y < hexStartPosition.y)
	{
		return false;
	}

	constexpr uint64_t BYTE_GROUP_SIZE = 8;

	const float spaceWidth = ImGui::CalcTextSize(" ").x;
	const float relativeY = mousePosition.y - hexStartPosition.y;
	const uint64_t lineIndex = static_cast<uint64_t>(relativeY / lineHeight);
	const uint64_t documentLine = firstVisibleLine + lineIndex;

	if (documentLine >= documentLineCount)
	{
		return false;
	}

	for (uint64_t byteIndex = 0;
		byteIndex < mCurrentBytesPerLine;
		++byteIndex)
	{
		const float byteX =
			hexStartPosition.x +
			static_cast<float>(byteIndex) * byteCellWidth +
			static_cast<float>(byteIndex / BYTE_GROUP_SIZE) * spaceWidth;

		if (mousePosition.x >= byteX &&
			mousePosition.x < byteX + byteCellWidth)
		{
			const uint64_t offset =
				documentLine * mCurrentBytesPerLine + byteIndex;

			if (mDocument != nullptr &&
				offset < mDocument->GetSize())
			{
				*outOffset = offset;
				return true;
			}

			return false;
		}
	}

	return false;
}

bool DuckHex::HexViewer::parseHexBytes(
	const char* text,
	std::vector<uint8_t>* outBytes) const
{
	assert(text != nullptr && "Text must not be null.");
	assert(outBytes != nullptr && "Output bytes pointer must not be null.");

	outBytes->clear();

	std::string hexDigits;

	for (const char* current = text; *current != '\0'; ++current)
	{
		const unsigned char ch = static_cast<unsigned char>(*current);

		if (std::isxdigit(ch) != 0)
		{
			hexDigits.push_back(static_cast<char>(ch));
			continue;
		}

		if (std::isspace(ch) != 0 ||
			*current == ',' ||
			*current == '-' ||
			*current == '_')
		{
			continue;
		}

		return false;
	}

	if (hexDigits.empty() ||
		hexDigits.size() % 2 != 0)
	{
		return false;
	}

	for (std::size_t index = 0;
		index < hexDigits.size();
		index += 2)
	{
		const char byteText[3]
		{
			hexDigits[index],
			hexDigits[index + 1],
			'\0'
		};

		char* endOrNull = nullptr;
		const unsigned long value = std::strtoul(byteText, &endOrNull, 16);

		if (endOrNull == byteText ||
			*endOrNull != '\0' ||
			value > 0xFF)
		{
			outBytes->clear();
			return false;
		}

		outBytes->push_back(static_cast<uint8_t>(value));
	}

	return true;
}

void DuckHex::HexViewer::handleKeyboardInput()
{
	ImGuiIO& io = ImGui::GetIO();

	if (io.KeyCtrl && ImGui::IsKeyPressed(ImGuiKey_F))
	{
		mbShouldFocusSearchInput = true;
	}

	if (!ImGui::IsWindowFocused(ImGuiFocusedFlags_RootAndChildWindows))
	{
		return;
	}

	if (io.KeyCtrl &&
		ImGui::IsKeyPressed(ImGuiKey_Z))
	{
		undo();
		return;
	}

	if (io.KeyCtrl && io.KeyShift && ImGui::IsKeyPressed(ImGuiKey_Z))
	{
		redo();
		return;
	}

	if (!mSelection.IsEmpty() &&
		(ImGui::IsKeyPressed(ImGuiKey_Enter) ||
			ImGui::IsKeyPressed(ImGuiKey_KeypadEnter)))
	{
		openEditPopup();
	}
}

void DuckHex::HexViewer::openEditPopup()
{
	if (mSelection.IsEmpty())
	{
		return;
	}

	mEditBuffer[0] = '\0';
	mEditError.clear();
	mbShouldOpenEditPopup = true;
	mbShouldFocusEditInput = true;
}

void DuckHex::HexViewer::renderEditPopup()
{
	if (mbShouldOpenEditPopup)
	{
		ImGui::OpenPopup("Replace Hex Range");
		mbShouldOpenEditPopup = false;
	}

	if (!ImGui::BeginPopupModal(
		"Replace Hex Range",
		nullptr,
		ImGuiWindowFlags_AlwaysAutoResize))
	{
		return;
	}

	ImGui::Text(
		"Offset: %08llX, Length: %llu bytes",
		static_cast<unsigned long long>(mSelection.Begin()),
		static_cast<unsigned long long>(mSelection.Length()));

	if (mbShouldFocusEditInput)
	{
		ImGui::SetKeyboardFocusHere();
		mbShouldFocusEditInput = false;
	}

	const bool bInputSubmitted = ImGui::InputText(
		"Hex",
		mEditBuffer,
		sizeof(mEditBuffer),
		ImGuiInputTextFlags_CharsHexadecimal |
		ImGuiInputTextFlags_CharsUppercase |
		ImGuiInputTextFlags_EnterReturnsTrue);

	if (!mEditError.empty())
	{
		ImGui::TextColored(
			ImVec4(0.80f, 0.10f, 0.10f, 1.0f),
			"%s",
			mEditError.c_str());
	}

	if (ImGui::Button("Replace") ||
		bInputSubmitted)
	{
		std::vector<uint8_t> newData;

		if (!parseHexBytes(mEditBuffer, &newData))
		{
			mEditError = "Enter an even number of hexadecimal digits.";
		}
		else if (replaceSelection(newData))
		{
			ImGui::CloseCurrentPopup();
		}
	}

	ImGui::SameLine();

	if (ImGui::Button("Cancel") ||
		ImGui::IsKeyPressed(ImGuiKey_Escape))
	{
		ImGui::CloseCurrentPopup();
	}

	ImGui::EndPopup();
}

void DuckHex::HexViewer::renderSearchPanel()
{
	ImGuiIO& io = ImGui::GetIO();

	if (io.KeyCtrl && ImGui::IsKeyPressed(ImGuiKey_F))
	{
		mbShouldFocusSearchInput = true;
	}

	ImGui::Begin("Search");

	if (mbShouldFocusSearchInput)
	{
		ImGui::SetKeyboardFocusHere();
		mbShouldFocusSearchInput = false;
	}

	const bool bSearchSubmitted =
		ImGui::InputText(
			"Pattern",
			mSearchBuffer,
			sizeof(mSearchBuffer),
			ImGuiInputTextFlags_CharsHexadecimal |
			ImGuiInputTextFlags_CharsUppercase |
			ImGuiInputTextFlags_EnterReturnsTrue);

	if (ImGui::Button("Find") || bSearchSubmitted)
	{
		searchBytePattern();
	}

	if (!mSearchError.empty())
	{
		ImGui::TextColored(
			ImVec4(0.80f, 0.10f, 0.10f, 1.0f),
			"%s",
			mSearchError.c_str());
	}

	if (!mSearchResults.empty())
	{
		ImGui::Text(
			"Found: %llu",
			static_cast<unsigned long long>(mSearchResults.size()));

		if (ImGui::Button("Previous"))
		{
			if (mCurrentSearchResultIndex == 0)
			{
				mCurrentSearchResultIndex = mSearchResults.size() - 1;
			}
			else
			{
				--mCurrentSearchResultIndex;
			}

			selectSearchResult(mCurrentSearchResultIndex);
		}

		ImGui::SameLine();

		if (ImGui::Button("Next"))
		{
			mCurrentSearchResultIndex =
				(mCurrentSearchResultIndex + 1) % mSearchResults.size();
			selectSearchResult(mCurrentSearchResultIndex);
		}

		const uint64_t selectedOffset =
			mSearchResults[mCurrentSearchResultIndex];

		ImGui::Text(
			"Offset: %08llX",
			static_cast<unsigned long long>(selectedOffset));
	}

	ImGui::End();
}

void DuckHex::HexViewer::searchBytePattern()
{
	std::vector<uint8_t> pattern;

	if (!parseHexBytes(mSearchBuffer, &pattern))
	{
		mSearchResults.clear();
		mSearchError = "Enter an even number of hexadecimal digits.";
		return;
	}

	if (mDocument == nullptr)
	{
		mSearchResults.clear();
		mSearchError = "No document is loaded.";
		return;
	}

	mSearchResults = mDocument->FindBytePattern(pattern);
	mSearchPatternLength = pattern.size();
	mCurrentSearchResultIndex = 0;
	mSearchError.clear();

	if (mSearchResults.empty())
	{
		mSearchError = "Pattern not found.";
		return;
	}

	selectSearchResult(0);
}

void DuckHex::HexViewer::selectSearchResult(std::size_t resultIndex)
{
	if (resultIndex >= mSearchResults.size())
	{
		return;
	}

	const uint64_t offset = mSearchResults[resultIndex];

	mSelection.SetRange(
		offset,
		offset + static_cast<uint64_t>(mSearchPatternLength));
	mCurrentSearchResultIndex = resultIndex;
	mbShouldScrollToSelection = true;
}

bool DuckHex::HexViewer::replaceSelection(const std::vector<uint8_t>& newData)
{
	if (mDocument == nullptr ||
		mSelection.IsEmpty())
	{
		return false;
	}

	const uint64_t selectionLength = mSelection.Length();

	if (newData.size() != selectionLength)
	{
		mEditError = "Overwrite-only replace requires the same byte count.";
		return false;
	}

	std::vector<uint8_t> oldData =
		mDocument->GetDataBySeekPositionOrNull(
			mSelection.Begin(),
			selectionLength);

	if (oldData.size() != newData.size())
	{
		mEditError = "Could not read the selected range.";
		return false;
	}

	if (!mDocument->OverwriteData(
		mSelection.Begin(),
		newData))
	{
		mEditError = "Could not write the selected range.";
		return false;
	}

	UndoManager::UndoAction action(
		static_cast<size_t>(mSelection.Begin()),
		oldData,
		newData);

	mUndoManager.AddAction(action);
	return true;
}

void DuckHex::HexViewer::applyUndoAction(
	const UndoManager::UndoAction& action,
	bool bUseNewData)
{
	if (mDocument == nullptr)
	{
		return;
	}

	const std::vector<uint8_t>& data =
		bUseNewData ? action.GetNewData() : action.GetOldData();

	if (mDocument->OverwriteData(action.GetOffset(), data))
	{
		mSelection.SetRange(
			action.GetOffset(),
			action.GetOffset() + data.size());
	}
}

void DuckHex::HexViewer::undo()
{
	std::optional<UndoManager::UndoAction> actionOrNull =
		mUndoManager.Undo();

	if (actionOrNull.has_value())
	{
		applyUndoAction(actionOrNull.value(), false);
	}
}

void DuckHex::HexViewer::redo()
{
	std::optional<UndoManager::UndoAction> actionOrNull =
		mUndoManager.Redo();

	if (actionOrNull.has_value())
	{
		applyUndoAction(actionOrNull.value(), true);
	}
}

void DuckHex::HexViewer::Render()
{
	#pragma region Window Setup

	if (mDocument == nullptr)
	{
		renderSearchPanel();
		return;
	}

	const uint64_t documentSize = mDocument->GetSize();

	ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(244, 233, 221, 255));
	ImGui::Begin("Hex Viewer");

	bool bScrollWasReset = false;


	#pragma endregion

	#pragma region Empty Document

	if (documentSize == 0)
	{
		ImGui::End();
		ImGui::PopStyleColor();
		renderSearchPanel();
		return;
	}

	#pragma endregion

	#pragma region Toolbar and Keyboard Input

	handleKeyboardInput();

	if (ImGui::Button("Edit"))
	{
		openEditPopup();
	}

	ImGui::SameLine();

	if (ImGui::Button("Undo"))
	{
		undo();
	}

	ImGui::SameLine();

	if (ImGui::Button("Redo"))
	{
		redo();
	}

	ImGui::Separator();

	#pragma endregion

	#pragma region Scroll and Visible Data

	ImGui::BeginChild(
		"HexData",
		ImVec2(0.0f, 0.0f),
		false,
		ImGuiWindowFlags_HorizontalScrollbar);

	if (mShouldResetScroll)
	{
		ImGui::SetScrollY(0.0f);
		mShouldResetScroll = false;
		bScrollWasReset = true;
	}

	const float lineHeight = ImGui::GetTextLineHeight();
	const ImVec2 availableRegion = ImGui::GetContentRegionAvail();
	const float availableHeight = availableRegion.y;

	if (mbShouldScrollToSelection)
	{
		const uint64_t selectionLine =
			mSelection.Begin() / mCurrentBytesPerLine;
		ImGui::SetScrollY(static_cast<float>(selectionLine) * lineHeight);
		mbShouldScrollToSelection = false;
	}

	const float scrollY = bScrollWasReset ? 0.0f : ImGui::GetScrollY();
	const uint64_t visibleLineCount = std::max<uint64_t>(1, static_cast<uint64_t>(availableHeight / lineHeight) + 1);

	const uint64_t documentLineCount = (documentSize + mCurrentBytesPerLine - 1) / mCurrentBytesPerLine;

	const float totalContentHeight = static_cast<float>(documentLineCount) * lineHeight;

	const uint64_t firstVisibleLine = std::min<uint64_t>(static_cast<uint64_t>(scrollY / lineHeight), documentLineCount - 1);

	mCurrentSeekPosition = firstVisibleLine * mCurrentBytesPerLine;
	mTotalLines = visibleLineCount;

	const uint64_t requestedSize = mCurrentBytesPerLine * visibleLineCount; // 현제 화면에 보이는 라인 수에 맞춰서 데이터 크기를 구한 변수

	std::vector<uint8_t> data = mDocument->GetDataBySeekPositionOrNull(mCurrentSeekPosition, requestedSize);

	if (data.empty())
	{
		ImGui::EndChild();
		ImGui::End();
		ImGui::PopStyleColor();
		renderSearchPanel();
		return;
	}

	#pragma endregion

	#pragma region Mouse Selection and Editing Input

	ImDrawList* drawList = ImGui::GetWindowDrawList();

	const ImVec2 cursorScreenPosition = ImGui::GetCursorScreenPos();
	const ImVec2 startPosition(
		cursorScreenPosition.x,
		cursorScreenPosition.y + static_cast<float>(firstVisibleLine) * lineHeight);
	const ImU32 textColor = ImGui::GetColorU32(ImGuiCol_Text);
	const ImU32 selectedBackgroundColor =
		ImGui::GetColorU32(ImVec4(0.22f, 0.42f, 0.80f, 0.35f));
	const ImU32 selectedTextColor =
		ImGui::GetColorU32(ImVec4(0.05f, 0.08f, 0.12f, 1.0f));

	constexpr uint64_t BYTE_GROUP_SIZE = 8;
	const float addressWidth = ImGui::CalcTextSize("00000000 ").x;
	const float byteCellWidth = ImGui::CalcTextSize("00 ").x;
	const float groupSpaceWidth = ImGui::CalcTextSize(" ").x;
	const ImVec2 hexStartPosition(
		startPosition.x + addressWidth,
		startPosition.y);

	uint64_t hoveredByteOffset = 0;
	const bool bHasHoveredByte =
		ImGui::IsWindowHovered() &&
		getByteOffsetFromMousePosition(
			ImGui::GetMousePos(),
			hexStartPosition,
			byteCellWidth,
			lineHeight,
			firstVisibleLine,
			documentLineCount,
			&hoveredByteOffset);

	if (bHasHoveredByte &&
		ImGui::IsMouseClicked(ImGuiMouseButton_Left))
	{
		mbIsSelecting = true;
		mSelectionAnchorOffset = hoveredByteOffset;
		mSelection.SetSingleByte(hoveredByteOffset);
	}

	if (mbIsSelecting &&
		ImGui::IsMouseDown(ImGuiMouseButton_Left) &&
		bHasHoveredByte)
	{
		const uint64_t begin =
			std::min(mSelectionAnchorOffset, hoveredByteOffset);
		const uint64_t end =
			std::max(mSelectionAnchorOffset, hoveredByteOffset) + 1;

		mSelection.SetRange(begin, end);
	}

	if (mbIsSelecting &&
		!ImGui::IsMouseDown(ImGuiMouseButton_Left))
	{
		mbIsSelecting = false;
	}

	if (bHasHoveredByte &&
		ImGui::IsMouseDoubleClicked(ImGuiMouseButton_Left))
	{
		mSelection.SetSingleByte(hoveredByteOffset);
		openEditPopup();
	}

	if (bHasHoveredByte &&
		ImGui::IsMouseClicked(ImGuiMouseButton_Right))
	{
		if (!mSelection.Contains(hoveredByteOffset))
		{
			mSelection.SetSingleByte(hoveredByteOffset);
		}

		openEditPopup();
	}

	#pragma endregion

	#pragma region Visible Byte Rendering

	for (uint64_t lineIndex = 0;
		lineIndex < visibleLineCount;
		++lineIndex)
	{
		const uint64_t lineDataIndex =
			lineIndex * mCurrentBytesPerLine;

		if (lineDataIndex >= data.size())
		{
			break;
		}

		const uint64_t remainingDataSize =
			static_cast<uint64_t>(data.size()) - lineDataIndex;

		const uint64_t bytesInLine =
			std::min(mCurrentBytesPerLine, remainingDataSize);

		const uint64_t lineOffset =
			mCurrentSeekPosition + lineDataIndex;

		char addressBuffer[32]{};

		const int writtenLength = std::snprintf(
			addressBuffer,
			sizeof(addressBuffer),
			"%08llX ",
			static_cast<unsigned long long>(lineOffset));

		if (writtenLength < 0)
		{
			continue;
		}

		const ImVec2 linePosition(
			startPosition.x,
			startPosition.y +
			static_cast<float>(lineIndex) * lineHeight);

		drawList->AddText(
			linePosition,
			textColor,
			addressBuffer);

		for (uint64_t byteIndex = 0;
			byteIndex < bytesInLine;
			++byteIndex)
		{
			const uint64_t documentOffset =
				lineOffset + byteIndex;

			const uint8_t value =
				data[static_cast<std::size_t>(
					lineDataIndex + byteIndex)];

			char byteBuffer[4]{};

			const int byteLength = std::snprintf(
				byteBuffer,
				sizeof(byteBuffer),
				"%02X",
				static_cast<unsigned int>(value));

			if (byteLength < 0)
			{
				continue;
			}

			const float byteX =
				hexStartPosition.x +
				static_cast<float>(byteIndex) * byteCellWidth +
				static_cast<float>(byteIndex / BYTE_GROUP_SIZE) *
				groupSpaceWidth;

			const ImVec2 bytePosition(
				byteX,
				linePosition.y);

			const bool bSelected =
				mSelection.Contains(documentOffset);

			if (bSelected)
			{
				drawList->AddRectFilled(
					bytePosition,
					ImVec2(
						bytePosition.x + byteCellWidth,
						bytePosition.y + lineHeight),
					selectedBackgroundColor);
			}

			drawList->AddText(
				bytePosition,
				bSelected ? selectedTextColor : textColor,
				byteBuffer);
		}
	}

	#pragma endregion

	#pragma region Content Size and Edit Popup

	ImGui::Dummy(
		ImVec2(
			availableRegion.x,
			totalContentHeight));

	ImGui::EndChild();

	renderEditPopup();

	ImGui::End();
	ImGui::PopStyleColor();

	renderSearchPanel();

	#pragma endregion
}
