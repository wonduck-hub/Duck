#pragma once

#include <memory>
#include <assert.h>

template <typename T>
class Vector
{
public:
	Vector() = delete;
	virtual ~Vector() = default;
	Vector(const size_t capacity);
	Vector(const Vector& other);

	void Add(const T& value);
	void Insert(const T& value, const size_t index);
	void RemoveAt(const size_t index);
	void Clear();

	size_t GetSize() const;
	size_t GetCapacity() const;
	void SetCapacity(const size_t capacity);

	T& operator[](const size_t index);
	const T& operator[](const size_t index) const;

private:
	std::unique_ptr<T[]> mData;
	size_t mSize = 0;
	size_t mCapacity = 0;
};

template<typename T>
inline Vector<T>::Vector(const size_t capacity)
	: mCapacity(capacity)
{
	mData.reset(new T[capacity]);
}

template<typename T>
inline Vector<T>::Vector(const Vector& other)
	: mCapacity(other.mCapacity), mSize(other.mSize)
{
	mData.reset(new T[mCapacity]);
	for (size_t i = 0; i < mSize; ++i)
	{
		mData[i] = other.mData[i];
	}
}

template<typename T>
inline void Vector<T>::Add(const T& value)
{
	if (mSize >= mCapacity)
	{
		SetCapacity(mCapacity == 0 ? 1 : mCapacity * 2);

	}

	mData[mSize] = value;
	++mSize;
}

template<typename T>
inline void Vector<T>::Insert(const T& value, const size_t index)
{
	assert(index <= mSize && "Vector index out of range");

	if (mSize >= mCapacity)
	{
		SetCapacity(mCapacity == 0 ? 1 : mCapacity * 2);
	}

	for (size_t i = mSize; i > index; --i)
	{
		mData[i] = mData[i - 1];
	}
	mData[index] = value;
	++mSize;
}

template<typename T>
inline void Vector<T>::RemoveAt(const size_t index)
{
	assert(index < mSize && "Vector index out of range");
	for (size_t i = index; i < mSize - 1; ++i)
	{
		mData[i] = mData[i + 1];
	}
	--mSize;
}

template<typename T>
inline void Vector<T>::Clear()
{
	mSize = 0;
}

template<typename T>
inline size_t Vector<T>::GetSize() const
{
	return mSize;
}

template<typename T>
inline size_t Vector<T>::GetCapacity() const
{
	return mCapacity;
}

template<typename T>
inline void Vector<T>::SetCapacity(const size_t capacity)
{
	if (capacity == mCapacity)
	{
		return;
	}

	std::unique_ptr<T[]> newData(new T[capacity]);
	for (size_t i = 0; i < (std::min)(mSize, capacity); ++i)
	{
		newData[i] = std::move(mData[i]);
	}
	mData = std::move(newData); // 기존 unique_ptr는 해제되고 새로운 unique_ptr로 교체
	mCapacity = capacity;

	if (mSize > mCapacity)
	{
		mSize = mCapacity;
	}
}

template<typename T>
inline T& Vector<T>::operator[](const size_t index) // 값을 대입할 때 사용하는 용도
{
	assert(index < mSize && "Vector index out of range");
	return mData[index];
}

template<typename T>
inline const T& Vector<T>::operator[](const size_t index) const // 읽을 때만 사용하는 용도
{
	assert(index < mSize && "Vector index out of range");
	return mData[index];
}
