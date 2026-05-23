-- Kiểm tra thông báo của doctor1@webyte.com
SELECT 
    u.Email,
    u.FullName,
    n.Id AS NotificationId,
    n.Title,
    n.Message,
    n.Type,
    n.IsRead,
    n.CreatedAt,
    n.RelatedId
FROM Users u
LEFT JOIN Notifications n ON u.Id = n.UserId
WHERE u.Email = 'doctor1@webyte.com'
ORDER BY n.CreatedAt DESC;

-- Kiểm tra tất cả thông báo trong hệ thống
SELECT 
    u.Email,
    u.FullName,
    n.Title,
    n.Type,
    n.IsRead,
    n.CreatedAt
FROM Notifications n
INNER JOIN Users u ON n.UserId = u.Id
ORDER BY n.CreatedAt DESC;

-- Đếm số lượng thông báo theo user
SELECT 
    u.Email,
    COUNT(*) AS TotalNotifications,
    SUM(CASE WHEN n.IsRead = 0 THEN 1 ELSE 0 END) AS UnreadCount
FROM Notifications n
INNER JOIN Users u ON n.UserId = u.Id
GROUP BY u.Email
ORDER BY TotalNotifications DESC;
