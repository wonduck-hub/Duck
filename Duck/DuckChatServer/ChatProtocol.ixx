module;
#include <iostream>
#include <vector>
#include <deque>
#include <assert.h>

export module DuckPacket;

using namespace std;

namespace Duck
{
	export enum class ECommand : std::uint8_t
	{
		CHAT = 0x00,
		NONE = -1
	};

	export enum class EVersion : std::uint8_t
	{
		VERSION1 = 1,
		NONE = -1
	};

	export enum class ETryParsePacketResult
	{
		Incomplete,
		Invalid,
		Success
	};

	export class DuckPacket final
	{
	public:
		DuckPacket()
		{
			mVersion = Duck::EVersion::NONE;
			mCommand = Duck::ECommand::NONE;
		}
		virtual ~DuckPacket() = default;

		DuckPacket(EVersion version, ECommand command, std::vector<uint8_t> payload) : mVersion(version), mCommand(command), mPayload(payload)
		{
			mPayloadLength = mPayload.size();
			mCheckSum += static_cast<uint8_t>(mVersion);
			mCheckSum += static_cast<uint8_t>(mPayload.size() >> 8);
			mCheckSum += static_cast<uint8_t>(mPayload.size());
			mCheckSum += static_cast<uint8_t>(mCommand);

			for (int i = 0; i < mPayloadLength; ++i)
			{
				mCheckSum += payload[i];
			}
		}

		// Getter
		EVersion GetVersion() const
		{
			return mVersion;
		}
		ECommand GetCommand() const
		{
			return mCommand;
		}
		size_t GetPayloadLength() const
		{
			return mPayloadLength;
		}
		std::vector<uint8_t> GetPayload() const
		{
			return mPayload;
		} 
		std::uint8_t GetCheckSum() const
		{
			return mCheckSum;
		}
		std::size_t GetProtocolSize() const
		{
			return HEADER_SIZE
				+ VERSION_SIZE
				+ COMMAND_SIZE
				+ PAYLOAD_LENGTH_SIZE
				+ mPayload.size()
				+ CHECKSUM_SIZE
				+ TAIL_SIZE;
		}

		// Setter

