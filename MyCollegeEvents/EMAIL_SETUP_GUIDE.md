# 📧 دليل إعداد البريد الإلكتروني

## 🔧 إعداد Gmail للإرسال

### الخطوة 1: إنشاء حساب Gmail
1. إنشاء حساب Gmail جديد أو استخدام حساب موجود
2. مثال: `collegeevents2025@gmail.com`

### الخطوة 2: تفعيل المصادقة الثنائية
1. اذهب إلى [myaccount.google.com](https://myaccount.google.com)
2. اختر "الأمان" (Security)
3. فعّل "التحقق بخطوتين" (2-Step Verification)

### الخطوة 3: إنشاء App Password
1. في صفحة الأمان، اختر "كلمات مرور التطبيقات" (App passwords)
2. اختر "البريد" (Mail) كتطبيق
3. اختر "جهاز آخر" (Other) كجهاز
4. أدخل اسم مثل "College Events System"
5. انسخ كلمة المرور المُنشأة (16 رقم)

### الخطوة 4: تحديث appsettings.json
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "collegeevents2025@gmail.com",
    "SenderPassword": "abcd efgh ijkl mnop",
    "SenderName": "نظام إدارة فعاليات الكلية",
    "EnableSsl": true,
    "TestMode": false,
    "AdminEmail": "admin@college.edu.sa"
  }
}
```

## 🧪 اختبار الإعدادات

### وضع الاختبار
لتفعيل وضع الاختبار (بدون إرسال فعلي):
```json
"TestMode": true
```

### اختبار الإرسال الفعلي
1. تأكد من `"TestMode": false`
2. اذهب إلى لوحة المشرفة
3. اضغط على "🧪 اختبار البريد"
4. تحقق من وصول البريد

## 🔍 حل المشاكل الشائعة

### خطأ: "Authentication failed"
- تأكد من تفعيل المصادقة الثنائية
- تأكد من استخدام App Password وليس كلمة المرور العادية
- تأكد من صحة البريد الإلكتروني

### خطأ: "SMTP server requires a secure connection"
- تأكد من `"EnableSsl": true`
- تأكد من `"SmtpPort": 587`

### خطأ: "Mailbox unavailable"
- تأكد من صحة عنوان البريد الإلكتروني
- تأكد من أن الحساب نشط

## 📨 إعدادات بدائل أخرى

### Outlook/Hotmail
```json
{
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "EnableSsl": true
}
```

### Yahoo Mail
```json
{
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "EnableSsl": true
}
```

## 🛡️ نصائح الأمان

1. **لا تشارك App Password**: احتفظ بها سرية
2. **استخدم حساب مخصص**: لا تستخدم حسابك الشخصي
3. **راجع النشاط**: تحقق من نشاط الحساب بانتظام
4. **احذف App Passwords غير المستخدمة**: من إعدادات Google

## 📞 الدعم

إذا واجهت مشاكل:
1. تحقق من الـ Console في المتصفح للأخطاء
2. راجع ملف الـ logs في التطبيق
3. جرب وضع الاختبار أولاً
4. تأكد من اتصال الإنترنت

---

**ملاحظة**: تأكد من عدم رفع ملف `appsettings.json` مع كلمات المرور الحقيقية إلى Git!
