#include <memory>
#include <vector>
#include <assert.h>
#include <cstddef>
#include <cstdint>
#include <utility>

import DuckClient;
import DuckPacket;

struct DuckClientHandle
{
	std::unique_ptr<Duck::DuckClient> Client;
};

extern "C"
{
	__declspec(dllexport) DuckClientHandle* __cdecl DuckClientConnect(const char* address, const char* port)
	{
		DuckClientHandle* handle = new DuckClientHandle();

		if (!Duck::DuckClient::TryConnect(address, port, handle->Client))
		{
			delete handle;
			return nullptr;
		}

		handle->Client->StartReceive();

		return handle;
	}

	__declspec(dllexport) void __cdecl DuckClientDisconnect(DuckClientHandle* handle)
	{
		assert(handle != nullptr);

		if (handle->Client != nullptr)
		{
			handle->Client->EndReceive();
		}
		delete handle;
	}

	__declspec(dllexport) std::int32_t __cdecl DuckClientSendChat(
		DuckClientHandle* handle,
		const uint8_t* payload,
		size_t payloadLength
	)
	{
		assert(handle != nullptr);
		assert(handle->Client != nullptr);

		std::vector<std::uint8_t> payloadBytes;
		if (payloadLength > 0)
		{
			if (payload == nullptr)
			{
				return 0;
			}

			payloadBytes.assign(payload, payload + payloadLength);
		}

		Duck::DuckPacket packet = Duck::DuckPacket(
			Duck::EVersion::VERSION1,
			static_cast<Duck::ECommand>(Duck::ECommand::CHAT),
			std::move(payloadBytes));

		return handle->Client->Send(packet) ? 1 : 0;
	}

	__declspec(dllexport) std::int32_t __cdecl DuckClientTryRecv(
		DuckClientHandle* handle,
		std::uint8_t* outChat,
		std::uint32_t chatCapacity,
		size_t* outChatLength
	)
	{
		assert(handle != nullptr);
		assert(handle->Client != nullptr);
		assert(outChatLength != nullptr);

		Duck::DuckPacket packet;
		if (!handle->Client->TryExtractPacket(packet))
		{
			return 0;
		}

		if (packet.GetVersion() != Duck::EVersion::VERSION1)
		{
			return 0;
		}

		if (packet.GetCommand() != Duck::ECommand::CHAT)
		{
			return 0;
		}

		// 이 방식은 Client GUI에서 한번에 보낼 수 있는 최대 길이를 정했다고 가정한 것
		// 따라서 GUI에서 사용자가 채팅을 입력할 때 chatCapacity보다 작게 입력하도록 강제 필요
		*outChatLength = packet.GetPayloadLength();

		if (*outChatLength > chatCapacity)
		{
			return 0;
		}

		std::vector<uint8_t> payload = packet.GetPayload();

		for (int index = 0; index < *outChatLength; ++index)
		{
			outChat[index] = payload[index];
		}

		return 1;
	}
}
