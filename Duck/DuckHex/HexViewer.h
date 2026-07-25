#pragma once
#include "pch.h"

namespace DuckHex
{
	class HexViewer
	{
	private:
		BinaryDocument* mDocument;
		uint64_t mCurrentSeekPosition;
		uint64_t mBytesPerLine;
		uint64_t mTotalLines;
		float mViewPortWidth;
		float mViewPortHeight;

	public:
		HexViewer();
		HexViewer(BinaryDocument* document);
		~HexViewer();
		bool LoadDocument(BinaryDocument* document);
		void UnloadDocument();
		bool SetCurrentSeekPosition(uint64_t position);
		void SetViewPortSize(float windowWidth, float windowHeight);
		uint64_t GetCurrentSeekPosition() const;
		uint64_t GetBytePerLine() const;
		uint64_t GetTotalLines() const;
		uint64_t GetDocumentSize() const;
	};
}