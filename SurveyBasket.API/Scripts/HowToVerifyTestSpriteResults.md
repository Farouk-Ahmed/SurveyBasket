# كيف تعرف إذا كانت مشاكل TestSprite اتحلت؟

## 1. تشغيل الـ API أولاً (إجباري)
الاختبارات بتتصل بالسيرفر المحلي على **البورت 7270**، فالسيرفر لازم يكون شغال قبل تشغيل TestSprite.

من مجلد الحل (SurveyBasket):
```powershell
dotnet run --project SurveyBasket.API\SurveyBasket.API.csproj
```
اتأكد إن الـ API شغال على **http://localhost:7270** أو **https://localhost:7270**.
أو من داخل `SurveyBasket.API`:
```powershell
dotnet run
```

اتأكد إن الـ API شغال على **http://localhost:7270** أو **https://localhost:7270** (حسب الـ profile).

---

## 2. إعادة تشغيل اختبارات TestSprite
- في Cursor: اطلب من الـ AI تشغيل **TestSprite Generate and Execute** (أو استخدم أداة TestSprite من القائمة/الأوامر).
- أو من الـ Terminal:
  ```bash
  npx @testsprite/testsprite-mcp run
  ```
- انتظر لحد ما التنفيذ يخلص (قد ياخد عدة دقايق).

---

## 3. مراجعة النتائج

### من ملف النتائج محلياً
افتح الملف:
```
SurveyBasket\bin\Debug\net8.0\testsprite_tests\tmp\test_results.json
```
(أو نفس المسار تحت مجلد المشروع اللي بيستخدمه TestSprite)

في الملف دور على:
- **`"testStatus": "PASSED"`** → الاختبار نجح.
- **`"testStatus": "FAILED"`** → الاختبار فشل (شوف حقل **`testError`** لسبب الفشل).

لو كل الاختبارات فيها `PASSED` يبقى المشاكل اتحلت.

### من لوحة TestSprite في Cursor
- افتح **TestSprite Test Result Dashboard** من أدوات TestSprite.
- استخدم **المسار اللي فيه الملف فعلاً**، مثلاً:
  `f:\Projects\API-Deev Cred\Project\SurveyBasket\SurveyBasket\bin\Debug\net8.0`
- اللوحة هتعرض كل اختبار وحالته (Pass/Fail) والتفاصيل.

---

## 4. ملخص سريع
| لو حصل كده | معناها |
|------------|--------|
| كل الـ `testStatus` = **PASSED** | المشاكل اتحلت. |
| في **FAILED** | لسه في مشاكل؛ راجع **testError** لكل اختبار فاشل. |
| ملف النتائج مش موجود أو قديم | أعد تشغيل الاختبارات بعد ما السيرفر يكون شغال. |
