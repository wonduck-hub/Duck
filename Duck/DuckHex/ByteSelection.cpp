#include "ByteSelection.h"

#include <algorithm>

DuckHex::ByteSelection::ByteSelection() : mAnchor(0), mCursor(0)
{}

void DuckHex::ByteSelection::Clear()
{
	mAnchor = 0;
	mCursor = 0;
}

void DuckHex::ByteSelection::SetRange(uint64_t begin, uint64_t end)
{
	mAnchor = begin;
	mCursor = end;
}

void DuckHex::ByteSelection::SetSingleByte(uint64_t offset)
{
	mAnchor = offset;
	mCursor = offset + 1;
}

uint64_t DuckHex::ByteSelection::Begin() const
{
	return std::min(mAnchor, mCursor);
}

uint64_t DuckHex::ByteSelection::End() const
{
	return std::max(mAnchor, mCursor);
}

uint64_t DuckHex::ByteSelection::Length() const
{
	return End() - Begin();
}

bool DuckHex::ByteSelection::IsEmpty() const
{
	return mAnchor == mCursor;
}

bool DuckHex::ByteSelection::Contains(uint64_t offset) const
{
	return offset >= Begin() && offset < End();
}
