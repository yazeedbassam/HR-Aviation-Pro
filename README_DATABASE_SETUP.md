# إعداد قاعدة البيانات على Railway

## المشكلة الحالية
التطبيق لا يستطيع الاتصال بقاعدة البيانات على Railway لأن متغيرات البيئة غير مُعدة.

## الحل

### 1. إضافة قاعدة بيانات SQL Server على Railway

1. اذهب إلى مشروعك على Railway
2. اضغط على "New Service" 
3. اختر "Database" ثم "SQL Server"
4. انتظر حتى يتم إنشاء قاعدة البيانات

### 2. إعداد متغيرات البيئة

بعد إنشاء قاعدة البيانات، Railway سيقوم تلقائياً بإضافة متغيرات البيئة التالية:

```
DB_SERVER=your-sql-server-host
DB_NAME=your-database-name
DB_USER=your-username
DB_PASSWORD=your-password
DB_PORT=1433
```

### 3. ربط قاعدة البيانات بالتطبيق

1. في مشروع Railway، اذهب إلى "Variables" tab
2. تأكد من وجود المتغيرات التالية:
   - `DB_SERVER`
   - `DB_NAME` 
   - `DB_USER`
   - `DB_PASSWORD`
   - `DB_PORT`

### 4. إنشاء جداول قاعدة البيانات

بعد ربط قاعدة البيانات، تحتاج إلى إنشاء الجداول. يمكنك:

1. استخدام SQL Server Management Studio للاتصال بقاعدة البيانات
2. تشغيل scripts إنشاء الجداول
3. أو استخدام Entity Framework migrations

### 5. إنشاء مستخدم Admin

بعد إنشاء الجداول، قم بإنشاء مستخدم admin:

```sql
INSERT INTO ControllerUsers (Username, PasswordHash, Role) 
VALUES ('admin', 'hashed_password', 'Admin');
```

## اختبار الاتصال

بعد إعداد قاعدة البيانات، يمكنك اختبار الاتصال عبر:

- `/health` - للتحقق من حالة التطبيق
- `/Account/Login` - لتسجيل الدخول

## ملاحظات مهمة

- تأكد من أن قاعدة البيانات SQL Server تعمل على Railway
- تأكد من صحة بيانات الاتصال
- في حالة وجود مشاكل، راجع logs في Railway 