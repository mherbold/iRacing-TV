; -- Example1.iss --
; Demonstrates copying 3 files and creating an icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
AppName=iRacing-TV
AppVersion=1.0
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

Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\RevolutionGothic_ExtraBold.otf"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\RevolutionGothic_ExtraBold_It.otf"; DestDir: "{app}"

Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\current-target.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\flag-caution-new.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\flag-checkered-new.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\flag-green-new.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\leaderboard.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\light-black.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\light-green.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\light-white.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\light-yellow.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\nascar-logo.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\race-status.png"; DestDir: "{app}"
Source: "C:\Users\marvi\OneDrive\Desktop\iRacing-TV\voice-of.png"; DestDir: "{app}"

[Dirs]
Name: "{userappdata}\iRacing-TV"

[Icons]
Name: "{group}\iRacing-TV"; Filename: "{app}\iRacing-TV.exe"
