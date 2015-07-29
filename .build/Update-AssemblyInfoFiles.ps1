Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string] $Version
)

#-------------------------------------------------------------------------------
# Update version numbers of AssemblyInfo.cs
#-------------------------------------------------------------------------------
$assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
$fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
$assemblyVersion = 'AssemblyVersion("' + $Version + '")';
$fileVersion = 'AssemblyFileVersion("' + $Version + '")';
    
Get-ChildItem -r -filter AssemblyInfo.cs | ForEach-Object {
    $filename = $_.Directory.ToString() + '\' + $_.Name
    $filename + ' -> ' + $Version
        
    # If you are using a source control that requires to check-out files before 
    # modifying them, make sure to check-out the file here.
    # For example, TFS will require the following command:
    # tf checkout $filename
    
    (Get-Content $filename) | ForEach-Object {
        % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
        % {$_ -replace $fileVersionPattern, $fileVersion }
    } | Set-Content $filename -Encoding UTF8
}
