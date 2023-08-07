[CmdletBinding()]
param
(
    [Parameter(Mandatory = $true)]
    [string]
    $Username,

    [Parameter(Mandatory = $true)]
    [string]
    $PAT
)

dotnet nuget update source github --username "$Username" --password "$PAT"
