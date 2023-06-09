# My Marketplace
This is a sample application to serve as a place to host private VS Code extensions.  

It is not official.  It also does not automatically enable you to connect to it from within VS Code itself.

To install an extension from this marketplace, it will copy it locally from the source first then install.

![Screenshot of the home page of this app](docs/screenshot.png)


## VS Code Extension
A complimentary extension to read this private marketplace (or directory sources) is available at [timheuer/mympclient](https://github.com/timheuer/mympclient).

## How to use (for auth--not fully implemented)
You'll need to have an Azure AD app registration and configure it in either your appsettings.json file or in environment/config for the deployed app.

You need to assign a group Users to the app registration and add users to that group.

## Credits
This was inspired by [Prashant](https://github.com/prashantvc)'s https://github.com/prashantvc/extensions repository which is a React front-end application and I wanted to convert this completely to .NET and Blazor.  All credit for the idea and functionality in the initial implementation goes to Prashant.

Default icons used in this app are from:
- <a href="https://www.flaticon.com/free-icons/box" title="box icons">Box icons created by Good Ware - Flaticon</a>
- Marketplace icon is 'market place' by Becris from <a href="https://thenounproject.com/browse/icons/term/market-place/" target="_blank" title="market place Icons">Noun Project</a>