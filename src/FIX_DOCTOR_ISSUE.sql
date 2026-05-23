-- BƯỚC 1: Kiểm tra User và Doctor của doctor1@webyte.com
SELECT 
    u.Id AS UserId,
    u.Email,
    u.FullName,
    u.UserRole,
    d.Id AS DoctorId,
    CASE WHEN d.Id IS NULL THEN 'KHÔNG CÓ DOCTOR RECORD!' ELSE 'OK' END AS Status
FROM Users u
LEFT JOIN Doctors d ON u.Id = d.UserId
WHERE u.Email = 'doctor1@webyte.com';

-- BƯỚC 2: Kiểm tra tất cả Doctors
SELECT 
    d.Id AS DoctorId,
    d.UserId,
    u.Email,
    u.FullName
FROM Doctors d
INNER JOIN Users u ON d.UserId = u.Id
ORDER BY u.Email;

-- BƯỚC 3: Kiểm tra appointments
SELECT 
    a.Id,
    a.DoctorId,
    a.AppointmentDate,
    CAST(a.AppointmentDate AS DATE) AS DateOnly,
    CAST(GETDATE() AS DATE) AS Today,
    a.Reason,
    u2.Email AS DoctorEmail
FROM Appointments a
INNER JOIN Doctors d ON a.DoctorId = d.Id
INNER JOIN Users u2 ON d.UserId = u2.Id
ORDER BY a.AppointmentDate;

-- BƯỚC 4: Nếu doctor1 KHÔNG có Doctor record, tạo lại
-- (Chỉ chạy nếu BƯỚC 1 cho thấy KHÔNG CÓ DOCTOR RECORD)
/*
DECLARE @UserId UNIQUEIDENTIFIER;
SELECT @UserId = Id FROM Users WHERE Email = 'doctor1@webyte.com';

DECLARE @SpecialtyId UNIQUEIDENTIFIER;
SELECT TOP 1 @SpecialtyId = Id FROM Specialties WHERE Name = N'Tim mạch';

IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Doctors WHERE UserId = @UserId)
BEGIN
    INSERT INTO Doctors (Id, UserId, SpecialtyId, Qualifications, Experience, ConsultationFee, Bio, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        NEWID(),
        @UserId,
        @SpecialtyId,
        N'Bác sĩ Chuyên khoa II',
        N'15 năm',
        500000,
        N'Chuyên gia tim mạch hàng đầu với nhiều năm kinh nghiệm',
        GETDATE(),
        GETDATE(),
        0
    );
    
    PRINT 'Đã tạo Doctor record cho doctor1@webyte.com';
END
*/
