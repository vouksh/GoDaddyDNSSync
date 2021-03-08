# GoDaddyDNSSync
Small service application to keep a GoDaddy domain sync'd to a dynamic DNS. This will only update A-records. 

How to use:
1. Download the application and save it to a directory on a Windows server. 
2. Go to https://developer.godaddy.com/, login to your account, and create a Production key/secret combo. 
3. Modify the appsettings.json file with your domain name, key, and secret. 
4. Run InstallAsService.bat as an administrator. 

If you wish to exclude certain A-records, add them as a comma-seperated list in the appsettings.json file. 

Currently only supports 1 domain per service worker.
