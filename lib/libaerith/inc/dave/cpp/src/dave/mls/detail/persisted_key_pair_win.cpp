#include "dave/mls/detail/persisted_key_pair.h"

#include <cassert>
#include <filesystem>
#include <fstream>
#include <functional>
#include <mutex>
#include <sstream>
#include <string>

#ifndef SECURITY_WIN32
#define SECURITY_WIN32 1
#endif
#include <Windows.h>
#include <Bcrypt.h>
#include <Security.h>
#include <ncrypt.h>
#include <wincrypt.h>

#include <bytes/bytes.h>
#include <mls/crypto.h>

#include "dave/logger.h"
#include "dave/mls/parameters.h"

static const std::string KeyTagPrefix = "discord-secure-frames-key-";

template <class T = NCRYPT_HANDLE>
struct ScopedNCryptHandle {
    ScopedNCryptHandle() = default;
    ScopedNCryptHandle(T handle)
      : handle_(handle)
    {
    }
    ScopedNCryptHandle(const ScopedNCryptHandle& other) = delete;
    ScopedNCryptHandle(ScopedNCryptHandle&& other)
      : handle_(std::exchange(other.handle_, T()))
    {
    }

    ~ScopedNCryptHandle() { finalize(); }

    ScopedNCryptHandle& operator=(T handle)
    {
        finalize();
        handle_ = handle;
        return *this;
    }

    T release() { return std::exchange(handle_, T()); }

    void finalize()
    {
        if (auto handle = release()) {
            NCryptFreeObject(handle);
        }
    }

    T& get() { return handle_; }

    T* getPtr() { return &handle_; }

    operator T&() { return get(); }

    explicit operator bool() { return handle_ != T(); }

