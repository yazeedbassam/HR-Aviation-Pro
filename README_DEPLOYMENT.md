# 🚀 AVIATION HR PRO - Deployment Guide

## 📋 Prerequisites
- GitHub account
- Railway account
- DBeaver (for database management)
- Local .NET 8.0 development environment

## 🔧 Step 1: GitHub Setup

### 1.1 Initialize Git Repository
```bash
git init
git add .
git commit -m "Initial commit: AVIATION HR PRO v2.0.0"
```

### 1.2 Push to GitHub
```bash
git remote add origin https://github.com/yazeedbassam/HR-Aviation.git
git branch -M main
git push -u origin main
```

## 🗄️ Step 2: Railway Database Setup

### 2.1 Create MySQL Database
1. Go to [Railway Dashboard](https://railway.app/dashboard)
2. Click "New Project"
3. Select "Provision MySQL"
4. Wait for database creation
5. Note the connection details

### 2.2 Get Database Connection Details
1. Click on your MySQL service
2. Go to "Connect" tab
3. Copy the connection details:
   - **Host**: `hopper.proxy.rlwy.net`
   - **Port**: `22626`
   - **Database**: `railway`
   - **Username**: `root`
   - **Password**: (from Railway dashboard)

## 🔗 Step 3: DBeaver Database Connection

### 3.1 Connect to Railway MySQL
1. Open DBeaver
2. Click "New Database Connection"
3. Select "MySQL"
4. Fill connection details:
   ```
   Server Host: hopper.proxy.rlwy.net
   Port: 22626
   Database: railway
   Username: root
   Password: [your_password]
   ```
5. Go to "Driver properties" tab
6. Add these properties:
   ```
   allowPublicKeyRetrieval = true
   useSSL = false
   ```
7. Test connection and save

### 3.2 Execute Database Setup Script
1. In DBeaver, right-click on "railway" database
2. Select "SQL Editor" > "New SQL Script"
3. Open file: `Database_Scripts/MySQL_Complete_Tables.sql`
4. Copy entire content and paste in DBeaver
5. Execute script (Ctrl+Enter)
6. Verify all tables are created

## 🚀 Step 4: Railway Application Deployment

### 4.1 Deploy Application
1. Go to Railway Dashboard
2. Click "New Project"
3. Select "Deploy from GitHub repo"
4. Choose your repository: `yazeedbassam/HR-Aviation`
5. Wait for deployment

### 4.2 Configure Environment Variables
In Railway dashboard, go to your application service and add these environment variables:

```bash
# Database Configuration
DB_SERVER=hopper.proxy.rlwy.net
DB_NAME=railway
DB_USER=root
DB_PASSWORD=[your_mysql_password]
DB_PORT=22626

# Email Configuration (Optional)
SENDGRID_API_KEY=[your_sendgrid_api_key]
FROM_EMAIL=noreply@aviationhr.com

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

### 4.3 Redeploy Application
1. After setting environment variables
2. Go to "Deployments" tab
3. Click "Redeploy"
4. Wait for deployment to complete

## 🔍 Step 5: Verification

### 5.1 Check Application Health
1. Get your application URL from Railway
2. Visit the URL in browser
3. Should see login page
4. Login with: `admin` / `123`

### 5.2 Verify Database Connection
1. Check Railway logs for any database errors
2. Verify all tables exist in DBeaver
3. Test basic functionality

## 🔒 Step 6: Security & Monitoring

### 6.1 Security Checklist
- [ ] Environment variables are set correctly
- [ ] Database connection uses SSL
- [ ] Application is accessible via HTTPS
- [ ] Admin password is changed after first login

### 6.2 Monitoring Setup
1. Enable Railway monitoring
2. Set up log aggregation
3. Configure health checks
4. Set up alerts for downtime

## 🛠️ Step 7: Data Migration (Optional)

### 7.1 Export from Local SQL Server
1. Use SQL Server Management Studio
2. Export data to CSV files
3. Or use DBeaver to export data

### 7.2 Import to Railway MySQL
1. Use DBeaver to import CSV files
2. Or execute INSERT statements
3. Verify data integrity

## 🔧 Troubleshooting

### Common Issues:

#### 1. Database Connection Error
```
Error: A network-related or instance-specific error occurred
```
**Solution**: Check environment variables and database connection string

#### 2. Health Check Failed
```
Healthcheck failed! 1/1 replicas never became healthy!
```
**Solution**: 
- Verify database is accessible
- Check application logs
- Ensure all environment variables are set

#### 3. MySQL Connection Issues
```
Public Key Retrieval is not allowed
```
**Solution**: Add `allowPublicKeyRetrieval=true` to connection string

#### 4. Application Won't Start
```
Unhandled exception. Microsoft.Data.SqlClient.SqlException
```
**Solution**: 
- Check if database tables exist
- Verify connection string format
- Ensure database is running

### Debug Steps:
1. Check Railway logs
2. Verify environment variables
3. Test database connection in DBeaver
4. Check application configuration

## 📞 Support

If you encounter issues:
1. Check Railway documentation
2. Review application logs
3. Verify database connectivity
4. Contact support with error details

## 🔄 Future Updates

### Updating Application:
```bash
git add .
git commit -m "Update description"
git push origin main
# Railway will auto-deploy
```

### Database Schema Changes:
1. Update MySQL scripts
2. Execute in DBeaver
3. Test locally first
4. Deploy to production

---

**🎉 Congratulations! Your AVIATION HR PRO application is now deployed and running on Railway!** 