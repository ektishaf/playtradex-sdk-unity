using System;
using PlayTradeX;
using PlayTradeX.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayTradeXTestUsage : MonoBehaviour
{
    [SerializeField]
    private PlayTradeXUnity playtradexUnity;

    [SerializeField]
    private PlayTradeXActivityLog activityLog;

    [SerializeField]
    private Image sdkConnectionIndicator;

    [SerializeField]
    private TextMeshProUGUI sdkConnectionText;

    [SerializeField]
    private TextMeshProUGUI sdkConnectionExplanationText;

    [SerializeField]
    private TextMeshProUGUI walletAddressText;

    public string symbol = "ETH";
    public string beneficiaryWalletAddress;
    public string ethAmountToSend = "0.01";
    public string tokenAmountToSend = "1";

    public string walletAddress;

    private string password =
        "My-Secure-Wallet-Password";

    void Awake()
    {
        playtradexUnity.Ready += PlaytradexUnity_Ready;

        playtradexUnity.NotReady += PlaytradexUnity_NotReady;
    }

    private void PlaytradexUnity_Ready(string address)
    {
        walletAddress = address;
        activityLog.Success("[PlayTradeX] PlayTradeX SDK is ready.");
        
        
        sdkConnectionIndicator.color = Color.green;
        sdkConnectionText.color = Color.green;
        sdkConnectionText.text = "SDK Ready";
        sdkConnectionExplanationText.text = "Initialized and ready";
        walletAddressText.text = $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
    }

    private void PlaytradexUnity_NotReady()
    {
        activityLog.Fail("[PlayTradeX] PlayTradeX SDK is not ready.");

        sdkConnectionIndicator.color = Color.red;
        sdkConnectionText.color = Color.red;
        sdkConnectionText.text = "SDK Not Ready";
        sdkConnectionExplanationText.text = "Not initialized";
        walletAddressText.text = "";
    }

    // ============================================================
    // Native Balance
    // ============================================================

    public async void CheckNativeEthBalance()
    {
        if (playtradexUnity == null) return;
        
        try
        {
            NativeBalanceResponse response = await playtradexUnity.GetNativeBalance();

            if (response == null) return;
            
            if (response.Success)
            {
                Debug.Log("TEST received balance: " + response.Balance);
                
                activityLog.Success($"[PlayTradeX] {response.Balance} {symbol}");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }


    // ============================================================
    // Send Native Currency
    // ============================================================

    public async void SendNativeEthBalance()
    {
        if (playtradexUnity == null) return;

        try
        {
            TransactionResponse response = await playtradexUnity.SendEth(beneficiaryWalletAddress, PlayTradeXUnits.ToWei(ethAmountToSend));

            if (response == null) return;

            if (response.Success)
            {
                Debug.Log("TEST transaction hash: " + response.TransactionHash);

                activityLog.Success($"[PlayTradeX] TxHash: {response.TransactionHash}");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }


    // ============================================================
    // Read Contract
    // ============================================================

    public async void ReadTokenBalanceFromContract()
    {
        if (playtradexUnity == null) return;

        try
        {
            ContractReadResponse response = await playtradexUnity.Read(SampleContract.Address, SampleContract.balanceOf_1_Address,
                @$"[""{beneficiaryWalletAddress}""]");

            if (response == null) return;

            if (response.Success)
            {
                Debug.Log("TEST token balance: " + response.Data);
                
                activityLog.Success($"[PlayTradeX] Data: {PlayTradeXUnits.FromWei(response.Data)}");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }


    // ============================================================
    // Write Contract
    // ============================================================

    public async void SendTransactionToContract()
    {
        if (playtradexUnity == null) return;

        try
        {
            TransactionResponse response = await playtradexUnity.Write(SampleContract.Address, SampleContract.transfer_2_Address_Uint256,
                @$"[""{beneficiaryWalletAddress}"", ""{PlayTradeXUnits.ToWei(tokenAmountToSend)}""]");

            if (response == null) return;

            if (response.Success)
            {
                Debug.Log("TEST contract transaction: " + response.TransactionHash);
                
                activityLog.Success($"[PlayTradeX] TxHash: {response.TransactionHash}");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    // ============================================================
    // Export Wallet
    // ============================================================

    public async void ExportWallet()
    {
        string outputPath =
            await PlayTradeXFilePicker.SaveWalletFileAsync();

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.Log(
                "Wallet export cancelled.");


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
            Debug.Log(
                "Wallet exported successfully.");
            activityLog.Success($"[PlayTradeX] Wallet exported successfully.");
        }
        else
        {
            Debug.LogError(
                response.ErrorMessage);
            activityLog.Fail($"[PlayTradeX] Failed to export wallet.");
        }
    }


    // ============================================================
    // Import Wallet
    // ============================================================

    public async void ImportWallet()
    {
        string inputPath =
            await PlayTradeXFilePicker.OpenWalletFileAsync();

        if (string.IsNullOrEmpty(inputPath))
        {
            Debug.Log(
                "Wallet import cancelled.");

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
            Debug.Log(
                "Wallet imported successfully.\n" +
                $"Address: {response.WalletAddress}");
            activityLog.Success($"[PlayTradeX] Wallet imported successfully, Address: {response.WalletAddress}");
        }
        else
        {
            Debug.LogError(
                response.ErrorMessage);

            activityLog.Fail($"[PlayTradeX] Failed to import wallet.");
        }
    }

    public void ClearActivityLogs()
    {
        activityLog.Clear();
    }

    public void CopyWalletAddress()
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            activityLog.Fail("[PlayTradeX] Wallet address is not available.");
            return;
        }

        GUIUtility.systemCopyBuffer = walletAddress;

        activityLog.Success("[PlayTradeX] Wallet address copied to clipboard.");
    }
}