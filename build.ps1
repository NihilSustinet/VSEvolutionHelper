# Build script for VSEvolutionHelper mod
param(
    [switch]$Deploy = $true
)

$ErrorActionPreference = "Stop"

Write-Host "Building VSEvolutionHelper..." -ForegroundColor Cyan

$gamePluginsDir = "D:\SteamLibrary\steamapps\common\Vampire Survivors\BepInEx\plugins"
$gameBepInExDir = "D:\SteamLibrary\steamapps\common\Vampire Survivors\BepInEx"

$shimPath = "$PSScriptRoot\NullableShim.cs"
$shimContent = @"
namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Event | System.AttributeTargets.Parameter | System.AttributeTargets.ReturnValue | System.AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
    internal sealed class NullableAttribute : System.Attribute
    {
        public readonly byte[] NullableFlags;
        public NullableAttribute(byte flag) { NullableFlags = new byte[] { flag }; }
        public NullableAttribute(byte[] flags) { NullableFlags = flags; }
    }
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Method | System.AttributeTargets.Interface | System.AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    internal sealed class NullableContextAttribute : System.Attribute
    {
        public readonly byte Flag;
        public NullableContextAttribute(byte flag) { Flag = flag; }
    }
}
"@

Set-Content -Path $shimPath -Value $shimContent

try {
    $managedDlls = Get-ChildItem "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.36\*.dll" | Where-Object {
        try { [System.Reflection.AssemblyName]::GetAssemblyName($_.FullName) | Out-Null; $true } catch { $false }
    } | ForEach-Object { "/r:`"$($_.FullName)`"" }

    $modRefs = @(
        "/r:`"$gameBepInExDir\core\0Harmony.dll`"",
        "/r:`"$gameBepInExDir\core\BepInEx.Core.dll`"",
        "/r:`"$gameBepInExDir\core\BepInEx.Unity.IL2CPP.dll`"",
        "/r:`"$gameBepInExDir\core\Il2CppInterop.Runtime.dll`"",
        "/r:`"$gameBepInExDir\interop\Il2Cppmscorlib.dll`"",
        "/r:`"$gameBepInExDir\interop\Assembly-CSharp.dll`"",
        "/r:`"$gameBepInExDir\interop\VampireSurvivors.Runtime.dll`"",
        "/r:`"$gameBepInExDir\interop\Unity.TextMeshPro.dll`"",
        "/r:`"$gameBepInExDir\interop\UnityEngine.CoreModule.dll`"",
        "/r:`"$gameBepInExDir\interop\UnityEngine.InputLegacyModule.dll`"",
        "/r:`"$gameBepInExDir\interop\UnityEngine.UI.dll`"",
        "/r:`"$gameBepInExDir\interop\UnityEngine.TextRenderingModule.dll`"",
        "/r:`"$gameBepInExDir\interop\UnityEngine.UIModule.dll`"",
        "/r:`"$gameBepInExDir\interop\PauseSystem.dll`""
    )

    $outPath = "$PSScriptRoot\VSEvolutionHelper.dll"
    $cscPath = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe"

    & $cscPath /target:library /nostdlib+ /out:$outPath /unsafe+ ($managedDlls + $modRefs) "$PSScriptRoot\ItemTooltips.cs"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build succeeded: $outPath" -ForegroundColor Green

        # Also copy to dist/
        $distDir = "$PSScriptRoot\dist"
        if (-not (Test-Path $distDir)) { New-Item -ItemType Directory -Path $distDir | Out-Null }
        Copy-Item -Path $outPath -Destination "$distDir\VSEvolutionHelper.dll" -Force

        if ($Deploy -and (Test-Path $gamePluginsDir)) {
            Copy-Item -Path $outPath -Destination "$gamePluginsDir\VSEvolutionHelper.dll" -Force
            Write-Host "Deployed to $gamePluginsDir\VSEvolutionHelper.dll" -ForegroundColor Green
        }
    } else {
        Write-Error "Build failed with exit code $LASTEXITCODE"
    }
} finally {
    Remove-Item -Path $shimPath -ErrorAction SilentlyContinue
}
