using System;

namespace PlayTradeX
{
    // ============================================================
    // Network Configuration
    // ============================================================

    /// <summary>
    /// Defines a blockchain network that can be used by PlayTradeX.
    /// </summary>
    /// <remarks>
    /// Networks are supplied to the SDK during initialization.
    ///
    /// The network identifier is the application-facing key used
    /// by PlayTradeX operations to select a configured network.
    ///
    /// Multiple RPC endpoints may be supplied for a network.
    /// </remarks>
    public sealed class NetworkConfig
    {
        /// <summary>
        /// Gets the unique application-facing identifier for the network.
        /// </summary>
        /// <example>
        /// ethereum, polygon, bsc, sepolia
        /// </example>
        public string Id { get; }

        /// <summary>
        /// Gets the EVM chain identifier.
        /// </summary>
        public ulong ChainId { get; }

        /// <summary>
        /// Gets the RPC endpoints configured for the network.
        /// </summary>
        public string[] RpcEndpoints { get; }

        /// <summary>
        /// Creates a blockchain network configuration.
        /// </summary>
        /// <param name="id">
        /// Unique application-facing network identifier.
        /// </param>
        /// <param name="chainId">
        /// EVM chain identifier.
        /// </param>
        /// <param name="rpcEndpoints">
        /// One or more RPC endpoints for the network.
        /// </param>
        public NetworkConfig(
            string id,
            ulong chainId,
            params string[] rpcEndpoints)
        {
            Id = id ?? string.Empty;
            ChainId = chainId;
            RpcEndpoints = rpcEndpoints ?? Array.Empty<string>();
        }
    }


    // ============================================================
    // Initialization
    // ============================================================

    /// <summary>
    /// Represents the result of PlayTradeX SDK initialization.
    /// </summary>
    public sealed class InitializeResult
    {
        /// <summary>
        /// Gets whether the SDK initialized successfully.
        /// </summary>
        public bool Initialized { get; }

        /// <summary>
        /// Gets the initialization error message, or an empty string
        /// when no error was reported.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Gets the wallet address associated with the initialized SDK.
        /// </summary>
        /// <remarks>
        /// This is the SDK-managed PlayTradeX identity wallet.
        /// It is separate from standalone wallets created through
        /// GenerateWallet.
        /// </remarks>
        public string WalletAddress { get; }

        internal InitializeResult(
            bool initialized,
            string error,
            string walletAddress)
        {
            Initialized = initialized;
            Error = error ?? string.Empty;
            WalletAddress = walletAddress ?? string.Empty;
        }
    }


    // ============================================================
    // Base Response
    // ============================================================

    /// <summary>
    /// Represents the common response data returned by PlayTradeX
    /// SDK operations.
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Gets whether the operation completed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the PlayTradeX error code returned by the operation.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Gets the raw response body associated with the operation.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Gets the error message returned by the operation.
        /// </summary>
        public string ErrorMessage { get; }

