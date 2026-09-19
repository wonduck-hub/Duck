#pragma once

#include <memory>
#include <cassert>
#include <cstddef>

template <typename T>
struct Node
{
public:
    explicit Node(const T& value);

public:
    T mValue;
    std::shared_ptr<Node<T>> mNext;
    std::weak_ptr<Node<T>> mPrev;
};

template <typename T>
Node<T>::Node(const T& value)
    : mValue(value)
{}

template <typename T>
class LinkedList
{
public:
    LinkedList() = default;
    explicit LinkedList(const T& value);
    LinkedList(const LinkedList& other);

    ~LinkedList() = default;

    size_t GetSize() const;

    void Clear();
    void RemoveAt(size_t index);
    void Insert(const T& value, size_t index);
    void Add(const T& value);

private:
    std::shared_ptr<Node<T>> mHead;
    std::weak_ptr<Node<T>> mTail;
    size_t mSize = 0;
};

template <typename T>
LinkedList<T>::LinkedList(const T& value)
{
    auto node = std::make_shared<Node<T>>(value);

    mHead = node;
    mTail = node;
    mSize = 1;
}

template <typename T>
LinkedList<T>::LinkedList(const LinkedList& other)
{
    auto current = other.mHead;

    while (current != nullptr)
    {
        Add(current->mValue);
        current = current->mNext;
    }
}

template <typename T>
size_t LinkedList<T>::GetSize() const
{
    return mSize;
}

template <typename T>
void LinkedList<T>::Clear()
{
    mHead.reset();
    mTail.reset();
    mSize = 0;
}

template <typename T>
void LinkedList<T>::Add(const T& value)
{
    auto newNode = std::make_shared<Node<T>>(value);

    if (mHead == nullptr)
    {
        mHead = newNode;
        mTail = newNode;
        mSize = 1;
        return;
    }

    auto tail = mTail.lock();

    assert(tail != nullptr);

    tail->mNext = newNode;
    newNode->mPrev = tail;

    mTail = newNode;

    ++mSize;
}

template <typename T>
void LinkedList<T>::Insert(const T& value, size_t index)
{
    assert(index <= mSize && "List index out of range");

    if (index == mSize)
    {
        Add(value);
        return;
    }

    auto newNode = std::make_shared<Node<T>>(value);

    if (index == 0)
    {
        newNode->mNext = mHead;

        if (mHead != nullptr)
        {
            mHead->mPrev = newNode;
        }

        mHead = newNode;

        if (mSize == 0)
        {
            mTail = newNode;
        }

        ++mSize;
        return;
    }

    auto current = mHead;

    for (size_t i = 0; i < index; ++i)
    {
        current = current->mNext;
    }

    assert(current != nullptr);

    auto prev = current->mPrev.lock();

    assert(prev != nullptr);

    newNode->mPrev = prev;
    newNode->mNext = current;

    prev->mNext = newNode;
    current->mPrev = newNode;

    ++mSize;
}

template <typename T>
void LinkedList<T>::RemoveAt(size_t index)
{
    assert(index < mSize && "List index out of range");

    if (index == 0)
    {
        mHead = mHead->mNext;

        if (mHead != nullptr)
        {
            mHead->mPrev.reset();
        }
        else
        {
            mTail.reset();
        }

        --mSize;
        return;
    }

    auto current = mHead;

    for (size_t i = 0; i < index; ++i)
    {
        current = current->mNext;
    }

    assert(current != nullptr);

    auto prev = current->mPrev.lock();
    auto next = current->mNext;

    assert(prev != nullptr);

    prev->mNext = next;

    if (next != nullptr)
    {
        next->mPrev = prev;
    }
    else
    {
        mTail = prev;
    }

    --mSize;
}