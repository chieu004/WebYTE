-- Kiểm tra Doctor ID của doctor1@webyte.com
SELECT 
    u.Id AS UserId,
    u.Email,
    u.FullName,
    d.Id AS DoctorId
FROM Users u
LEFT JOIN Doctors d ON u.Id = d.UserId
WHERE u.Email = 'doctor1@webyte.com';

-- Kiểm tra tất cả appointments
SELECT 
    a.Id,
    a.AppointmentDate,
    CAST(a.AppointmentDate AS DATE) AS DateOnly,
    CAST(GETDATE() AS DATE) AS Today,
    a.Reason,
    a.Status,
    a.DoctorId,
    u.FullName AS PatientName,
    u2.FullName AS DoctorName,
    u2.Email AS DoctorEmail
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.Id
INNER JOIN Users u ON p.UserId = u.Id
INNER JOIN Doctors d ON a.DoctorId = d.Id
INNER JOIN Users u2 ON d.UserId = u2.Id
ORDER BY a.AppointmentDate;

-- Kiểm tra appointments của doctor1 cụ thể
SELECT 
    a.Id,
    a.AppointmentDate,
    CAST(a.AppointmentDate AS DATE) AS DateOnly,
    DATEPART(HOUR, a.AppointmentDate) AS Hour,
    a.Reason,
    a.Status,
    u.FullName AS PatientName
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.Id
INNER JOIN Users u ON p.UserId = u.Id
INNER JOIN Doctors d ON a.DoctorId = d.Id
INNER JOIN Users u2 ON d.UserId = u2.Id
WHERE u2.Email = 'doctor1@webyte.com'
ORDER BY a.AppointmentDate;

-- Đếm số lượng appointments
SELECT 
    u2.Email AS DoctorEmail,
    COUNT(*) AS TotalAppointments
FROM Appointments a
INNER JOIN Doctors d ON a.DoctorId = d.Id
INNER JOIN Users u2 ON d.UserId = u2.Id
GROUP BY u2.Email;
