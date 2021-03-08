@echo off
set curDir=%~dp0
sc create "GoDaddy DNS Sync Service" binPath="%curDir%GoDaddyDNSSync.exe" start=auto 
