module;
#include <iostream>
#include <vector>

export module ChatProtocol;

using namespace std;

namespace Duck
{
	export enum class ECommand : std::uint8_t
	{
		CHAT = 0x00
	};

	export class ChatProtocol final
	{
	public:
		ChatProtocol() = delete;
		virtual ~ChatProtocol() = default;

		ChatProtocol(std::vector<uint8_t> payload, ECommand command) : mPayload(payload), mCommand(command)
		{
			mPayloadLength = mPayload.size();
			mCheckSum += static_cast<uint8_t>(mPayload.size() >> 8);
			mCheckSum += static_cast<uint8_t>(mPayload.size());
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
		size_t GetPayloadLength()
		{
			return mPayloadLength;
		}
		std::vector<uint8_t> GetPayload()
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
		static constexpr std::size_t HEADER_SIZE = 4;
		static constexpr std::size_t COMMAND_SIZE = 1;
		static constexpr std::size_t PAYLOAD_LENGTH_SIZE = 2;
		static constexpr std::size_t CHECKSUM_SIZE = 1;
		static constexpr std::size_t TAIL_SIZE = 4;
		static constexpr std::size_t PACKET_MIN_SIZE = HEADER_SIZE + COMMAND_SIZE + PAYLOAD_LENGTH_SIZE + CHECKSUM_SIZE + TAIL_SIZE;


	private:
		ECommand mCommand = ECommand::CHAT;
		size_t mPayloadLength;
		std::vector<std::uint8_t> mPayload;
		std::uint8_t mCheckSum = 0;

	};
}
