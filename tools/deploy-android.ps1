# Shapes - Android 에뮬레이터 설치·실행 스크립트
# 사용 전: Godot에서 APK를 먼저 만드세요.
#   프로젝트 → 보내기 → Android → 프로젝트보내기…

$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ApkCandidates = @(
    (Join-Path $ProjectRoot "shapes.apk"),
    (Join-Path $ProjectRoot "build\android\shapes.apk"),
    (Join-Path $ProjectRoot "android\build\build\outputs\apk\debug\android_debug.apk")
)
$ApkPath = $ApkCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$AdbPath = Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"
$PackageName = "com.shapes.game"
$Activity = "com.godot.game.GodotApp"

if (-not (Test-Path $AdbPath)) {
    Write-Host "adb를 찾을 수 없습니다: $AdbPath" -ForegroundColor Red
    Write-Host "Android Studio SDK 경로를 확인하세요."
    exit 1
}

Write-Host "연결된 기기 확인..."
& $AdbPath devices

$devices = & $AdbPath devices | Select-String "device$"
if (-not $devices) {
    Write-Host "에뮬레이터/기기가 연결되지 않았습니다. AVD를 먼저 실행하세요." -ForegroundColor Red
    exit 1
}

if (-not $ApkPath) {
    Write-Host "APK를 찾을 수 없습니다. 다음 경로를 확인했습니다:" -ForegroundColor Red
    $ApkCandidates | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
    Write-Host "Godot에서 APK를 먼저 만드세요:"
    Write-Host "  1. 프로젝트 → 보내기…"
    Write-Host "  2. Android 선택"
    Write-Host "  3. armeabi-v7a 체크 해제 (arm64-v8a, x86_64만)"
    Write-Host "  4. 프로젝트보내기… 클릭"
    exit 1
}

Write-Host "APK 설치 중: $ApkPath"
& $AdbPath install -r $ApkPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "설치 실패" -ForegroundColor Red
    exit 1
}

Write-Host "앱 실행 중..."
& $AdbPath shell am start -n "$PackageName/$Activity"

Write-Host ""
Write-Host "완료. 에뮬레이터 화면에서 Shapes를 확인하세요." -ForegroundColor Green
