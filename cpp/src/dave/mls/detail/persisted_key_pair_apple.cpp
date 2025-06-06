#include "dave/mls/detail/persisted_key_pair.h"

#include <cassert>
#include <filesystem>
#include <fstream>
#include <functional>
#include <mutex>
#include <sstream>
#include <string>

#include <CoreFoundation/CoreFoundation.h>
#include <Security/Security.h>
#include <unistd.h>

#include <bytes/bytes.h>
#include <mls/crypto.h>

#include "dave/logger.h"
#include "dave/mls/parameters.h"

static const CFStringRef KeyServiceLabel = CFSTR("Discord Secure Frames Key");
static const std::string KeyLabelPrefix = "Discord Secure Frames Key: ";
static const std::string KeyTagPrefix = "discord-secure-frames-key-";

static void AddAccessGroup([[maybe_unused]] CFMutableDictionaryRef dict)
{
#if TARGET_OS_IPHONE
    CFDictionaryAddValue(dict, kSecAttrAccessGroup, CFSTR("group.com.hammerandchisel.discord"));
#endif
}

template <class T = CFTypeRef>
struct ScopedCFTypeRef {
    ScopedCFTypeRef() = default;
    ScopedCFTypeRef(T ref)
      : ref_(ref)
    {
    }
    ScopedCFTypeRef(ScopedCFTypeRef& other)
      : ref_(other.ref_)
    {
        if (ref_) {
            CFRetain(ref_);
        }
    }
    ScopedCFTypeRef(ScopedCFTypeRef&& other)
      : ref_(std::exchange(other.ref_, nullptr))
    {
    }

    ~ScopedCFTypeRef() { release(); }

    ScopedCFTypeRef& operator=(T ref)
    {
        release();
        ref_ = ref;
        return *this;
    }

    void release()
    {
        if (ref_) {
            CFRelease(ref_);
        }
        ref_ = nullptr;
    }

    T& get() { return ref_; }

    T* getPtr() { return &ref_; }
    CFTypeRef* getGenericPtr() { return (CFTypeRef*)getPtr(); }

    operator T&() { return get(); }

    explicit operator bool() { return ref_ != nullptr; }

    T ref_ = nullptr;
};

static std::string ConvertCFString(CFStringRef string)
{
    if (const char* str = CFStringGetCStringPtr(string, kCFStringEncodingUTF8)) {
        return str;
    }

    CFIndex len = CFStringGetLength(string);
    std::string ret(CFStringGetMaximumSizeForEncoding(len, kCFStringEncodingUTF8), 0);

    CFStringGetBytes(string,
                     CFRangeMake(0, len),
                     kCFStringEncodingUTF8,
                     '?',
                     false,
                     (UInt8*)ret.data(),
                     ret.size(),
                     &len);

    ret.resize(len);

    return ret;
}

static std::string SecStatusToString(OSStatus status)
{
    std::string ret = std::to_string(status);

    if (__builtin_available(macOS 10.3, iOS 11.3, *)) {
        ScopedCFTypeRef string = SecCopyErrorMessageString(status, NULL);
        if (string) {
            ret += " (";
            ret += ConvertCFString(string);
            ret += ")";
        }
    }

    return ret;
}

static std::string ErrorToString(CFErrorRef error)
{
    if (!error) {
        return "(null)";
    }

    if (__builtin_available(macOS 10.3, iOS 11.3, *)) {
        OSStatus status = CFErrorGetCode(error);
        ScopedCFTypeRef string = SecCopyErrorMessageString(status, NULL);
        if (string) {
            std::string ret = std::to_string(status);

            ret += " (";
            ret += ConvertCFString(string);
            ret += ")";

            return ret;
        }
    }

    if (ScopedCFTypeRef string = CFErrorCopyDescription(error)) {
        return ConvertCFString(string);
    }

    return "(unknown)";
}

