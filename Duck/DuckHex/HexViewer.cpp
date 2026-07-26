#define NOMINMAX
#include "HexViewer.h"

DuckHex::HexViewer::HexViewer()
	: mDocument(nullptr), mCurrentSeekPosition(0),
	mCurrentBytesPerLine(DEFAULT_BYTES_PER_LINE), mTotalLines(0),
	mWindowWidth(0), mWindowHeight(0)
{}

DuckHex::HexViewer::HexViewer(BinaryDocument* document)
	: mDocument(document), mCurrentSeekPosition(0),
	mCurrentBytesPerLine(DEFAULT_BYTES_PER_LINE), mTotalLines(0),
	mWindowWidth(0), mWindowHeight(0)
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
	return true;
}

void DuckHex::HexViewer::UnloadDocument()
{
	if (mDocument != nullptr)
	{
		mDocument = nullptr;
	}
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

	// format
	/*
	offset: 8
	whitespace: 1
	one byte: "00 " = 3
	line: 00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00 -> 8바이트에서 한칸 더 비움
	*/

	const float offsetWidth = ImGui::CalcTextSize("00000000 ").x;
	const float byteCellWidth = ImGui::CalcTextSize("00 ").x; // 추후 넓이 고정을 해제할 때 사용

	// 현재는 한 행당 16Byte를 고정으로 사용한다.
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
		return;

	const uint64_t documentSize = mDocument->GetSize();

	if (documentSize == 0 || mCurrentSeekPosition >= documentSize)
		return;

	/*
		현재 화면에 필요한 데이터 크기

		예:
		16 bytes/line * 30 lines = 480 bytes
	*/
	const uint64_t requestedSize =
		mCurrentBytesPerLine * mTotalLines;

	std::vector<uint8_t> data =
		mDocument->GetDataBySeekPositionOrNull(
			mCurrentSeekPosition,
			requestedSize);

	if (data.empty())
		return;

	ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(244, 233, 221, 255));
	ImGui::Begin("Hex Viewer");
	ImDrawList* drawList = ImGui::GetWindowDrawList();

	const ImVec2 startPosition = ImGui::GetCursorScreenPos();
	const ImU32 textColor = ImGui::GetColorU32(ImGuiCol_Text);
	const float lineHeight = ImGui::GetTextLineHeight();

	constexpr uint64_t BYTE_GROUP_SIZE = 8;

	for (uint64_t lineIndex = 0;
		lineIndex < mTotalLines;
		++lineIndex)
	{
		const uint64_t lineDataIndex =
			lineIndex * mCurrentBytesPerLine;

		if (lineDataIndex >= data.size())
			break;

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
			continue;

		std::size_t currentLength =
			static_cast<std::size_t>(writtenLength);

		for (uint64_t byteIndex = 0;
			byteIndex < bytesInLine;
			++byteIndex)
		{
			// 8바이트 이후 한 칸을 추가로 띄운다.
			if (byteIndex > 0 &&
				byteIndex % BYTE_GROUP_SIZE == 0)
			{
				if (currentLength + 1 >= sizeof(lineBuffer))
					break;

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
				break;

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
			static_cast<float>(lineIndex) * lineHeight);

		drawList->AddText(
			linePosition,
			textColor,
			lineBuffer);
	}

	/*
		ImDrawList로 직접 그린 내용은 ImGui 레이아웃 크기에
		자동 반영되지 않으므로 사용한 영역을 직접 확보한다.
	*/
	const uint64_t renderedLineCount =
		(static_cast<uint64_t>(data.size()) +
			mCurrentBytesPerLine - 1) /
		mCurrentBytesPerLine;

	const float renderedHeight =
		static_cast<float>(renderedLineCount) * lineHeight;

	ImGui::Dummy(
		ImVec2(
			mWindowWidth,
			renderedHeight));

	ImGui::End();
	ImGui::PopStyleColor();
}
