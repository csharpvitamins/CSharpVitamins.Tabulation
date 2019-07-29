cd ..\CSharpVitamins.Tabulation\

$packaged = $false

Write-Host 'Packaging' -ForegroundColor Cyan
Write-Host 'Please enter package version...'

$semver = Read-Host "SemVer"
if ($semver) {

    Write-Host "> pack '$semver'..."

    & nuget.exe pack -version $semver -prop Configuration=Release -verbosity detailed -symbol -outputdirectory ..\build\

    if ($?) {
        $packaged = $true;
        Write-Host "> packed okay" -ForegroundColor Green
    } else {
        Write-Host "> pack failed" -ForegroundColor Red
    }

}

if ($packaged) {

    Write-Host ""
    Write-Host "Push?" -ForegroundColor Cyan

    $message  = "Push?"
    $question = "Do you want to push '$semver' to nuget?"

    $choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
    $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Push'))
    $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Cancel'))

    $decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
    if ($decision -eq 0) {

        Write-Host "> pushing '$semver' to nuget..."

        & nuget.exe push ..\build\CSharpVitamins.Tabulation.$semver.*.nupkg

        if ($?) {
            Write-Host "> pushed okay" -ForegroundColor Green
        } else {
            Write-Host "> push failed" -ForegroundColor Red
        }

    } else {
        Write-Host "> skipping"
    }

}

Write-Host "finito!"
