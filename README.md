# Welcome to Staples!

This application allows users to post donations on StreamLabs for OnScreen alerts.

For more information, join our Discord server: https://discord.gg/ffkevXD


## Turn on  "Less secure app access"

To retrieve donation's email from your Gmail inbox. You need to turn on "Less secure app access".
```
Note: We do not store any sensitive information.
      All information are stored on your local device.
```

1.  Go to the  [Less secure app access](https://myaccount.google.com/lesssecureapps)  section of your Google Account. You might need to sign in.
2.  Turn  **Allow less secure apps**  on.

> **Do not share your password with anyone.
> Please try to use strong password for your email account.**


## StreamLabs Setup

 1. Login to your StreamLabs account.
 2. Go to Setting > API Settings and click on "Register An App"
 3. Fill all the information. <br> Whitelist Users -> Your email address <br> Redirect URI - > `http://127.0.0.1:55555/staples/streamlabs`
 4. After clicking on "Create" paste Client ID and Client Secret Key in applications StreamLabs setting page
