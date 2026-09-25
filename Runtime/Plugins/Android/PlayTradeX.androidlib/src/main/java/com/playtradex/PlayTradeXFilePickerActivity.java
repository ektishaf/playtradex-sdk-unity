package com.playtradex;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;

public final class PlayTradeXFilePickerActivity
        extends Activity
{
    private static final int REQUEST_DOCUMENT = 1001;

    private static final String EXTRA_ACTION =
            "playtradex.action";

    private static PlayTradeXFilePicker.Callback callback;


    public static void start(
            Activity activity,
            String action,
            PlayTradeXFilePicker.Callback resultCallback)
    {
        callback = resultCallback;

        Intent intent =
                new Intent(
                        activity,
                        PlayTradeXFilePickerActivity.class);

        intent.putExtra(
                EXTRA_ACTION,
                action);

        activity.startActivity(intent);
    }


    @Override
    protected void onCreate(
            Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        String action =
                getIntent().getStringExtra(
                        EXTRA_ACTION);

        Intent picker;

        if ("export".equals(action))
        {
            picker =
                    new Intent(
                            Intent.ACTION_CREATE_DOCUMENT);
	    
            picker.addCategory(
                    Intent.CATEGORY_OPENABLE);

            picker.setType(
                    "application/octet-stream");

            picker.putExtra(
                    Intent.EXTRA_TITLE,
                    "playtradex-wallet.ptx");

picker.addFlags(
        Intent.FLAG_GRANT_WRITE_URI_PERMISSION |
        Intent.FLAG_GRANT_READ_URI_PERMISSION |
        Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION);
        }
        else
        {
            picker =
                    new Intent(
                            Intent.ACTION_OPEN_DOCUMENT);


            picker.addCategory(
                    Intent.CATEGORY_OPENABLE);

            picker.setType("*/*");

picker.addFlags(
        Intent.FLAG_GRANT_READ_URI_PERMISSION |
        Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION);
        }

        startActivityForResult(
                picker,
                REQUEST_DOCUMENT);
    }


    @Override
    protected void onActivityResult(
            int requestCode,
            int resultCode,
            Intent data)
    {
        super.onActivityResult(
                requestCode,
                resultCode,
                data);

        String uriString = "";

        if (requestCode == REQUEST_DOCUMENT &&
            resultCode == RESULT_OK &&
            data != null &&
            data.getData() != null)
        {
            Uri uri =
        data.getData();

int flags =
        data.getFlags() &
        (Intent.FLAG_GRANT_READ_URI_PERMISSION |
         Intent.FLAG_GRANT_WRITE_URI_PERMISSION);

try
{
    getContentResolver()
            .takePersistableUriPermission(
                    uri,
                    flags);
}
catch (SecurityException ignored)
{
    // Some document providers do not support
    // persistable URI permissions.
}

uriString =
        uri.toString();
        }

        if (callback != null)
        {
            callback.onResult(uriString);
            callback = null;
        }

        finish();
    }


    @Override
    protected void onDestroy()
    {
        super.onDestroy();

        if (isFinishing())
        {
            callback = null;
        }
    }
}