#pragma once

#include "pch.h"

namespace DuckHex
{
	class BinaryDocument
	{
	private:
		fstream mFileStream;
		vector<uint8_t> mDataByOffset;
		uint64_t mFileSize;
		//TODO: 추후 수정, Undo/Redo 기능을 위해 변경에 대한 자료구조와 기능을 추가해야 함
		// 우선은 단순히 파일을 읽고 쓰는 기능만 구현

	public:
		BinaryDocument();
		~BinaryDocument();
		bool LoadFromFile(const filesystem::path& filePath);
		void Unload();
		const vector<uint8_t> GetDataBySeekPositionOrNull(const uint64_t seekPosition, const uint64_t size) const;
		uint64_t GetSize() const;
	};
}