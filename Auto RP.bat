set Git=%userprofile%\Documents\GitHub\Angry-Dash
set AD=%APPDATA%\..\LocalLow\06Games\Angry Dash\Ressources

rmdir /Q /S "%AD%\default"
xcopy "%Git%\default" "%AD%\default" /e /Q /Y /i
rmdir /Q /S "%AD%\light"
xcopy "%Git%\light" "%AD%\light" /e /Q /Y /i
rmdir /Q /S "%AD%\oled"
xcopy "%Git%\oled" "%AD%\oled" /e /Q /Y /i