# Railway Environment Variables المطلوبة

## متغيرات Supabase (مطلوبة):

```
SUPABASE_HOST=hzweniqfssqorruiujwc.supabase.co
SUPABASE_PORT=5432
SUPABASE_DB=postgres
SUPABASE_USER=postgres
SUPABASE_PASSWORD=Y@Z105213eed
```

## متغيرات أخرى (اختيارية):

```
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

## كيفية إضافة المتغيرات في Railway:

1. اذهب إلى **Railway Dashboard**
2. اختر مشروعك **HR-Aviation**
3. اذهب إلى **Variables** tab
4. أضف كل متغير من القائمة أعلاه
5. احفظ التغييرات
6. سيعيد Railway تشغيل التطبيق تلقائياً

## ملاحظة مهمة:
بدون هذه المتغيرات، التطبيق لن يتمكن من الاتصال بقاعدة البيانات Supabase!