[CmdletBinding()]
param(
    [string]$SolutionPath = (Join-Path $PSScriptRoot '..' '..' 'media-vault-app.slnx'),
    [string]$BaselinePath = (Join-Path $PSScriptRoot '..' 'dependency-advisory-baseline.json')
)

$ErrorActionPreference = 'Stop'

function Get-AdvisoryKey {
    param(
        [Parameter(Mandatory)]
        [object]$Entry
    )

    $values = @(
        [string]$Entry.project
        [string]$Entry.framework
        [string]$Entry.kind
        [string]$Entry.package
        [string]$Entry.version
        [string]$Entry.severity
        [string]$Entry.advisory
    )

    return (($values | ForEach-Object { $_.Trim().ToLowerInvariant() }) -join '|')
}

function Assert-AdvisoryFields {
    param(
        [Parameter(Mandatory)]
        [object]$Entry,

        [Parameter(Mandatory)]
        [string]$Source
    )

    foreach ($field in 'project', 'framework', 'kind', 'package', 'version', 'severity', 'advisory') {
        if ([string]::IsNullOrWhiteSpace([string]$Entry.$field)) {
            throw "$Source is missing required field '$field'."
        }
    }
}

$solution = (Resolve-Path -LiteralPath $SolutionPath).Path
$baselineFile = (Resolve-Path -LiteralPath $BaselinePath).Path
$repoRoot = (Split-Path -Parent $solution)
$reportPath = [IO.Path]::GetTempFileName()
$errorPath = [IO.Path]::GetTempFileName()

try {
    $baseline = Get-Content -LiteralPath $baselineFile -Raw | ConvertFrom-Json

    if ([int]$baseline.schemaVersion -ne 1) {
        throw "Unsupported dependency advisory baseline schema version '$($baseline.schemaVersion)'."
    }

    foreach ($field in 'owner', 'reviewAfter', 'reviewTrigger', 'reason') {
        if ([string]::IsNullOrWhiteSpace([string]$baseline.$field)) {
            throw "Dependency advisory baseline is missing required field '$field'."
        }
    }

    $reviewAfter = [DateTimeOffset]::Parse(
        [string]$baseline.reviewAfter,
        [Globalization.CultureInfo]::InvariantCulture
    )
    if ($reviewAfter.Date -lt [DateTimeOffset]::UtcNow.Date) {
        throw "Dependency advisory baseline review date '$($baseline.reviewAfter)' has passed."
    }

    & dotnet list $solution package --include-transitive --vulnerable --format json --output-version 1 --no-restore 1>$reportPath 2>$errorPath
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $errorOutput = Get-Content -LiteralPath $errorPath -Raw
        throw "dotnet list package failed with exit code $exitCode. $errorOutput"
    }

    $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
    $observed = [System.Collections.Generic.List[object]]::new()

    foreach ($project in @($report.projects)) {
        if ($null -eq $project) {
            continue
        }

        $projectPath = [IO.Path]::GetFullPath([string]$project.path)
        $relativeProjectPath = [IO.Path]::GetRelativePath($repoRoot, $projectPath).Replace('\', '/')

        foreach ($framework in @($project.frameworks)) {
            if ($null -eq $framework) {
                continue
            }

            foreach ($kind in 'topLevelPackages', 'transitivePackages') {
                $kindName = if ($kind -eq 'topLevelPackages') { 'top-level' } else { 'transitive' }

                foreach ($package in @($framework.$kind)) {
                    if ($null -eq $package) {
                        continue
                    }

                    foreach ($vulnerability in @($package.vulnerabilities)) {
                        if ($null -eq $vulnerability) {
                            continue
                        }

                        $observed.Add([pscustomobject]@{
                                project = $relativeProjectPath
                                framework = [string]$framework.framework
                                kind = $kindName
                                package = [string]$package.id
                                version = [string]$package.resolvedVersion
                                severity = [string]$vulnerability.severity
                                advisory = [string]$vulnerability.advisoryurl
                            })
                    }
                }
            }
        }
    }

    $baselineEntries = @($baseline.advisories)
    $baselineByKey = @{}
    foreach ($entry in $baselineEntries) {
        if ($null -eq $entry) {
            continue
        }

        Assert-AdvisoryFields -Entry $entry -Source 'Baseline entry'
        $key = Get-AdvisoryKey -Entry $entry
        if ($baselineByKey.ContainsKey($key)) {
            throw "Dependency advisory baseline contains a duplicate entry for '$key'."
        }

        $baselineByKey[$key] = $entry
    }

    $observedByKey = @{}
    foreach ($entry in $observed) {
        Assert-AdvisoryFields -Entry $entry -Source 'dotnet list package output'
        $observedByKey[(Get-AdvisoryKey -Entry $entry)] = $entry
    }

    $newAdvisories = @(
        $observed | Where-Object {
            -not $baselineByKey.ContainsKey((Get-AdvisoryKey -Entry $_))
        }
    )
    $staleBaselineEntries = @(
        $baselineEntries | Where-Object {
            $null -ne $_ -and -not $observedByKey.ContainsKey((Get-AdvisoryKey -Entry $_))
        }
    )

    if ($staleBaselineEntries.Count -gt 0) {
        $staleDetails = $staleBaselineEntries | ForEach-Object {
            "$($_.project): $($_.package) $($_.version) ($($_.advisory))"
        }
        Write-Warning "Baseline entries are no longer reported; remove them after confirming the remediation:`n$($staleDetails -join [Environment]::NewLine)"
    }

    if ($newAdvisories.Count -gt 0) {
        $newDetails = $newAdvisories | ForEach-Object {
            "$($_.project): $($_.package) $($_.version), severity $($_.severity), $($_.advisory)"
        }
        throw "New dependency advisories detected outside the recorded baseline:`n$($newDetails -join [Environment]::NewLine)"
    }

    Write-Output "Dependency advisory baseline passed. Observed $($observed.Count) advisory entries; owner $($baseline.owner), review after $($baseline.reviewAfter)."
}
finally {
    Remove-Item -LiteralPath $reportPath, $errorPath -Force -ErrorAction SilentlyContinue
}
