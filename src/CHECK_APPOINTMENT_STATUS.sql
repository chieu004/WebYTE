-- Kiểm tra trạng thái lịch hẹn trong database

USE WebYTEDb;
GO

-- 1. Xem tất cả lịch hẹn với trạng thái
SELECT 
    a.Id,
    a.AppointmentDate,
    p.FullName AS PatientName,
    d.FullName AS DoctorName,
    a.Reason,
    a.Status,
    CASE a.Status
        WHEN 0 THEN 'Pending (Chờ xác nhận)'
        WHEN 1 THEN 'Confirmed (Đã xác nhận)'
        WHEN 2 THEN 'Completed (Hoàn thành)'
        WHEN 3 THEN 'Cancelled (Đã hủy)'
        WHEN 4 THEN 'NoShow (Không đến)'
        ELSE 'Unknown'
    END AS StatusText,
    a.PaymentStatus,
    CASE a.PaymentStatus
        WHEN 0 THEN 'Unpaid (Chưa thanh toán)'
        WHEN 1 THEN 'Pending (Chờ thanh toán)'
        WHEN 2 THEN 'Paid (Đã thanh toán)'
        WHEN 3 THEN 'Failed (Thất bại)'
        WHEN 4 THEN 'Refunded (Đã hoàn tiền)'
        ELSE 'Unknown'
    END AS PaymentStatusText
FROM Appointments a
INNER JOIN Patients pt ON a.PatientId = pt.Id
INNER JOIN Users p ON pt.UserId = p.Id
INNER JOIN Doctors doc ON a.DoctorId = doc.Id
INNER JOIN Users d ON doc.UserId = d.Id
ORDER BY a.AppointmentDate DESC;

-- 2. Đếm số lượng theo trạng thái
SELECT 
    Status,
    CASE Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Confirmed'
        WHEN 2 THEN 'Completed'
        WHEN 3 THEN 'Cancelled'
        WHEN 4 THEN 'NoShow'
        ELSE 'Unknown'
    END AS StatusText,
    COUNT(*) AS Count
FROM Appointments
GROUP BY Status
ORDER BY Status;

-- 3. Lịch hẹn chờ xác nhận (Status = 0)
SELECT 
    a.Id,
    a.AppointmentDate,
    p.FullName AS PatientName,
    d.FullName AS DoctorName,
    a.Reason
FROM Appointments a
INNER JOIN Patients pt ON a.PatientId = pt.Id
INNER JOIN Users p ON pt.UserId = p.Id
INNER JOIN Doctors doc ON a.DoctorId = doc.Id
INNER JOIN Users d ON doc.UserId = d.Id
WHERE a.Status = 0
ORDER BY a.AppointmentDate;

-- 4. Test update một lịch hẹn (KHÔNG CHẠY - CHỈ ĐỂ THAM KHẢO)
-- UPDATE Appointments SET Status = 1 WHERE Id = 'YOUR_APPOINTMENT_ID_HERE';
