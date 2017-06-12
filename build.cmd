@echo off

set NuGet=packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe
set MsBuildConfig=WebAPI.OutputCache.sln  /p:Configuration=Debug /m /nr:false /t:Clean,Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /verbosity:minimal /p:CreateHardLinksForCopyLocalIfPossible=true
set NUnit=packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe

nuget restore
msbuild  %MsBuildConfig%
%NUnit% --noresult test\WebApi.OutputCache.Core.Tests\bin\Debug\WebApi.OutputCache.Core.Tests.dll
%NUnit% --noresult test\WebApi.OutputCache.V2.Tests\bin\Debug\WebApi.OutputCache.V2.Tests.dll

echo.