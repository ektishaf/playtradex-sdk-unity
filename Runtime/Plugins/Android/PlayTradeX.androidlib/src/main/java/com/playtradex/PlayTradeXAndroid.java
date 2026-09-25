package com.playtradex;

import android.content.ContentResolver;
import android.content.Context;
import android.net.Uri;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

import java.io.File;
import java.io.FileOutputStream;

public final class PlayTradeXAndroid
{
    private static Context applicationContext;
    private static String caBundlePath;

    private static native void nativeInitialize();

    static
    {
        System.loadLibrary("PlayTradeXSDK");
        nativeInitialize();
    }

    private PlayTradeXAndroid()
    {
    }

    public static synchronized boolean initialize(
    Context context)
{
    if (context == null)
    {
        return false;
    }

    if (applicationContext == null)
    {
        applicationContext =
            context.getApplicationContext();

        if (applicationContext == null)
        {
            return false;
        }
    }

    PlayTradeXStorage.initialize();

    if (caBundlePath != null &&
        !caBundlePath.isEmpty() &&
        new File(caBundlePath).isFile())
    {
        return true;
    }

    return
        prepareCaBundle();
}

    public static boolean isInitialized()
    {
        return applicationContext != null;
    }

    private static Context requireContext()
    {
        if (applicationContext == null)
        {
            throw new IllegalStateException(
                    "PlayTradeX Android bridge is not initialized.");
        }

        return applicationContext;
    }

    public static byte[] readContentUri(String uriString)
            throws Exception
    {
        if (uriString == null || uriString.isEmpty())
        {
            throw new IllegalArgumentException(
                    "URI cannot be empty.");
        }

        Context context = requireContext();

        Uri uri = Uri.parse(uriString);

        ContentResolver resolver =
                context.getContentResolver();

        try (InputStream input =
                     resolver.openInputStream(uri))
        {
            if (input == null)
            {
                throw new IllegalStateException(
                        "Failed to open content URI for reading.");
            }

            ByteArrayOutputStream output =
                    new ByteArrayOutputStream();

            byte[] buffer = new byte[4096];

            int totalBytes = 0;
            int bytesRead;

            while ((bytesRead = input.read(buffer)) != -1)
            {
                totalBytes += bytesRead;

                if (totalBytes > 64 * 1024)
                {
                    throw new IllegalArgumentException(
                            "Wallet backup file exceeds maximum allowed size.");
                }

                output.write(buffer, 0, bytesRead);
            }

            return output.toByteArray();
        }
    }

    public static boolean writeContentUri(
            String uriString,
            byte[] data)
            throws Exception
    {
        if (uriString == null || uriString.isEmpty())
        {
            throw new IllegalArgumentException(
                    "URI cannot be empty.");
        }

        if (data == null)
        {
            throw new IllegalArgumentException(
                    "Data cannot be null.");
        }

        Context context = requireContext();

        Uri uri = Uri.parse(uriString);

        ContentResolver resolver =
                context.getContentResolver();

        try (OutputStream output =
                     resolver.openOutputStream(uri, "w"))
        {
            if (output == null)
            {
                throw new IllegalStateException(
                        "Failed to open content URI for writing.");
            }

            output.write(data);
            output.flush();

            return true;
        }
    }

    private static boolean prepareCaBundle()
    {
        if (applicationContext == null)
        {
            return false;
        }

        File directory =
                new File(
                        applicationContext.getFilesDir(),
                        "playtradex");

        if (!directory.exists() &&
            !directory.mkdirs())
        {
            return false;
        }

        File caFile =
                new File(
                        directory,
                        "cacert.pem");

        try
        {
            InputStream input =
                    applicationContext
                            .getAssets()
                            .open("playtradex/cacert.pem");

            try
            {
                FileOutputStream output =
                        new FileOutputStream(
                                caFile,
                                false);

                try
                {
                    byte[] buffer =
                            new byte[8192];

                    int bytesRead;

                    while ((bytesRead = input.read(buffer)) != -1)
                    {
                        output.write(
                                buffer,
                                0,
                                bytesRead);
                    }

                    output.flush();
                }
                finally
                {
                    output.close();
                }
            }
            finally
            {
                input.close();
            }

            if (!caFile.isFile())
            {
                return false;
            }

            if (caFile.length() <= 0)
            {
                return false;
            }

            caBundlePath =
                    caFile.getAbsolutePath();

            return true;
        }
        catch (Exception e)
        {
            caBundlePath = null;

            return false;
        }
    }

    public static synchronized String getCaBundlePath()
    {
        if (applicationContext == null)
        {
            return null;
        }

        if (caBundlePath == null ||
            caBundlePath.isEmpty())
        {
            if (!prepareCaBundle())
            {
                return null;
            }
        }

        return caBundlePath;
    }
}