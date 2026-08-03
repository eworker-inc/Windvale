<#
.SYNOPSIS
Synchronizes the ten standard Windvale development lanes.

.EXAMPLE
Sync-Development-Lanes.ps1
Fetches and fast-forwards clean dev01 through dev10 lanes on main.

.EXAMPLE
Sync-Development-Lanes.ps1 -CreateMissing
Clones missing lanes from dev01's origin, then synchronizes all ten lanes.

.EXAMPLE
Sync-Development-Lanes.ps1 -StatusOnly
Refreshes remote refs and reports lane state without changing a checkout.

.EXAMPLE
Sync-Development-Lanes.ps1 -Lane 2,7..10 -SwitchToMain -Push
Switches clean selected lanes to main, fast-forwards them, and pushes a clean
main lane when it is ahead of origin/main.
#>
[CmdletBinding()]
param(
  [object[]]$Lane = (1..10),
  [string]$Root = (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))),
  [string]$Remote = 'origin',
  [string]$MainBranch = 'main',
  [string]$CloneUrl = '',
  [switch]$CreateMissing,
  [switch]$SwitchToMain,
  [switch]$StatusOnly,
  [switch]$Push,
  [switch]$NoFetch,
  [switch]$Json,
  [switch]$NoColor
)

$ErrorActionPreference = 'Stop'
$VALID_LANES = 1..10

function Invoke-Git {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][string[]]$Arguments,
    [switch]$AllowFailure
  )

  $Output = @(& git -C $Path @Arguments 2>&1)
  $ExitCode = $LASTEXITCODE
  if ($ExitCode -ne 0 -and -not $AllowFailure) {
    throw "git -C $Path $($Arguments -join ' ') failed with exit code $ExitCode.`n$($Output -join "`n")"
  }

  [pscustomobject]@{
    ExitCode = $ExitCode
    Lines = $Output
    Text = ($Output -join "`n").Trim()
  }
}

function Resolve-Lanes {
  param([object[]]$Values)

  $Resolved = New-Object System.Collections.Generic.List[int]
  foreach ($Value in $Values) {
    if ($null -eq $Value) { continue }
    foreach ($Token in ($Value.ToString().Trim() -split '[,\s]+')) {
      if ($Token.Length -eq 0) { continue }
      if ($Token -match '^(?<Start>\d+)\.\.(?<End>\d+)$') {
        $Start = [int]$Matches.Start
        $End = [int]$Matches.End
        $Step = if ($Start -le $End) { 1 } else { -1 }
        for ($LaneNumber = $Start; $LaneNumber -ne ($End + $Step); $LaneNumber += $Step) {
          $Resolved.Add($LaneNumber)
        }
      } elseif ($Token -match '^dev(?<Number>\d{1,2})$') {
        $Resolved.Add([int]$Matches.Number)
      } elseif ($Token -match '^\d+$') {
        $Resolved.Add([int]$Token)
      } else {
        throw "Invalid lane '$Token'. Use values such as 1, dev01, 1,2, or 7..10."
      }
    }
  }

  $Unique = @($Resolved | Sort-Object -Unique)
  if ($Unique.Count -eq 0) { throw 'At least one development lane must be selected.' }
  $Invalid = @($Unique | Where-Object { $_ -notin $VALID_LANES })
  if ($Invalid.Count -gt 0) { throw "Windvale lanes must be between 1 and 10. Invalid: $($Invalid -join ', ')." }
  $Unique
}

function Read-Lane-Path {
  param([string]$RootPath, [int]$LaneNumber)
  Join-Path $RootPath ('dev{0:d2}' -f $LaneNumber)
}

function Test-Git-Repository {
  param([string]$Path)
  if (-not (Test-Path -LiteralPath $Path -PathType Container)) { return $false }
  $Result = Invoke-Git -Path $Path -Arguments @('rev-parse', '--is-inside-work-tree') -AllowFailure
  $Result.ExitCode -eq 0 -and $Result.Text -eq 'true'
}

