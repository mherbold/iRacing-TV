; -- Example1.iss --
; Demonstrates copying 3 files and creating an icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
AppName=iRacing-TV
AppVersion=1.7
AppCopyright=Created by Marvin Herbold
AppPublisher=Marvin Herbold
AppPublisherURL=http://herboldracing.com/blog/iracing/iracing-tv/
WizardStyle=modern
DefaultDirName={autopf}\iRacing-TV
DefaultGroupName=iRacing-TV
UninstallDisplayIcon={app}\iRacing-TV.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=iRacing-TV-Setup
OutputDir=userdocs:iRacing-TV
PrivilegesRequired=lowest

[Files]
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\iRacing-TV.exe"; DestDir: "{app}"

Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\cimgui.dll"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\D3DCompiler_47_cor3.dll"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\PenImc_cor3.dll"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\PresentationNative_cor3.dll"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\SDL2.dll"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\vcruntime140_cor3.dll"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\wpfgfx_cor3.dll"; DestDir: "{app}"

Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\RevolutionGothic_ExtraBold.otf"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\RevolutionGothic_ExtraBold_It.otf"; DestDir: "{app}\Assets"

Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\current-target.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\flag-caution-new.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\flag-checkered-new.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\flag-green-new.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\leaderboard.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\light-black.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\light-green.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\light-white.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\light-yellow.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\nascar-logo.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\race-status.png"; DestDir: "{app}\Assets"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\Assets\voice-of.png"; DestDir: "{app}\Assets"

[Dirs]
Name: "{userappdata}\iRacing-TV"

[Icons]
Name: "{group}\iRacing-TV"; Filename: "{app}\iRacing-TV.exe"
