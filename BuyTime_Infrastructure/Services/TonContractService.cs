using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Dto;
using BuyTime_Infrastructure.Common.Settings;
using ErrorOr;
using Microsoft.Extensions.Options;
using TonSdk.Client;
using TonSdk.Client.Stack;
using TonSdk.Contracts.Wallet;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace BuyTime_Infrastructure.Services;

public class TonContractService : ITonContractService
{
    private readonly TonClient _tonClient;
    private readonly TonSettings _settings;
    private readonly IBlockchainService _blockchainService;

    public TonContractService(IOptions<TonSettings> settings, IBlockchainService blockchainService)
    {
        _settings = settings.Value;
        _blockchainService = blockchainService;

        string endpoint = _settings.IsTestnet
            ? "https://testnet.toncenter.com/api/v2/jsonRPC"
            : "https://toncenter.com/api/v2/jsonRPC";

        _tonClient = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, new HttpParameters
        {
            Endpoint = endpoint,
            ApiKey = _settings.ApiKey
        });
    }

    public async Task<ErrorOr<TonConnectPayloadDto>> GenerateCreateBookingPayloadAsync(
        string studentWalletAddress,
        string expertWalletAddress,
        DateTime startTime,
        DateTime endTime,
        decimal priceAmount)
    {
        try
        {
            var platformAddressStr = await _blockchainService.GetPlatformAddressAsync();
            var arbiterAddressStr = await _blockchainService.GetArbiterAddressAsync();

            var arbiterAddress = new Address(_settings.ArbiterAddress.Trim());
            var platformAddress = new Address(_settings.PlatformAddress.Trim());
            var expertAddress = new Address(expertWalletAddress.Trim());
            var studentAddress = new Address(studentWalletAddress.Trim());

            uint ctxId = (uint)new Random().Next(100000, 999999);

            // для деплою
            Cell detailsCell = new CellBuilder()
                .StoreAddress(arbiterAddress)
                .StoreAddress(platformAddress)
                .StoreUInt(0, 64)
                .StoreUInt(0, 64)
                .StoreUInt(ctxId, 32)
                .Build();

            Cell dataCell = new CellBuilder()
                .StoreUInt(0, 8)
                .StoreAddress(studentAddress)
                .StoreAddress(expertAddress)
                .StoreCoins(new Coins(0))
                .StoreRef(detailsCell)
                .Build();

            Cell codeCell = Cell.From(_settings.ContractCodeHex);

            var stateInit = new StateInit(new StateInitOptions { Code = codeCell, Data = dataCell });
            var futureAddress = new Address(0, stateInit);

            Cell stateInitCell = new CellBuilder()
                .StoreBit(false)
                .StoreBit(false)
                .StoreBit(true)
                .StoreRef(codeCell)
                .StoreBit(true)
                .StoreRef(dataCell)
                .StoreBit(false)
                .Build();

            // збірка Payload
            long startUnix = ((DateTimeOffset)startTime).ToUnixTimeSeconds();
            long endUnix = ((DateTimeOffset)endTime).ToUnixTimeSeconds();
            uint OP_CREATE_BOOKING = 0xA1B2C3D1;

            Cell payloadCell = new CellBuilder()
                .StoreUInt(OP_CREATE_BOOKING, 32)
                .StoreAddress(expertAddress)
                .StoreAddress(arbiterAddress)
                .StoreUInt((ulong)startUnix, 64)
                .StoreUInt((ulong)endUnix, 64)
                .StoreUInt(ctxId, 32)
                .StoreAddress(platformAddress)
                .Build();

            decimal totalAmount = priceAmount + 0.1m;

            string stateInitBase64 = stateInitCell.ToString("base64");
            string payloadBase64 = payloadCell.ToString("base64");

            return new TonConnectPayloadDto
            {
                ContractAddress = futureAddress.ToString(),
                StateInitBase64 = stateInitBase64,
                PayloadBase64 = payloadBase64,
                AmountNanoTon = new Coins(totalAmount).ToNano()
            };
        }
        catch (Exception ex)
        {
            return Error.Failure("TonContract.PayloadGenerationFailed", ex.Message);
        }
    }

    public async Task<ErrorOr<string>> ResolveBookingByArbiterAsync(string contractAddress, bool isExpertPresent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ArbiterMnemonic))
                return Error.Validation("Arbiter.NoMnemonic", "Немає мнемоніки в налаштуваннях.");

            var getPriceResult = await _tonClient.RunGetMethod(new Address(contractAddress), "getPrice", Array.Empty<IStackItem>());

            if (getPriceResult == null || getPriceResult.Value.ExitCode != 0)
                return Error.Failure("Arbiter.GetPriceFailed", "Не вдалося прочитати storage.price з смарт-контракту.");

            var stackItem = getPriceResult.Value.Stack[0];
            decimal exactPriceDecimal = 0;

            if (stackItem is System.Numerics.BigInteger bigIntPrice)
            {
                exactPriceDecimal = (decimal)bigIntPrice / 1_000_000_000m;
            }
            else
            {
                string priceStr = stackItem.ToString().Replace("0x", "");

                if (stackItem.ToString().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    exactPriceDecimal = Convert.ToUInt64(priceStr, 16) / 1_000_000_000m;
                }
                else
                {
                    exactPriceDecimal = Convert.ToDecimal(priceStr) / 1_000_000_000m;
                }
            }

            decimal studentAmount = 0;
            decimal expertAmount = 0;

            if (isExpertPresent)
            {
                expertAmount = exactPriceDecimal;
            }
            else
            {
                studentAmount = exactPriceDecimal;
            }

            var mnemonic = new Mnemonic(_settings.ArbiterMnemonic.Split(' '));
            var keys = mnemonic.Keys;
            var walletOptions = new WalletV4Options { PublicKey = keys.PublicKey, Workchain = 0 };
            var wallet = new WalletV4(walletOptions, 2);

            uint seqno = (await _tonClient.Wallet.GetSeqno(wallet.Address)) ?? 0;
            uint OP_ARBITER_RESOLVE = 0xA1B2C3D5;

            Cell bodyCell = new CellBuilder()
                .StoreUInt(OP_ARBITER_RESOLVE, 32)
                .StoreCoins(new Coins(studentAmount))
                .StoreCoins(new Coins(expertAmount))
                .Build();

            var msgInfoOptions = new IntMsgInfoOptions
            {
                Dest = new Address(contractAddress),
                Value = new Coins("0.02"),
                Bounce = true,
            };

            var transfer = new WalletTransfer
            {
                Message = new MessageX(new MessageXOptions
                {
                    Info = new IntMsgInfo(msgInfoOptions),
                    Body = bodyCell,
                    StateInit = null
                }),
                Mode = 1
            };

            var externalMessage = wallet.CreateTransferMessage(new[] { transfer }, seqno);
            externalMessage.Sign(keys.PrivateKey);

            await _tonClient.SendBoc(externalMessage.Cell);

            return "Success";
        }
        catch (Exception ex)
        {
            return Error.Failure("Arbiter.ConfirmationFailed", ex.Message);
        }
    }

    public Task<ErrorOr<TonConnectPayloadDto>> GenerateCancelBookingPayloadAsync(bool isStudent, string contractAddress)
    {
        try
        {
            uint opCode = isStudent ? 0xA1B2C3D2 : 0xA1B2C3D3;

            Cell payloadCell = new CellBuilder()
                .StoreUInt(opCode, 32)
                .Build();

            var dto = new TonConnectPayloadDto
            {
                ContractAddress = contractAddress,
                StateInitBase64 = null,
                PayloadBase64 = payloadCell.ToString("base64"),
                AmountNanoTon = new Coins("0.02").ToNano()
            };

            return Task.FromResult<ErrorOr<TonConnectPayloadDto>>(dto);
        }
        catch (Exception ex)
        {
            return Task.FromResult<ErrorOr<TonConnectPayloadDto>>(Error.Failure("TonContract.CancelPayloadFailed", ex.Message));
        }
    }
}