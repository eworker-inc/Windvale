[CmdletBinding()]
param([Parameter(Mandatory)][object]$NativePlan)

$ErrorActionPreference = 'Stop'
$MaximumJobMinutes = 180L
$ControlSeconds = 300L
if ($NativePlan.NativeSharedCompilerHostRecordRequired -isnot [bool]) {
    throw 'Development sharding requires an explicit shared-host admission selection.'
}
$AdmissionSeconds = if ($NativePlan.NativeSharedCompilerHostRecordRequired) { 600L } else { 0L }
$OwnerLimit = $MaximumJobMinutes * 60 - $ControlSeconds - $AdmissionSeconds
$Owners = @($NativePlan.OwnerBudgets)
if ($Owners.Count -gt 512 -or $Owners.Count -ne @($NativePlan.Suites).Count -or
    @($NativePlan.Gaps).Count -ne 0) { throw 'Development shard owner inventory is invalid or incomplete.' }
$Seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$Groups = [Collections.Generic.List[object]]::new()
$GroupByName = @{}
for ($Index = 0; $Index -lt $Owners.Count; $Index++) {
    $Owner = $Owners[$Index]
    if ($Owner.Name -cne $NativePlan.Suites[$Index] -or !$Seen.Add($Owner.Name) -or
        $Owner.Name -cnotmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$' -or
        $Owner.Group -cnotmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
        throw 'Development shard owner order, identity or group is invalid.'
    }
    foreach ($Field in @('ExpectedSeconds', 'MaximumSeconds')) {
        if (($Owner.$Field -isnot [long] -and $Owner.$Field -isnot [int]) -or
            $Owner.$Field -lt 0 -or $Owner.$Field -gt 10800) {
            throw 'Development shard duration must be a bounded integer.'
        }
    }
    if ($Owner.ExpectedSeconds -gt $Owner.MaximumSeconds) { throw 'Development shard duration is inconsistent.' }
    if (!$GroupByName.ContainsKey($Owner.Group)) {
        $Group = [pscustomobject]@{ Owners = [Collections.Generic.List[string]]::new(); Expected = 0L; Maximum = 0L }
        $GroupByName[$Owner.Group] = $Group
        $Groups.Add($Group)
    }
    $Group = $GroupByName[$Owner.Group]
    $Group.Owners.Add($Owner.Name)
    $Group.Expected += $Owner.ExpectedSeconds
    $Group.Maximum += $Owner.MaximumSeconds
}
$Expected = [long](($Owners | Measure-Object ExpectedSeconds -Sum).Sum)
$Maximum = [long](($Owners | Measure-Object MaximumSeconds -Sum).Sum)
if ($Expected -ne $NativePlan.ExpectedSeconds -or $Maximum -ne $NativePlan.MaximumSeconds) {
    throw 'Development shard durations do not cover the complete selected plan.'
}
$Shards = [Collections.Generic.List[object]]::new()
foreach ($Group in $Groups) {
    if ($Group.Maximum -le 0 -or $Group.Maximum -gt $OwnerLimit) {
        throw 'One inseparable native owner group exceeds the finite 180-minute CI cap.'
    }
    # Stable first fit preserves owner order and keeps shared executions together.
    $Shard = @($Shards | Where-Object { $_.MaximumSeconds + $Group.Maximum -le $OwnerLimit } | Select-Object -First 1)
    if ($Shard.Count -eq 0) {
        if ($Shards.Count -ge 16) { throw 'Development plan exceeds the sixteen-shard limit.' }
        $Current = [pscustomobject]@{ Index = $Shards.Count + 1; Count = 0; Suites = [Collections.Generic.List[string]]::new()
            ExpectedSeconds = 0L; MaximumSeconds = 0L; AdmissionSeconds = $AdmissionSeconds
            ControlSeconds = $ControlSeconds; TimeoutMinutes = 15L }
        $Shards.Add($Current)
    } else { $Current = $Shard[0] }
    foreach ($Name in $Group.Owners) { $Current.Suites.Add($Name) }
    $Current.ExpectedSeconds += $Group.Expected
    $Current.MaximumSeconds += $Group.Maximum
}
if ($Shards.Count -eq 0) {
    $Shards.Add([pscustomobject]@{ Index = 1; Count = 1; Suites = @(); ExpectedSeconds = 0L; MaximumSeconds = 0L
        AdmissionSeconds = 0L; ControlSeconds = $ControlSeconds; TimeoutMinutes = 15L })
}
foreach ($Shard in $Shards) {
    $Shard.Count = $Shards.Count
    $Shard.Suites = @($NativePlan.Suites | Where-Object { $Shard.Suites -ccontains $_ })
    $Shard.TimeoutMinutes = [long][Math]::Max(15, [Math]::Ceiling(
        ($Shard.MaximumSeconds + $Shard.AdmissionSeconds + $Shard.ControlSeconds) / 60.0))
    if ($Shard.TimeoutMinutes -gt $MaximumJobMinutes) { throw 'Development shard exceeds its job deadline.' }
    $Shard
}