namespace discord {
namespace dave {
namespace mls {
namespace detail {

std::shared_ptr<::mlspp::SignaturePrivateKey> GetNativePersistedKeyPair(
  [[maybe_unused]] KeyPairContextType ctx,
  const std::string& id,
  ::mlspp::CipherSuite suite,
  bool& supported)
{
    std::shared_ptr<::mlspp::SignaturePrivateKey> ret;

    CFStringRef keyType = nullptr;
    int keySize = 0;
    std::function<bytes(CFDataRef)> convertKey;

    ScopedCFTypeRef query = CFDictionaryCreateMutable(
      NULL, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);

    CFDictionaryAddValue(query, kSecReturnRef, kCFBooleanTrue);
    CFDictionaryAddValue(query, kSecUseAuthenticationUI, kSecUseAuthenticationUISkip);
    AddAccessGroup(query);

    auto suiteId = suite.cipher_suite();
    switch (suiteId) {
    case ::mlspp::CipherSuite::ID::P256_AES128GCM_SHA256_P256:
    case ::mlspp::CipherSuite::ID::P384_AES256GCM_SHA384_P384:
    case ::mlspp::CipherSuite::ID::P521_AES256GCM_SHA512_P521:
        supported = true;
        keyType = kSecAttrKeyTypeECSECPrimeRandom;
        if (suiteId == ::mlspp::CipherSuite::ID::P521_AES256GCM_SHA512_P521) {
            keySize = 521;
        }
        else if (suiteId == ::mlspp::CipherSuite::ID::P384_AES256GCM_SHA384_P384) {
            keySize = 384;
        }
        else {
            keySize = 256;
        }
        convertKey = [keySize](CFDataRef data) {
            // https://developer.apple.com/documentation/security/1643698-seckeycopyexternalrepresentation
            // Input has a 1-byte header (always 0x04, per ANSI X9.63), followed by 3
            // keySize-bit left-padded byte-aligned big-endian integers: X, Y, and K.
            // X and Y are the public key (represented as the coordinates);
            // K is the private key.
            bytes ret;
            constexpr size_t HeaderSize = 1;
            constexpr size_t ValueCount = 3;
            constexpr size_t PublicValues = 2;
            constexpr uint8_t HeaderByte = 0x04;

            // Convert keySize from bits to bytes (rounding up)
            CFIndex byteLen = (keySize + 7) / 8;

            CFIndex len = CFDataGetLength(data);
            if (len < 0 || (size_t)len < HeaderSize + ValueCount * byteLen) {
                DISCORD_LOG(LS_ERROR)
                  << "Exported key blob too small in GetPersistedKeyPair/convertKey: " << len;
                return ret;
            }

            const uint8_t* ptr = CFDataGetBytePtr(data);
            if (ptr[0] != HeaderByte) {
                DISCORD_LOG(LS_ERROR)
                  << "Exported key blob has unexpected format in GetPersistedKeyPair/convertKey: "
                  << ptr[0];
                return ret;
            }

            // Skip header, X, and Y, and extract K.
            ptr += HeaderSize + PublicValues * byteLen;
            ret.as_vec().assign(ptr, ptr + byteLen);

            return ret;
        };
        break;
    default:
        // Other suites will need to store keys as generic data items
        return nullptr;
    }

    assert(keyType && keySize && convertKey);

    ScopedCFTypeRef<CFNumberRef> sizeRef = CFNumberCreate(NULL, kCFNumberIntType, &keySize);

    std::string labelString = KeyLabelPrefix + id;
    std::string tagString = KeyTagPrefix + id;
    ScopedCFTypeRef labelStringRef =
      CFStringCreateWithCString(NULL, labelString.c_str(), kCFStringEncodingUTF8);
    ScopedCFTypeRef tagDataRef =
      CFDataCreate(NULL, (const UInt8*)tagString.c_str(), tagString.size());

    CFDictionaryAddValue(query, kSecClass, kSecClassKey);
    CFDictionaryAddValue(query, kSecAttrKeyType, keyType);
    CFDictionaryAddValue(query, kSecAttrApplicationTag, tagDataRef);
    CFDictionaryAddValue(query, kSecAttrCanSign, kCFBooleanTrue);

    ScopedCFTypeRef<CFErrorRef> cfError;
    ScopedCFTypeRef<SecKeyRef> key;

    // If we get errSecMissingEntitlement, try again with the file-based keychain
    constexpr int AttemptCount = 2;
    for (int attempt = 0; attempt < AttemptCount && !key; attempt++) {
        cfError.release();

        CFBooleanRef useDataProtection = attempt == 0 ? kCFBooleanTrue : kCFBooleanFalse;
        if (__builtin_available(macOS 10.15, *)) {
            CFDictionarySetValue(query, kSecUseDataProtectionKeychain, useDataProtection);
        }
        else if (attempt == 1) {
            return nullptr;
        }

        OSStatus status = SecItemCopyMatching(query, key.getGenericPtr());

        if (status == errSecItemNotFound) {
            DISCORD_LOG(LS_INFO) << "Item not found in GetPersistedKeyPair; generating new: "
                                 << SecStatusToString(status);

            ScopedCFTypeRef params = CFDictionaryCreateMutable(
              NULL, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
            AddAccessGroup(params);
            CFDictionaryAddValue(params, kSecAttrKeyType, keyType);
            CFDictionaryAddValue(params, kSecAttrKeySizeInBits, sizeRef);
            CFDictionaryAddValue(params, kSecAttrCanEncrypt, kCFBooleanFalse);
            CFDictionaryAddValue(params, kSecAttrCanDecrypt, kCFBooleanFalse);
            CFDictionaryAddValue(params, kSecAttrCanWrap, kCFBooleanFalse);
            CFDictionaryAddValue(params, kSecAttrCanUnwrap, kCFBooleanFalse);
            if (__builtin_available(macOS 10.15, *)) {
                CFDictionaryAddValue(params, kSecUseDataProtectionKeychain, useDataProtection);
            }

            ScopedCFTypeRef privParams = CFDictionaryCreateMutable(
              NULL, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
            CFDictionaryAddValue(privParams, kSecAttrIsPermanent, kCFBooleanTrue);
            CFDictionaryAddValue(privParams, kSecAttrLabel, labelStringRef);
            CFDictionaryAddValue(privParams, kSecAttrApplicationTag, tagDataRef);

            CFDictionaryAddValue(params, kSecPrivateKeyAttrs, privParams);

            key = SecKeyCreateRandomKey(params, cfError.getPtr());

            if (!key || cfError) {
                DISCORD_LOG(LS_WARNING)
                  << "Failed to create key in GetPersistedKeyPair: " << ErrorToString(cfError);

                if (!cfError || CFErrorGetCode(cfError) != errSecMissingEntitlement) {
                    return nullptr;
                }

                key.release();
            }
        }
        else if (status != 0 || !key) {
            DISCORD_LOG(LS_WARNING)
              << "Item not found GetPersistedKeyPair: " << SecStatusToString(status);
            if (status != errSecMissingEntitlement) {
                return nullptr;
            }
        }
    }

    if (!key) {
        return nullptr;
    }

    ScopedCFTypeRef data = SecKeyCopyExternalRepresentation(key, cfError.getPtr());
    if (!data) {
        DISCORD_LOG(LS_ERROR) << "Failed to export key in GetPersistedKeyPair: "
                              << ErrorToString(cfError);
        return nullptr;
    }

    bytes converted = convertKey(data);
    if (converted.empty()) {
        DISCORD_LOG(LS_ERROR) << "Failed to convert key in GetPersistedKeyPair";
        return nullptr;
    }

    return std::make_shared<::mlspp::SignaturePrivateKey>(
      ::mlspp::SignaturePrivateKey::parse(suite, converted));
}

std::shared_ptr<::mlspp::SignaturePrivateKey> GetGenericPersistedKeyPair(
  [[maybe_unused]] KeyPairContextType ctx,
  const std::string& id,
  ::mlspp::CipherSuite suite)
{
    ::mlspp::SignaturePrivateKey ret;

    ScopedCFTypeRef query = CFDictionaryCreateMutable(
      NULL, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);

    ScopedCFTypeRef accountString =
      CFStringCreateWithCString(NULL, id.c_str(), kCFStringEncodingUTF8);
    CFDictionaryAddValue(query, kSecReturnData, kCFBooleanTrue);
    CFDictionaryAddValue(query, kSecUseAuthenticationUI, kSecUseAuthenticationUISkip);
    CFDictionaryAddValue(query, kSecAttrService, KeyServiceLabel);
    CFDictionaryAddValue(query, kSecAttrAccount, accountString);
    CFDictionaryAddValue(query, kSecClass, kSecClassGenericPassword);
    AddAccessGroup(query);

    // If we get errSecMissingEntitlement, try again with the file-based keychain
    constexpr int AttemptCount = 2;
    for (int attempt = 0; attempt < AttemptCount && ret.public_key.data.empty(); attempt++) {
        if (__builtin_available(macOS 10.15, *)) {
            CFDictionarySetValue(query,
                                 kSecUseDataProtectionKeychain,
                                 attempt == 0 ? kCFBooleanTrue : kCFBooleanFalse);
        }
        else if (attempt == 1) {
            return nullptr;
        }

        ScopedCFTypeRef<CFDataRef> result;
        OSStatus status = SecItemCopyMatching(query, result.getGenericPtr());

        std::string curstr;
        if (status == 0 && result) {
            curstr.assign((char*)CFDataGetBytePtr(result), CFDataGetLength(result));

            try {
                ret = ::mlspp::SignaturePrivateKey::from_jwk(suite, curstr);
            }
            catch (std::exception& ex) {
                DISCORD_LOG(LS_WARNING)
                  << "Failed to parse key in GetPersistedKeyPair: " << ex.what();
                return nullptr;
            }
        }
        else if (status == errSecItemNotFound) {
            DISCORD_LOG(LS_INFO) << "Did not receive item in GetPersistedKeyPair; generating new: "
                                 << SecStatusToString(status);

            ret = ::mlspp::SignaturePrivateKey::generate(suite);

            std::string newstr = ret.to_jwk(suite);

            ScopedCFTypeRef data =
              CFDataCreate(NULL, (const UInt8*)newstr.c_str(), newstr.length());

            CFDictionaryRemoveValue(query, kSecReturnData);
            CFDictionaryAddValue(query, kSecValueData, data);

            status = SecItemAdd(query, nullptr);
            if (status) {
                DISCORD_LOG(LS_WARNING) << "Failed to create keychain item in GetPersistedKeyPair: "
                                        << SecStatusToString(status);

                if (status != errSecMissingEntitlement) {
                    return nullptr;
                }

                ret = ::mlspp::SignaturePrivateKey();
            }
        }
        else {
            DISCORD_LOG(LS_WARNING)
              << "Failed to retrieve item in GetPersistedKeyPair: " << SecStatusToString(status);
            if (status != errSecMissingEntitlement) {
                return nullptr;
            }
        }
    }

    if (!ret.public_key.data.empty()) {
        return std::make_shared<::mlspp::SignaturePrivateKey>(std::move(ret));
    }
    else {
        return nullptr;
    }
}

static bool DeleteWithQuery(CFMutableDictionaryRef query)
{
#if !TARGET_OS_IPHONE
    if (__builtin_available(macOS 10.15, *)) {
        CFDictionarySetValue(query, kSecUseDataProtectionKeychain, kCFBooleanTrue);
    }
#endif

    auto ret = SecItemDelete(query);

#if !TARGET_OS_IPHONE
    if (__builtin_available(macOS 10.15, *)) {
        if (ret == errSecMissingEntitlement) {
            CFDictionarySetValue(query, kSecUseDataProtectionKeychain, kCFBooleanFalse);
            ret = SecItemDelete(query);
        }
    }
#endif

    return ret == errSecSuccess;
}

bool DeleteNativePersistedKeyPair([[maybe_unused]] KeyPairContextType ctx, const std::string& id)
{
    std::string tagString = KeyTagPrefix + id;
    ScopedCFTypeRef tagDataRef =
      CFDataCreate(NULL, (const UInt8*)tagString.c_str(), tagString.size());

    ScopedCFTypeRef query = CFDictionaryCreateMutable(
      NULL, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);

    CFDictionaryAddValue(query, kSecClass, kSecClassKey);
    CFDictionaryAddValue(query, kSecAttrApplicationTag, tagDataRef);
    AddAccessGroup(query);

    return DeleteWithQuery(query);
}

bool DeleteGenericPersistedKeyPair([[maybe_unused]] KeyPairContextType ctx, const std::string& id)
{
    ScopedCFTypeRef accountString =
      CFStringCreateWithCString(NULL, id.c_str(), kCFStringEncodingUTF8);

    ScopedCFTypeRef query = CFDictionaryCreateMutable(
      NULL, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);

    CFDictionaryAddValue(query, kSecAttrService, KeyServiceLabel);
    CFDictionaryAddValue(query, kSecAttrAccount, accountString);
    CFDictionaryAddValue(query, kSecClass, kSecClassGenericPassword);
    AddAccessGroup(query);

    return DeleteWithQuery(query);
}

} // namespace detail
} // namespace mls
} // namespace dave
} // namespace discord