; Inno Setup script for QuickCopyTags.
; Built by package/build-windows.ps1, which passes -DAppVersion and -DPublishDir.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\QuickCopyTags\bin\Release\net10.0\win-x64\publish"
#endif

[Setup]
AppId={{6C8E9E2A-6E5C-4C8E-9F3E-2F5D8C5A6B1D}
AppName=QuickCopyTags
AppVersion={#AppVersion}
AppPublisher=QuickCopyTags
DefaultDirName={autopf}\QuickCopyTags
DefaultGroupName=QuickCopyTags
UninstallDisplayIcon={app}\QuickCopyTags.exe
SetupIconFile=..\QuickCopyTags\Assets\icon.ico
OutputDir=..\dist
OutputBaseFilename=quickcopytags_{#AppVersion}_win-x64_setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{group}\QuickCopyTags"; Filename: "{app}\QuickCopyTags.exe"
Name: "{group}\Uninstall QuickCopyTags"; Filename: "{uninstallexe}"
Name: "{autodesktop}\QuickCopyTags"; Filename: "{app}\QuickCopyTags.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\QuickCopyTags.exe"; Description: "Launch QuickCopyTags"; Flags: nowait postinstall skipifsilent
