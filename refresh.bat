@echo off

REM Echo feedback about deleting the Git index.
ECHO Removing Git index...
del ".git\index" /f

REM Refresh the Git index and add all changes.
ECHO Refreshing Git index...
git update-index --really-refresh

REM Add all changes.
ECHO Staging changes...
git add .