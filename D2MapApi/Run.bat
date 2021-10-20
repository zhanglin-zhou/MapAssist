@echo off
reg query "HKCU\SOFTWARE\Blizzard Entertainment\Diablo II" /v "InstallPath" >nul 2>nul
if %ERRORLEVEL% EQU 1 goto GetD2Path
	:AutoPath
		for /F "tokens=2*" %%a in ('reg query "HKCU\SOFTWARE\Blizzard Entertainment\Diablo II" /v "InstallPath"') do (set InstallPath=%%b)
			if "%InstallPath:~-1%"=="\" set InstallPath=%InstallPath:~0,-1%
				goto Launch
	:GetD2Path
		echo Diablo II 1.13c directory was not detected
			set /P InstallPath="Please enter the location of your Diablo 2 1.13c directory: 
				for %%G in ("%InstallPath%") do if "%%~aG" lss "d" if "%%~aG" GEq "-" (
					goto GetD2Path ) else goto GetD2Path
						reg add "HKCU\SOFTWARE\Blizzard Entertainment\Diablo II" /v "InstallPath" /t REG_SZ /d "%InstallPath%" /f
							goto :Launch
	:Launch
		start cmd /k d2mapapi "%InstallPath%"
			for /f %%G in ('dir /b %~dp0..\*.exe') do (start %~dp0..\%%G)
