package com.playtradex;

import android.app.Activity;

public final class PlayTradeXFilePicker
{
    public interface Callback
    {
        void onResult(String uri);
    }


    private PlayTradeXFilePicker()
    {
    }


    public static void openExportPicker(
            Activity activity,
            Callback callback)
    {
        open(
                activity,
                "export",
                callback);
    }


    public static void openImportPicker(
            Activity activity,
            Callback callback)
    {
        open(
                activity,
                "import",
                callback);
    }


    private static void open(
            Activity activity,
            String action,
            Callback callback)
    {
        if (activity == null)
        {
            if (callback != null)
            {
                callback.onResult("");
            }

            return;
        }

        PlayTradeXFilePickerActivity.start(
                activity,
                action,
                callback);
    }
}