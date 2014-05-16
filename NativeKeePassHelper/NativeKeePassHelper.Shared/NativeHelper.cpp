#include "pch.h"
#include "NativeHelper.h"
#include "Rijndael.h"

using namespace NativeKeePassHelper;
using namespace Platform;

using Windows::Security::Cryptography::CryptographicBuffer;


// Native implementation of the KeePass key transformation algorithm, to be
// called from managed code as a performance improvement.
IBuffer^ NativeHelper::TransformKey(uint64 numRounds, IBuffer^ key, IBuffer^ iv, IBuffer^ data, ConditionChecker^ checkCancel)
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
	CRijndael aes;
	aes.Init(CRijndael::ECB, CRijndael::EncryptDir, keyArray->Data, CRijndael::Key32Bytes, (hasIv ? ivArray->Data : 0));

	for (uint64 i = 0; i < numRounds; i++) {
		if (checkCancel()) {
			break;
		}

		// Encrypt the data - CryptographicEngine::Encrypt is *too slow*
		aes.BlockEncrypt(dataBytes, 128, dataBytes);
	}

	// Return an IBuffer back to the managed code
	return CryptographicBuffer::CreateFromByteArray(dataArray);
}
