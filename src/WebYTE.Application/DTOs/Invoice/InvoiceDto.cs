using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Invoice;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Thông tin bệnh nhân
    public string PatientName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string PatientAddress { get; set; } = string.Empty;
    
    // Thông tin bác sĩ
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    
    // Thông tin cuộc hẹn
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    
    // Chi phí
    public decimal ConsultationFee { get; set; } // Phí khám
    public decimal MedicationTotal { get; set; } // Tổng tiền thuốc
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Thông tin đơn thuốc
    public List<InvoiceMedicationDto> Medications { get; set; } = new();
    
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    
    // Thông tin ngân hàng để thanh toán
    public BankingInfoDto? BankingInfo { get; set; }
}

public class BankingInfoDto
{
    public string BankId { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}

public class InvoiceMedicationDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Instructions { get; set; }
}