function Read-Lane-State {
  param([string]$Path)

  $Branch = (Invoke-Git -Path $Path -Arguments @('branch', '--show-current')).Text
  $Head = (Invoke-Git -Path $Path -Arguments @('rev-parse', '--short=9', 'HEAD')).Text
  $RemoteResult = Invoke-Git -Path $Path -Arguments @('rev-parse', '--short=9', "$Remote/$MainBranch") -AllowFailure
  $RemoteHead = if ($RemoteResult.ExitCode -eq 0) { $RemoteResult.Text } else { '' }
  $CountsResult = Invoke-Git -Path $Path -Arguments @('rev-list', '--left-right', '--count', "HEAD...$Remote/$MainBranch") -AllowFailure
  $Ahead = 0
  $Behind = 0
  if ($CountsResult.ExitCode -eq 0 -and $CountsResult.Text -match '^(?<Ahead>\d+)\s+(?<Behind>\d+)$') {
    $Ahead = [int]$Matches.Ahead
    $Behind = [int]$Matches.Behind
  }
  $Status = @((Invoke-Git -Path $Path -Arguments @('status', '--short')).Lines | Where-Object {
    $null -ne $_ -and $_.ToString().Trim().Length -gt 0
  })

  [pscustomobject]@{
    Branch = $Branch
    Head = $Head
    RemoteHead = $RemoteHead
    Ahead = $Ahead
    Behind = $Behind
    TrackedDirty = @($Status | Where-Object { $_ -notmatch '^\?\? ' }).Count
    Untracked = @($Status | Where-Object { $_ -match '^\?\? ' }).Count
  }
}

function Read-Clone-Url {
  param([string]$RootPath)
  if ($CloneUrl.Trim().Length -gt 0) { return $CloneUrl.Trim() }
  $Dev01 = Read-Lane-Path -RootPath $RootPath -LaneNumber 1
  if (-not (Test-Git-Repository -Path $Dev01)) {
    throw "Cannot discover a clone URL because $Dev01 is unavailable. Supply -CloneUrl."
  }
  (Invoke-Git -Path $Dev01 -Arguments @('remote', 'get-url', $Remote)).Text
}

function New-Result {
  param(
    [int]$LaneNumber,
    [string]$Path,
    [string]$State,
    [string]$Action = '',
    [string]$Note = '',
    [object]$GitState = $null
  )

  [pscustomobject]@{
    Lane = ('dev{0:d2}' -f $LaneNumber)
    State = $State
    Branch = if ($null -eq $GitState) { '' } else { $GitState.Branch }
    Head = if ($null -eq $GitState) { '' } else { $GitState.Head }
    Remote = if ($null -eq $GitState) { '' } else { $GitState.RemoteHead }
    Ahead = if ($null -eq $GitState) { 0 } else { $GitState.Ahead }
    Behind = if ($null -eq $GitState) { 0 } else { $GitState.Behind }
    Dirty = if ($null -eq $GitState) { '0/0' } else { "$($GitState.TrackedDirty)/$($GitState.Untracked)" }
    Action = $Action
    Note = $Note.Trim()
    Path = $Path
  }
}

function Write-Result {
  param([object[]]$Rows)
  if ($Json) {
    $Rows | ConvertTo-Json -Depth 3 -AsArray
    return
  }

  $Rows | Select-Object Lane, State, Branch, Head, Remote, Ahead, Behind, Dirty, Action | Format-Table -AutoSize
  $Notes = @($Rows | Where-Object { $_.Note.Length -gt 0 })
  if ($Notes.Count -gt 0) {
    Write-Host 'Notes'
    foreach ($Row in $Notes) {
      if ($NoColor) { Write-Host "  $($Row.Lane): $($Row.Note)" }
      else { Write-Host "  $($Row.Lane): $($Row.Note)" -ForegroundColor Yellow }
    }
  }
}

$Root = [System.IO.Path]::GetFullPath($Root)
$SelectedLanes = Resolve-Lanes -Values $Lane
$Results = New-Object System.Collections.Generic.List[object]
$Failures = New-Object System.Collections.Generic.List[string]
$ResolvedCloneUrl = if ($CreateMissing) { Read-Clone-Url -RootPath $Root } else { '' }

