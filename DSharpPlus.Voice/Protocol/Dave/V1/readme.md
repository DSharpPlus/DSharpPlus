# Implementation Notes for DAVE, Protocol Version 1

## 1. MLS

- MLS protocol version is 1.0
- MLS ciphersuite is `DHKEMP256_AES128GCM_SHA256_P256` (MLS ciphersuite 2)
- One group extension: External Senders
  - Includes one external sender for voice gateway to send external proposals for add/remove
- No leaf node extensions
- Allowed credential type for members and external sender is basic only

## 2. Voice Payloads

Terminology:
- Clientbound means that the packet is sent *to* the client.
- Serverbound means that the packet is sent *to* the server.
- Bidirectional should be fairly self-explanatory

Empty payloads do, naturally, not have an associated class.

### 2.1. Opcode 0 - Identify - serverbound, plaintext JSON

| Type | Field Name | Optional | Notes |
| ---- | ---------- | -------- | ----- |
| snowflake | server_id | yes | The snowflake ID of the guild containing this voice channel. Theoretically 0 for DMs, but that doesn't apply to us. |
| snowflake | user_id | no | The snowflake ID of the current user. |
| string | session_id | no | The identifier of the current voice session. |
| string | token | no | The token for this connection. |
| int | max_dave_protocol_version | no | The maximum supported DAVE protocol version, ranging from 0 to 1. DSharpPlus always sends 1. |

### 2.2. Opcode 1 - Select Protocol - serverbound, plaintext JSON

| Type | Field Name | Optional | Notes |
| ---- | ---------- | -------- | ----- |
| string | protocol | no | The protocol mode to use for the transport connection, either `udp` for legacy connections or `webrtc` for modern connections. |
| object | data | no | Information passed about the transport connection, see table below. |

Select Protocol Data structure:

| Type | Field Name | Optional | Notes |
| ---- | ---------- | -------- | ----- |
| string | address | no | The discovered IP address we'll stream data to. |
| int | port | no | The discovered port we'll stream data to. |
| string | mode | no | The encryption mode of the current connection. Currently, `aead_aes256_gcm_rtpsize` and `aead_xchacha20_poly1305_rtpsize` are supported. |

Note: This payload is where codecs are selected. For lack of any documentation, DSharpPlus cannot currently implement this feature, with unforeseeable consequences.

### 2.3. Opcode 2 - Ready - clientbound, plaintext JSON

| Type | Field Name | Optional | Notes |
| ---- | ---------- | -------- | ----- |
| int | ssrc | no | Informs the client of its SSRC for the current connection. |
| string | ip | no | Informs the client of the IP address for the current connection. |
| int | port | no | Informs the client of the port for the current connection. |
| string[] | modes | no | Informs the client of supported encryption modes for the current connection. |

There is a heartbeat interval field sent as part of this packet, but it is incorrect and cannot be used. We therefore neglect it entirely.

### 2.4. Opcode 3 - Heartbeat - serverbound, plaintext JSON

| Type | Field Name | Optional | Notes |
| ---- | ---------- | -------- | ----- |
| long | nonce | no | An integer nonce constructed using the current timestamp. |
| int | seq_ack | no | The last received sequence number received from the gateway. This is utilized for resume streaming missed payloads to us. |

### 2.5. Opcode 4 - Session Description - clientbound, plaintext JSON

| Type | Field Name | Optional | Notes |
| ---- | ---------- | -------- | ----- |
| string | mode | no | The encryption mode selected by the server, based on which modes it and the client support. |
| byte[] | secret_key | no | The secret key used for transport encryption. |
| int | dave_protocol_version | no | The DAVE protocol version selected by the server, based on which versions the client and the existing call support. |

### 2.6. Opcode 5 - Speaking - bidirectional, plaintext JSON