#define AppName "KebuzForge"
#define AppVersion "1.0.0"
#define AppPublisher "Kebuz"
#define AppExeName "KebuzForge.exe"

[Setup]
AppId={{B7E9D2C4-3F61-4A8E-9C5D-1E0A7B6F4D28}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppPublisher}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\..\release
OutputBaseFilename=KebuzForge-Setup-{#AppVersion}-win-x64
SetupIconFile=..\KebuzForge.App\kebuforg.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#AppExeName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: "main"; Description: "KebuzForge"; Types: full compact custom; Flags: fixed
Name: "faviconplugin"; Description: "Плагин Favicon Generator"; Types: full

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\..\release\KebuzForge-win-x64-lite.exe"; DestDir: "{app}"; DestName: "{#AppExeName}"; Components: main; Flags: ignoreversion
Source: "..\FaviconForgePlugin\bin\Release\net8.0-windows\FaviconForgePlugin.dll"; DestDir: "{app}\Plugins"; Components: faviconplugin; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\windowsdesktop-runtime-win-x64.exe"; Parameters: "/install /quiet /norestart"; StatusMsg: "Установка .NET 8 Desktop Runtime..."; Check: DotNetMissing; Flags: skipifdoesntexist
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DownloadPage: TDownloadWizardPage;

function DotNetMissing: Boolean;
var
  FindRec: TFindRec;
begin
  Result := True;
  if FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*'), FindRec) then
  begin
    Result := False;
    FindClose(FindRec);
  end;
end;

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if (CurPageID = wpReady) and DotNetMissing then
  begin
    DownloadPage.Clear;
    DownloadPage.Add('https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe', 'windowsdesktop-runtime-win-x64.exe', '');
    DownloadPage.Show;
    try
      try
        DownloadPage.Download;
      except
        if not DownloadPage.AbortedByUser then
          SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbError, MB_OK, IDOK);
      end;
    finally
      DownloadPage.Hide;
    end;
  end;
end;
