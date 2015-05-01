#pragma once

using Windows::Storage::Streams::IBuffer;

namespace NativeRijndael
{
	public delegate bool ConditionChecker();

	// Managed helper class for KeePass algorithms
    public ref class KeePassHelper sealed
    {
    public:
		// Native implementation of key transformation algorithm
        static IBuffer^ TransformKey(uint64 numRounds, IBuffer^ key, IBuffer^ iv, IBuffer^ data, ConditionChecker^ checkCancel);
    };
}