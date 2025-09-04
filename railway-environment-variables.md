# Railway Environment Variables Configuration

## Required Environment Variables for Railway Deployment

### Database Configuration
```
SUPABASE_HOST=hzweniqfssqorruiujwc.supabase.co
SUPABASE_DB=postgres
SUPABASE_USER=postgres
SUPABASE_PASSWORD=Y@Z105213eed
SUPABASE_PORT=5432
SUPABASE_URL=https://hzweniqfssqorruiujwc.supabase.co
SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imh6d2VuaXFmc3Nxb3JydWl1andjIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTY5MjE4MTIsImV4cCI6MjA3MjQ5NzgxMn0.U4GomCprtgLqKzwwX64DCD1P5lAdw2jQgH78_EjBr_U
SUPABASE_SERVICE_ROLE_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imh6d2VuaXFmc3Nxb3JydWl1andjIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjkyMTgxMiwiZXhwIjoyMDcyNDk3ODEyfQ.S--2fv9J8Ebrdn79W0R_Bjh-BkmVSTi--XfgXf75q8s
```

### Application Configuration
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

### Email Configuration (Optional)
```
EMAIL_SMTP_SERVER=smtp-relay.brevo.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=8e2caf001@smtp-brevo.com
EMAIL_PASSWORD=3HzgVG7nwKMxqcA2
EMAIL_FROM=yazeedbassam1987@gmail.com
EMAIL_FROM_NAME=HR Aviation System
```

## How to Set Environment Variables in Railway

1. Go to your Railway project dashboard
2. Click on your service
3. Go to the "Variables" tab
4. Add each environment variable with its corresponding value
5. Click "Deploy" to apply the changes

## Important Notes

- The `PORT` variable is automatically set by Railway
- Make sure all Supabase variables are set correctly
- The application will use Supabase as the default database
- Admin credentials: username=`admin`, password=`admin123`