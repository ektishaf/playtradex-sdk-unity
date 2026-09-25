using System;
using System.Globalization;
using System.Numerics;

namespace PlayTradeX
{
    /// <summary>
    /// Provides exact conversion utilities between human-readable
    /// blockchain amounts and integer base-unit amounts.
    /// </summary>
    /// <remarks>
    /// Blockchain amounts should be represented as strings whenever
    /// possible to avoid floating-point precision loss.
    ///
    /// For ERC-20 tokens, use ToBaseUnit and FromBaseUnit with the
    /// token's decimals value.
    ///
    /// For EVM native currency such as ETH, BNB, POL, and other
    /// 18-decimal native currencies, ToWei and FromWei provide
    /// convenient 18-decimal conversions.
    /// </remarks>
    public static class PlayTradeXUnits
    {
        /// <summary>
        /// Number of decimal places used by wei-based EVM native currency.
        /// </summary>
        public const int WeiDecimals = 18;


        // ============================================================
        // Generic Base Unit Conversion
        // ============================================================

        /// <summary>
        /// Converts a human-readable blockchain amount into its
        /// integer base-unit representation.
        /// </summary>
        /// <param name="amount">
        /// Human-readable amount.
        ///
        /// Example: "1.5"
        /// </param>
        /// <param name="decimals">
        /// Number of decimal places used by the token or currency.
        /// </param>
        /// <returns>
        /// Integer base-unit amount represented as a decimal string.
        /// </returns>
        /// <example>
        /// ToBaseUnit("1", 18) returns
        /// "1000000000000000000".
        ///
        /// ToBaseUnit("1.5", 18) returns
        /// "1500000000000000000".
        ///
        /// ToBaseUnit("25.50", 6) returns
        /// "25500000".
        /// </example>
        public static string ToBaseUnit(
            string amount,
            int decimals)
        {
            ValidateDecimals(decimals);

            if (string.IsNullOrWhiteSpace(amount))
            {
                throw new ArgumentException(
                    "Amount cannot be null or empty.",
                    nameof(amount));
            }

            amount = amount.Trim();

            if (amount.StartsWith(
                "-",
                StringComparison.Ordinal))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Amount cannot be negative.");
            }

            if (amount.StartsWith(
                "+",
                StringComparison.Ordinal))
            {
                amount = amount.Substring(1);
            }

            if (amount.Length == 0)
            {
                throw new ArgumentException(
                    "Amount is invalid.",
                    nameof(amount));
            }

            int decimalPoint =
                amount.IndexOf('.');

            if (decimalPoint != amount.LastIndexOf('.'))
            {
                throw new FormatException(
                    "Amount contains multiple decimal points.");
            }

            string wholePart;
            string fractionalPart;

            if (decimalPoint >= 0)
            {
                wholePart =
                    amount.Substring(0, decimalPoint);

                fractionalPart =
                    amount.Substring(decimalPoint + 1);
            }
            else
            {
                wholePart = amount;
                fractionalPart = string.Empty;
            }

            if (wholePart.Length == 0)
            {
                wholePart = "0";
            }

            if (!IsDigitsOnly(wholePart) ||
                !IsDigitsOnly(fractionalPart))
            {
                throw new FormatException(
                    "Amount must contain only decimal digits " +
                    "and an optional decimal point.");
            }

            if (fractionalPart.Length > decimals)
            {
                throw new ArgumentException(
                    $"Amount contains more than {decimals} " +
                    "decimal places.",
                    nameof(amount));
            }

            fractionalPart =
                fractionalPart.PadRight(decimals, '0');

            string combined =
                wholePart + fractionalPart;

            if (combined.Length == 0)
            {
                return "0";
            }

            BigInteger value =
                BigInteger.Parse(
                    combined,
                    CultureInfo.InvariantCulture);

