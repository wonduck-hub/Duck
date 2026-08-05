#define NOMINMAX
#include "HexViewer.h"

#include <algorithm>

DuckHex::HexViewer::HexViewer()
	: mDocument(nullptr), mCurrentSeekPosition(0),
	mCurrentBytesPerLine(DEFAULT_BYTES_PER_LINE), mTotalLines(0),
	mShouldResetScroll(false), mWindowWidth(0), mWindowHeight(0)
{}

DuckHex::HexViewer::HexViewer(BinaryDocument* document)
	: mDocument(document), mCurrentSeekPosition(0),
	mCurrentBytesPerLine(DEFAULT_BYTES_PER_LINE), mTotalLines(0),
	mShouldResetScroll(false), mWindowWidth(0), mWindowHeight(0)
{}

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

void DuckHex::HexViewer::Render()
{
	if (mDocument == nullptr)
	{
		return;
	}

	const uint64_t documentSize = mDocument->GetSize();

	ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(244, 233, 221, 255));
	ImGui::Begin("Hex Viewer");

	bool bScrollWasReset = false;

	if (mShouldResetScroll)
	{
		ImGui::SetScrollY(0.0f);
		mShouldResetScroll = false;
		bScrollWasReset = true;
	}

	if (documentSize == 0)
	{
		ImGui::End();
		ImGui::PopStyleColor();
		return;
	}

	const float lineHeight = ImGui::GetTextLineHeight();
	const ImVec2 availableRegion = ImGui::GetContentRegionAvail();
	const float availableHeight = availableRegion.y;
	const float scrollY = bScrollWasReset ? 0.0f : ImGui::GetScrollY();
	const uint64_t visibleLineCount = std::max<uint64_t>(1, static_cast<uint64_t>(availableHeight / lineHeight) + 1);

	const uint64_t documentLineCount = (documentSize + mCurrentBytesPerLine - 1) / mCurrentBytesPerLine;

	const float totalContentHeight = static_cast<float>(documentLineCount) * lineHeight;

	const uint64_t firstVisibleLine = std::min<uint64_t>(static_cast<uint64_t>(scrollY / lineHeight), documentLineCount - 1);

	mCurrentSeekPosition = firstVisibleLine * mCurrentBytesPerLine;
	mTotalLines = visibleLineCount;

	const uint64_t requestedSize = mCurrentBytesPerLine * visibleLineCount;

	std::vector<uint8_t> data = mDocument->GetDataBySeekPositionOrNull(mCurrentSeekPosition, requestedSize);

	if (data.empty())
	{
		ImGui::End();
		ImGui::PopStyleColor();
		return;
	}

	ImDrawList* drawList = ImGui::GetWindowDrawList();

	const ImVec2 startPosition = ImGui::GetCursorScreenPos();
	const ImU32 textColor = ImGui::GetColorU32(ImGuiCol_Text);

	constexpr uint64_t BYTE_GROUP_SIZE = 8;

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

		char lineBuffer[128]{};

		int writtenLength = std::snprintf(
			lineBuffer,
			sizeof(lineBuffer),
			"%08llX ",
			static_cast<unsigned long long>(lineOffset));

		if (writtenLength < 0)
		{
			continue;
		}

		std::size_t currentLength =
			static_cast<std::size_t>(writtenLength);

		for (uint64_t byteIndex = 0;
			byteIndex < bytesInLine;
			++byteIndex)
		{
			if (byteIndex > 0 &&
				byteIndex % BYTE_GROUP_SIZE == 0)
			{
				if (currentLength + 1 >= sizeof(lineBuffer))
				{
					break;
				}

				lineBuffer[currentLength++] = ' ';
				lineBuffer[currentLength] = '\0';
			}

			const uint8_t value =
				data[static_cast<std::size_t>(
					lineDataIndex + byteIndex)];

			const int byteLength = std::snprintf(
				lineBuffer + currentLength,
				sizeof(lineBuffer) - currentLength,
				"%02X ",
				static_cast<unsigned int>(value));

			if (byteLength < 0)
			{
				break;
			}

			const std::size_t convertedByteLength =
				static_cast<std::size_t>(byteLength);

			if (convertedByteLength >=
				sizeof(lineBuffer) - currentLength)
			{
				break;
			}

			currentLength += convertedByteLength;
		}

		const ImVec2 linePosition(
			startPosition.x,
			startPosition.y +
			static_cast<float>(firstVisibleLine + lineIndex) *
			lineHeight);

		drawList->AddText(
			linePosition,
			textColor,
			lineBuffer);
	}

	ImGui::Dummy(
		ImVec2(
			availableRegion.x,
			totalContentHeight));

	ImGui::End();
	ImGui::PopStyleColor();
}
