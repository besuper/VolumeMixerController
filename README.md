# Volume Mixer Controller

A program to control Windows volume mixer with WebSocket.


## Download & installation

You can download the lastest version in [releases sections](https://github.com/besuper/VolumeMixerController/releases).

Extract all files in a folder and run `MixerController.exe`

## Usage

You can connect to the WebSocket server as you want. 

You can use those commands: 
|Instruction| Description |
|--|--|
| VOLUME:app_name:volume | Set volume for an app,  ex: VOLUME:Spotify:50 to set master volume of Spotify to 50%. Volume value is between 0-100. |
|MUTE:app_name|Mute a specified app.|
|UNMUTE:app_name|Unmute a specified app.|
|APPS|Show all application available in volume mixer in JSON format. <br>Ex: {"apps":{"Spotify": "1", "Discord": "0,5"}}|

## Settings

You can change listen ip and port as you want in settings.txt. 

Find it at `%appdata%/MixerController` or right-click in system tray app and click on Settings.
