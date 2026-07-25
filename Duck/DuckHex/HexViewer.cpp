#include "HexViewer.h"

DuckHex::HexViewer::HexViewer()
	: mDocument(nullptr), mCurrentSeekPosition(0),
	mBytesPerLine(0), mTotalLines(0)
{}

DuckHex::HexViewer::HexViewer(BinaryDocument* document)
	: mDocument(document), mCurrentSeekPosition(0),
	mBytesPerLine(0), mTotalLines(0)
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
	assert(document != nullptr, "Document is null. Please provide a valid BinaryDocument.");

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
	assert(mDocument != nullptr, "Document is not loaded. Please load a document first.");
	assert(position < mDocument->GetSize(), "Seek position is out of bounds. Please provide a valid position.");

	mCurrentSeekPosition = position;
	return true;
}

void DuckHex::HexViewer::SetViewPortSize(float bytesPerLine, float totalLines)
{
	assert(bytesPerLine > 0 && totalLines > 0, "Bytes per line and total lines must be greater than zero.");

	mViewPortWidth = bytesPerLine;
	mViewPortHeight = totalLines;

	//TODO: BytesPerLine과 TotalLines를 기반으로 mBytesPerLine과 mTotalLines를 계산하는 로직을 추가해야 함
}

uint64_t DuckHex::HexViewer::GetCurrentSeekPosition() const
{
	return mCurrentSeekPosition;
}

uint64_t DuckHex::HexViewer::GetBytePerLine() const
{
	return mBytesPerLine;
}

uint64_t DuckHex::HexViewer::GetTotalLines() const
{
	return mTotalLines;
}

uint64_t DuckHex::HexViewer::GetDocumentSize() const
{
	assert(mDocument != nullptr, "Document is not loaded. Please load a document first.");
	return mDocument->GetSize();
}

void DuckHex::HexViewer::Render()
{
	//TOOD: ImDrawLine을 사용하여 HexViewer를 렌더링하는 로직을 추가해야 함
}
