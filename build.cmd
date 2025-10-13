@echo off
setlocal

echo Building TaskApiService...

:: Restore NuGet packages
echo Restoring packages...
dotnet restore
if %ERRORLEVEL% neq 0 goto BuildFail

:: Build the solution
echo Building solution...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% neq 0 goto BuildFail

:: Run tests
echo Running tests...
dotnet test --configuration Release --no-build --verbosity normal
if %ERRORLEVEL% neq 0 goto BuildFail

:: Pack NuGet packages (if applicable)
echo Creating packages...
dotnet pack --configuration Release --no-build --output ./artifacts
if %ERRORLEVEL% neq 0 goto BuildFail

echo Build completed successfully!
goto BuildSuccess

:BuildFail
echo Build failed!
exit /b 1

:BuildSuccess
exit /b 0
