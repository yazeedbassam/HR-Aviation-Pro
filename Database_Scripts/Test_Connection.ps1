# Test Supabase Connection
Write-Host "Testing Supabase connection..." -ForegroundColor Yellow

$connectionString = "Host=db.hzweniqfssqorruiujwc.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=Y@Z105213eed;SSL Mode=Require;Trust Server Certificate=true;Timeout=60;CommandTimeout=30;"

Write-Host "Connection string: $($connectionString.Replace('Password=Y@Z105213eed', 'Password=***'))" -ForegroundColor Cyan

try {
    # Test network connectivity
    Write-Host "Testing network connectivity..." -ForegroundColor Yellow
    $ping = Test-NetConnection -ComputerName "db.hzweniqfssqorruiujwc.supabase.co" -Port 6543 -WarningAction SilentlyContinue
    if ($ping.TcpTestSucceeded) {
        Write-Host "Network connectivity to Supabase successful" -ForegroundColor Green
    } else {
        Write-Host "Network connectivity to Supabase failed" -ForegroundColor Red
    }
    
    # Test DNS resolution
    Write-Host "Testing DNS resolution..." -ForegroundColor Yellow
    $dns = Resolve-DnsName -Name "db.hzweniqfssqorruiujwc.supabase.co" -ErrorAction SilentlyContinue
    if ($dns) {
        Write-Host "DNS resolution successful: $($dns[0].IPAddress)" -ForegroundColor Green
    } else {
        Write-Host "DNS resolution failed" -ForegroundColor Red
    }
    
} catch {
    Write-Host "Error during connection test: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Testing with telnet..." -ForegroundColor Yellow
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect("db.hzweniqfssqorruiujwc.supabase.co", 6543)
    if ($tcpClient.Connected) {
        Write-Host "TCP connection to Supabase successful" -ForegroundColor Green
        $tcpClient.Close()
    } else {
        Write-Host "TCP connection to Supabase failed" -ForegroundColor Red
    }
} catch {
    Write-Host "TCP connection failed: $($_.Exception.Message)" -ForegroundColor Red
}