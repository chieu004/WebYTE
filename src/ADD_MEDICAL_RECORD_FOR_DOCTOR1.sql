-- Script để thêm bệnh án mẫu cho doctor1

-- 1. Tìm doctor1 ID
DECLARE @Doctor1Id UNIQUEIDENTIFIER;
SELECT @Doctor1Id = d.Id 
FROM Doctors d
INNER JOIN Users u ON d.UserId = u.Id
WHERE u.Email = 'doctor1@webyte.com';

-- 2. Tạo một appointment completed cho doctor1
DECLARE @Patient1Id UNIQUEIDENTIFIER;
SELECT TOP 1 @Patient1Id = Id FROM Patients;

DECLARE @NewAppointmentId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Appointments (Id, PatientId, DoctorId, AppointmentDate, Reason, Symptoms, Status, PaymentStatus, CreatedAt, IsDeleted)
VALUES (
    @NewAppointmentId,
    @Patient1Id,
    @Doctor1Id,
    DATEADD(DAY, -3, GETDATE()), -- 3 ngày trước
    N'Khám tim mạch định kỳ',
    N'Đau ngực, khó thở',
    2, -- Completed
    3, -- Paid
    DATEADD(DAY, -3, GETDATE()),
    0
);

-- 3. Tạo bệnh án cho appointment này
DECLARE @NewMedicalRecordId UNIQUEIDENTIFIER = NEWID();

INSERT INTO MedicalRecords (Id, PatientId, AppointmentId, DoctorId, Diagnosis, TreatmentPlan, Notes, CreatedAt, IsDeleted)
VALUES (
    @NewMedicalRecordId,
    @Patient1Id,
    @NewAppointmentId,
    @Doctor1Id,
    N'Rối loạn nhịp tim nhẹ',
    N'Dùng thuốc chống loạn nhịp, theo dõi ECG định kỳ, hạn chế caffeine',
    N'Bệnh nhân cần tái khám sau 2 tuần, theo dõi nhịp tim hàng ngày',
    DATEADD(DAY, -3, GETDATE()),
    0
);

-- 4. Tạo đơn thuốc
DECLARE @NewPrescriptionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Prescriptions (Id, MedicalRecordId, Notes, CreatedAt, IsDeleted)
VALUES (
    @NewPrescriptionId,
    @NewMedicalRecordId,
    N'Uống thuốc đúng giờ, không bỏ liều. Tránh caffeine và rượu bia.',
    DATEADD(DAY, -3, GETDATE()),
    0
);

-- 5. Thêm chi tiết đơn thuốc
DECLARE @AspirinId UNIQUEIDENTIFIER;
DECLARE @LosartanId UNIQUEIDENTIFIER;

SELECT @AspirinId = Id FROM Medications WHERE Name LIKE 'Aspirin%';
SELECT @LosartanId = Id FROM Medications WHERE Name LIKE 'Losartan%';

INSERT INTO PrescriptionDetails (Id, PrescriptionId, MedicationId, Quantity, Dosage, Instructions, CreatedAt, IsDeleted)
VALUES 
(
    NEWID(),
    @NewPrescriptionId,
    @AspirinId,
    30,
    N'1 viên/ngày',
    N'Uống sau bữa ăn sáng'
),
(
    NEWID(),
    @NewPrescriptionId,
    @LosartanId,
    30,
    N'1 viên/ngày',
    N'Uống trước khi ngủ'
);

-- 6. Tạo thêm 1 bệnh án nữa (không có đơn thuốc)
DECLARE @NewAppointmentId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Patient2Id UNIQUEIDENTIFIER;
SELECT TOP 1 @Patient2Id = Id FROM Patients WHERE Id != @Patient1Id;

INSERT INTO Appointments (Id, PatientId, DoctorId, AppointmentDate, Reason, Symptoms, Status, PaymentStatus, CreatedAt, IsDeleted)
VALUES (
    @NewAppointmentId2,
    @Patient2Id,
    @Doctor1Id,
    DATEADD(DAY, -5, GETDATE()), -- 5 ngày trước
    N'Khám sức khỏe tổng quát',
    N'Không có triệu chứng đặc biệt',
    2, -- Completed
    3, -- Paid
    DATEADD(DAY, -5, GETDATE()),
    0
);

INSERT INTO MedicalRecords (Id, PatientId, AppointmentId, DoctorId, Diagnosis, TreatmentPlan, Notes, CreatedAt, IsDeleted)
VALUES (
    NEWID(),
    @Patient2Id,
    @NewAppointmentId2,
    @Doctor1Id,
    N'Sức khỏe tốt',
    N'Duy trì lối sống lành mạnh, tập thể dục đều đặn',
    N'Không phát hiện bất thường. Tái khám sau 6 tháng.',
    DATEADD(DAY, -5, GETDATE()),
    0
);

-- Kiểm tra kết quả
SELECT 
    mr.Id,
    mr.Diagnosis,
    mr.CreatedAt,
    pu.FullName as PatientName,
    a.AppointmentDate,
    CASE WHEN p.Id IS NOT NULL THEN N'Đã kê đơn' ELSE N'Chưa kê đơn' END as PrescriptionStatus
FROM MedicalRecords mr
INNER JOIN Doctors d ON mr.DoctorId = d.Id
INNER JOIN Users u ON d.UserId = u.Id
INNER JOIN Patients pat ON mr.PatientId = pat.Id
INNER JOIN Users pu ON pat.UserId = pu.Id
LEFT JOIN Appointments a ON mr.AppointmentId = a.Id
LEFT JOIN Prescriptions p ON mr.Id = p.MedicalRecordId
WHERE u.Email = 'doctor1@webyte.com'
ORDER BY mr.CreatedAt DESC;

PRINT N'✅ Đã thêm 2 bệnh án mẫu cho doctor1@webyte.com';
