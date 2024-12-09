using Iot.Device.Card.Mifare;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ciacco85.IoTTester.Shared;

public class Pn532ManagerTest : Pn532Manager, IPn532ManagerTest
{
    public Pn532ManagerTest(ILogger<Pn532ManagerTest> logger, IOptions<BadgerSettings> badgerSettings)
        : base(logger, badgerSettings)
    {

    }

    public async Task<Memory<byte>> Test()
    {
        var nfcData = ListPassiveTargetAndGetMifareUUID();
        return nfcData.uuid;
    }

    public override (Memory<byte> passiveTarget, Memory<byte> uuid) ListPassiveTargetAndGetMifareUUID()
    {
        (Memory<byte> passiveTarget, Memory<byte> uuid) = (Memory<byte>.Empty, Memory<byte>.Empty);
        try
        {
            AddDelay();
            passiveTarget = _pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
            if (passiveTarget.IsEmpty)
            {
                _logger.LogInformation("Card not present!");
                return (passiveTarget, uuid);
            }

            //GetMifare
            AddDelay();
            var card = _pn532.TryDecode106kbpsTypeA(passiveTarget.Slice(1).Span);
            if (card is null)
            {
                _logger.LogWarning("Not a valid Mifare card, please try again.");
                return (passiveTarget, uuid);
            }

            const byte block = 0;
            MifareCard mifareCard = new MifareCard(_pn532, card.TargetNumber)
            {
                BlockNumber = block,
                Command = MifareCardCommand.AuthenticationA
            };
            mifareCard.SetCapacity(card.Atqa, card.Sak);
            mifareCard.SerialNumber = card.NfcId;
            mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            //ReadMifare
            AddDelay();
            var ret = mifareCard.RunMifareCardCommand();
            if (ret < 0)
            {
                // Try another one
                _logger.LogInformation("MifareCard authentication error with command {Command}", mifareCard.Command);
                mifareCard.Command = MifareCardCommand.AuthenticationB;
                AddDelay();
                ret = mifareCard.RunMifareCardCommand();
            }

            if (ret >= 0)
            {
                mifareCard.BlockNumber = block;
                mifareCard.Command = MifareCardCommand.Read16Bytes;
                AddDelay();
                ret = mifareCard.RunMifareCardCommand();
                if (ret >= 0)
                {
                    _logger.LogInformation("Data: {Data}", BitConverter.ToString(mifareCard.Data));
                    return (passiveTarget, mifareCard.Data);
                }
                else
                {
                    _logger.LogInformation("Error reading bloc: {Block}", block);
                    return (passiveTarget, uuid);
                }
            }
            else
            {
                _logger.LogInformation("MifareCard authentication error with command {Command}", mifareCard.Command);
                return (passiveTarget, uuid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(ListPassiveTargetAndGetMifareUUID));
            //if (ex is not ArgumentException)
            throw new HardwareException(nameof(ListPassiveTargetAndGetMifareUUID), ex, HardwareDevice.PN532);
            return (passiveTarget, uuid);
        }
    }


}
