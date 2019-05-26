set Git=%userprofile%\Documents\GitHub\Angry-Dash
set AD=%APPDATA%\..\LocalLow\06Games\Angry Dash\Ressources

echo O | del "%AD%\default"
xcopy "%Git%\default" "%AD%\default" /e /Q /Y /i
echo O | del "%AD%\light"
xcopy "%Git%\light" "%AD%\light" /e /Q /Y /i
echo O | del "%AD%\oled"
xcopy "%Git%\oled" "%AD%\oled" /e /Q /Y /i