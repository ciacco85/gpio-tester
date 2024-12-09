using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Device.I2c;
using System.Linq;
using System.Threading;

namespace Ciacco85.IoTTester.Shared;


public class Pn532Manager : IPn532Manager
{
    protected readonly ILogger<Pn532Manager> _logger;
    protected readonly BadgerSettings _settings;
    protected readonly Lazy<Pn532> pn532;
    protected Pn532 _pn532 { get => pn532.Value; }
    protected TimeSpan _pn532AccessDelay = TimeSpan.FromMilliseconds(150);

    public Pn532Manager(ILogger<Pn532Manager> logger, IOptions<BadgerSettings> badgerSettings)
    {
        _logger = logger;
        _settings = badgerSettings.Value;
        pn532 = new(() => InitPN532());
    }

    private Pn532 InitPN532()
    {
        try
        {
            if (_settings.UseSerial.HasValue && _settings.UseSerial.Value && !string.IsNullOrWhiteSpace(_settings.SerialPort))
            {
                var ports = System.IO.Directory.GetFiles("/dev/");
                _logger.LogInformation($"Ports: {string.Join(':', ports)}");
                return new Pn532(_settings.SerialPort);
            }
            else
            {
                I2cDevice? _i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Pn532.I2cDefaultAddress));
                if (_i2cDevice != null)
                {
                    _logger.LogInformation("I2C info: {0}", _i2cDevice.QueryComponentInformation().ToString());
                }
                else
                {
                    throw new HardwareException("I2C null on Create", HardwareDevice.I2C);
                }
                return new Pn532(_i2cDevice);
            }
        }
        catch (Exception ex)
        {
            throw new HardwareException(nameof(InitPN532), ex, HardwareDevice.PN532);
        }
    }

    public virtual bool Init()
    {
        bool testOk = false;
        if (_pn532.FirmwareVersion is FirmwareVersion version)
        {
            _logger.LogInformation($"Is it a PN532!: {version.IsPn532}, Version: {version.Version}, Version supported: {version.VersionSupported}");
            testOk = RunTests();
        }
        return testOk;
    }

    public virtual bool RunTests()
    {
        try
        {
            _logger.LogDebug($"{DiagnoseMode.CommunicationLineTest}: {_pn532.RunSelfTest(DiagnoseMode.CommunicationLineTest)}");
            _logger.LogDebug($"{DiagnoseMode.ROMTest}: {_pn532.RunSelfTest(DiagnoseMode.ROMTest)}");
            _logger.LogDebug($"{DiagnoseMode.RAMTest}: {_pn532.RunSelfTest(DiagnoseMode.RAMTest)}");
            // Check couple of SFR registers
            SfrRegister[] regs = new SfrRegister[]
            {
            SfrRegister.HSU_CNT, SfrRegister.HSU_CTR, SfrRegister.HSU_PRE, SfrRegister.HSU_STA
            };
            Span<byte> redSfrs = stackalloc byte[regs.Length];
            var ret = _pn532.ReadRegisterSfr(regs, redSfrs);
            if (ret)
            {
                for (int i = 0; i < regs.Length; i++)
                {
                    _logger.LogDebug($"Readregisters: {regs[i]}, value: {BitConverter.ToString(redSfrs.ToArray(), i, 1)} ");
                }
            }

            // This should give the same result as
            ushort[] regus = new ushort[] { 0xFFAE, 0xFFAC, 0xFFAD, 0xFFAB };
            Span<byte> redSfrus = stackalloc byte[regus.Length];
            ret = _pn532.ReadRegister(regus, redSfrus);
            for (int i = 0; i < regus.Length; i++)
            {
                _logger.LogDebug($"Readregisters: {regus[i]}, value: {BitConverter.ToString(redSfrus.ToArray(), i, 1)} ");
            }

            _logger.LogDebug($"Are results same: {redSfrus.SequenceEqual(redSfrs)}");
            // Access GPIO
            ret = _pn532.ReadGpio(out Port3 p3, out Port7 p7, out OperatingMode l0L1);
            if (ret)
            {
                _logger.LogDebug($"P7: {p7}");
                _logger.LogDebug($"P3: {p3}");
                _logger.LogDebug($"L0L1: {l0L1} ");
            }
            return ret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RunTests));
            return false;
        }
    }

    public virtual (Memory<byte> passiveTarget, Memory<byte> uuid) ListPassiveTargetAndGetMifareUUID()
    {
        (Memory<byte> passiveTarget, Memory<byte> uuid) = (null, null);
        try
        {
            (passiveTarget, uuid) = (ListPassiveTarget(), null);
            if (!passiveTarget.IsEmpty)
            {
                uuid = GetMifareUUID(passiveTarget);
            }
            return (passiveTarget, uuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(ListPassiveTargetAndGetMifareUUID));
            if (ex is not ArgumentException)
                throw new HardwareException(nameof(ListPassiveTargetAndGetMifareUUID), ex, HardwareDevice.PN532);
            return (passiveTarget, uuid);
        }
    }

    public virtual Memory<byte> ListPassiveTarget()
    {
        Thread.Sleep(_pn532AccessDelay);
        Memory<byte> passiveTarget = _pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);

        if (passiveTarget.IsEmpty)
        {
            return Enumerable.Empty<byte>().ToArray();
        }

        return passiveTarget;
    }

    public virtual void WakeUp()
    {
        _pn532.WakeUp();
    }

    public virtual bool PowerDown()
    {
        WakeUpEnable wakeUpEnable = _settings.UseSerial.HasValue && _settings.UseSerial.Value ? WakeUpEnable.Hsu : WakeUpEnable.I2c;
        return _pn532.PowerDown(wakeUpEnable);
    }

    public virtual byte[]? AutoPoll(TimeSpan timeSpan)
    {
        byte[]? passiveTarget = null;
        DateTime now = DateTime.UtcNow;
        while (DateTime.UtcNow - now <= timeSpan)
        {
            passiveTarget = _pn532.AutoPoll(5, 300, new PollingType[] { PollingType.GenericPassive106kbps });
            if (passiveTarget is object)
            {
                break;
            }

            // Give time to PN532 to process
            Thread.Sleep(_pn532AccessDelay);
        }

        if (passiveTarget is null)
        {
            return Enumerable.Empty<byte>().ToArray();
        }

        return passiveTarget;
    }

    public virtual byte[]? GetUltralightSerialNumber(byte[] passiveTarget)
    {
        var ultralight = GetUltralightCard(passiveTarget);
        return ultralight?.SerialNumber;
    }

    public virtual byte[]? GetUltralightSignature(byte[] passiveTarget)
    {
        var ultralight = GetUltralightCard(passiveTarget);
        var sign = ultralight?.GetSignature();
        return sign;
    }

    public virtual byte[]? GetMifareSerialNumber(byte[] passiveTarget)
    {
        var mifare = GetMifareCard(passiveTarget);
        return mifare?.SerialNumber;
    }

    public virtual Memory<byte> GetMifareUUID(Memory<byte> passiveTarget)
    {
        var mifareCard = GetMifareCard(passiveTarget);
        if (mifareCard == null)
            return null;
        //unwritable block 0 containing UUID
        const byte block = 0;
        mifareCard.BlockNumber = block;
        mifareCard.Command = MifareCardCommand.AuthenticationB;
        Thread.Sleep(_pn532AccessDelay);
        var ret = mifareCard.RunMifareCardCommand();
        if (ret < 0)
        {
            // Try another one
            mifareCard.Command = MifareCardCommand.AuthenticationA;
            Thread.Sleep(_pn532AccessDelay);
            ret = mifareCard.RunMifareCardCommand();
        }

        if (ret >= 0)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.Read16Bytes;
            Thread.Sleep(_pn532AccessDelay);
            ret = mifareCard.RunMifareCardCommand();
            if (ret >= 0)
            {
                _logger.LogDebug("Data: {Data}", BitConverter.ToString(mifareCard.Data));
                return mifareCard.Data;
            }
            else
            {
                _logger.LogInformation("Error reading bloc: {Block}", block);
                return null;
            }

            //if (block % 4 == 3)
            //{
            //    // Check what are the permissions
            //    for (byte j = 3; j > 0; j--)
            //    {
            //        var access = mifareCard.BlockAccess((byte)(block - j), mifareCard.Data);
            //        _logger.LogInformation($"Bloc: {block - j}, Access: {access}");
            //    }
            //    var sector = mifareCard.SectorTailerAccess(block, mifareCard.Data);
            //    _logger.LogInformation($"Bloc: {block}, Access: {sector}");
            //}
        }
        else
        {
            _logger.LogInformation("MifareCard authentication error with command {Command}", mifareCard.Command);
            return null;
        }
    }

    internal virtual UltralightCard? GetUltralightCard(byte[] passiveTarget)
    {
        var card = _pn532.TryDecode106kbpsTypeA(passiveTarget.AsSpan().Slice(1));
        if (card is not object || !UltralightCard.IsUltralightCard(card.Atqa, card.Sak))
        {
            _logger.LogWarning("Not a valid card, please try again.");
            return null;
        }

        return new UltralightCard(_pn532!, card.TargetNumber);
    }

    internal virtual MifareCard? GetMifareCard(Memory<byte> passiveTarget)
    {
        Thread.Sleep(_pn532AccessDelay);
        var card = _pn532.TryDecode106kbpsTypeA(passiveTarget.Slice(1).Span);
        if (card is null)
        {
            _logger.LogWarning("Not a valid Mifare card, please try again.");
            return null;
        }

        MifareCard mifareCard = new MifareCard(_pn532, card.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
        mifareCard.SetCapacity(card.Atqa, card.Sak);
        mifareCard.SerialNumber = card.NfcId;
        mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        return mifareCard;
    }

    protected virtual void AddDelay()
    {
        if (_settings.UseDelay.HasValue && _settings.UseDelay.Value && _settings.DelayMs.HasValue && _settings.DelayMs.Value > 0)
        {
            Thread.Sleep(_settings.DelayMs.Value);
        }
    }
}