for ($Index = 0; $Index -lt $SelectedLanes.Count; $Index++) {
  $LaneNumber = $SelectedLanes[$Index]
  $Path = Read-Lane-Path -RootPath $Root -LaneNumber $LaneNumber
  $Actions = New-Object System.Collections.Generic.List[string]
  $Notes = New-Object System.Collections.Generic.List[string]
  if (-not $Json) {
    Write-Progress -Activity 'Windvale lane sync' -Status ('dev{0:d2}' -f $LaneNumber) -PercentComplete ([int](100 * $Index / $SelectedLanes.Count))
  }

  try {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
      if (-not $CreateMissing) {
        $Results.Add((New-Result -LaneNumber $LaneNumber -Path $Path -State 'missing' -Action 'none' -Note 'Use -CreateMissing to clone this lane.'))
        continue
      }
      $CloneOutput = @(& git clone --branch $MainBranch $ResolvedCloneUrl $Path 2>&1)
      if ($LASTEXITCODE -ne 0) { throw "git clone failed.`n$($CloneOutput -join "`n")" }
      $Actions.Add('created')
    }

    if (-not (Test-Git-Repository -Path $Path)) {
      $Results.Add((New-Result -LaneNumber $LaneNumber -Path $Path -State 'not-git' -Action ($Actions -join ', ') -Note 'The lane path exists but is not a Git repository.'))
      continue
    }

    if (-not $NoFetch) {
      Invoke-Git -Path $Path -Arguments @('fetch', '--prune', $Remote) | Out-Null
      $Actions.Add('fetched')
    }

    $State = Read-Lane-State -Path $Path
    if (-not $StatusOnly) {
      if ($State.Branch -ne $MainBranch -and $SwitchToMain) {
        if ($State.TrackedDirty -gt 0 -or $State.Untracked -gt 0) {
          $Notes.Add("Cannot switch to $MainBranch while working-tree changes are present.")
        } else {
          Invoke-Git -Path $Path -Arguments @('switch', $MainBranch) | Out-Null
          $Actions.Add('switched')
          $State = Read-Lane-State -Path $Path
        }
      }

      if ($State.Branch -eq $MainBranch) {
        if ($State.TrackedDirty -gt 0 -or $State.Untracked -gt 0) {
          $Notes.Add('Pull skipped because working-tree changes are present.')
        } else {
          $Pull = Invoke-Git -Path $Path -Arguments @('pull', '--ff-only', $Remote, $MainBranch) -AllowFailure
          if ($Pull.ExitCode -ne 0) {
            throw "Fast-forward pull failed.`n$($Pull.Text)"
          }
          $Actions.Add('pulled')
          $State = Read-Lane-State -Path $Path
        }

        if ($Push -and $State.Ahead -gt 0) {
          if ($State.Behind -gt 0 -or $State.TrackedDirty -gt 0 -or $State.Untracked -gt 0) {
            $Notes.Add('Push skipped because the lane is behind, diverged, or dirty.')
          } else {
            Invoke-Git -Path $Path -Arguments @('push', $Remote, "HEAD:$MainBranch") | Out-Null
            $Actions.Add('pushed')
            if (-not $NoFetch) { Invoke-Git -Path $Path -Arguments @('fetch', '--prune', $Remote) | Out-Null }
            $State = Read-Lane-State -Path $Path
          }
        }
      } else {
        $Notes.Add("Lane is on '$($State.Branch)'; use -SwitchToMain to switch a clean lane.")
      }
    }

    $State = Read-Lane-State -Path $Path
    $LaneState = if ($State.Branch -ne $MainBranch -or $State.Ahead -gt 0 -or $State.Behind -gt 0) {
      'attention'
    } elseif ($State.TrackedDirty -gt 0 -or $State.Untracked -gt 0) {
      'dirty'
    } else {
      'aligned'
    }
    $Results.Add((New-Result -LaneNumber $LaneNumber -Path $Path -State $LaneState -Action ($Actions -join ', ') -Note ($Notes -join ' ') -GitState $State))
  } catch {
    $Failures.Add(('dev{0:d2}: {1}' -f $LaneNumber, $_.Exception.Message))
    $Results.Add((New-Result -LaneNumber $LaneNumber -Path $Path -State 'error' -Action ($Actions -join ', ') -Note $_.Exception.Message))
  }
}

if (-not $Json) { Write-Progress -Activity 'Windvale lane sync' -Completed }
Write-Result -Rows $Results.ToArray()
if ($Failures.Count -gt 0) {
  Write-Error ($Failures -join "`n")
  exit 1
}
