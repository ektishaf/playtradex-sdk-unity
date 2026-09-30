using System;
using System.Collections;

using PlayTradeX;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class PlayTradeXTransactionPopup : MonoBehaviour
{
    // ============================================================
    // Animation / Root
    // ============================================================

    [Header("Popup")]

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private GameObject popup;


    // ============================================================
    // Transaction Information
    // ============================================================

    [Header("Transaction")]

    [SerializeField]
    private TMP_Text operationText;

    [SerializeField]
    private TMP_Text toText;

    [SerializeField]
    private TMP_Text parametersText;

    [SerializeField]
    private TMP_Text valueText;


    // ============================================================
    // Network / Wallet Information
    // ============================================================

    [Header("Network / Wallet")]

    [Tooltip("Optional. Displays the configured PlayTradeX network ID.")]
    [SerializeField]
    private TMP_Text networkText;

    [Tooltip("Optional. Displays the blockchain chain ID.")]
    [SerializeField]
    private TMP_Text chainIdText;

    [Tooltip("Optional. Displays the address that will sign/send the transaction.")]
    [SerializeField]
    private TMP_Text fromAddressText;


    // ============================================================
    // Gas Information
    // ============================================================

    [Header("Gas / Fees")]

    [SerializeField]
    private TMP_Text gasLimitText;

    [SerializeField]
    private TMP_Text baseFeePerGasText;

    [SerializeField]
    private TMP_Text priorityFeePerGasText;

    [SerializeField]
    private TMP_Text maxFeePerGasText;

    [SerializeField]
    private TMP_Text maxNetworkFeeText;


    // ============================================================
    // Preparation / Simulation
    // ============================================================

    [Header("Simulation")]

    [SerializeField]
    private TMP_Text simulationStatus;


    // ============================================================
    // Actions
    // ============================================================

    [Header("Actions")]

    [SerializeField]
    private Button approveButton;

    [SerializeField]
    private Button denyButton;


    // ============================================================
    // State
    // ============================================================

    private Action _approve;
    private Action _deny;

    private Coroutine _hideCoroutine;


    // ============================================================
    // Unity Lifecycle
    // ============================================================

    private void Awake()
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }

        if (approveButton != null)
        {
            approveButton.onClick.AddListener(
                OnApproveClicked);
        }

        if (denyButton != null)
        {
            denyButton.onClick.AddListener(
                OnDenyClicked);
        }
    }


    private void OnDestroy()
    {
        if (approveButton != null)
        {
            approveButton.onClick.RemoveListener(
                OnApproveClicked);
        }

        if (denyButton != null)
        {
            denyButton.onClick.RemoveListener(
                OnDenyClicked);
        }
    }


    private void OnDisable()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(
                _hideCoroutine);

            _hideCoroutine = null;
        }

        Clear();

        if (popup != null)
        {
            popup.SetActive(false);
        }
    }


    // ============================================================
    // Show
    // ============================================================

    /// <summary>
    /// Displays a prepared transaction and waits for application
    /// consent.
    /// </summary>
    /// <remarks>
    /// The transaction has already passed through PlayTradeX
    /// preparation before reaching this popup.
    ///
    /// Network selection and signer resolution are therefore already
    /// complete. This component only presents the resulting transaction
    /// metadata and forwards the user's approve/deny decision.
    /// </remarks>
    public void Show(
        PreparedTransaction transaction,
        Action approve,
        Action deny)
    {
        Debug.Log("Show called man");
        if (transaction == null)
        {
            Debug.LogError(
                "[PlayTradeX Consent] Cannot display a null transaction.");

            return;
        }

        _approve = approve;
        _deny = deny;


        // --------------------------------------------------------
        // Transaction
        // --------------------------------------------------------

        string operation =
            string.IsNullOrEmpty(
                transaction.FunctionName)
                ? "Native Transfer"
                : transaction.FunctionName;


        SetText(
            operationText,
            operation);

        SetText(
            toText,
            DisplayValue(
                transaction.ContractAddress));

        SetText(
            parametersText,
            DisplayValue(
                transaction.Params));

        SetText(
            valueText,
            DisplayValue(
                transaction.Value));


        // --------------------------------------------------------
        // Network / Signer
        // --------------------------------------------------------

        PlayTradeXSettings settings = PlayTradeXSettings.Instance;
        string networkId = transaction.NetworkId;

        NetworkConfigSettings config;
        if(settings.TryGetNetwork(transaction.NetworkId, out config))
        {
            if (!string.IsNullOrEmpty(config.NetworkName))
                networkId = config.NetworkName;
        }

        SetText(
            networkText,
            DisplayValue(
                networkId));

        SetText(
            chainIdText,
            transaction.ChainId > 0
                ? transaction.ChainId.ToString()
                : "Unavailable");

        SetText(
            fromAddressText,
            DisplayValue(
                transaction.FromAddress));


        // --------------------------------------------------------
        // Gas / Fees
        // --------------------------------------------------------

        SetText(
            gasLimitText,
            transaction.GasLimit > 0
                ? transaction.GasLimit.ToString()
                : "Unavailable");

        SetText(
            baseFeePerGasText,
            DisplayValue(
                transaction.BaseFeePerGas));

        SetText(
            priorityFeePerGasText,
            DisplayValue(
                transaction.MaxPriorityFeePerGas));

        SetText(
            maxFeePerGasText,
            DisplayValue(
                transaction.MaxFeePerGas));

        SetText(
            maxNetworkFeeText,
            DisplayValue(
                transaction.EstimatedMaxNetworkFee));


        // --------------------------------------------------------
        // Simulation / Preparation
        // --------------------------------------------------------

        UpdateSimulationStatus(
            transaction);


        // --------------------------------------------------------
        // Submission
        // --------------------------------------------------------

        if (approveButton != null)
        {
            approveButton.interactable =
                transaction.CanSubmit;
        }

        if (denyButton != null)
        {
            denyButton.interactable =
                true;
        }


        // --------------------------------------------------------
        // Popup
        // --------------------------------------------------------

        if (_hideCoroutine != null)
        {
            StopCoroutine(
                _hideCoroutine);

            _hideCoroutine = null;
        }

        if (popup == null)
        {
            Debug.LogError(
                "[PlayTradeX Consent] Popup GameObject is not assigned.");

            return;
        }

        popup.SetActive(true);

        if (_animator != null)
        {
            _animator.Play(
                "ConsentShow",
                0,
                0f);
        }
    }


    // ============================================================
    // Simulation Status
    // ============================================================

    private void UpdateSimulationStatus(
        PreparedTransaction transaction)
    {
        if (simulationStatus == null)
        {
            return;
        }

        if (transaction.SimulationSucceeded)
        {
            simulationStatus.text =
                "Simulation succeeded.\n" +
                "Final execution can still fail if blockchain state " +
                "or network conditions change.";

            simulationStatus.color =
                Color.green;
        }
        else
        {
            string simulationError =
                string.IsNullOrWhiteSpace(
                    transaction.SimulationError)
                    ? "Transaction simulation failed."
                    : transaction.SimulationError;

            simulationStatus.text =
                "WARNING: Simulation failed.\n" +
                simulationError;

            simulationStatus.color =
                Color.red;
        }


        // --------------------------------------------------------
        // Preparation / Submission
        // --------------------------------------------------------

        if (!transaction.CanSubmit)
        {
            string preparationError =
                string.IsNullOrWhiteSpace(
                    transaction.PreparationError)
                    ? "Transaction cannot currently be submitted."
                    : transaction.PreparationError;

            simulationStatus.text +=
                "\n\nSubmission unavailable:\n" +
                preparationError;
        }
    }


    // ============================================================
    // Hide
    // ============================================================

    public void Hide()
    {
        Clear();

        if (_hideCoroutine != null)
        {
            StopCoroutine(
                _hideCoroutine);

            _hideCoroutine = null;
        }

        if (popup == null)
        {
            return;
        }

        /*
         * The component or one of its parents may already be inactive
         * while Unity is shutting down the scene or exiting Play Mode.
         *
         * Starting a coroutine in that state would fail, so immediately
         * hide the popup instead.
         */
        if (!isActiveAndEnabled ||
            !gameObject.activeInHierarchy)
        {
            popup.SetActive(false);

            return;
        }

        _hideCoroutine =
            StartCoroutine(
                HidePopup());
    }


    // ============================================================
    // User Actions
    // ============================================================

    private void OnApproveClicked()
    {
        Action callback =
            _approve;

        /*
         * Clear callbacks before invoking application code so repeated
         * button events cannot approve the same transaction twice.
         */
        Clear();

        callback?.Invoke();
    }


    private void OnDenyClicked()
    {
        Action callback =
            _deny;

        /*
         * Clear callbacks before invoking application code so repeated
         * button events cannot deny the same transaction twice.
         */
        Clear();

        callback?.Invoke();
    }


    // ============================================================
    // Animation
    // ============================================================

    private IEnumerator HidePopup()
    {
        if (popup == null)
        {
            _hideCoroutine = null;

            yield break;
        }

        if (_animator == null)
        {
            popup.SetActive(false);

            _hideCoroutine = null;

            yield break;
        }

        _animator.Play(
            "ConsentHide",
            0,
            0f);

        /*
         * Wait one frame so Animator state information reflects the
         * newly requested hide animation.
         */
        yield return null;

        while (popup.activeInHierarchy &&
               _animator != null &&
               _animator.GetCurrentAnimatorStateInfo(0)
                   .normalizedTime < 1f)
        {
            yield return null;
        }

        if (popup != null)
        {
            popup.SetActive(false);
        }

        _hideCoroutine = null;
    }


    // ============================================================
    // State Cleanup
    // ============================================================

    private void Clear()
    {
        _approve = null;
        _deny = null;

        if (approveButton != null)
        {
            approveButton.interactable =
                false;
        }

        if (denyButton != null)
        {
            denyButton.interactable =
                false;
        }
    }


    // ============================================================
    // UI Helpers
    // ============================================================

    /// <summary>
    /// Writes text only when the corresponding optional UI field
    /// has been assigned.
    /// </summary>
    private static void SetText(
        TMP_Text textComponent,
        string value)
    {
        if (textComponent == null)
        {
            return;
        }

        textComponent.text =
            value;
    }


    /// <summary>
    /// Converts an empty transaction field into a UI-friendly value.
    /// </summary>
    private static string DisplayValue(
        string value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? "Unavailable"
            : value;
    }
}