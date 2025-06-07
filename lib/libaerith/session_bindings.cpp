#include<cstdint>
#include<cstring>
#include<vector>

#include<dave/mls/persisted_key_pair.h>
#include<dave/mls/session.h>

using namespace discord::dave;
using namespace discord::dave::mls;

using namespace mlspp;

// this code simply serves as a compatibility wrapper to ensure C# gets to work with char* rather than std::string
void (*mlsErrorHandler)(const char*, const char*);

void MLSErrorCallback(const std::string& source, const std::string& reason)
{
    const char* pSource = source.c_str();
    const char* pReason = reason.c_str();

    mlsErrorHandler(pSource, pReason);
}

// create a new session with ID and set the error handler. the error handler is always the same as far as C# is concerned, so we don't have to
// worry about potentially resetting it (it's a global variable).
extern "C" __declspec(__dllexport__) Session* AerithCreateSession
(
    const char* authSessionId,
    size_t authSessionLength,
    void (*errorHandler)(const char*, const char*)
)
{
    mlsErrorHandler = errorHandler;

    std::string authSession(authSessionId, authSessionLength);
    Session* session = new Session("unused", authSession, &MLSErrorCallback);
    return session;
}

// get a new signature key. this may either be entirely new or already persisted on-device. we don't really care which it is, but for us it's highly
// unlikely to not just be a new one
// returning a pointer to a std::shared_ptr is cursed but this has to survive contact with C# for a bit
extern "C" __declspec(__dllexport__) std::shared_ptr<SignaturePrivateKey>* AerithGetSignaturePrivateKey
(
    const char* sessionId,
    size_t sessionLength
)
{
    std::string session(sessionId, sessionLength);

    std::shared_ptr<SignaturePrivateKey> key = GetPersistedKeyPair("unused", session, 1);
    return &key;
}

// initializes a created session. C# has to first obtain a pointer to a SignaturePrivateKey above.
extern "C" __declspec(__dllexport__) void AerithInitSession
(
    Session* __restrict__ session,
    uint64_t groupId,
    const char* currentUserId,
    size_t currentUserLength,
    std::shared_ptr<SignaturePrivateKey>* privateTransientKey
)
{
    std::string currentUser(currentUserId, currentUserLength);

    session->Init(1, groupId, currentUser, *privateTransientKey);
}

// gets the amount of bytes needed to store the last epoch's authenticator.
extern "C" __declspec(__dllexport__) int32_t AerithGetLastEpochAuthenticatorSize(Session* __restrict__ session)
{
    std::vector<uint8_t> authenticator = session->GetLastEpochAuthenticator();
    return authenticator.size();
}

// gets the last epoch's authenticator and copies it to C#-provided memory
extern "C" __declspec(__dllexport__) void AerithGetLastEpochAuthenticator
(
    Session* __restrict__ session,
    uint8_t* buffer
)
{
    std::vector<uint8_t> authenticator = session->GetLastEpochAuthenticator();
    uint8_t* raw = &authenticator[0];
    memcpy(buffer, raw, authenticator.size());
}

// sets the voice gateway external sender
extern "C" __declspec(__dllexport__) void AerithSetExternalSender
(
    Session* __restrict__ session,
    const uint8_t* externalSenderPackage,
    size_t externalSenderLength
)
{
    std::vector<uint8_t> externalSender(externalSenderPackage, externalSenderPackage + externalSenderLength);
    session->SetExternalSender(externalSender);
}

// resets the current session. it must be re-initialized before further use.
extern "C" __declspec(__dllexport__) void AerithResetSession(Session* session)
{
    session->Reset();
}

// destroys an existing session.
extern "C" __declspec(__dllexport__) void AerithDestroySession(Session* session)
{
    delete session;
}