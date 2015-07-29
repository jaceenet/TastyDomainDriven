Param(
    [switch]$BumpMajor, 
    [switch]$BumoMinor, 
    [switch]$BumpPatch, 
    [string]$Pre = "", 
    [string]$File = "VERSION", 
    [string]$Version = "",
    [switch]$Version_Out,
    [switch]$Commit,
    [int]$Patch = -1
)
    
[int]$MajorVersion = 0;
[int]$MinorVersion = 0;
[int]$PatchVersion = 0;
    
if ((Test-Path $File) -and $Version -eq ""){      
    $Version = Get-Content $File
}    

if ($Version -notmatch "[0-9]+(\.([0-9]+|\*)){1,3}")
{
    Write-Host "Version format not supported, please use semver format. Or use -Version 1.0.0 option. The value was '$Version'";
    return "0.0.0";
}
    
$semver = $($Version) -split ("[.-]") | select -First 3
    
try{ $MajorVersion = [int]$semver[0]; } catch { Write-Host "Invalid major" $semver }
try{ $MinorVersion = [int]$semver[1]; } catch { Write-Host "Invalid minor" $semver }
try{ $PatchVersion = [int]$semver[2]; } catch { Write-Host "Invalid patch" $semver }
    
if ($BumpMajor){
    $MajorVersion += 1;    
    $MinorVersion = 0;
    $PatchVersion =0;
}

if ($BumpMinor){
    $MinorVersion += 1;
    $PatchVersion = 0;
}
    
if ($BumpPatch){
    $PatchVersion += 1;    
}

if ($Patch -gt 0){
    $PatchVersion = $Patch
}
    
$file_version = [string]::Format("{0}.{1}.{2}", $MajorVersion, $MinorVersion, $PatchVersion) 
$file_version | Out-File $File       

if ($Version_Out -or $Pre -eq ""){        
    $result = $file_version
}
else
{        
    $result = [string]::Format("{0}.{1}.{2}{3}", $MajorVersion, $MinorVersion, $PatchVersion, "-"+$Pre);
}


if ($Commit)
{        
    Echo Commit is on
    git add $File    
    git commit -m "Version bump $result"
}

return $result