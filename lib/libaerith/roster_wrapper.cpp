#include "roster_wrapper.h"

#include<cstring>

using namespace discord::dave;

extern "C" __declspec(__dllimport__) int32_t AerithGetRosterCount(RosterWrapper* __restrict__ wrapper)
{
    return wrapper->roster.size();
}

extern "C" __declspec(__dllimport__) uint64_t AerithGetRosterKeyAtIndex(RosterWrapper* __restrict__ wrapper, int32_t index)
{
    RosterMap::iterator it = wrapper->roster.begin();
    std::advance(it, index);
    return it->first;
}

extern "C" __declspec(__dllimport__) VectorWrapper* AerithGetRosterValue(RosterWrapper* __restrict__ wrapper, uint64_t key)
{
    return new VectorWrapper(wrapper->roster.at(key));
}

extern "C" __declspec(__dllimport__) void AerithDestroyRoster(RosterWrapper* wrapper)
{
    delete wrapper;
}