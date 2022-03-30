// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Dicom.Core.Crypto;
public interface ICryptoService
{
    Task<byte[]> Encrypt(byte[] data);
    Task<byte[]> Decrypt(byte[] data);
    async Task<string> EncryptString(string data)
    {
        byte[] inputAsByteArray = Encoding.UTF8.GetBytes(data);
        var res = await Encrypt(inputAsByteArray);
        return Convert.ToBase64String(res);
    }
    async Task<string> DecryptString(string data)
    {
        byte[] inputAsByteArray = Convert.FromBase64String(data);
        var res = await Decrypt(inputAsByteArray);
        return Encoding.UTF8.GetString(res);
    }
}
