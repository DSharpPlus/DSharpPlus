#pragma once

#include<cstdint>

#include<dave/common.h>

#include "vector_wrapper.h"

using namespace discord::dave;

// represents a wrapper around a std::map<uint64_t, std::vector<uint8_t>> used to keep track of users in the MLS group.
struct RosterWrapper
{
public:
    RosterMap roster;

    RosterWrapper(RosterMap map)
    {
        this->roster = map;
    }
};

// gets the amount of users in this roster
extern "C" __declspec(__dllimport__) int32_t AerithGetRosterCount(RosterWrapper* __restrict__ wrapper);

// gets the ID of the user at the specified index
extern "C" __declspec(__dllimport__) uint64_t AerithGetRosterKeyAtIndex(RosterWrapper* __restrict__ wrapper, int32_t index);

// gets the value associated with the user
extern "C" __declspec(__dllimport__) VectorWrapper* AerithGetRosterValue(RosterWrapper* __restrict__ wrapper, uint64_t key);

// destroys the roster wrapper
extern "C" __declspec(__dllimport__) void AerithDestroyRoster(RosterWrapper* wrapper);