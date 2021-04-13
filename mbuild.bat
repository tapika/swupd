@echo off
powershell -executionpolicy bypass -file "%~dpn0.ps1" %*
