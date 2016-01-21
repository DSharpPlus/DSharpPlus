# `unavailable: true` (server outtages)
okay
I shall enumerate this
sec
then I go to bed
Cases where "unavailable": true
- READY
    - You will get a GUILD_CREATE event in the future with unavailable: false
- GUILD_CREATE
    - You will get another one later with the actual data with unavailable: false.
    - You get this usually by accepting invites.
- GUILD_DELETE
    - The guild just became unavailable.


Cases where "unavailable": false

- GUILD_CREATE
    - Used to receive data of a recently unavailable guild.


Note that the "unavailable" key won't be always be available. If it is available then it means that the state changed. So
if it isn't false or true then it means it's not there.
hopefully I remembered all that right

#Redirects (No longer relevant)

kay so
Every message has an "s" value (sequence). You need the store the value of the last message you've seen.

If you connect to the wrong endpoint, or in the extremely rare case that new gateways are added, you will get a redirect opcode (opcode 7) with a url parameter of the new gateway to connect to.

After connecting to the new one, you send a resume opcode (6) with the properties session_id and seq, where seq is the last s value you've seen
Then server responds with opcode 0, t RESUMED and gives you a new heartbeat interval
