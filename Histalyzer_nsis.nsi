# define the name of the installer
Outfile "Histalyzer.exe"
Icon "Histalyzer\Resources\Icon.ico"

InstallDir $TEMP\HistalyzerSetup

AutoCloseWindow true

# default section
Section

HideWindow

SetOutPath $INSTDIR

File /r HistalyzerSetup\Release\*.*
ExecWait "$INSTDIR\setup.exe"

RMDir /r "$INSTDIR"

SectionEnd
