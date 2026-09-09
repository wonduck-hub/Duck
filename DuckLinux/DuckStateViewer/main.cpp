#include <iostream>
#include <vector>

using namespace std;

int main(void)
{
    vector<int> nums;
    nums.reserve(10);

    nums.push_back(10);
    nums.push_back(20);
    nums.push_back(30);

    vector<int>::iterator it = nums.begin();
    it = nums.insert(it, 100);

    cout << "Capacity: " << nums.capacity() << endl;
    cout << "Size: " << nums.size() << endl;
    cout << endl;

    for (vector<int>::iterator iter = nums.begin(); iter != nums.end(); ++iter)
    {
        cout << *iter << endl;
    }
    for (vector<int>::reverse_iterator iter = nums.rbegin(); iter != nums.rend(); ++iter)
    {
        cout << *iter << endl;
    }

    return 0;
}