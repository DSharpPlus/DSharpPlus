cmake_minimum_required(VERSION 3.10)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED true)
set(CMAKE_EXPORT_COMPILE_COMMANDS true)
set(CMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES true)

project(libaerith VERSION 0.1.0)

include_directories("./inc/dave/cpp/src")
include_directories(INTERFACE_SYSTEM_INCLUDE_DIRECTORIES)

set(
    SOURCES
    session_bindings.cpp
    vector_wrapper.h
    roster_wrapper.h
)

add_library(libaerith SHARED ${SOURCES})
target_compile_options(libaerith PUBLIC -O3 -Isystem)