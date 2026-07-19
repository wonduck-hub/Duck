#pragma once
#include <fstream>
#include <filesystem>

#include "DHexDocument.h"

using namespace std;

class DHexDocument {
private:
	fstream mFileStream;
	filesystem::path mPath;

public:
	DHexDocument(const filesystem::path& path);
	~DHexDocument();

	bool ChangeFile(const filesystem::path& path);
};

