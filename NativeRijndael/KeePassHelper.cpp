#include "pch.h"
#include "KeePassHelper.h"
#include "Cipher.h"

using namespace Platform;
using Windows::Security::Cryptography::CryptographicBuffer;
using namespace NativeRijndael;

// Transforms data until cancellation is requested - useful for computing how many rounds fit into a given time span.
uint64 KeePassHelper::TransformUntilCancelled(IBuffer^ key, IBuffer^ data, ConditionChecker^ cancellationToken)
{
	// Copy the AES key IBuffer to a Platform::Array
	Array<unsigned char, 1U> ^keyArray = ref new Array<unsigned char, 1U>(key->Length);
	CryptographicBuffer::CopyToByteArray(key, &keyArray);

	// Copy the data to be encrypted to a Platform::Array and get a reference to the
	// underlying data pointer
	Array<unsigned char, 1U> ^dataArray = ref new Array<unsigned char, 1U>(data->Length);
	CryptographicBuffer::CopyToByteArray(data, &dataArray);
	unsigned char *dataBytes = dataArray->Data;

	// Initialize the AES cipher
	Cipher aes;
	aes.Init(Cipher::ECB, Cipher::EncryptDir, keyArray->Data, Cipher::Key32Bytes, /* iv */ 0);

	uint64 rounds;
	for (rounds = 1; rounds < 0xffffffffffffffff; rounds++) {
		if (cancellationToken()) {
			break;
		}

		aes.BlockEncrypt(dataBytes, 128, dataBytes);
	}

	return rounds;
}

// Native implementation of the KeePass key transformation algorithm, to be
// called from managed code as a performance improvement.
IBuffer^ KeePassHelper::TransformKey(uint64 numRounds, IBuffer^ key, IBuffer^ iv, IBuffer^ data, ConditionChecker^ cancellationToken)
{
	// Copy the AES key IBuffer to a Platform::Array
	Array<unsigned char, 1U> ^keyArray = ref new Array<unsigned char, 1U>(key->Length);
	CryptographicBuffer::CopyToByteArray(key, &keyArray);

	// Copy the AES IV to a Platform::Array - if appropriate
	// The typical case for KeePass is to pass in null/0
	bool hasIv = iv != nullptr;
	Array<unsigned char, 1U> ^ivArray;
	if (hasIv) {
		 ivArray = ref new Array<unsigned char, 1U>(iv->Length);
		CryptographicBuffer::CopyToByteArray(iv, &ivArray);
	}

	// Copy the data to be encrypted to a Platform::Array and get a reference to the
	// underlying data pointer
	Array<unsigned char, 1U> ^dataArray = ref new Array<unsigned char, 1U>(data->Length);
	CryptographicBuffer::CopyToByteArray(data, &dataArray);
	unsigned char *dataBytes = dataArray->Data;

	// Initialize the AES cipher
	Cipher aes;
	aes.Init(Cipher::ECB, Cipher::EncryptDir, keyArray->Data, Cipher::Key32Bytes, (hasIv ? ivArray->Data : 0));

	for (uint64 i = 0; i < numRounds; i++) {
		if (cancellationToken()) {
			break;
		}

		// Encrypt the data - CryptographicEngine::Encrypt is *too slow*
		aes.BlockEncrypt(dataBytes, 128, dataBytes);
	}

	// Return an IBuffer back to the managed code
	return CryptographicBuffer::CreateFromByteArray(dataArray);
}
