-- Kiểm tra thông tin doctor1
SELECT 
    u.Id as UserId,
    u.Email,
    u.FullName,
    u.Role,
    d.Id as DoctorId
FROM Users u
LEFT JOIN Doctors d ON u.Id = d.UserId
WHERE u.Email = 'doctor1@webyte.com';

-- Kiểm tra lịch hẹn của doctor1
SELECT 
    a.Id as AppointmentId,
    a.AppointmentDate,
    a.Status,
    p.FullName as PatientName,
    d.Id as DoctorId
FROM Appointments a
INNER JOIN Doctors d ON a.DoctorId = d.Id
INNER JOIN Users u ON d.UserId = u.Id
INNER JOIN Patients p ON a.PatientId = p.Id
INNER JOIN Users pu ON p.UserId = pu.Id
WHERE u.Email = 'doctor1@webyte.com'
ORDER BY a.AppointmentDate DESC;

-- Kiểm tra bệnh án của doctor1
SELECT 
    mr.Id as MedicalRecordId,
    mr.Diagnosis,
    mr.CreatedAt,
    p.FullName as PatientName,
    a.AppointmentDate,
    d.Id as DoctorId
FROM MedicalRecords mr
INNER JOIN Doctors d ON mr.DoctorId = d.Id
INNER JOIN Users u ON d.UserId = u.Id
INNER JOIN Patients p ON mr.PatientId = p.Id
INNER JOIN Users pu ON p.UserId = pu.Id
LEFT JOIN Appointments a ON mr.AppointmentId = a.Id
WHERE u.Email = 'doctor1@webyte.com'
ORDER BY mr.CreatedAt DESC;

-- Kiểm tra tất cả bệnh án trong hệ thống
SELECT 
    mr.Id,
    mr.Diagnosis,
    du.Email as DoctorEmail,
    pu.Email as PatientEmail,
    mr.CreatedAt
FROM MedicalRecords mr
INNER JOIN Doctors d ON mr.DoctorId = d.Id
INNER JOIN Users du ON d.UserId = du.Id
INNER JOIN Patients p ON mr.PatientId = p.Id
INNER JOIN Users pu ON p.UserId = pu.Id
ORDER BY mr.CreatedAt DESC;
