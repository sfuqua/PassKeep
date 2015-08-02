#pragma once

using Windows::Storage::Streams::IBuffer;

namespace NativeRijndael
{
	public delegate bool ConditionChecker();

	// Managed helper class for KeePass algorithms
    public ref class KeePassHelper sealed
    {
    public:
		// Transforms data until cancellation is requested - useful for computing how many rounds fit into a given time span.
		static uint64 TransformUntilCancelled(IBuffer^ key, IBuffer^ data, ConditionChecker^ cancellationToken);

		// Native implementation of key transformation algorithm
        static IBuffer^ TransformKey(uint64 numRounds, IBuffer^ key, IBuffer^ iv, IBuffer^ data, ConditionChecker^ cancellationToken);
    };
}