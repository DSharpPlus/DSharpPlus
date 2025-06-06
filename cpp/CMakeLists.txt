cmake_minimum_required(VERSION 3.20)

project(
  libdave
  VERSION 1.0
  LANGUAGES CXX
)

option(REQUIRE_BORINGSSL "Require BoringSSL instead of OpenSSL" ON)
option(TESTING "Build tests" OFF)
option(PERSISTENT_KEYS "Enable storage of persistent signature keys" OFF)

include(CheckCXXCompilerFlag)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if (CMAKE_CXX_COMPILER_ID MATCHES "Clang" OR CMAKE_CXX_COMPILER_ID MATCHES "GNU")
  add_compile_options(-Wall -pedantic -Wextra -Werror)
elseif(MSVC)
  add_compile_options(/W4 /WX)
  add_definitions(-DWINDOWS)

  # MSVC helpfully recommends safer equivalents for things like
  # getenv, but they are not portable.
  add_definitions(-D_CRT_SECURE_NO_WARNINGS)  
endif()

find_package(OpenSSL REQUIRED)
if (OPENSSL_FOUND)
  find_path(BORINGSSL_INCLUDE_DIR openssl/is_boringssl.h HINTS ${OPENSSL_INCLUDE_DIR} NO_DEFAULT_PATH)

  if (BORINGSSL_INCLUDE_DIR)
    message(STATUS "Found OpenSSL includes are for BoringSSL")
    
    add_compile_definitions(WITH_BORINGSSL)

    if (CMAKE_CXX_COMPILER_ID MATCHES "Clang" OR CMAKE_CXX_COMPILER_ID MATCHES "GNU")
      add_compile_options(-Wno-gnu-anonymous-struct -Wno-nested-anon-types)
    endif ()

    file(STRINGS "${OPENSSL_INCLUDE_DIR}/openssl/crypto.h" boringssl_version_str
          REGEX "^#[\t ]*define[\t ]+OPENSSL_VERSION_TEXT[\t ]+\"OpenSSL ([0-9])+\\.([0-9])+\\.([0-9])+ .+")
    
    string(REGEX REPLACE "^.*OPENSSL_VERSION_TEXT[\t ]+\"OpenSSL ([0-9]+\\.[0-9]+\\.[0-9])+ .+$"
            "\\1" OPENSSL_VERSION "${boringssl_version_str}")

  elseif (REQUIRE_BORINGSSL)
    message(FATAL_ERROR "BoringSSL required but not found")
  endif ()

  if (${OPENSSL_VERSION} VERSION_GREATER_EQUAL 3)
    message(FATAL_ERROR "OpenSSL 3.x is not supported")
  elseif(${OPENSSL_VERSION} VERSION_LESS 1.1.1)
    message(FATAL_ERROR "OpenSSL 1.1.1 or greater is required")
  endif()

  message(STATUS "OpenSSL Found: ${OPENSSL_VERSION}")
  message(STATUS "OpenSSL Include: ${OPENSSL_INCLUDE_DIR}")
  message(STATUS "OpenSSL Libraries: ${OPENSSL_LIBRARIES}")
else()
  message(FATAL_ERROR "No OpenSSL library found")
endif()

find_package(MLSPP CONFIG REQUIRED)

set(CMAKE_STATIC_LIBRARY_PREFIX "")

SET(LIB_NAME ${PROJECT_NAME})
file(GLOB_RECURSE LIB_HEADERS CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/*.h")
file(GLOB_RECURSE LIB_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")

# remove all of the persistent key files
list(FILTER LIB_SOURCES EXCLUDE REGEX ".*persisted_key.*")

if (PERSISTENT_KEYS)
  # persistent keys enabled
  list(APPEND LIB_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/dave/mls/persisted_key_pair.cpp")

  if (APPLE)
    # Apple has its own native and generic implementation, we just add the _apple.cpp file
    list(APPEND LIB_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/dave/mls/detail/persisted_key_pair_apple.cpp")
  else ()
    # Other platforms share the generic implementation
    list(APPEND LIB_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/dave/mls/detail/persisted_key_pair_generic.cpp")

    if (WIN32)
      # Windows has a native implementation
      list(APPEND LIB_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/dave/mls/detail/persisted_key_pair_win.cpp")
    else ()
      # We don't have a native implementation, so we include the nullified native
      list(APPEND LIB_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/dave/mls/detail/persisted_key_pair_null.cpp")
    endif ()
  endif ()
  
else ()
  # not using persistent keys, so we just need to add the null implementation
  list (APPEND LIB_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/dave/mls/persisted_key_pair_null.cpp")
endif ()

if (NOT WIN32)
  list(FILTER LIB_SOURCES EXCLUDE REGEX ".*_win.cpp")
endif ()

if (NOT APPLE)
  list(FILTER LIB_SOURCES EXCLUDE REGEX ".*_apple.cpp")
endif ()

add_library(${LIB_NAME} ${LIB_HEADERS} ${LIB_SOURCES})

target_include_directories(
  ${LIB_NAME}
  PUBLIC
    $<BUILD_INTERFACE:${PROJECT_SOURCE_DIR}/src>
    $<INSTALL_INTERFACE:include/${PROJECT_NAME}>
    ${OPENSSL_INCLUDE_DIR}
)

target_link_libraries(${LIB_NAME} PUBLIC OpenSSL::Crypto)
target_link_libraries(${LIB_NAME} PUBLIC MLSPP::mlspp)

if (TESTING)
  add_subdirectory(test)
endif()