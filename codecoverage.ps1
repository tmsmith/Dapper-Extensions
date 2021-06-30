echo "Removing existing Coverage data"
Get-ChildItem -Path . -Filter "TestResults" -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path . -Filter "CoverageReports" -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path . -Filter "coveragereport" -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path . -Filter "*cobertura.xml" -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path . -Filter "*coverage" -Recurse | Remove-Item -force -recurse

if ($target -eq "" -OR $target -eq "core21")
{
	echo "Testing for netcoreapp21"
	dotnet test .\DapperExtensions.Test -c debug --collect:"XPlat Code Coverage" -verbosity:diagnostic -bl:msbuild.binlog -noconsolelogger --framework:netcoreapp2.1 --settings:coverlet.runsettings

	reportgenerator "-reports:.\**\coverage.cobertura.xml" "-targetdir:.\CoverageReports\netcore21" -reporttypes:Html
	Invoke-Item .\CoverageReports\netcore21\index.html

	Get-ChildItem -Path . -Filter "*cobertura.xml" -Recurse | Remove-Item -force -recurse
}

if ($target -eq "" -OR $target -eq "core31")
{
	echo "Testing for netcoreapp31"
	dotnet test .\DapperExtensions.Test -c debug --collect:"XPlat Code Coverage" -verbosity:diagnostic -bl:msbuild.binlog -noconsolelogger --framework:netcoreapp3.1 --settings:coverlet.runsettings

	reportgenerator "-reports:.\**\coverage.cobertura.xml" "-targetdir:.\CoverageReports\netcore31" -reporttypes:Html
	Invoke-Item .\CoverageReports\netcore31\index.html

	Get-ChildItem -Path . -Filter "*cobertura.xml" -Recurse | Remove-Item -force -recurse
}

if ($target -eq "" -OR $target -eq "net5")
{
	echo "Testing for net50"
	dotnet test .\DapperExtensions.Test -c debug --collect:"XPlat Code Coverage" -verbosity:diagnostic -bl:msbuild.binlog -noconsolelogger --framework:net5.0 --settings:coverlet.runsettings

	reportgenerator "-reports:.\**\coverage.cobertura.xml" "-targetdir:.\CoverageReports\net50" -reporttypes:Html
	Invoke-Item .\CoverageReports\net50\index.html

	Get-ChildItem -Path . -Filter "*cobertura.xml" -Recurse | Remove-Item -force -recurse
}

if ($target -eq "" -OR $target -eq "netfull")
{
	echo "Testing for net461"
	dotnet test .\DapperExtensions.Test -c debug --collect:"Code Coverage" -verbosity:diagnostic -bl:msbuild.binlog -noconsolelogger --framework:net461 --settings:mscoverage.runsettings

	#Convert to a XML format to be used with reportgenerator
	$filepath="$env:userprofile\.nuget\packages\microsoft.codecoverage\16.9.4\build\netstandard1.0\CodeCoverage\CodeCoverage.exe"
	foreach ($f in Get-ChildItem -Path . -Filter "*.coverage" -Recurse){ 
		$outpath=Split-Path -Path $f.Directory -Resolve -NoQualifier
		$arguments="analyze /output:$outpath\net461.coveragexml $f"
		Start-Process -FilePath $filepath -ArgumentList $arguments -NoNewWindow -Wait -WorkingDirectory:.
	}

	foreach ($f in Get-ChildItem -Path . -Filter "net461.coveragexml" -Recurse){ 
		reportgenerator "-reports:$f" "-targetdir:.\CoverageReports\net461" -reporttypes:Html
	}

	Invoke-Item .\CoverageReports\net461\index.html

	Get-ChildItem -Path . -Filter "net461.coveragexml" -Recurse | Remove-Item -force -recurse
}