            return value.ToString(
                CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Converts an integer blockchain base-unit amount into a
        /// human-readable decimal amount.
        /// </summary>
        /// <param name="amount">
        /// Integer base-unit amount represented as a decimal string.
        /// </param>
        /// <param name="decimals">
        /// Number of decimal places used by the token or currency.
        /// </param>
        /// <returns>
        /// Human-readable decimal amount.
        /// </returns>
        /// <example>
        /// FromBaseUnit("1000000000000000000", 18)
        /// returns "1".
        ///
        /// FromBaseUnit("1500000000000000000", 18)
        /// returns "1.5".
        ///
        /// FromBaseUnit("25500000", 6)
        /// returns "25.5".
        /// </example>
        public static string FromBaseUnit(
            string amount,
            int decimals)
        {
            ValidateDecimals(decimals);

            if (string.IsNullOrWhiteSpace(amount))
            {
                throw new ArgumentException(
                    "Amount cannot be null or empty.",
                    nameof(amount));
            }

            amount = amount.Trim();

            bool negative =
                amount.StartsWith(
                    "-",
                    StringComparison.Ordinal);

            if (negative)
            {
                amount = amount.Substring(1);
            }
            else if (amount.StartsWith(
                "+",
                StringComparison.Ordinal))
            {
                amount = amount.Substring(1);
            }

            if (amount.Length == 0 ||
                !IsDigitsOnly(amount))
            {
                throw new FormatException(
                    "Base-unit amount must be an integer.");
            }

            BigInteger parsed =
                BigInteger.Parse(
                    amount,
                    CultureInfo.InvariantCulture);

            string digits =
                parsed.ToString(
                    CultureInfo.InvariantCulture);

            if (decimals == 0)
            {
                return negative && parsed != BigInteger.Zero
                    ? "-" + digits
                    : digits;
            }

            if (digits.Length <= decimals)
            {
                digits =
                    digits.PadLeft(
                        decimals + 1,
                        '0');
            }

            int decimalPosition =
                digits.Length - decimals;

            string wholePart =
                digits.Substring(
                    0,
                    decimalPosition);

            string fractionalPart =
                digits.Substring(
                    decimalPosition);

            fractionalPart =
                fractionalPart.TrimEnd('0');

            string result =
                fractionalPart.Length == 0
                    ? wholePart
                    : wholePart + "." + fractionalPart;

            if (negative && parsed != BigInteger.Zero)
            {
                result = "-" + result;
            }

            return result;
        }


        // ============================================================
        // Wei Conversion
        // ============================================================

        /// <summary>
        /// Converts a human-readable 18-decimal EVM native currency
        /// amount into wei.
        /// </summary>
        /// <param name="amount">
        /// Human-readable amount such as "0.01".
        /// </param>
        /// <returns>
        /// Amount in wei represented as a decimal integer string.
        /// </returns>
        /// <example>
        /// ToWei("1") returns "1000000000000000000".
        ///
        /// ToWei("0.01") returns "10000000000000000".
        /// </example>
        public static string ToWei(
            string amount)
        {
            return ToBaseUnit(
                amount,
                WeiDecimals);
        }


        /// <summary>
        /// Converts wei into a human-readable 18-decimal EVM
        /// native currency amount.
        /// </summary>
        /// <param name="wei">
        /// Amount in wei represented as a decimal integer string.
        /// </param>
        /// <returns>
        /// Human-readable amount.
        /// </returns>
        /// <example>
        /// FromWei("1000000000000000000") returns "1".
        ///
        /// FromWei("10000000000000000") returns "0.01".
        /// </example>
        public static string FromWei(
            string wei)
        {
            return FromBaseUnit(
                wei,
                WeiDecimals);
        }


        // ============================================================
        // Validation
        // ============================================================

        private static void ValidateDecimals(
            int decimals)
        {
            if (decimals < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(decimals),
                    "Decimals cannot be negative.");
            }
        }

        private static bool IsDigitsOnly(
            string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];

                if (character < '0' ||
                    character > '9')
                {
                    return false;
                }
            }

            return true;
        }
    }
}