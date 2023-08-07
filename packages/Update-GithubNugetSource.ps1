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

# documentation: https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry#authenticating-to-github-packages
dotnet nuget update source github --username "$Username" --password "$PAT"
