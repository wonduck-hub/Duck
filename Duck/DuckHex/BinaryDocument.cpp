#define NOMINMAX
#include "BinaryDocument.h"

DuckHex::BinaryDocument::BinaryDocument()
	: mFileStream(), mFileSize(0)
{}

DuckHex::BinaryDocument::~BinaryDocument()
{
	Unload();
}

bool DuckHex::BinaryDocument::LoadFile(const filesystem::path& filePath)
{
	if (mFileStream.is_open())
	{
		Unload();
	}

	mFileStream.open(filePath, ios::out | ios::in | ios::binary);
	if (!mFileStream.is_open())
	{
		cerr << "Failed to open file: " << filePath << endl;
		return false;
	}

	const std::streampos currentPosition = mFileStream.tellg();

	mFileStream.seekg(0, std::ios::end);
	const std::streampos endPosition = mFileStream.tellg();

	mFileStream.seekg(currentPosition);

	assert(endPosition != std::streampos(-1) && "Failed to determine file size.");
	mFileSize = static_cast<std::uint64_t>(endPosition);

	return true;
}

void DuckHex::BinaryDocument::Unload()
{
	if (mFileStream.is_open())
	{
		mFileStream.close();
	}
}

vector<uint8_t> DuckHex::BinaryDocument::GetDataBySeekPositionOrNull(const uint64_t seekPosition, const uint64_t size)
{
	assert(mFileStream.is_open() && "File stream is not open. Please load a file first.");
	assert(seekPosition < GetSize() && "Seek position is out of bounds. Please provide a valid position.");

	const uint64_t fileSize = GetSize();

	if (seekPosition >= fileSize)
		return {};

	// 파일 끝을 넘어가지 않도록 조정
	const uint64_t readSize =
		std::min(size, fileSize - seekPosition);

	std::vector<uint8_t> result(
		static_cast<size_t>(readSize));

	mFileStream.clear();

	mFileStream.seekg(
		static_cast<std::streamoff>(seekPosition),
		std::ios::beg);

	mFileStream.read(
		reinterpret_cast<char*>(result.data()),
		static_cast<std::streamsize>(readSize));

	// 실제 읽은 크기로 조정
	result.resize(
		static_cast<size_t>(mFileStream.gcount()));

	return result;
}

vector<uint64_t> DuckHex::BinaryDocument::FindBytePattern(const vector<uint8_t>& pattern)
{
	vector<uint64_t> result;

	if (!mFileStream.is_open() || pattern.empty() || pattern.size() > mFileSize)
	{
		return result;
	}

	const vector<uint8_t> fileData = GetDataBySeekPositionOrNull(0, mFileSize);
	const size_t patternSize = pattern.size();

	if (fileData.size() < patternSize)
	{
		return result;
	}

	const size_t searchEnd = fileData.size() - patternSize;

	for (size_t offset = 0; offset <= searchEnd; ++offset)
	{
		if (std::equal(pattern.begin(), pattern.end(), fileData.begin() + offset))
		{
			result.push_back(static_cast<uint64_t>(offset));
		}
	}

	return result;
}

bool DuckHex::BinaryDocument::FindFirstBytePattern(const vector<uint8_t>& pattern, uint64_t* outOffset)
{
	if (outOffset == nullptr)
	{
		return false;
	}

	const vector<uint64_t> offsets = FindBytePattern(pattern);

	if (offsets.empty())
	{
		return false;
	}

	*outOffset = offsets.front();
	return true;
}

bool DuckHex::BinaryDocument::OverwriteData(uint64_t offset, const vector<uint8_t>& data)
{
	assert(mFileStream.is_open() && "File stream is not open. Please load a file first.");

	if (data.empty())
	{
		return false;
	}

	if (offset >= mFileSize)
	{
		return false;
	}

	if (static_cast<uint64_t>(data.size()) > mFileSize - offset)
	{
		return false;
	}

	mFileStream.clear();
	mFileStream.seekp(
		static_cast<std::streamoff>(offset),
		std::ios::beg);

	mFileStream.write(
		reinterpret_cast<const char*>(data.data()),
		static_cast<std::streamsize>(data.size()));

	mFileStream.flush();

	return !mFileStream.fail();
}

uint64_t DuckHex::BinaryDocument::GetSize() const
{
	assert(mFileStream.is_open() && "File stream is not open. Please load a file first.");

	return mFileSize;
}
