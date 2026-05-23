using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebYTE.Core.Entities;
using WebYTE.Core.Enums;

namespace WebYTE.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Roles
        string[] roleNames = { RoleType.Admin.ToString(), RoleType.Doctor.ToString(), RoleType.Staff.ToString(), RoleType.Patient.ToString() };
        
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }
        }

        // Seed Admin Account
        var adminEmail = "admin@webyte.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                UserRole = RoleType.Admin,
                DateOfBirth = new DateTime(1985, 1, 1),
                Gender = Gender.Male,
                PhoneNumber = "0901234567"
            };

            var createPowerUser = await userManager.CreateAsync(user, "Admin@123");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleType.Admin.ToString());
            }
        }
    }

    public static async Task SeedSampleDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Kiểm tra nếu đã có data thì không seed nữa
        if (await context.Specialties.AnyAsync())
        {
            return; // Đã có data
        }

        // 1. SEED SPECIALTIES
        var specialties = new[]
        {
            new Specialty { Name = "Tim mạch", Description = "Chuyên khoa điều trị các bệnh về tim mạch", IconUrl = "cardiology.png" },
            new Specialty { Name = "Nội khoa", Description = "Chuyên khoa điều trị các bệnh nội khoa tổng quát", IconUrl = "internal.png" },
            new Specialty { Name = "Ngoại khoa", Description = "Chuyên khoa phẫu thuật và điều trị ngoại khoa", IconUrl = "surgery.png" },
            new Specialty { Name = "Nhi khoa", Description = "Chuyên khoa điều trị bệnh cho trẻ em", IconUrl = "pediatrics.png" },
            new Specialty { Name = "Sản phụ khoa", Description = "Chuyên khoa chăm sóc sức khỏe phụ nữ và thai sản", IconUrl = "obstetrics.png" },
            new Specialty { Name = "Da liễu", Description = "Chuyên khoa điều trị các bệnh về da", IconUrl = "dermatology.png" },
            new Specialty { Name = "Tai Mũi Họng", Description = "Chuyên khoa điều trị bệnh tai mũi họng", IconUrl = "ent.png" },
            new Specialty { Name = "Mắt", Description = "Chuyên khoa điều trị các bệnh về mắt", IconUrl = "ophthalmology.png" },
            new Specialty { Name = "Răng Hàm Mặt", Description = "Chuyên khoa điều trị bệnh răng hàm mặt", IconUrl = "dental.png" },
            new Specialty { Name = "Thần kinh", Description = "Chuyên khoa điều trị các bệnh về thần kinh", IconUrl = "neurology.png" }
        };

        await context.Specialties.AddRangeAsync(specialties);
        await context.SaveChangesAsync();

        // 2. SEED DOCTORS
        var doctorUsers = new[]
        {
            new { FullName = "BS. Nguyễn Văn An", Email = "doctor1@webyte.com", Phone = "0912345001", Gender = Gender.Male, DOB = new DateTime(1980, 3, 15), SpecialtyIndex = 0, Qualifications = "Bác sĩ Chuyên khoa II", Experience = "15 năm", Fee = 500000m, Bio = "Chuyên gia tim mạch hàng đầu với nhiều năm kinh nghiệm" },
            new { FullName = "BS. Trần Thị Bình", Email = "doctor2@webyte.com", Phone = "0912345002", Gender = Gender.Female, DOB = new DateTime(1985, 7, 20), SpecialtyIndex = 1, Qualifications = "Bác sĩ Chuyên khoa I", Experience = "10 năm", Fee = 400000m, Bio = "Bác sĩ nội khoa giàu kinh nghiệm" },
            new { FullName = "BS. Lê Minh Cường", Email = "doctor3@webyte.com", Phone = "0912345003", Gender = Gender.Male, DOB = new DateTime(1978, 11, 5), SpecialtyIndex = 2, Qualifications = "Tiến sĩ Y khoa", Experience = "20 năm", Fee = 800000m, Bio = "Phẫu thuật viên ngoại khoa xuất sắc" },
            new { FullName = "BS. Phạm Thị Dung", Email = "doctor4@webyte.com", Phone = "0912345004", Gender = Gender.Female, DOB = new DateTime(1988, 4, 12), SpecialtyIndex = 3, Qualifications = "Bác sĩ Chuyên khoa I", Experience = "8 năm", Fee = 350000m, Bio = "Bác sĩ nhi khoa tận tâm với trẻ em" },
            new { FullName = "BS. Hoàng Văn Em", Email = "doctor5@webyte.com", Phone = "0912345005", Gender = Gender.Male, DOB = new DateTime(1982, 9, 25), SpecialtyIndex = 4, Qualifications = "Bác sĩ Chuyên khoa II", Experience = "12 năm", Fee = 600000m, Bio = "Chuyên gia sản phụ khoa" },
            new { FullName = "BS. Đỗ Thị Phương", Email = "doctor6@webyte.com", Phone = "0912345006", Gender = Gender.Female, DOB = new DateTime(1990, 2, 8), SpecialtyIndex = 5, Qualifications = "Bác sĩ Đa khoa", Experience = "6 năm", Fee = 300000m, Bio = "Bác sĩ da liễu" },
            new { FullName = "BS. Vũ Minh Giang", Email = "doctor7@webyte.com", Phone = "0912345007", Gender = Gender.Male, DOB = new DateTime(1983, 6, 18), SpecialtyIndex = 6, Qualifications = "Bác sĩ Chuyên khoa I", Experience = "11 năm", Fee = 450000m, Bio = "Chuyên gia tai mũi họng" },
            new { FullName = "BS. Bùi Thị Hoa", Email = "doctor8@webyte.com", Phone = "0912345008", Gender = Gender.Female, DOB = new DateTime(1987, 12, 30), SpecialtyIndex = 7, Qualifications = "Bác sĩ Chuyên khoa I", Experience = "9 năm", Fee = 400000m, Bio = "Bác sĩ chuyên khoa mắt" }
        };

        foreach (var doc in doctorUsers)
        {
            var user = new ApplicationUser
            {
                UserName = doc.Email,
                Email = doc.Email,
                FullName = doc.FullName,
                EmailConfirmed = true,
                UserRole = RoleType.Doctor,
                DateOfBirth = doc.DOB,
                Gender = doc.Gender,
                PhoneNumber = doc.Phone
            };

            var result = await userManager.CreateAsync(user, "Doctor@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleType.Doctor.ToString());

                var doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecialtyId = specialties[doc.SpecialtyIndex].Id,
                    Qualifications = doc.Qualifications,
                    Experience = doc.Experience,
                    ConsultationFee = doc.Fee,
                    Bio = doc.Bio
                };

                await context.Doctors.AddAsync(doctor);
            }
        }

        await context.SaveChangesAsync();

        // 3. SEED PATIENTS
        var patientUsers = new[]
        {
            new { FullName = "Nguyễn Văn Khoa", Email = "patient1@webyte.com", Phone = "0923456001", Gender = Gender.Male, DOB = new DateTime(1995, 5, 10), Address = "123 Lê Lợi, Q1, TP.HCM", BloodType = "O+", History = "Không có tiền sử bệnh lý", Allergies = "Không" },
            new { FullName = "Trần Thị Lan", Email = "patient2@webyte.com", Phone = "0923456002", Gender = Gender.Female, DOB = new DateTime(1992, 8, 22), Address = "456 Nguyễn Huệ, Q1, TP.HCM", BloodType = "A+", History = "Tiền sử viêm dạ dày", Allergies = "Penicillin" },
            new { FullName = "Lê Minh Tuấn", Email = "patient3@webyte.com", Phone = "0923456003", Gender = Gender.Male, DOB = new DateTime(1988, 3, 15), Address = "789 Trần Hưng Đạo, Q5, TP.HCM", BloodType = "B+", History = "Tiền sử cao huyết áp", Allergies = "Không" },
            new { FullName = "Phạm Thị Mai", Email = "patient4@webyte.com", Phone = "0923456004", Gender = Gender.Female, DOB = new DateTime(2000, 11, 8), Address = "321 Võ Văn Tần, Q3, TP.HCM", BloodType = "AB+", History = "Không có tiền sử bệnh lý", Allergies = "Hải sản" },
            new { FullName = "Hoàng Văn Nam", Email = "patient5@webyte.com", Phone = "0923456005", Gender = Gender.Male, DOB = new DateTime(1975, 7, 30), Address = "654 Cách Mạng Tháng 8, Q10, TP.HCM", BloodType = "O-", History = "Tiền sử đái tháo đường type 2", Allergies = "Aspirin" },
            new { FullName = "Đỗ Thị Oanh", Email = "patient6@webyte.com", Phone = "0923456006", Gender = Gender.Female, DOB = new DateTime(1998, 2, 14), Address = "987 Lý Thường Kiệt, Q11, TP.HCM", BloodType = "A-", History = "Không có tiền sử bệnh lý", Allergies = "Không" },
            new { FullName = "Vũ Minh Phúc", Email = "patient7@webyte.com", Phone = "0923456007", Gender = Gender.Male, DOB = new DateTime(1990, 9, 5), Address = "147 Hai Bà Trưng, Q1, TP.HCM", BloodType = "B-", History = "Tiền sử hen suyễn", Allergies = "Phấn hoa" },
            new { FullName = "Bùi Thị Quỳnh", Email = "patient8@webyte.com", Phone = "0923456008", Gender = Gender.Female, DOB = new DateTime(1985, 12, 20), Address = "258 Điện Biên Phủ, Q3, TP.HCM", BloodType = "AB-", History = "Tiền sử viêm gan B đã điều trị", Allergies = "Không" },
            new { FullName = "Trương Văn Sơn", Email = "patient9@webyte.com", Phone = "0923456009", Gender = Gender.Male, DOB = new DateTime(2005, 4, 18), Address = "369 Nguyễn Thị Minh Khai, Q1, TP.HCM", BloodType = "O+", History = "Không có tiền sử bệnh lý", Allergies = "Không" },
            new { FullName = "Ngô Thị Tâm", Email = "patient10@webyte.com", Phone = "0923456010", Gender = Gender.Female, DOB = new DateTime(1993, 6, 25), Address = "741 Phan Xích Long, Phú Nhuận, TP.HCM", BloodType = "A+", History = "Không có tiền sử bệnh lý", Allergies = "Thuốc kháng sinh nhóm Quinolone" }
        };

        foreach (var pat in patientUsers)
        {
            var user = new ApplicationUser
            {
                UserName = pat.Email,
                Email = pat.Email,
                FullName = pat.FullName,
                EmailConfirmed = true,
                UserRole = RoleType.Patient,
                DateOfBirth = pat.DOB,
                Gender = pat.Gender,
                PhoneNumber = pat.Phone,
                Address = pat.Address
            };

            var result = await userManager.CreateAsync(user, "Patient@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleType.Patient.ToString());

                var patient = new Patient
                {
                    UserId = user.Id,
                    BloodType = pat.BloodType,
                    MedicalHistory = pat.History,
                    Allergies = pat.Allergies
                };

                await context.Patients.AddAsync(patient);
            }
        }

        await context.SaveChangesAsync();

        // 4. SEED STAFF
        var staffUsers = new[]
        {
            new { FullName = "Lê Thị Uyên", Email = "staff1@webyte.com", Phone = "0934567001", Gender = Gender.Female, DOB = new DateTime(1992, 3, 10), Position = "Lễ tân", Department = "Tiếp nhận" },
            new { FullName = "Trần Văn Việt", Email = "staff2@webyte.com", Phone = "0934567002", Gender = Gender.Male, DOB = new DateTime(1989, 7, 15), Position = "Kế toán", Department = "Tài chính" },
            new { FullName = "Phạm Thị Xuân", Email = "staff3@webyte.com", Phone = "0934567003", Gender = Gender.Female, DOB = new DateTime(1995, 11, 20), Position = "Y tá", Department = "Điều dưỡng" },
            new { FullName = "Hoàng Văn Yên", Email = "staff4@webyte.com", Phone = "0934567004", Gender = Gender.Male, DOB = new DateTime(1987, 5, 8), Position = "Trưởng phòng", Department = "Hành chính" },
            new { FullName = "Đỗ Thị Ánh", Email = "staff5@webyte.com", Phone = "0934567005", Gender = Gender.Female, DOB = new DateTime(1993, 9, 12), Position = "Dược sĩ", Department = "Nhà thuốc" }
        };

        foreach (var stf in staffUsers)
        {
            var user = new ApplicationUser
            {
                UserName = stf.Email,
                Email = stf.Email,
                FullName = stf.FullName,
                EmailConfirmed = true,
                UserRole = RoleType.Staff,
                DateOfBirth = stf.DOB,
                Gender = stf.Gender,
                PhoneNumber = stf.Phone
            };

            var result = await userManager.CreateAsync(user, "Staff@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleType.Staff.ToString());

                var staff = new Staff
                {
                    UserId = user.Id,
                    Position = stf.Position,
                    Department = stf.Department
                };

                await context.Staffs.AddAsync(staff);
            }
        }

        await context.SaveChangesAsync();

        // 5. SEED APPOINTMENTS (Một số lịch hẹn mẫu)
        var doctors = await context.Doctors.Include(d => d.User).ToListAsync();
        var patients = await context.Patients.ToListAsync();

        if (doctors.Any() && patients.Any())
        {
            // Tìm doctor1@webyte.com
            var doctor1 = doctors.FirstOrDefault(d => d.User.Email == "doctor1@webyte.com");
            var doctor2 = doctors.FirstOrDefault(d => d.User.Email == "doctor2@webyte.com");
            var doctor4 = doctors.FirstOrDefault(d => d.User.Email == "doctor4@webyte.com");
            
            if (doctor1 == null || doctor2 == null || doctor4 == null)
            {
                Console.WriteLine("[SEED ERROR] Could not find required doctors!");
                return;
            }

            var appointments = new[]
            {
                new Appointment
                {
                    PatientId = patients[0].Id,
                    DoctorId = doctor1.Id,
                    AppointmentDate = DateTime.Now.AddDays(2).Date.AddHours(9),
                    Reason = "Khám tim định kỳ",
                    Symptoms = "Đau ngực nhẹ, khó thở khi gắng sức",
                    Status = AppointmentStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Paid
                },
                new Appointment
                {
                    PatientId = patients[1].Id,
                    DoctorId = doctor2.Id,
                    AppointmentDate = DateTime.Now.AddDays(3).Date.AddHours(10),
                    Reason = "Khám đau dạ dày",
                    Symptoms = "Đau bụng, ợ nóng, khó tiêu",
                    Status = AppointmentStatus.Pending,
                    PaymentStatus = PaymentStatus.Unpaid
                },
                new Appointment
                {
                    PatientId = patients[2].Id,
                    DoctorId = doctor1.Id,
                    AppointmentDate = DateTime.Now.AddDays(1).Date.AddHours(14),
                    Reason = "Tái khám cao huyết áp",
                    Symptoms = "Huyết áp không ổn định",
                    Status = AppointmentStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Paid
                },
                new Appointment
                {
                    PatientId = patients[3].Id,
                    DoctorId = doctor4.Id,
                    AppointmentDate = DateTime.Now.AddDays(4).Date.AddHours(15),
                    Reason = "Khám sức khỏe định kỳ",
                    Symptoms = "Không có triệu chứng",
                    Status = AppointmentStatus.Pending,
                    PaymentStatus = PaymentStatus.Unpaid
                },
                new Appointment
                {
                    PatientId = patients[4].Id,
                    DoctorId = doctor2.Id,
                    AppointmentDate = DateTime.Now.AddDays(-2).Date.AddHours(9),
                    Reason = "Khám đái tháo đường",
                    Symptoms = "Đường huyết cao",
                    Status = AppointmentStatus.Completed,
                    PaymentStatus = PaymentStatus.Paid
                },
                new Appointment
                {
                    PatientId = patients[5].Id,
                    DoctorId = doctor1.Id,
                    AppointmentDate = DateTime.Now.Date.AddHours(9),
                    Reason = "Khám tim mạch",
                    Symptoms = "Đau ngực, hồi hộp",
                    Status = AppointmentStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Paid
                },
                new Appointment
                {
                    PatientId = patients[6].Id,
                    DoctorId = doctor1.Id,
                    AppointmentDate = DateTime.Now.Date.AddHours(11),
                    Reason = "Tái khám hen suyễn",
                    Symptoms = "Khó thở, ho",
                    Status = AppointmentStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Paid
                }
            };

            await context.Appointments.AddRangeAsync(appointments);
            await context.SaveChangesAsync();
            
            // Debug: Log appointment dates
            Console.WriteLine($"[SEED DEBUG] Today is: {DateTime.Now.Date}");
            foreach (var apt in appointments)
            {
                Console.WriteLine($"[SEED DEBUG] Appointment created for: {apt.AppointmentDate} - {apt.Reason}");
            }

            // 6. SEED MEDICATIONS
            var medications = new[]
            {
                new Medication { Name = "Aspirin 100mg", GenericName = "Acetylsalicylic Acid", Usage = "Chống đông máu", SideEffects = "Chảy máu dạ dày", Unit = "Viên", Price = 500, InStock = 1000 },
                new Medication { Name = "Paracetamol 500mg", GenericName = "Acetaminophen", Usage = "Giảm đau, hạ sốt", SideEffects = "Tổn thương gan nếu dùng quá liều", Unit = "Viên", Price = 300, InStock = 2000 },
                new Medication { Name = "Amoxicillin 500mg", GenericName = "Amoxicillin", Usage = "Kháng sinh", SideEffects = "Dị ứng, tiêu chảy", Unit = "Viên", Price = 1000, InStock = 500 },
                new Medication { Name = "Metformin 500mg", GenericName = "Metformin HCl", Usage = "Điều trị đái tháo đường type 2", SideEffects = "Buồn nôn, tiêu chảy", Unit = "Viên", Price = 800, InStock = 800 },
                new Medication { Name = "Losartan 50mg", GenericName = "Losartan Potassium", Usage = "Điều trị cao huyết áp", SideEffects = "Chóng mặt, mệt mỏi", Unit = "Viên", Price = 1200, InStock = 600 },
                new Medication { Name = "Omeprazole 20mg", GenericName = "Omeprazole", Usage = "Điều trị viêm loét dạ dày", SideEffects = "Đau đầu, tiêu chảy", Unit = "Viên", Price = 1500, InStock = 400 },
                new Medication { Name = "Salbutamol 100mcg", GenericName = "Salbutamol", Usage = "Điều trị hen suyễn", SideEffects = "Run tay, tim đập nhanh", Unit = "Ống xịt", Price = 50000, InStock = 100 }
            };

            await context.Medications.AddRangeAsync(medications);
            await context.SaveChangesAsync();

            // 7. SEED MEDICAL RECORDS (cho appointment đã completed)
            var completedAppointment = appointments.FirstOrDefault(a => a.Status == AppointmentStatus.Completed);
            if (completedAppointment != null)
            {
                var medicalRecord = new MedicalRecord
                {
                    PatientId = completedAppointment.PatientId,
                    AppointmentId = completedAppointment.Id,
                    DoctorId = completedAppointment.DoctorId,
                    Diagnosis = "Đái tháo đường type 2",
                    TreatmentPlan = "Dùng thuốc Metformin, kiểm soát chế độ ăn, tập thể dục đều đặn",
                    Notes = "Bệnh nhân cần theo dõi đường huyết hàng ngày, tái khám sau 1 tháng"
                };

                await context.MedicalRecords.AddAsync(medicalRecord);
                await context.SaveChangesAsync();

                // 8. SEED PRESCRIPTION
                var prescription = new Core.Entities.Prescription
                {
                    MedicalRecordId = medicalRecord.Id,
                    Notes = "Uống thuốc đúng giờ, không bỏ liều"
                };

                await context.Prescriptions.AddAsync(prescription);
                await context.SaveChangesAsync();

                // 9. SEED PRESCRIPTION DETAILS
                var prescriptionDetails = new[]
                {
                    new Core.Entities.PrescriptionDetail
                    {
                        PrescriptionId = prescription.Id,
                        MedicationId = medications[3].Id, // Metformin
                        Quantity = 60,
                        Dosage = "1 viên/lần, 2 lần/ngày",
                        Instructions = "Uống sau bữa ăn sáng và tối"
                    },
                    new Core.Entities.PrescriptionDetail
                    {
                        PrescriptionId = prescription.Id,
                        MedicationId = medications[1].Id, // Paracetamol
                        Quantity = 20,
                        Dosage = "1-2 viên khi đau đầu",
                        Instructions = "Không dùng quá 8 viên/ngày"
                    }
                };

                await context.PrescriptionDetails.AddRangeAsync(prescriptionDetails);
                await context.SaveChangesAsync();
            }
        }

        // 10. SEED NOTIFICATIONS (Sample notifications for testing)
        var allUsers = await context.Users.ToListAsync();
        var doctor1User = allUsers.FirstOrDefault(u => u.Email == "doctor1@webyte.com");
        var patient1User = allUsers.FirstOrDefault(u => u.Email == "patient1@webyte.com");
        var patient2User = allUsers.FirstOrDefault(u => u.Email == "patient2@webyte.com");
        
        // Get appointments for RelatedId
        var allAppointments = await context.Appointments.ToListAsync();

        if (doctor1User != null && patient1User != null && patient2User != null)
        {
            var sampleNotifications = new[]
            {
                // Notifications for doctor1
                new Notification
                {
                    UserId = doctor1User.Id,
                    Title = "Lịch hẹn mới",
                    Message = "Bạn có lịch hẹn mới với bệnh nhân Nguyễn Văn Khoa vào ngày mai lúc 9:00 AM",
                    Type = "AppointmentConfirmed",
                    IsRead = false,
                    RelatedId = allAppointments.FirstOrDefault()?.Id,
                    CreatedAt = DateTime.Now.AddHours(-2)
                },
                new Notification
                {
                    UserId = doctor1User.Id,
                    Title = "Nhắc nhở lịch hẹn",
                    Message = "Bạn có lịch hẹn với bệnh nhân Đỗ Thị Phương vào hôm nay lúc 9:00 AM (còn 2 giờ)",
                    Type = "AppointmentReminder",
                    IsRead = false,
                    RelatedId = allAppointments.Skip(5).FirstOrDefault()?.Id,
                    CreatedAt = DateTime.Now.AddMinutes(-30)
                },
                new Notification
                {
                    UserId = doctor1User.Id,
                    Title = "Bệnh án đã tạo",
                    Message = "Bệnh án cho bệnh nhân Hoàng Văn Nam đã được tạo thành công",
                    Type = "MedicalRecordCreated",
                    IsRead = true,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },

                // Notifications for patient1
                new Notification
                {
                    UserId = patient1User.Id,
                    Title = "Xác nhận lịch hẹn",
                    Message = "Lịch hẹn của bạn với BS. Nguyễn Văn An đã được xác nhận. Thời gian: Ngày mai 9:00 AM",
                    Type = "AppointmentConfirmed",
                    IsRead = false,
                    RelatedId = allAppointments.FirstOrDefault()?.Id,
                    CreatedAt = DateTime.Now.AddHours(-3)
                },
                new Notification
                {
                    UserId = patient1User.Id,
                    Title = "Nhắc nhở khám bệnh",
                    Message = "Bạn có lịch khám với BS. Nguyễn Văn An vào ngày mai lúc 9:00 AM. Vui lòng đến đúng giờ.",
                    Type = "AppointmentReminder",
                    IsRead = false,
                    RelatedId = allAppointments.FirstOrDefault()?.Id,
                    CreatedAt = DateTime.Now.AddMinutes(-45)
                },
                new Notification
                {
                    UserId = patient1User.Id,
                    Title = "Kết quả khám bệnh",
                    Message = "Bệnh án của bạn đã được cập nhật. Vui lòng xem chi tiết và làm theo hướng dẫn của bác sĩ.",
                    Type = "MedicalRecordCreated",
                    IsRead = true,
                    CreatedAt = DateTime.Now.AddDays(-2)
                },

                // Notifications for patient2
                new Notification
                {
                    UserId = patient2User.Id,
                    Title = "Lịch hẹn sắp tới",
                    Message = "Bạn có lịch hẹn với BS. Trần Thị Bình vào 3 ngày nữa lúc 10:00 AM",
                    Type = "AppointmentReminder",
                    IsRead = false,
                    RelatedId = allAppointments.Skip(1).FirstOrDefault()?.Id,
                    CreatedAt = DateTime.Now.AddHours(-1)
                },
                new Notification
                {
                    UserId = patient2User.Id,
                    Title = "Đơn thuốc mới",
                    Message = "Bác sĩ đã kê đơn thuốc cho bạn. Vui lòng đến nhà thuốc để nhận thuốc.",
                    Type = "PrescriptionCreated",
                    IsRead = false,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new Notification
                {
                    UserId = patient2User.Id,
                    Title = "Chào mừng đến WebYTE",
                    Message = "Cảm ơn bạn đã đăng ký tài khoản. Chúc bạn có trải nghiệm tốt với dịch vụ của chúng tôi!",
                    Type = "General",
                    IsRead = true,
                    CreatedAt = DateTime.Now.AddDays(-7)
                }
            };

            await context.Notifications.AddRangeAsync(sampleNotifications);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {sampleNotifications.Length} sample notifications");
        }

        Console.WriteLine("✅ Sample data seeded successfully!");
    }
}