		// Parser
		// TODO: 추후 TryParsepacket 과 같은 파싱 함수를 DuckProtocol 이라는 정적 클래스로 분리 고려 [2026.10.20]
		static ETryParsePacketResult TryParsePacket(
			const std::deque<std::uint8_t>& buffer,
			DuckPacket& outProtocol) 
		{
			const std::size_t HEADER_SIZE = Duck::DuckPacket::HEADER_SIZE;
			const std::size_t VERSION_IDNEX = HEADER_SIZE;
			const std::size_t COMMAND_INDEX = VERSION_IDNEX + VERSION_SIZE;
			const std::size_t PAYLOAD_LENGTH_INDEX = COMMAND_INDEX + Duck::DuckPacket::COMMAND_SIZE;
			const std::size_t PAYLOAD_START_INDEX = PAYLOAD_LENGTH_INDEX + Duck::DuckPacket::PAYLOAD_LENGTH_SIZE;
			const std::size_t LENGTH_FIELD_END = PAYLOAD_START_INDEX;

			if (buffer.size() < LENGTH_FIELD_END)
			{
				return ETryParsePacketResult::Incomplete;
			}

			const std::size_t PAYLOAD_SIZE =
				static_cast<std::size_t>(buffer[PAYLOAD_LENGTH_INDEX])
				| (static_cast<std::size_t>(buffer[PAYLOAD_LENGTH_INDEX + 1]) << 8);
			const std::size_t CHECKSUM_INDEX = PAYLOAD_START_INDEX + PAYLOAD_SIZE;
			const std::size_t TAIL_INDEX = CHECKSUM_INDEX + Duck::DuckPacket::CHECKSUM_SIZE;
			const std::size_t PACKET_SIZE = Duck::DuckPacket::PACKET_MIN_SIZE + PAYLOAD_SIZE;

			if (buffer.size() < PACKET_SIZE)
			{
				return ETryParsePacketResult::Incomplete;
			}

			if (buffer[TAIL_INDEX] != Duck::DuckPacket::TAIL[0]
				|| buffer[TAIL_INDEX + 1] != Duck::DuckPacket::TAIL[1]
				|| buffer[TAIL_INDEX + 2] != Duck::DuckPacket::TAIL[2]
				|| buffer[TAIL_INDEX + 3] != Duck::DuckPacket::TAIL[3])
			{
				return ETryParsePacketResult::Invalid;
			}

			Duck::EVersion parsedVersion;
			switch (buffer[VERSION_IDNEX])
			{
			case static_cast<int>(Duck::EVersion::VERSION1):
				parsedVersion = Duck::EVersion::VERSION1;
				break;
			default:
				assert(false && "EVersion 처리 누락 또는 잘못된 버전");
				return ETryParsePacketResult::Invalid;
			}

			Duck::ECommand parsedCommand;
			switch (buffer[COMMAND_INDEX])
			{
			case static_cast<int>(Duck::ECommand::CHAT):
				parsedCommand = Duck::ECommand::CHAT;
				break;
			default:
				assert(false && "ECommand 처리 누락 또는 잘못된 명령");
				return ETryParsePacketResult::Invalid;
			}

			std::uint8_t calcCheckSum = 0;
			calcCheckSum += static_cast<std::uint8_t>(parsedVersion);
			calcCheckSum += static_cast<std::uint8_t>(parsedCommand);
			calcCheckSum += buffer[PAYLOAD_LENGTH_INDEX];
			calcCheckSum += buffer[PAYLOAD_LENGTH_INDEX + 1];

			// TODO: 추후 switch 문으로 version에 따른 Payload 파싱 함수로 변환 필요
			std::vector<std::uint8_t> parsedPayload;
			parsedPayload.reserve(PAYLOAD_SIZE);
			for (std::size_t payloadIndex = 0; payloadIndex < PAYLOAD_SIZE; ++payloadIndex)
			{
				const std::uint8_t payloadByte = buffer[PAYLOAD_START_INDEX + payloadIndex];
				parsedPayload.push_back(payloadByte);
				calcCheckSum += payloadByte;
			}

			if (calcCheckSum != buffer[CHECKSUM_INDEX])
			{
				return ETryParsePacketResult::Invalid;
			}

			outProtocol = DuckPacket(parsedVersion, parsedCommand, parsedPayload);

			return ETryParsePacketResult::Success;
		}

		static bool IsHeaderAtBufferFront(const std::deque<std::uint8_t>& buffer)
		{
			return buffer[0] == Duck::DuckPacket::HEADER[0]
				&& buffer[1] == Duck::DuckPacket::HEADER[1]
				&& buffer[2] == Duck::DuckPacket::HEADER[2]
				&& buffer[3] == Duck::DuckPacket::HEADER[3];
		}

	private:

	public:
		static constexpr std::uint8_t HEADER[4] = { 0xFF, 0x00, 0xFF, 0x00 };
		static constexpr std::uint8_t TAIL[4] = { 0x00, 0xFF, 0x00, 0xFF };
		static constexpr std::size_t HEADER_SIZE = 4;
		static constexpr std::size_t VERSION_SIZE = 1;
		static constexpr std::size_t COMMAND_SIZE = 1;
		static constexpr std::size_t PAYLOAD_LENGTH_SIZE = 2;
		static constexpr std::size_t CHECKSUM_SIZE = 1;
		static constexpr std::size_t TAIL_SIZE = 4;
		static constexpr std::size_t PACKET_MIN_SIZE = HEADER_SIZE + VERSION_SIZE + COMMAND_SIZE + PAYLOAD_LENGTH_SIZE + CHECKSUM_SIZE + TAIL_SIZE;


	private:
		EVersion mVersion;
		ECommand mCommand = ECommand::CHAT;
		size_t mPayloadLength;
		std::vector<std::uint8_t> mPayload;
		std::uint8_t mCheckSum = 0;

	};
}
