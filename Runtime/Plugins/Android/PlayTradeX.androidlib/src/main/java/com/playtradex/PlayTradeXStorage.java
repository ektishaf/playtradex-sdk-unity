package com.playtradex;

import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.util.Base64;

import java.nio.charset.StandardCharsets;
import java.security.KeyStore;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;

public final class PlayTradeXStorage
{
    private static final String KEYSTORE =
            "AndroidKeyStore";

    private static final String KEY_ALIAS =
            "PlayTradeX.StorageKey";

    private static final String TRANSFORMATION =
            "AES/GCM/NoPadding";

    /*
     * Native JNI initialization.
     *
     * C++ will receive:
     * - JavaVM
     * - this Java class
     * - method IDs
     */
    private static native void nativeInitialize();

    /*
     * When this class is loaded, make sure the
     * native PlayTradeX library is loaded first.
     */
    static
    {
        System.loadLibrary("PlayTradeXSDK");

        nativeInitialize();
    }

    private PlayTradeXStorage()
    {
    }

    public static String initialize()
    {
        return "PlayTradeXStorage";
    }

    private static SecretKey getOrCreateKey()
            throws Exception
    {
        KeyStore keyStore =
                KeyStore.getInstance(KEYSTORE);

        keyStore.load(null);

        /*
         * Existing key?
         */
        if (keyStore.containsAlias(KEY_ALIAS))
        {
            return ((KeyStore.SecretKeyEntry)
                    keyStore.getEntry(
                            KEY_ALIAS,
                            null))
                    .getSecretKey();
        }

        /*
         * Generate a new AES-256 key directly
         * inside Android Keystore.
         *
         * The key is NON-EXPORTABLE.
         */
        KeyGenerator keyGenerator =
                KeyGenerator.getInstance(
                        KeyProperties.KEY_ALGORITHM_AES,
                        KEYSTORE);

        KeyGenParameterSpec spec =
                new KeyGenParameterSpec.Builder(
                        KEY_ALIAS,

                        KeyProperties.PURPOSE_ENCRYPT |
                        KeyProperties.PURPOSE_DECRYPT)

                        .setBlockModes(
                                KeyProperties.BLOCK_MODE_GCM)

                        .setEncryptionPaddings(
                                KeyProperties.ENCRYPTION_PADDING_NONE)

                        .setKeySize(256)

                        .setRandomizedEncryptionRequired(true)

                        .build();

        keyGenerator.init(spec);

        return keyGenerator.generateKey();
    }

    public static String encrypt(
            String plaintext)
            throws Exception
    {
        SecretKey key =
                getOrCreateKey();

        Cipher cipher =
                Cipher.getInstance(
                        TRANSFORMATION);

        cipher.init(
                Cipher.ENCRYPT_MODE,
                key);

        /*
         * Android generates a fresh IV
         * for every encryption.
         */
        byte[] iv =
                cipher.getIV();

        byte[] ciphertext =
                cipher.doFinal(
                        plaintext.getBytes(
                                StandardCharsets.UTF_8));

        String ivBase64 =
                Base64.encodeToString(
                        iv,
                        Base64.NO_WRAP);

        String ciphertextBase64 =
                Base64.encodeToString(
                        ciphertext,
                        Base64.NO_WRAP);

        /*
         * Format:
         *
         * IV:CIPHERTEXT
         *
         * The GCM authentication tag is included
         * at the end of ciphertext by Cipher.doFinal().
         */
        return ivBase64
                + ":"
                + ciphertextBase64;
    }

    public static String decrypt(
            String encrypted)
            throws Exception
    {
        SecretKey key =
                getOrCreateKey();

        String[] parts =
                encrypted.split(":", 2);

        if (parts.length != 2)
        {
            throw new IllegalArgumentException(
                    "Invalid PlayTradeX encrypted data");
        }

        byte[] iv =
                Base64.decode(
                        parts[0],
                        Base64.NO_WRAP);

        byte[] ciphertext =
                Base64.decode(
                        parts[1],
                        Base64.NO_WRAP);

        Cipher cipher =
                Cipher.getInstance(
                        TRANSFORMATION);

        GCMParameterSpec parameterSpec =
                new GCMParameterSpec(
                        128,
                        iv);

        cipher.init(
                Cipher.DECRYPT_MODE,
                key,
                parameterSpec);

        byte[] plaintext =
                cipher.doFinal(
                        ciphertext);

        return new String(
                plaintext,
                StandardCharsets.UTF_8);
    }

    public static boolean deleteKey()
    {
        try
        {
            KeyStore keyStore =
                    KeyStore.getInstance(
                            KEYSTORE);

            keyStore.load(null);

            if (keyStore.containsAlias(
                    KEY_ALIAS))
            {
                keyStore.deleteEntry(
                        KEY_ALIAS);
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static void main(String[] args) {
    try {
        System.out.println("Starting KeyStore JNI Test...");
        
        // This triggers static { System.loadLibrary("PlayTradeXSDK"); nativeInitialize(); }
        String encrypted = encrypt("PlayTradeX_Test_Secret_2026");
        System.out.println("Encrypted: " + encrypted);

        String decrypted = decrypt(encrypted);
        System.out.println("Decrypted: " + decrypted);
    } catch (Exception e) {
        e.printStackTrace();
    }
}
}