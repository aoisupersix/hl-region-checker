@echo off
cd /d %~dp0

SET VS=.\.vs\*
SET B1=.\HLRegionChecker\HLRegionChecker\bin\*
SET O1=.\HLRegionChecker\HLRegionChecker\obj\*
SET B2=.\HLRegionChecker\HLRegionChecker.Android\bin\*
SET O2=.\HLRegionChecker\HLRegionChecker.Android\obj\*
SET B3=.\HLRegionChecker\HLRegionChecker.iOS\bin\*
SET O3=.\HLRegionChecker\HLRegionChecker.iOS\obj\*

echo %DATE% %TIME% 処理開始

rem ファイル削除
del /S /Q %VS%
del /S /Q %B1%
del /S /Q %O1%
del /S /Q %B2%
del /S /Q %O2%
del /S /Q %B3%
del /S /Q %O3%

rem ディレクトリ削除
for /D %%1 in (%VS%) do rd /S /Q "%%1"
for /D %%1 in (%B1%) do rd /S /Q "%%1"
for /D %%1 in (%O1%) do rd /S /Q "%%1"
for /D %%1 in (%B2%) do rd /S /Q "%%1"
for /D %%1 in (%O2%) do rd /S /Q "%%1"
for /D %%1 in (%B3%) do rd /S /Q "%%1"
for /D %%1 in (%O3%) do rd /S /Q "%%1"

echo %DATE% %TIME% 処理終了

set /P USR_INPUT_STR="処理が終了しました。 "
exit /b