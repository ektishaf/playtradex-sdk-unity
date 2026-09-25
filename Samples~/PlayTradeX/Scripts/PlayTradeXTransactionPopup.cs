using System;
using System.Collections;
using PlayTradeX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayTradeXTransactionPopup : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private GameObject popup;

    [SerializeField]
    private TMP_Text operationText;

    [SerializeField]
    private TMP_Text toText;

    [SerializeField]
    private TMP_Text parametersText;

    [SerializeField]
    private TMP_Text valueText;

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

    [SerializeField]
    private TMP_Text simulationStatus;

    [SerializeField]
    private Button approveButton;

    [SerializeField]
    private Button denyButton;

    private Action _approve;
    private Action _deny;

    private Coroutine _hideCoroutine;

    private void Awake()
    {
        popup.SetActive(false);

        approveButton.onClick.AddListener(OnApproveClicked);
        denyButton.onClick.AddListener(OnDenyClicked);
    }

    private void OnDisable()
{
    if (_hideCoroutine != null)
    {
        StopCoroutine(_hideCoroutine);
        _hideCoroutine = null;
    }

    Clear();

    if (popup != null)
    {
        popup.SetActive(false);
    }
}

    public void Show(PreparedTransaction transaction, Action approve, Action deny)
    {
        _approve = approve;
        _deny = deny;

        // ---------------------------------------------------------
        // Transaction
        // ---------------------------------------------------------

        string operation;

        if (string.IsNullOrEmpty(transaction.FunctionName))
        {
            operation = "Native Transfer";
        }
        else
        {
            operation = transaction.FunctionName;
        }

        string gasLimit = transaction.GasLimit > 0 ? transaction.GasLimit.ToString() : "Unavailable";
        string baseFee = string.IsNullOrEmpty(transaction.BaseFeePerGas) ? "Unavailable" : transaction.BaseFeePerGas;
        string priorityFee = string.IsNullOrEmpty(transaction.MaxPriorityFeePerGas) ? "Unavailable" : transaction.MaxPriorityFeePerGas;
        string maxFee = string.IsNullOrEmpty(transaction.MaxFeePerGas) ? "Unavailable" : transaction.MaxFeePerGas;
        string maxNetworkFee = string.IsNullOrEmpty(transaction.EstimatedMaxNetworkFee) ? "Unavailable" : transaction.EstimatedMaxNetworkFee;

        operationText.text = operation;
        toText.text = transaction.ContractAddress;
        parametersText.text = transaction.Params;
        valueText.text = transaction.Value;
        gasLimitText.text = gasLimit;
        baseFeePerGasText.text = baseFee;
        priorityFeePerGasText.text = priorityFee;
        maxFeePerGasText.text = maxFee;
        maxNetworkFeeText.text = maxNetworkFee;

        // ---------------------------------------------------------
        // Simulation / preparation status
        // ---------------------------------------------------------

        if (transaction.SimulationSucceeded)
        {
            simulationStatus.text =
                "Simulation succeeded.\n" +
                "Final execution can still fail if blockchain state " +
                "or network conditions change.";

            simulationStatus.color = Color.green;
        }
        else
        {
            string error = string.IsNullOrEmpty(transaction.SimulationError) ? "Transaction simulation failed." : transaction.SimulationError;

            simulationStatus.text =
                "WARNING: Simulation failed.\n" +
                error;

            simulationStatus.color = Color.red;
        }

        // ---------------------------------------------------------
        // Submission availability
        // ---------------------------------------------------------

        approveButton.interactable = transaction.CanSubmit;

        if (!transaction.CanSubmit)
        {
            string preparationError = string.IsNullOrEmpty(transaction.PreparationError) ? "Transaction cannot currently be submitted." : transaction.PreparationError;

            simulationStatus.text +=
                "\n\nSubmission unavailable:\n" +
                preparationError;
        }

        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        popup.SetActive(true);

        if (_animator)
        {
            _animator.Play("ConsentShow", 0, 0f);
        }
    }

    public void Hide()
{
    Clear();

    if (_hideCoroutine != null)
    {
        StopCoroutine(_hideCoroutine);
        _hideCoroutine = null;
    }

    // The component or one of its parents may already be inactive
    // while Unity is shutting down the scene / exiting Play Mode.
    if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
    {
        popup.SetActive(false);
        return;
    }

    _hideCoroutine = StartCoroutine(HidePopup());
}

    private void OnApproveClicked()
    {
        Action callback = _approve;

        Clear();

        callback?.Invoke();
    }

    private void OnDenyClicked()
    {
        Debug.Log("PlayTradeXTransactionPopup - OnDenyClicked");
        Action callback = _deny;

        Clear();
        
        callback?.Invoke();

        //StartCoroutine(HidePopup());
    }

    private IEnumerator HidePopup()
    {
        if (!_animator)
        {
            popup.SetActive(false);
            _hideCoroutine = null;
            yield break;
        }

        _animator.Play("ConsentHide", 0, 0f);

        yield return null;

        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        Debug.Log("PlayTradeXTransactionPopup - HidePopup");

        popup.SetActive(false);

        _hideCoroutine = null;
    }

    private void Clear()
    {
        _approve = null;
        _deny = null;

        approveButton.interactable = false;
    }
}