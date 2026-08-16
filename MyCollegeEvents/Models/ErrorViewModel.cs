using System.ComponentModel.DataAnnotations;

namespace MyCollegeEvents.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class Event
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "عنوان الفعالية مطلوب")]
        [Display(Name = "عنوان الفعالية")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف الفعالية مطلوب")]
        [Display(Name = "وصف الفعالية")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الفعالية مطلوب")]
        [Display(Name = "تاريخ الفعالية")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Display(Name = "منشئ الفعالية")]
        public string CreatedBy { get; set; } = "المشرفة";

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }

    public class Participant
    {
        public int ParticipantID { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم الجامعي مطلوب")]
        [Display(Name = "الرقم الجامعي")]
        public string UniversityID { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم مطلوب")]
        [Display(Name = "القسم")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب اختيار فعالية")]
        [Display(Name = "الفعالية")]
        public int EventID { get; set; }

        [Display(Name = "هل شاركت في فعاليات سابقة؟")]
        public bool AttendedBefore { get; set; }

        [Display(Name = "هل تريدين شهادة مشاركة؟")]
        public bool WantCertificate { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "موافقة المشرفة")]
        public bool Approved { get; set; } = false;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Event? Event { get; set; }
    }
}
