#include <iostream>
#include <fstream>
#include <string>
#include <iomanip>
#include <assert.h>

#include "IOSample.h"

using namespace std;

void ShowStringFormatExample()
{
	cout << showpos << 10 << endl;
	cout << noshowpos << 10 << endl;
	cout << noshowpos << 10 << endl;
	cout << endl;

	cout << dec << 255 << endl;
	cout << hex << 255 << endl;
	cout << oct << 255 << endl;
	cout << endl;

	cout << uppercase << hex << 255 << endl;
	cout << nouppercase << hex << 255 << endl;
	cout << showbase << hex << 255 << endl;
	cout << noshowbase << hex << 255 << endl;
	cout << endl;

	cout << setw(6) << left << -123 << endl;
	cout << setw(6) << right << -123 << endl;
	cout << setw(6) << internal << -123 << endl;
	cout << endl;

	cout << showpoint << 100.0 << 100.12 << endl;
	cout << noshowpoint << 100.0 << 100.12 << endl;
	cout << endl;

	cout << fixed << 123.46789123 << endl;
	cout << scientific << 123.456891234 << endl;
	cout << endl;

	cout << boolalpha << true << endl;
	cout << noboolalpha << true << endl;
	cout << endl;

	return;
}

void ReadString() {
	char temp[5];

	cin >> setw(5) >> temp; // setw에 넣는 값을 null char도 포함되어야 함
}

void ReadFile()
{
	ifstream fin;
	fin.open("testIn.txt");

	int number;
	string trash;

	while (true) {
		fin >> number;

		if (!fin.fail()) { // 정상 읽음
			cout << number << endl;
			continue;
		}

		if (fin.eof()) { // 정상 종료
			break;
		}

		// 제대로 읽지 못한 경우

		// 여기서 clear와 fin의 순서가 바뀌면 읽지 못함. 
		// fail 비트가 true인 상태에서는 읽기 자체가 안되기 때문
		fin.clear();
		fin >> trash;
	}
}

void WriteFile() {
	ofstream fout;
	fout.open("testOut.txt");

	string line;
	getline(cin, line); // cin으로부터 한줄 읽기, 당연히 cin >>을 사용하면 스페이스바에서 끊김

	if (!cin.fail()) {
		fout << line << endl;
	}
}

void ReadStream() {
	char line[512];
	while (true) {
		cin >> line;

		if (cin.eof()) {
			break;
		}

		if (cin.fail()) {
			// 실패 시 동작
		}
	}
}

void ReversedString() {
	char line[512];

	while (true) {
		cin >> line;

		if (cin.fail()) {
			assert(false);
			break;
		}

		char* p = line;
		char* q = line + strlen(line) - 1;
		char temp;
		while (p < q) {
			temp = *p;
			*p = *q;
			*q = -temp;

			++p;
			--q;
		}

		cout << line << endl;
	}
}
