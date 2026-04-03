-- ============================================================
-- مسح كل بيانات الحسابات: العملاء، الأدمن، والمستخدمين (Identity)
-- يشمل: AuditLogs, Attachments, Polls, Admins, Clients,
--       RefrechTokens, AspNetUserTokens, AspNetUserRoles,
--       AspNetUserLogins, AspNetUserClaims, AspNetUsers
-- ============================================================
-- تحذير: هذا يمسح كل المستخدمين والحسابات. الأدوار (AspNetRoles) تبقى
--         ويمكن إعادة إنشاء المستخدم الافتراضي عند تشغيل التطبيق (DataSeeder).
-- ============================================================

BEGIN TRANSACTION;

BEGIN TRY
    -- 1) جداول تعتمد على Poll / User / Client
    DELETE FROM AuditLogs;
    DELETE FROM Attachments;

    -- 2) الاستبيانات (تعتمد على Client و User)
    DELETE FROM Polls;

    -- 3) بروفايل الأدمن والعملاء (تعتمد على AspNetUsers)
    DELETE FROM Admins;
    DELETE FROM Clients;

    -- 4) توكنات التحديث (تعتمد على AspNetUsers)
    DELETE FROM RefrechTokens;

    -- 5) جداول Identity المرتبطة بالمستخدم
    DELETE FROM AspNetUserTokens;
    DELETE FROM AspNetUserRoles;
    DELETE FROM AspNetUserLogins;
    DELETE FROM AspNetUserClaims;

    -- 6) المستخدمون
    DELETE FROM AspNetUsers;

    -- تأكيد التعديلات (شغّل السطر التالي بعد التأكد)
    COMMIT TRANSACTION;
    PRINT N'تم مسح كل بيانات الحسابات بنجاح.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'حدث خطأ: ' + ERROR_MESSAGE();
END CATCH;
