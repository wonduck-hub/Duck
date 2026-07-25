#include "BinaryDocument.h"

DuckHex::BinaryDocument::BinaryDocument()
	: mFileStream(), mDataByOffset(4096, 0), mFileSize(0)
{}

DuckHex::BinaryDocument::~BinaryDocument()
{
	Unload();
}

bool DuckHex::BinaryDocument::LoadFromFile(const filesystem::path& filePath)
{
	mFileStream.open(filePath, ios::out | ios::in | ios::binary);
	if (!mFileStream.is_open())
	{
		cerr << "Failed to open file: " << filePath << endl;
		return false;
	}

	if (mFileStream.is_open())
	{
		Unload();
	}

	const std::streampos currentPosition = mFileStream.tellg();

	mFileStream.seekg(0, std::ios::end);
	const std::streampos endPosition = mFileStream.tellg();

	mFileStream.seekg(currentPosition);

	assert(endPosition >= 0 && "Failed to determine file size. The end position is negative.");
	mFileSize = static_cast<std::uint64_t>(endPosition);

	return true;
}

void DuckHex::BinaryDocument::Unload()
{
	if (mFileStream.is_open())
	{
		mFileStream.close();
	}

	mDataByOffset = vector<uint8_t>(4096);
}

const vector<uint8_t> DuckHex::BinaryDocument::GetDataBySeekPositionOrNull(const uint64_t seekPosition, const uint64_t size) const
{
	assert(mFileStream.is_open() && "File stream is not open. Please load a file first.");
	assert(seekPosition < GetSize() && "Seek position is out of bounds. Please provide a valid position.");

	// TODO: 여기에 return 문을 삽입합니다.
	vector<uint8_t> result = vector<uint8_t>(4096, 0);

	return result;
}

uint64_t DuckHex::BinaryDocument::GetSize() const
{
	assert(mFileStream.is_open() && "File stream is not open. Please load a file first.");

	return mFileSize;
}
