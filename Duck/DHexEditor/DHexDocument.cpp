#include "pch.h"
#include "DHexDocument.h"

DHexDocument::DHexDocument(const filesystem::path& path)
	: mPath(path)
{
	mFileStream.open(path, ios_base::in | ios_base::out | ios_base::binary);

	if (mFileStream.is_open())
	{
		throw std::runtime_error("Failed to open the file.");
	}
}

DHexDocument::~DHexDocument() {
	if (mFileStream.is_open()) {
		mFileStream.close();
	}
}

bool DHexDocument::ChangeFile(const filesystem::path& path)
{
	if (mFileStream.is_open()) {
		mFileStream.close();
		mFileStream.open(path, ios_base::in | ios_base::out | ios_base::binary);
	}
	if (!mFileStream.is_open())
	{
		return false;
	}
	return true;
}
