# Asset Store Uploader

The Asset Store Uploader allows uploading your content to the [Unity Publisher Portal](https://publisher.unity.com/) .

# Opening the Uploader window

To open the Uploader window, navigate to the Unity Editor main menu and select **Asset Store Tools > Asset Store Uploader**

# Uploader window usage

## Logging in

![The Uploader window](images/uploader-window-login.png)

Before any content uploading can be performed, you must first authenticate with your Unity account. Logging in can be done using either of the following:
- Account credentials (email and password)
- Cloud access token
- Session token

>[!NOTE]
>Logging in can be done with any Unity account, but only Unity accounts with a valid publisher id are able to upload content to the Publisher Portal.
>
>If you do not have a Unity Publisher account, you can create one [here](https://publisher.unity.com/access)

### Cloud access token

A cloud access token is a token associated with your Unity account. In order to use this log-in method, Unity Editor needs to be authenticated with your Unity account beforehand. This can be done in Unity Hub, before launching the Editor or within the Unity Editor 

### Session token

Once successfully logged in using either credential authentication or cloud access token authentication, session token is used for future authentication by default. Session token is cleared when logging out.

## The package list

After successfully authenticating with the Publisher Portal, a list of packages is displayed in the Uploader window.

![List of packages](images/uploader-window-package-list.png)

![label-a](images/label-a.png) **Package search bar** can be used to filter packages by name or by category

![label-b](images/label-b.png) **Package sorting dropdown** can be used to sort packages by name, category or last updated date

![label-c](images/label-c.png) **Package category dropdown** can be expanded or closed to reveal packages of different status

![label-d](images/label-d.png) **A single package element** displays information about a specific package - name, category, size and last updated date

![label-e](images/label-e.png) **Publisher Portal link** opens the Publisher Portal URL that is associated with the package

![label-f](images/label-f.png) **Current email** displays the authenticated account

![label-g](images/label-g.png) **Package refresh button** refreshes the list of packages

![label-h](images/label-h.png) **Logout button** logs the user out and displays the login interface

## Uploading content to the Publisher Portal

>[!NOTE]
>Only packages with a 'Draft' status can be selected for uploading content.

Selecting a package from the list of packages presents an uploading workflow selection

### Asset Upload workflow (From Assets Folder)

![Assets folder workflow](images/uploader-window-asset-workflow.png)

![label-a](images/label-a.png) **Upload Type** allows selecting between different upload workflows. Currently supported workflows are:

- **From Assets Folder** - packages and uploads a selected folder within the project's Assets folder
- **Pre-exported .unitypackage** - uploads a previously exported .unitypackage file

![label-b](images/label-b.png) **Folder Path** is used to specify the main package content folder

![label-c](images/label-c.png) **Dependencies** toggle is used to mark whether the package content has dependencies on any Unity packages.

Enabling this option includes the current project's [manifest.json](https://docs.unity3d.com/Manual/upm-manifestPrj.html) with the package.

![label-d](images/label-d.png) **Validation** button validates the currently set Folder Path using the [Validator](validator-overview) tool.

![label-e](images/label-e.png) **Special folders** options can be used to include one or more special folders in the package that gets exported and uploaded.

Currently supported special folders:
- Editor Default Resources
- Gizmos
- Plugins
- StreamingAssets
- Standard Assets
- WebGLTemplates

>[!NOTE]
>Special folders are detected dynamically - if no special folders are present in the project, this section of the interface does not appear

![label-f](images/label-f.png) **Upload** button exports the selected paths as a .unitypackage file and starts uploading it to the Publisher Portal

### Pre-exported .unitypackage workflow

![Preexported package workflow](images/uploader-window-preexported-workflow.png)

![label-a](images/label-a.png) **Upload Type** allows selecting between different upload workflows. Currently supported workflows are:

- **From Assets Folder** - packages and uploads a selected folder within the project's Assets folder
- **Pre-exported .unitypackage** - uploads a previously exported .unitypackage file

![label-b](images/label-b.png) **Package Path** is used to specify the path to the pre-exported .unitypackage file that you wish to upload

![label-c](images/label-c.png) **Upload** button starts uploading the specified package file to the Publisher Portal
