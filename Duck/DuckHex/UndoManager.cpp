#include "UndoManager.h"

namespace DuckHex
{
	UndoManager::UndoManager()
	{}
	UndoManager::~UndoManager()
	{}

	void DuckHex::UndoManager::AddAction(const UndoAction& action)
	{
		mUndoStack.push_back(action);
		mRedoStack.clear();
	}

	std::optional<UndoManager::UndoAction> DuckHex::UndoManager::Undo()
	{
		if (mUndoStack.empty())
			return std::nullopt;

		auto action = mUndoStack.back();
		mUndoStack.pop_back();
		mRedoStack.push_back(action);
		return std::optional<UndoAction>(action);
	}

	std::optional<UndoManager::UndoAction> DuckHex::UndoManager::Redo()
	{
		if (mRedoStack.empty())
			return std::nullopt;

		auto action = mRedoStack.back();
		mRedoStack.pop_back();
		mUndoStack.push_back(action);
		return std::optional<UndoAction>(action);
	}

	bool DuckHex::UndoManager::CanUndo() const
	{
		return !mUndoStack.empty();
	}

	bool DuckHex::UndoManager::CanRedo() const
	{
		return !mRedoStack.empty();
	}

}