module;
#include <iostream>
#include <EASTL/vector.h>

export module ChatProtocol;

using namespace std;

enum class ECommand : std::uint8_t
{
	CHAT = 0x00
};

export class ChatProtocal final
{
public:
	ChatProtocal() = delete;
	virtual ~ChatProtocal() = default;

	ChatProtocal(eastl::vector<uint8_t> payload, ECommand command) : mPayload(payload), mCommand(command)
	{
		mPayloadLength = mPayload.size();
		mCheckSum += static_cast<uint8_t>(mCommand);

		for (int i = 0; i < mPayloadLength; ++i)
		{
			mCheckSum += payload[i];
		}
	}

	// Getter
	ECommand GetCommand() 
	{
		return mCommand;
	}
	std::uint16_t GetPayloadLength()
	{
		return mPayloadLength;
	}
	eastl::vector<uint8_t> GetPayload()
	{
		return mPayload;
	}
	std::uint8_t GetCheckSum()
	{
		return mCheckSum;
	}

	// Setter

private:

public:
	static constexpr std::uint8_t HEADER[4] = { 0xFF, 0x00, 0xFF, 0x00 };
	static constexpr std::uint8_t TAIL[4] = { 0x00, 0xFF, 0x00, 0xFF };

	

private:
	ECommand mCommand = ECommand::CHAT;
	std::uint16_t mPayloadLength;
	eastl::vector<std::uint8_t> mPayload;
	std::uint8_t mCheckSum = 0;

};