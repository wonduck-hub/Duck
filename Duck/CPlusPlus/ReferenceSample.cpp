#include "ReferenceSample.h"

void Swap(int& a, int& b)
{
	/*
	컴퓨터 입장에서는 레퍼런스나 포인터나 똑같음
	메모리 접근이 굳이 필요하지 않을 떄 레퍼런스로 메모리 접근을 막아 사용자로 하여금 실수를 줄이자는 것이 목표
	또한 NULL이 될 수도 없음
	*/
	int temp = a;
	a = b;
	b = temp;
}
