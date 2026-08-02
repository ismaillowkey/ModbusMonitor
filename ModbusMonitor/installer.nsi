!define APPNAME "Modbus Monitor"
!define APPVERSION "0.1.1"
!define COMPANYNAME "Ismail Lowkey"
!define DESCRIPTION "A Modern WPF Modbus TCP Client"

; The name of the installer
Name "${APPNAME} ${APPVERSION}"
OutFile "setup_modbusMonitor.exe"

; Default installation directory (64-bit Program Files)
InstallDir "$PROGRAMFILES64\${COMPANYNAME}\${APPNAME}"

; Request application privileges for Windows (Require admin rights)
RequestExecutionLevel admin

; Include modern UI
!include "MUI2.nsh"

; Interface settings
!define MUI_ABORTWARNING
!define MUI_ICON "img\logo.ico"
!define MUI_UNICON "img\logo.ico"

; Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; Languages
!insertmacro MUI_LANGUAGE "English"

; Default section (Installation)
Section "Install"
  
  ; Set output path to the installation directory
  SetOutPath "$INSTDIR"
  
  ; Put all files from the publish folder into the installation directory
  File /r "publish\*.*"
  
  ; Create the uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  ; Create Start Menu folder and shortcuts
  CreateDirectory "$SMPROGRAMS\${APPNAME}"
  CreateShortcut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\ModbusMonitor.exe"
  CreateShortcut "$SMPROGRAMS\${APPNAME}\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  
  ; Create Desktop shortcut
  CreateShortcut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\ModbusMonitor.exe"
  
  ; Add uninstall information to Add/Remove Programs (Control Panel)
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$\"$INSTDIR$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$\"$INSTDIR\ModbusMonitor.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${COMPANYNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${APPVERSION}"
  
SectionEnd

; Uninstaller section
Section "Uninstall"

  ; Remove Start Menu folder and shortcuts
  RMDir /r "$SMPROGRAMS\${APPNAME}"
  
  ; Remove Desktop shortcut
  Delete "$DESKTOP\${APPNAME}.lnk"

  ; Remove all files and the installation directory
  RMDir /r "$INSTDIR"

  ; Remove uninstaller registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
  
SectionEnd
