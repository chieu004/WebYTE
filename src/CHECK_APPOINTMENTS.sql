-- Kiểm tra appointments đã được tạo cho doctor1@webyte.com
SELECT 
    a.Id,
    a.AppointmentDate,
    CAST(a.AppointmentDate AS DATE) AS DateOnly,
    DATEPART(HOUR, a.AppointmentDate) AS Hour,
    a.Reason,
    a.Status,
    d.Id AS DoctorId,
    u.Email AS DoctorEmail,
    u.FullName AS DoctorName
FROM Appointments a
INNER JOIN Doctors d ON a.DoctorId = d.Id
INNER JOIN Users u ON d.UserId = u.Id
WHERE u.Email = 'doctor1@webyte.com'
ORDER BY a.AppointmentDate;

-- Kiểm tra ngày hôm nay
SELECT GETDATE() AS CurrentDateTime, CAST(GETDATE() AS DATE) AS Today;

-- Đếm số lượng appointments cho mỗi doctor
SELECT 
    u.Email,
    u.FullName,
    COUNT(a.Id) AS AppointmentCount
FROM Doctors d
INNER JOIN Users u ON d.UserId = u.Id
LEFT JOIN Appointments a ON d.Id = a.DoctorId
GROUP BY u.Email, u.FullName
ORDER BY u.Email;
