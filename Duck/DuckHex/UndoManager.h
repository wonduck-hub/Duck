#pragma once

#include <memory>
#include <optional>
#include <vector>

namespace DuckHex
{
	class UndoManager
	{
	public:
		class UndoAction
		{
		private:
			size_t mOffset;
			std::vector<uint8_t> mOldData;
			std::vector<uint8_t> mNewData;

		public:
			UndoAction(size_t offset, const std::vector<uint8_t>& oldData, const std::vector<uint8_t>& newData)
				: mOffset(offset), mOldData(oldData), mNewData(newData)
			{}
			size_t GetOffset() const { return mOffset; }
			const std::vector<uint8_t>& GetOldData() const { return mOldData; }
			const std::vector<uint8_t>& GetNewData() const { return mNewData; }
		};

	private:
		std::vector<UndoAction> mUndoStack;
		std::vector<UndoAction> mRedoStack;

	public:
		UndoManager();
		~UndoManager();
		void AddAction(const UndoAction& action);
		std::optional<UndoManager::UndoAction> Undo();
		std::optional<UndoManager::UndoAction> Redo();
		bool CanUndo() const;
		bool CanRedo() const;
	};
}
