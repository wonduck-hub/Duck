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

    return ;
}

void ReadString() {
    char temp[5];

    cin >> setw(5) >> temp; // setw에 넣는 값을 null char도 포함되어야 함
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
            *q =- temp;

            ++p;
            --q;
        }

        cout << line << endl;
    }
}
