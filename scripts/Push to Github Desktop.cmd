set Git=%userprofile%\Documents\GitHub\Git\Angry_Dash
set Unity=%userprofile%\Documents\Unity\Projets\Angry Dash
set Installer=%userprofile%\Documents\Unity\Compiller\Angry Dash\Windows

IF EXIST "C:\Users\evan\Documents\GitHub\Git\Angry_Dash\docs\commands.md" (
echo /!\
echo The bransh isn't master
pause
) ELSE (
xcopy "%Unity%\Assets" "%Git%\Assets" /e /Q /Y /i
xcopy "%Unity%\ProjectSettings" "%Git%\ProjectSettings" /e /Q /Y /i
xcopy "%Unity%\Packages" "%Git%\Packages" /e /Q /Y /i
xcopy "%Unity%\Library\PackageCache" "%Git%\Library\PackageCache" /e /Q /Y /i /exclude:AutoGitExcluded.txt
echo F| xcopy "%Unity%\Angry Dash.keystore" "%Git%\Angry Dash.keystore" /Q /Y
echo F| xcopy "%Unity%\Auto Git.cmd" "%Git%\Scripts\Push to Github Desktop.cmd" /Q /Y
echo F| xcopy "%Unity%\AutoGitExcluded.txt" "%Git%\Scripts\AutoGitExcluded.txt" /Q /Y
echo F| xcopy "%Unity%\Auto RP.bat" "%Git%\Scripts\Update RPs.cmd" /Q /Y
echo F| xcopy "%Installer%\setup - stable.iss" "%Git%\Installer\setup - stable.iss" /Q /Y
echo F| xcopy "%Installer%\setup - pre.iss" "%Git%\Installer\setup - pre.iss" /Q /Y
echo F| xcopy "%Installer%\angry dash.ico" "%Git%\Installer\angry dash.ico" /Q /Y
)