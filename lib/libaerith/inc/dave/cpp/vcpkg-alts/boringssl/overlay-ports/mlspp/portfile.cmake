vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO cisco/mlspp
    REF "${VERSION}"
    SHA512 ca2a7e9cb512f38c49d84e351ca304d7aca176b2686a7ad1326d72dbb6f4b4063dabdf36c57336674b71b1b74b5135abd274adbc79f30d46f792e7862ef5306c
)

vcpkg_cmake_configure(
    SOURCE_PATH "${SOURCE_PATH}"
    OPTIONS 
        -DDISABLE_GREASE=ON 
        -DVCPKG_MANIFEST_DIR="alternatives/boringssl"
        -DMLS_CXX_NAMESPACE="mlspp"
)

vcpkg_cmake_install()

file(REMOVE_RECURSE "${CURRENT_PACKAGES_DIR}/debug/include")
file(REMOVE_RECURSE "${CURRENT_PACKAGES_DIR}/debug/share")