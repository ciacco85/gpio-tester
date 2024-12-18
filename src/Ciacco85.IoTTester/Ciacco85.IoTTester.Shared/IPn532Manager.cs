﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ciacco85.IoTTester.Shared;




public interface IPn532ManagerTest : IPn532Manager
{
    Task<Memory<byte>> Test();
}

public interface IPn532Manager
{
    byte[]? AutoPoll(TimeSpan timeSpan);
    byte[]? GetMifareSerialNumber(byte[] passiveTarget);
    Memory<byte> GetMifareUUID(Memory<byte> passiveTarget);
    byte[]? GetUltralightSerialNumber(byte[] passiveTarget);
    byte[]? GetUltralightSignature(byte[] passiveTarget);
    bool Init();
    Memory<byte> ListPassiveTarget();
    (Memory<byte> passiveTarget, Memory<byte> uuid) ListPassiveTargetAndGetMifareUUID();
    bool RunTests();
    void WakeUp();
    bool PowerDown();
}
