!define APPNAME "Modbus Monitor"
!define APPVERSION "0.8.0"
!define COMPANYNAME "Ismail Lowkey"
!define DESCRIPTION "A Modern WPF Modbus TCP Client"

!macro ChooseTarget ARCH
  !if "${ARCH}" == "x86"
    OutFile "setup_${APPNAME}_v${APPVERSION}_x86.exe"
    InstallDir "$PROGRAMFILES\${COMPANYNAME}\${APPNAME}"
  !else
    OutFile "setup_${APPNAME}_v${APPVERSION}_x64.exe"
    InstallDir "$PROGRAMFILES64\${COMPANYNAME}\${APPNAME}"
  !endif
!macroend

; The name of the installer
Name "${APPNAME} ${APPVERSION}"

; Set target based on /DARCH=x86 or /DARCH=x64
!ifndef ARCH
  !define ARCH "x64"   ; default x64 kalau tidak di-pass
!endif
!insertmacro ChooseTarget "${ARCH}"

; Default installation directory (64-bit Program Files)
; InstallDir is set by the macro ChooseTarget

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

Function .onInit
  ; Check for the very first old version (named differently) and uninstall it
  ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Modbus Reader Poll By Ismail Lowkey" "QuietUninstallString"
  StrCmp $R0 "" check_new
  ClearErrors
  ExecWait '$R0 _?=$PROGRAMFILES64\Modbus Reader Poll By Ismail Lowkey'

check_new:
  ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "QuietUninstallString"
  StrCmp $R0 "" done
  
  ; Run the uninstaller silently and wait for it to finish
  ClearErrors
  ExecWait '$R0 _?=$INSTDIR'
done:
FunctionEnd

; Default section (Installation)
Section "Install"
  
  ; Set output path to the installation directory
  SetOutPath "$INSTDIR"
    ; Put all files from the correct publish folder into the installation directory
    !if "${ARCH}" == "x86"
      File /r "bin\Release\net10.0-windows\win-x86\*.*"
    !else
      File /r "bin\Release\net10.0-windows\win-x64\*.*"
    !endif
  
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
