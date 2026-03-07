## Installation
1. Install Melonloader
2. Download the latest release
3. Drag everything inside the "Mods" folder from the .zip into your game's Mods folder
4. Drag everything from the "UserLibs" folder from the .zip into your game's "UserLibs" folder
5. Inside your game's Mods folder, create a new folder called "mszdlg".
 > This folder is created automatically on game startup
6. Drag your .mszdlg file into this folder. 
> You should ony put one dialogue pack. If you put more, the mod will just load the first one it finds. I might add a GUI to load the one you want in the future

## Building

1. Clone this repo
2. Replace the refrences to the game's assemblies with your own
3. Build
4. Put the built dll into your Mods folder
5.  Download bass.dll from https://www.un4seen.com/
> Click the win32 button on the top. Inside bass24.zip you want the dll from the x64 folder.
6. Put base.dll inside Userlibs

### Note
bass.dll is a native C library. The only way to use it in c# is to use dllimport. If you don't want to call its native methods manually, you can use [BASS.NET](https://www.radio42.com/bass/help/html/4b031b64-36a5-4f7c-977c-acb0540e1b45.htm). I chose not to use it since I didn't want to add more dependencies 