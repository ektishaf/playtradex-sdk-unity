using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayTradeX;
using PlayTradeX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayTradeXTestUsage : MonoBehaviour
{
    // ============================================================
    // PlayTradeX
    // ============================================================

    [Header("PlayTradeX")]

    [SerializeField]
    private PlayTradeXUnity playtradexUnity;

    [SerializeField]
    private PlayTradeXActivityLog activityLog;


    // ============================================================
    // SDK Status UI
    // ============================================================

    [Header("SDK Status")]

    [SerializeField]
    private Image sdkConnectionIndicator;

    [SerializeField]
    private TextMeshProUGUI sdkConnectionText;

    [SerializeField]
    private TextMeshProUGUI sdkConnectionExplanationText;

    [SerializeField]
    private TextMeshProUGUI walletAddressText;


    // ============================================================
    // Demo Selection
    // ============================================================

    [Header("Demo Selection")]

    [Tooltip(
        "Network used by the sample buttons. " +
        "This does not change any global SDK network.")]
    [SerializeField]
    private TMP_Dropdown networkDropdown;

    [Tooltip(
        "Wallet used by the sample buttons. " +
        "This does not change any global SDK wallet.")]
    [SerializeField]
    private TMP_Dropdown walletDropdown;


    private readonly List<NetworkConfigSettings> _networks =
        new List<NetworkConfigSettings>();

    private readonly List<WalletConfigSettings> _wallets =
        new List<WalletConfigSettings>();


    private NetworkConfigSettings _selectedNetwork;

    private WalletConfigSettings _selectedWallet;


    // ============================================================
    // Transaction Test Data
    // ============================================================

    [Header("Transaction Test Data")]

    [SerializeField]
    private string beneficiaryWalletAddress;

    [SerializeField]
    private string nativeAmountToSend =
        "0.01";

    [SerializeField]
    private string tokenAmountToSend =
        "1";


    // ============================================================
    // Wallet Backup
    // ============================================================

    [Header("Wallet Backup")]

    [SerializeField]
    private string password =
        "My-Secure-Wallet-Password";


    // ============================================================
    // Selected Context
    // ============================================================

    public string SelectedNetworkId =>
        _selectedNetwork != null
            ? _selectedNetwork.Id
            : string.Empty;


    public string SelectedWalletId =>
        _selectedWallet != null
            ? _selectedWallet.Id
            : string.Empty;


    public string SelectedWalletAddress =>
        _selectedWallet != null
            ? _selectedWallet.Address
            : string.Empty;


    // ============================================================
    // Unity Lifecycle
    // ============================================================

    private void Awake()
    {
        if (playtradexUnity == null)
        {
            Debug.LogError(
                "[PlayTradeX Sample] PlayTradeXUnity is not assigned.");

            return;
        }


        playtradexUnity.Ready +=
            PlayTradeXUnity_Ready;


        playtradexUnity.NotReady +=
            PlayTradeXUnity_NotReady;

        playtradexUnity.TransactionMined +=
            PlayTradeXUnity_TransactionMined;


        if (networkDropdown != null)
        {
            networkDropdown.onValueChanged.AddListener(
                OnNetworkChanged);
        }


        if (walletDropdown != null)
        {
            walletDropdown.onValueChanged.AddListener(
                OnWalletChanged);
        }
    }


    private void Start()
    {
        LoadProjectSettings();
    }


    private void OnDestroy()
    {
        if (playtradexUnity != null)
        {
            playtradexUnity.Ready -=
                PlayTradeXUnity_Ready;


            playtradexUnity.NotReady -=
                PlayTradeXUnity_NotReady;

            playtradexUnity.TransactionMined -=
                PlayTradeXUnity_TransactionMined;
        }


        if (networkDropdown != null)
        {
            networkDropdown.onValueChanged.RemoveListener(
                OnNetworkChanged);
        }


        if (walletDropdown != null)
        {
            walletDropdown.onValueChanged.RemoveListener(
                OnWalletChanged);
        }
    }


    // ============================================================
    // Project Settings
    // ============================================================

    private void LoadProjectSettings()
    {
        PlayTradeXSettings settings =
            PlayTradeXSettings.Instance;


        if (settings == null)
        {
            activityLog?.Fail(
                "[PlayTradeX] Project Settings could not be loaded.");

            return;
        }


        LoadNetworks(
            settings);


        LoadWallets(
            settings);


        RefreshSelectionUI();
    }


    // ============================================================
    // Networks
    // ============================================================

    private void LoadNetworks(
        PlayTradeXSettings settings)
    {
        _networks.Clear();


        if (settings.Networks != null)
        {
            for (int i = 0;
                 i < settings.Networks.Count;
                 ++i)
            {
                NetworkConfigSettings network =
                    settings.Networks[i];


                if (network == null ||
                    string.IsNullOrWhiteSpace(
                        network.Id))
                {
                    continue;
                }


                _networks.Add(
                    network);
            }
        }


        if (networkDropdown == null)
        {
            return;
        }


        networkDropdown.ClearOptions();


        List<string> options =
            new List<string>();


        for (int i = 0;
             i < _networks.Count;
             ++i)
        {
            options.Add(
                GetNetworkDisplayName(
                    _networks[i]));
        }


        networkDropdown.AddOptions(
            options);


        if (_networks.Count > 0)
        {
            networkDropdown.SetValueWithoutNotify(
                0);


            SelectNetwork(
                0);
        }
        else
        {
            _selectedNetwork =
                null;


            activityLog?.Fail(
                "[PlayTradeX] No networks are configured in Project Settings.");
        }


        networkDropdown.RefreshShownValue();
    }


    // ============================================================
    // Wallets
    // ============================================================

    private void LoadWallets(
        PlayTradeXSettings settings)
    {
        _wallets.Clear();


        if (settings.Wallets != null)
        {
            for (int i = 0;
                 i < settings.Wallets.Count;
                 ++i)
            {
                WalletConfigSettings wallet =
                    settings.Wallets[i];


                if (wallet == null ||
                    string.IsNullOrWhiteSpace(
                        wallet.Id))
                {
                    continue;
                }


                _wallets.Add(
                    wallet);
            }
        }


        if (walletDropdown == null)
        {
            return;
        }


        walletDropdown.ClearOptions();


        List<string> options =
            new List<string>();


        for (int i = 0;
             i < _wallets.Count;
             ++i)
        {
            options.Add(
                GetWalletDisplayName(
                    _wallets[i]));
        }


        walletDropdown.AddOptions(
            options);


        if (_wallets.Count > 0)
        {
            walletDropdown.SetValueWithoutNotify(
                0);


            SelectWallet(
                0);
        }
        else
        {
            _selectedWallet =
                null;


            activityLog?.Fail(
                "[PlayTradeX] No wallets are configured in Project Settings.");
        }


        walletDropdown.RefreshShownValue();
    }


    // ============================================================
    // Dropdown Events
    // ============================================================

    private void OnNetworkChanged(
        int index)
    {
        SelectNetwork(
            index);


        RefreshSelectionUI();
    }


    private void OnWalletChanged(
        int index)
    {
        SelectWallet(
            index);


        RefreshSelectionUI();
    }


    private void SelectNetwork(
        int index)
    {
        if (index < 0 ||
            index >= _networks.Count)
        {
            _selectedNetwork =
                null;

            return;
        }


        _selectedNetwork =
            _networks[index];


        activityLog?.Success(
            $"[PlayTradeX] Demo network selected: " +
            $"{_selectedNetwork.Id}");
    }


    private void SelectWallet(
        int index)
    {
        if (index < 0 ||
            index >= _wallets.Count)
        {
            _selectedWallet =
                null;

            return;
        }


        _selectedWallet =
            _wallets[index];


        activityLog?.Success(
            $"[PlayTradeX] Demo wallet selected: " +
            $"{_selectedWallet.Id}");
    }


    // ============================================================
    // SDK State
    // ============================================================

    private void PlayTradeXUnity_Ready(
        string address)
    {
        activityLog?.Success(
            "[PlayTradeX] PlayTradeX SDK is ready.");


        if (sdkConnectionIndicator != null)
        {
            sdkConnectionIndicator.color =
                Color.green;
        }


        if (sdkConnectionText != null)
        {
            sdkConnectionText.color =
                Color.green;


            sdkConnectionText.text =
                "SDK Ready";
        }


        RefreshSelectionUI();
    }


    private void PlayTradeXUnity_NotReady()
    {
        activityLog?.Fail(
            "[PlayTradeX] PlayTradeX SDK is not ready.");


        if (sdkConnectionIndicator != null)
        {
            sdkConnectionIndicator.color =
                Color.red;
        }


        if (sdkConnectionText != null)
        {
            sdkConnectionText.color =
                Color.red;


            sdkConnectionText.text =
                "SDK Not Ready";
        }


        if (sdkConnectionExplanationText != null)
        {
            sdkConnectionExplanationText.text =
                "Not initialized";
        }


        if (walletAddressText != null)
        {
            walletAddressText.text =
                string.Empty;
        }
    }

    // ============================================================
    // Transaction Events
    // ============================================================

    private void PlayTradeXUnity_TransactionMined(
        TransactionEvent transactionEvent)
    {
        if (transactionEvent == null)
        {
            activityLog?.Fail(
                "[PlayTradeX] Received null transaction mined event.");

            return;
        }


        string message =
            "[PlayTradeX] Transaction mined.\n" +
            $"Transaction ID: {transactionEvent.TransactionId}\n" +
            $"Network: {transactionEvent.NetworkId}\n" +
            $"Chain ID: {transactionEvent.ChainId}\n" +
            $"TxHash: {transactionEvent.TransactionHash}\n" +
            $"Status: {transactionEvent.Status}\n" +
            $"Receipt: {transactionEvent.Receipt}";


        if (transactionEvent.Status ==
            TransactionEventStatus.Confirmed)
        {
            activityLog?.Success(
                message);
        }
        else
        {
            activityLog?.Fail(
                message);
        }
    }


    // ============================================================
    // Selection UI
    // ============================================================

    private void RefreshSelectionUI()
    {
        if (sdkConnectionExplanationText != null)
        {
            if (_selectedNetwork != null)
            {
                sdkConnectionExplanationText.text =
                    $"Network: {_selectedNetwork.Id}";
            }
            else
            {
                sdkConnectionExplanationText.text =
                    "No network selected";
            }
        }


        if (walletAddressText != null)
        {
            walletAddressText.text =
                _selectedWallet != null
                    ? ShortenAddress(
                        _selectedWallet.Address)
                    : string.Empty;
        }
    }


    // ============================================================
    // Get Native Balance
    // ============================================================

    public async void GetNativeBalance()
    {
        if (!CanExecute(
                requireWallet: true))
        {
            return;
        }


        try
        {
            NativeBalanceResponse response =
                await playtradexUnity.GetNativeBalanceForAddress(
                    SelectedNetworkId,
                    SelectedWalletAddress);


            if (response == null)
            {
                return;
            }


            if (response.Success)
            {
                activityLog?.Success(
                    $"[PlayTradeX] {SelectedWalletId}: " +
                    $"{response.Balance} " +
                    $"{GetSelectedNetworkSymbol()}");
            }
            else
            {
                LogFailure(
                    "Get native balance",
                    response.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            HandleException(
                "Get native balance",
                exception);
        }
    }


    // ============================================================
    // Send Native Balance
    // ============================================================

    public async void SendNativeBalance()
    {
        if (!CanExecute(
                requireWallet: true))
        {
            return;
        }


        if (!ValidateBeneficiary())
        {
            return;
        }


        if (!ValidateSelectedWalletPrivateKey())
        {
            return;
        }


        try
        {
            TransactionResponse response =
                await playtradexUnity.SendEthWithWallet(
                    SelectedNetworkId,
                    _selectedWallet.PrivateKey,
                    beneficiaryWalletAddress,
                    PlayTradeXUnits.ToWei(
                        nativeAmountToSend));


            HandleTransactionResponse(
                "Native transfer",
                response);
        }
        catch (Exception exception)
        {
            HandleException(
                "Native transfer",
                exception);
        }
    }


    // ============================================================
    // Read Contract
    // ============================================================

    public async void ReadContract()
    {
        if (!CanExecute(
                requireWallet: false))
        {
            return;
        }


        if (!ValidateBeneficiary())
        {
            return;
        }


        try
        {
            ContractReadResponse response =
                await playtradexUnity.Read(
                    SelectedNetworkId,
                    SampleContract.Address,
                    SampleContract.balanceOf_1_Address,
                    @$"[""{beneficiaryWalletAddress}""]");


            if (response == null)
            {
                return;
            }


            if (response.Success)
            {
                activityLog?.Success(
                    "[PlayTradeX] Contract read: " +
                    PlayTradeXUnits.FromWei(
                        response.Data));
            }
            else
            {
                LogFailure(
                    "Contract read",
                    response.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            HandleException(
                "Contract read",
                exception);
        }
    }


    // ============================================================
    // Write Contract
    // ============================================================

    public async void WriteContract()
    {
        if (!CanExecute(
                requireWallet: true))
        {
            return;
        }


        if (!ValidateBeneficiary())
        {
            return;
        }


        if (!ValidateSelectedWalletPrivateKey())
        {
            return;
        }


        try
        {
            TransactionResponse response =
                await playtradexUnity.WriteWithWallet(
                    SelectedNetworkId,
                    _selectedWallet.PrivateKey,
                    SampleContract.Address,
                    SampleContract.transfer_2_Address_Uint256,
                    @$"[""{beneficiaryWalletAddress}"", " +
                    @$"""{PlayTradeXUnits.ToWei(tokenAmountToSend)}""]");


            HandleTransactionResponse(
                "Contract write",
                response);
        }
        catch (Exception exception)
        {
            HandleException(
                "Contract write",
                exception);
        }
    }


    // ============================================================
    // Export Selected Wallet
    // ============================================================

    public async void ExportWallet()
    {
        if (!CanExecute(
                requireWallet: true))
        {
            return;
        }


        /*
         * The current ExportWallet API exports the SDK-managed
         * identity wallet.
         *
         * If ExportWallet has already been updated to accept a wallet
         * ID/private key in your current runtime API, replace this call
         * with that overload.
         */
        try
        {
            string outputPath =
                await PlayTradeXFilePicker.SaveWalletFileAsync();


            if (string.IsNullOrEmpty(
                    outputPath))
            {
                activityLog?.Fail(
                    "[PlayTradeX] Wallet export cancelled.");

                return;
            }


            WalletExportResponse response =
                await playtradexUnity.ExportWallet(
                    password,
                    outputPath);


            if (response == null)
            {
                return;
            }


            if (response.Success)
            {
                activityLog?.Success(
                    "[PlayTradeX] Wallet exported successfully.");
            }
            else
            {
                LogFailure(
                    "Wallet export",
                    response.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            HandleException(
                "Wallet export",
                exception);
        }
    }


    // ============================================================
    // Import Wallet
    // ============================================================

    public async void ImportWallet()
    {
        if (playtradexUnity == null)
        {
            return;
        }


        try
        {
            string inputPath =
                await PlayTradeXFilePicker.OpenWalletFileAsync();


            if (string.IsNullOrEmpty(
                    inputPath))
            {
                activityLog?.Fail(
                    "[PlayTradeX] Wallet import cancelled.");

                return;
            }


            WalletImportResponse response =
                await playtradexUnity.ImportWallet(
                    password,
                    inputPath);


            if (response == null)
            {
                return;
            }


            if (response.Success)
            {
                activityLog?.Success(
                    "[PlayTradeX] Wallet imported: " +
                    ShortenAddress(
                        response.WalletAddress));
            }
            else
            {
                LogFailure(
                    "Wallet import",
                    response.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            HandleException(
                "Wallet import",
                exception);
        }
    }


    // ============================================================
    // Activity Log
    // ============================================================

    public void ClearActivityLogs()
    {
        activityLog?.Clear();
    }


    // ============================================================
    // Validation
    // ============================================================

    private bool CanExecute(
        bool requireWallet)
    {
        if (playtradexUnity == null)
        {
            Debug.LogError(
                "[PlayTradeX Sample] PlayTradeXUnity is not assigned.");

            return false;
        }


        if (!playtradexUnity.IsReady)
        {
            activityLog?.Fail(
                "[PlayTradeX] SDK is not ready.");

            return false;
        }


        if (_selectedNetwork == null ||
            string.IsNullOrWhiteSpace(
                SelectedNetworkId))
        {
            activityLog?.Fail(
                "[PlayTradeX] Select a network.");

            return false;
        }


        if (requireWallet &&
            (_selectedWallet == null ||
             string.IsNullOrWhiteSpace(
                 SelectedWalletId)))
        {
            activityLog?.Fail(
                "[PlayTradeX] Select a wallet.");

            return false;
        }


        return true;
    }


    private bool ValidateSelectedWalletPrivateKey()
    {
        if (_selectedWallet != null &&
            !string.IsNullOrWhiteSpace(
                _selectedWallet.PrivateKey))
        {
            return true;
        }


        activityLog?.Fail(
            "[PlayTradeX] The selected wallet does not contain " +
            "a private key.");


        return false;
    }


    private bool ValidateBeneficiary()
    {
        if (!string.IsNullOrWhiteSpace(
                beneficiaryWalletAddress))
        {
            return true;
        }


        activityLog?.Fail(
            "[PlayTradeX] Beneficiary wallet address is not configured.");


        return false;
    }


    // ============================================================
    // Response Helpers
    // ============================================================

    private void HandleTransactionResponse(
        string operation,
        TransactionResponse response)
    {
        if (response == null)
        {
            return;
        }


        if (response.Success)
        {
            activityLog?.Success(
                $"[PlayTradeX] {operation} successful. " +
                $"TxHash: {response.TransactionHash}");
        }
        else
        {
            LogFailure(
                operation,
                response.ErrorMessage);
        }
    }


    private void LogFailure(
        string operation,
        string error)
    {
        string message =
            string.IsNullOrWhiteSpace(
                error)
                ? "Unknown error."
                : error;


        activityLog?.Fail(
            $"[PlayTradeX] {operation} failed: {message}");
    }


    private void HandleException(
        string operation,
        Exception exception)
    {
        Debug.LogException(
            exception);


        activityLog?.Fail(
            $"[PlayTradeX] {operation} exception: " +
            exception.Message);
    }


    // ============================================================
    // Display Helpers
    // ============================================================

    private static string GetNetworkDisplayName(
        NetworkConfigSettings network)
    {
        if (network == null)
        {
            return "Unknown";
        }


        if (!string.IsNullOrWhiteSpace(
                network.Symbol))
        {
            return
                $"{network.NetworkName}";
        }


        return network.Id;
    }


    private static string GetWalletDisplayName(
        WalletConfigSettings wallet)
    {
        if (wallet == null)
        {
            return "Unknown";
        }


        if (!string.IsNullOrWhiteSpace(
                wallet.Address))
        {
            return
                $"{wallet.Id} - " +
                $"{ShortenAddress(wallet.Address)}";
        }


        return wallet.Id;
    }


    private string GetSelectedNetworkSymbol()
    {
        if (_selectedNetwork == null ||
            string.IsNullOrWhiteSpace(
                _selectedNetwork.Symbol))
        {
            return string.Empty;
        }


        return _selectedNetwork.Symbol;
    }


    private static string ShortenAddress(
        string address)
    {
        if (string.IsNullOrWhiteSpace(
                address))
        {
            return string.Empty;
        }


        if (address.Length <= 10)
        {
            return address;
        }


        return
            $"{address.Substring(0, 6)}..." +
            $"{address.Substring(address.Length - 4)}";
    }

    public void CopyWalletAddress()
{
    if (string.IsNullOrWhiteSpace(
            SelectedWalletAddress))
    {
        activityLog?.Fail(
            "[PlayTradeX] No wallet is selected.");

        return;
    }


    GUIUtility.systemCopyBuffer =
        SelectedWalletAddress;


    activityLog?.Success(
        $"[PlayTradeX] Wallet address copied: " +
        $"{ShortenAddress(SelectedWalletAddress)}");
}
}