        internal BaseResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage)
        {
            Success = success;
            ErrorCode = errorCode;
            Body = body ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }
    }


    // ============================================================
    // Blockchain Responses
    // ============================================================

    /// <summary>
    /// Represents a native blockchain balance response.
    /// </summary>
    public sealed class NativeBalanceResponse : BaseResponse
    {
        /// <summary>
        /// Gets the native blockchain balance.
        /// </summary>
        public string Balance { get; }

        internal NativeBalanceResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage,
            string balance)
            : base(success, errorCode, body, errorMessage)
        {
            Balance = balance ?? string.Empty;
        }
    }


    /// <summary>
    /// Represents the result of a blockchain transaction.
    /// </summary>
    public sealed class TransactionResponse : BaseResponse
    {
        /// <summary>
        /// Gets the transaction hash.
        /// </summary>
        public string TransactionHash { get; }

        /// <summary>
        /// Gets the transaction receipt.
        /// </summary>
        public string Receipt { get; }

        internal TransactionResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage,
            string transactionHash,
            string receipt)
            : base(success, errorCode, body, errorMessage)
        {
            TransactionHash = transactionHash ?? string.Empty;
            Receipt = receipt ?? string.Empty;
        }
    }


    /// <summary>
    /// Represents the human-readable form of a contract ABI.
    /// </summary>
    public sealed class HumanReadableAbiResponse : BaseResponse
    {
        /// <summary>
        /// Gets the human-readable ABI.
        /// </summary>
        public string Abi { get; }

        internal HumanReadableAbiResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage,
            string abi)
            : base(success, errorCode, body, errorMessage)
        {
            Abi = abi ?? string.Empty;
        }
    }


    /// <summary>
    /// Represents the result of a smart contract read operation.
    /// </summary>
    public sealed class ContractReadResponse : BaseResponse
    {
        /// <summary>
        /// Gets the data returned by the contract call.
        /// </summary>
        public string Data { get; }

        internal ContractReadResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage,
            string data)
            : base(success, errorCode, body, errorMessage)
        {
            Data = data ?? string.Empty;
        }
    }


    // ============================================================
    // Standalone Wallet Generation
    // ============================================================

    /// <summary>
    /// Represents a standalone EVM wallet generated by PlayTradeX.
    /// </summary>
    /// <remarks>
    /// A generated wallet is not automatically persisted by the
    /// native SDK and does not replace the SDK-managed identity wallet.
    ///
    /// The caller owns the returned wallet credentials and is
    /// responsible for deciding whether and how they are stored.
    /// </remarks>
    public sealed class GeneratedWallet
    {
        /// <summary>
        /// Gets the EVM wallet address.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Gets the wallet public key.
        /// </summary>
        public string PublicKey { get; }

        /// <summary>
        /// Gets the wallet private key.
        /// </summary>
        /// <remarks>
        /// This value grants control of the wallet and should be
        /// handled as sensitive credential material.
        /// </remarks>
        public string PrivateKey { get; }

        internal GeneratedWallet(
            string address,
            string publicKey,
            string privateKey)
        {
            Address = address ?? string.Empty;
            PublicKey = publicKey ?? string.Empty;
            PrivateKey = privateKey ?? string.Empty;
        }
    }


    /// <summary>
    /// Represents the result of standalone wallet generation.
    /// </summary>
    /// <remarks>
    /// This response intentionally does not derive from BaseResponse
    /// because the native GenerateWallet response has a different
    /// C ABI layout.
    /// </remarks>
    public sealed class GenerateWalletResponse
    {
        /// <summary>
        /// Gets whether wallet generation completed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the error code returned by wallet generation.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Gets the generated wallet.
        /// </summary>
        /// <remarks>
        /// This is null when wallet generation fails.
        /// </remarks>
        public GeneratedWallet Wallet { get; }

        /// <summary>
        /// Gets the wallet-generation error message.
        /// </summary>
        public string ErrorMessage { get; }

        internal GenerateWalletResponse(
            bool success,
            int errorCode,
            GeneratedWallet wallet,
            string errorMessage)
        {
            Success = success;
            ErrorCode = errorCode;
            Wallet = wallet;
            ErrorMessage = errorMessage ?? string.Empty;
        }
    }


    // ============================================================
    // Identity Wallet Responses
    // ============================================================

    /// <summary>
    /// Represents the result of a wallet export operation.
    /// </summary>
    /// <remarks>
    /// ExportWallet operates on the SDK-managed PlayTradeX
    /// identity wallet.
    /// </remarks>
    public sealed class WalletExportResponse : BaseResponse
    {
        /// <summary>
        /// Gets the address of the exported wallet.
        /// </summary>
        public string WalletAddress { get; }

        /// <summary>
        /// Gets the destination path of the exported wallet file.
        /// </summary>
        public string FilePath { get; }

        internal WalletExportResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage,
            string walletAddress,
            string filePath)
            : base(success, errorCode, body, errorMessage)
        {
            WalletAddress = walletAddress ?? string.Empty;
            FilePath = filePath ?? string.Empty;
        }
    }


    /// <summary>
    /// Represents the result of a wallet import operation.
    /// </summary>
    /// <remarks>
    /// ImportWallet operates on the SDK-managed PlayTradeX
    /// identity wallet.
    /// </remarks>
    public sealed class WalletImportResponse : BaseResponse
    {
        /// <summary>
        /// Gets the address of the imported wallet.
        /// </summary>
        public string WalletAddress { get; }

        internal WalletImportResponse(
            bool success,
            int errorCode,
            string body,
            string errorMessage,
            string walletAddress)
            : base(success, errorCode, body, errorMessage)
        {
            WalletAddress = walletAddress ?? string.Empty;
        }
    }


    // ============================================================
    // Transaction Preparation
    // ============================================================

    /// <summary>
    /// Represents a transaction prepared by PlayTradeX and awaiting
    /// application consent before submission.
    /// </summary>
    /// <remarks>
    /// The prepared transaction describes the exact network, sender,
    /// transaction data, simulation result, and fee information that
    /// will be used if the transaction is approved.
    /// </remarks>
    public sealed class PreparedTransaction
    {
        /// <summary>
        /// Gets the unique transaction identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the configured PlayTradeX network identifier.
        /// </summary>
        public string NetworkId { get; }

        /// <summary>
        /// Gets the EVM chain identifier of the selected network.
        /// </summary>
        public ulong ChainId { get; }

        /// <summary>
        /// Gets the address that will sign and submit the transaction.
        /// </summary>
        /// <remarks>
        /// For identity-wallet operations this is the PlayTradeX
        /// identity wallet address.
        ///
        /// For external-wallet operations this is the address derived
        /// from the supplied external private key.
        /// </remarks>
        public string FromAddress { get; }

        /// <summary>
        /// Gets the RPC endpoint selected for the transaction.
        /// </summary>
        public string Rpc { get; }

        /// <summary>
        /// Gets the target contract address.
        /// </summary>
        public string ContractAddress { get; }

        /// <summary>
        /// Gets the contract ABI.
        /// </summary>
        public string Abi { get; }

        /// <summary>
        /// Gets the contract function name.
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Gets the encoded or serialized function parameters.
        /// </summary>
        public string Params { get; }

        /// <summary>
        /// Gets the native value attached to the transaction.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the transaction gas limit.
        /// </summary>
        public ulong GasLimit { get; }

        /// <summary>
        /// Gets the base fee per gas.
        /// </summary>
        public string BaseFeePerGas { get; }

        /// <summary>
        /// Gets the maximum priority fee per gas.
        /// </summary>
        public string MaxPriorityFeePerGas { get; }

        /// <summary>
        /// Gets the maximum fee per gas.
        /// </summary>
        public string MaxFeePerGas { get; }

        /// <summary>
        /// Gets the estimated maximum network fee.
        /// </summary>
        public string EstimatedMaxNetworkFee { get; }

        /// <summary>
        /// Gets whether transaction simulation completed successfully.
        /// </summary>
        public bool SimulationSucceeded { get; }

        /// <summary>
        /// Gets the simulation error message, if any.
        /// </summary>
        public string SimulationError { get; }

        /// <summary>
        /// Gets whether the prepared transaction can be submitted.
        /// </summary>
        public bool CanSubmit { get; }

        /// <summary>
        /// Gets the transaction preparation error, if any.
        /// </summary>
        public string PreparationError { get; }

        internal PreparedTransaction(
            string id,
            string networkId,
            ulong chainId,
            string fromAddress,
            string rpc,
            string contractAddress,
            string abi,
            string functionName,
            string params_,
            string value,
            ulong gasLimit,
            string baseFeePerGas,
            string maxPriorityFeePerGas,
            string maxFeePerGas,
            string estimatedMaxNetworkFee,
            bool simulationSucceeded,
            string simulationError,
            bool canSubmit,
            string preparationError)
        {
            Id = id ?? string.Empty;

            NetworkId = networkId ?? string.Empty;
            ChainId = chainId;
            FromAddress = fromAddress ?? string.Empty;

            Rpc = rpc ?? string.Empty;

            ContractAddress = contractAddress ?? string.Empty;
            Abi = abi ?? string.Empty;
            FunctionName = functionName ?? string.Empty;
            Params = params_ ?? string.Empty;
            Value = value ?? string.Empty;

            GasLimit = gasLimit;

            BaseFeePerGas = baseFeePerGas ?? string.Empty;
            MaxPriorityFeePerGas = maxPriorityFeePerGas ?? string.Empty;
            MaxFeePerGas = maxFeePerGas ?? string.Empty;
            EstimatedMaxNetworkFee =
                estimatedMaxNetworkFee ?? string.Empty;

            SimulationSucceeded = simulationSucceeded;
            SimulationError = simulationError ?? string.Empty;

            CanSubmit = canSubmit;
            PreparationError = preparationError ?? string.Empty;
        }
    }
}