    T handle_ = T();
};

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
    LPCWSTR keyType = nullptr;
    ULONG keyBlobMagic = 0;
    std::function<bool(bytes&)> convertBlob;

    auto suiteId = suite.cipher_suite();
    switch (suiteId) {
    case ::mlspp::CipherSuite::ID::P256_AES128GCM_SHA256_P256:
    case ::mlspp::CipherSuite::ID::P384_AES256GCM_SHA384_P384:
    case ::mlspp::CipherSuite::ID::P521_AES256GCM_SHA512_P521:
        supported = true;
        if (suiteId == ::mlspp::CipherSuite::ID::P521_AES256GCM_SHA512_P521) {
            keyType = BCRYPT_ECDSA_P521_ALGORITHM;
            keyBlobMagic = BCRYPT_ECDSA_PRIVATE_P521_MAGIC;
        }
        else if (suiteId == ::mlspp::CipherSuite::ID::P384_AES256GCM_SHA384_P384) {
            keyType = BCRYPT_ECDSA_P384_ALGORITHM;
            keyBlobMagic = BCRYPT_ECDSA_PRIVATE_P384_MAGIC;
        }
        else {
            keyType = BCRYPT_ECDSA_P256_ALGORITHM;
            keyBlobMagic = BCRYPT_ECDSA_PRIVATE_P256_MAGIC;
        }

        convertBlob = [](bytes& blob) {
            // https://learn.microsoft.com/en-us/windows/win32/api/bcrypt/ns-bcrypt-bcrypt_ecckey_blob
            // Input has an PBCRYPT_ECCKEY_BLOB header, followed by 3 cbKey-byte big-endian
            // integers: X, Y, and d. X and Y are the public key (represented as the coordinates);
            // d is the private key.
            constexpr size_t ValueCount = 3;
            constexpr size_t PublicValues = 2;

            if (blob.size() < sizeof(BCRYPT_ECCKEY_BLOB)) {
                DISCORD_LOG(LS_ERROR)
                  << "Exported key blob too small in GetPersistedKeyPair/convertBlob: "
                  << blob.size();
                return false;
            }

            PBCRYPT_ECCKEY_BLOB header = (PBCRYPT_ECCKEY_BLOB)blob.data();
            ULONG keySize = header->cbKey;
            if (blob.size() < sizeof(BCRYPT_ECCKEY_BLOB) + keySize * ValueCount) {
                DISCORD_LOG(LS_ERROR)
                  << "Exported key blob too small in GetPersistedKeyPair/convertBlob: "
                  << blob.size();
                return false;
            }
            blob.resize(sizeof(BCRYPT_ECCKEY_BLOB) + keySize * ValueCount);
            blob.as_vec().erase(blob.begin(),
                                blob.begin() + sizeof(BCRYPT_ECCKEY_BLOB) + keySize * PublicValues);
            return true;
        };
        break;
    default:
        // Other suites will need to store keys as JWK files on disk
        return nullptr;
    }

    assert(keyType && keyBlobMagic && convertBlob);

    ScopedNCryptHandle<NCRYPT_PROV_HANDLE> provider;
    SECURITY_STATUS status =
      NCryptOpenStorageProvider(provider.getPtr(), MS_KEY_STORAGE_PROVIDER, 0);
    if (status != ERROR_SUCCESS) {
        DISCORD_LOG(LS_ERROR) << "Failed to open storage provider in GetPersistedKeyPair: "
                              << status;
        return nullptr;
    }

    std::filesystem::path keyName = KeyTagPrefix + id;

    ScopedNCryptHandle<NCRYPT_KEY_HANDLE> key;
    status =
      NCryptOpenKey(provider, key.getPtr(), keyName.c_str(), AT_SIGNATURE, NCRYPT_SILENT_FLAG);

    if (status == NTE_BAD_KEYSET) {
        DISCORD_LOG(LS_INFO) << "No key found in GetPersistedKeyPair; generating new";

        status = NCryptCreatePersistedKey(
          provider, key.getPtr(), keyType, keyName.c_str(), AT_SIGNATURE, 0);
        if (status != ERROR_SUCCESS) {
            DISCORD_LOG(LS_ERROR) << "Failed to create key in GetPersistedKeyPair: " << status;
            return nullptr;
        }

        DWORD exportPolicyValue = NCRYPT_ALLOW_EXPORT_FLAG | NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
        status = NCryptSetProperty(key,
                                   NCRYPT_EXPORT_POLICY_PROPERTY,
                                   (PBYTE)&exportPolicyValue,
                                   sizeof(exportPolicyValue),
                                   NCRYPT_PERSIST_FLAG | NCRYPT_SILENT_FLAG);
        if (status != ERROR_SUCCESS) {
            DISCORD_LOG(LS_ERROR)
              << "Failed to configure key export policy in GetPersistedKeyPair: " << status;
            return nullptr;
        }

        // struct {
        //     DWORD   dwVersion;
        //     DWORD   dwFlags;
        //     LPCWSTR pszCreationTitle;
        //     LPCWSTR pszFriendlyName;
        //     LPCWSTR pszDescription;
        // } NCRYPT_UI_POLICY;

        NCRYPT_UI_POLICY uiPolicyValue = {1, 0, nullptr, nullptr, nullptr};
        status = NCryptSetProperty(key,
                                   NCRYPT_UI_POLICY_PROPERTY,
                                   (PBYTE)&uiPolicyValue,
                                   sizeof(uiPolicyValue),
                                   NCRYPT_PERSIST_FLAG | NCRYPT_SILENT_FLAG);
        if (status != ERROR_SUCCESS) {
            DISCORD_LOG(LS_ERROR) << "Failed to configure key UI policy in GetPersistedKeyPair: "
                                  << status;
            return nullptr;
        }

        status = NCryptFinalizeKey(key, NCRYPT_SILENT_FLAG);
        if (status != ERROR_SUCCESS) {
            DISCORD_LOG(LS_ERROR) << "Failed to finalize key in GetPersistedKeyPair: " << status;
            return nullptr;
        }
    }
    else if (status != ERROR_SUCCESS) {
        DISCORD_LOG(LS_ERROR) << "Failed to open key in GetPersistedKeyPair: " << status;
        return nullptr;
    }

    DWORD keySize = 0;
    status = NCryptExportKey(
      key, NULL, BCRYPT_PRIVATE_KEY_BLOB, NULL, NULL, 0, &keySize, NCRYPT_SILENT_FLAG);
    if (status != ERROR_SUCCESS) {
        DISCORD_LOG(LS_ERROR) << "Failed to size key in GetPersistedKeyPair: " << status;
        return nullptr;
    }

    bytes keyData(keySize);

    status = NCryptExportKey(key,
                             NULL,
                             BCRYPT_PRIVATE_KEY_BLOB,
                             NULL,
                             keyData.data(),
                             keySize,
                             &keySize,
                             NCRYPT_SILENT_FLAG);
    if (status != ERROR_SUCCESS) {
        DISCORD_LOG(LS_ERROR) << "Failed to export key in GetPersistedKeyPair: " << status;
        return nullptr;
    }

    if (keyData.size() < sizeof(BCRYPT_KEY_BLOB)) {
        DISCORD_LOG(LS_ERROR) << "Exported key blob too small in GetPersistedKeyPair/convertBlob: "
                              << keyData.size();
        return nullptr;
    }

    BCRYPT_KEY_BLOB* header = (BCRYPT_KEY_BLOB*)keyData.data();
    if (header->Magic != keyBlobMagic) {
        DISCORD_LOG(LS_ERROR) << "Exported key blob has unexpected magic in GetPersistedKeyPair: "
                              << header->Magic;
        return nullptr;
    }

    if (!convertBlob(keyData)) {
        DISCORD_LOG(LS_ERROR) << "Failed to convert key in GetPersistedKeyPair";
        return nullptr;
    }

    return std::make_shared<::mlspp::SignaturePrivateKey>(
      ::mlspp::SignaturePrivateKey::parse(suite, keyData));
}

bool DeleteNativePersistedKeyPair([[maybe_unused]] KeyPairContextType ctx, const std::string& id)
{
    ScopedNCryptHandle<NCRYPT_PROV_HANDLE> provider;
    SECURITY_STATUS status =
      NCryptOpenStorageProvider(provider.getPtr(), MS_KEY_STORAGE_PROVIDER, 0);
    if (status != ERROR_SUCCESS) {
        DISCORD_LOG(LS_ERROR) << "Failed to open storage provider in DeletePersistedKeyPair: "
                              << status;
        return false;
    }

    std::filesystem::path keyName = KeyTagPrefix + id;

    ScopedNCryptHandle<NCRYPT_KEY_HANDLE> key;
    status =
      NCryptOpenKey(provider, key.getPtr(), keyName.c_str(), AT_SIGNATURE, NCRYPT_SILENT_FLAG);
    if (status != ERROR_SUCCESS) {
        return false;
    }

    auto ret = NCryptDeleteKey(key, NCRYPT_SILENT_FLAG);
    if (ret == ERROR_SUCCESS) {
        // If NCryptDeleteKey succeeds, it frees the handle, so our wrapper shouldn't also do so.
        key.release();
        return true;
    }
    else {
        return false;
    }
}

} // namespace detail
} // namespace mls
} // namespace dave
} // namespace discord
