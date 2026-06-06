the folder structure here is a bit intuitive, so:

- in Bidirectional, Clientbound and Serverbound we put the largely DAVE-independent payloads depending on what direction they get sent in: clientbound = from server to client; serverbound = from client to server; bidirectional = both, naturally
- in DaveV1 we put DAVE-v1-specific payloads according to the same substructure
- if, hypothetically DAVE v2 ever releases, we'll fully duplicate any unchanged payloads into a DaveV2 folder instead of trying to share code, so that we can properly make the split in consuming code. matchingly, VoiceConnection.VGW.cs shouldn't reference dave at all (beyond needing to set the version of course), VoiceConnection.VGW.DaveV1.cs should only reference types and logic from DAVE v1, et cetera.