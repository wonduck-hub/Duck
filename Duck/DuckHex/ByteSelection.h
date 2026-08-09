#pragma once

#include <memory>
#include <optional>
#include <vector>

namespace DuckHex
{
	class ByteSelection
	{
	private:
		uint64_t mAnchor = 0;
		uint64_t mCursor = 0;
	public:
		ByteSelection();
		void Clear();
		void SetRange(uint64_t begin, uint64_t end);
		void SetSingleByte(uint64_t offset);
		uint64_t Begin() const;
		uint64_t End() const;
		uint64_t Length() const;
		bool IsEmpty() const;
		bool Contains(uint64_t offset) const;
	};
}
