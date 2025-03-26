# Stop on error
$ErrorActionPreference = "Stop"

# Paths
$solutionPath = "TCPChatApp.sln"
$clientExePath = ".\TCPChatApp.Client\bin\Release\net8.0-windows\TCPChatApp.Client.exe"
$serverExePath = ".\TCPChatApp.Server\bin\Release\net8.0\TCPChatApp.Server.exe"

# Kill any running server to avoid file lock
Write-Host "Terminating any existing server process..."
Get-Process TCPChatApp.Server -ErrorAction SilentlyContinue | Stop-Process -Force

# Build the solution
Write-Host "Building the solution..."
dotnet build $solutionPath -c Release

# Start the server
Write-Host "Starting the server..."
$serverProcess = Start-Process $serverExePath -NoNewWindow -PassThru

# Give server a moment to initialize
Start-Sleep -Seconds 1

# Start two clients
Write-Host "Starting two client instances..."
Start-Process $clientExePath
Start-Process $clientExePath

Write-Host "Done."
