@echo off
set DOTNET_NOLOGO=1
dotnet run --project cakebuild\cakebuild.csproj -- %*
exit /b
