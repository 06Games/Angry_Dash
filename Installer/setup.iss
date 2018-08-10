#define MyAppName "Angry Dash"
#define MyAppVersion "0.2"
#define MyAppPublisher "06Games"
#define MyAppURL "https://06games.ddns.net/"
#define MyAppExeName "Angry Dash.exe"
#define MyDateTimeString GetDateTimeString('yyyy/mm/dd', '.', '');

[Setup]
AppId={{9E04A9A3-FBB9-41B7-84DE-202772E87201}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\Games\06Games\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=C:\Users\evan\Documents\Unity\Compiller\Angry Dash\Windows\Windows_Angry Dash pre{#MyDateTimeString}
OutputBaseFilename=Windows_Angry_Dash_{#MyAppVersion}-{#MyDateTimeString}_installer
SetupIconFile=C:\Users\evan\Documents\Unity\Compiller\Angry Dash\Windows\angry dash.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "C:\Users\evan\Documents\Unity\Compiller\Angry Dash\Windows\Windows_Angry Dash pre{#MyDateTimeString}\Angry Dash.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\evan\Documents\Unity\Compiller\Angry Dash\Windows\Windows_Angry Dash pre{#MyDateTimeString}\*"; Excludes: "Angry Dash_Data\StreamingAssets\FFmpegOut~\OSX,Angry Dash_Data\StreamingAssets\FFmpegOut~\Linux"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent