# Verifica se os scripts do jogo compilam, SEM precisar abrir o Unity.
# Uso:  powershell -ExecutionPolicy Bypass -File verificar-compilacao.ps1
# Sai com código 0 = sem erros de compilacao.

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $MyInvocation.MyCommand.Path

# 1) Achar a instalacao do Unity usada pelo projeto (Unity Hub)
$versao = ''
$projSettings = Join-Path $raiz 'ProjectSettings\ProjectVersion.txt'
if (Test-Path $projSettings) {
    $linha = Select-String -Path $projSettings -Pattern 'm_EditorVersion:\s*(\S+)' | Select-Object -First 1
    if ($linha) { $versao = $linha.Matches[0].Groups[1].Value }
}
$raizEditor = 'C:\Program Files\Unity\Hub\Editor'
$ed = $null
if ($versao -and (Test-Path (Join-Path $raizEditor $versao))) { $ed = Join-Path $raizEditor $versao }
elseif (Test-Path $raizEditor) { $ed = (Get-ChildItem $raizEditor -Directory | Select-Object -First 1).FullName }
if (-not $ed) { Write-Host 'ERRO: Unity nao encontrado em C:\Program Files\Unity\Hub\Editor' -ForegroundColor Red; exit 2 }

$mono     = Join-Path $ed 'Editor\Data\MonoBleedingEdge\bin\mono.exe'
$csc      = Join-Path $ed 'Editor\Data\MonoBleedingEdge\lib\mono\msbuild\Current\bin\Roslyn\csc.exe'
$api      = Join-Path $ed 'Editor\Data\MonoBleedingEdge\lib\mono\4.7.1-api'
$managed  = Join-Path $ed 'Editor\Data\Managed\UnityEngine'
$scriptAs = Join-Path $raiz 'Library\ScriptAssemblies'

if (-not (Test-Path $csc)) { Write-Host "ERRO: csc nao encontrado: $csc" -ForegroundColor Red; exit 2 }

# 2) Referencias (mesma base do Unity: .NET 4.x + facades + dlls do Unity + pacotes)
$refs = @(); $seen = @{}
foreach ($n in @('mscorlib.dll','System.dll','System.Core.dll','System.Xml.dll',
                 'System.Runtime.Serialization.dll','System.Numerics.dll','System.Xml.Linq.dll')) {
    $p = Join-Path $api $n
    if (Test-Path -LiteralPath $p) { $refs += ('"' + $p + '"'); $seen[$n] = 1 }
}
$facades = Join-Path $api 'Facades'
if (Test-Path -LiteralPath $facades) {
    Get-ChildItem $facades -Filter '*.dll' | ForEach-Object {
        if (-not $seen[$_.Name]) { $seen[$_.Name] = 1; $refs += ('"' + $_.FullName + '"') }
    }
}
Get-ChildItem $managed -Filter '*.dll' | Where-Object {
    $_.Name -ne 'UnityEditor.dll' -and $_.Name -notlike 'System*.dll' -and $_.Name -ne 'mscorlib.dll' -and -not $seen[$_.Name]
} | ForEach-Object { $seen[$_.Name] = 1; $refs += ('"' + $_.FullName + '"') }

if (Test-Path $scriptAs) {
    Get-ChildItem $scriptAs -Filter '*.dll' | Where-Object {
        $_.Name -notlike 'Assembly-CSharp*' -and $_.Name -notlike 'System*.dll' -and $_.Name -ne 'mscorlib.dll' -and -not $seen[$_.Name]
    } | ForEach-Object { $seen[$_.Name] = 1; $refs += ('"' + $_.FullName + '"') }
}

# 3) Fontes do jogo
$srcs = @()
Get-ChildItem (Join-Path $raiz 'Assets\script'), (Join-Path $raiz 'Assets\Editor') -Recurse -Filter '*.cs' -ErrorAction SilentlyContinue |
    ForEach-Object { $srcs += ('"' + $_.FullName + '"') }

# 4) Compilar
$dirTmp = Join-Path $env:TEMP 'opencode'
if (-not (Test-Path $dirTmp)) { New-Item -ItemType Directory -Path $dirTmp | Out-Null }
$out  = Join-Path $dirTmp 'check.dll'
$rsp  = Join-Path $dirTmp 'check.rsp'
@('-target:library', ('-out:"' + $out + '"'), '-nostdlib+',
  '-nowarn:0436,1701,1702,0649,0414', '-langversion:9.0') +
  ($refs | ForEach-Object { '-r:' + $_ }) + $srcs |
    Set-Content -Path $rsp -Encoding UTF8

Write-Host ("Unity:  " + $ed)
Write-Host ("Refs:   " + $refs.Count + "   Scripts: " + $srcs.Count)
$saida = & $mono $csc "@$rsp" 2>&1
$erros = @($saida | Where-Object { $_ -match 'error CS' })

if ($erros.Count -eq 0) {
    Write-Host 'OK - nenhum erro de compilacao.' -ForegroundColor Green
    exit 0
}
Write-Host ("ERROS: " + $erros.Count) -ForegroundColor Red
$erros | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
exit 1
