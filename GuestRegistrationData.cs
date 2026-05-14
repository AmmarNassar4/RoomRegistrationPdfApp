namespace RoomRegistrationPdfApp;

public sealed class GuestRegistrationData
{
    public string RegNo { get; set; } = "";
    public string FolioNo { get; set; } = "";
    public string GuestCode { get; set; } = "";
    public string RoomNo { get; set; } = "";
    public string ConfirmationNo { get; set; } = "";
    public string ArrivalDate { get; set; } = "";
    public string ArrivalTime { get; set; } = "";
    public string DepartureDate { get; set; } = "";
    public string DepartureTime { get; set; } = "";
    public string MealPlan { get; set; } = "";
    public string RoomRate { get; set; } = "";
    public string Currency { get; set; } = "SAR";
    public string NoOfGuests { get; set; } = "";
    public string RoomType { get; set; } = "";
    public string AdvanceDeposit { get; set; } = "";
    public string GuestTitle { get; set; } = "";
    public string GuestName { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string LoyaltyNumber { get; set; } = "";
    public string GuestAddress { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telephone { get; set; } = "";
    public string BillingInstructions { get; set; } = "";
    public string ModeOfPayment { get; set; } = "";
    public string PassportNo { get; set; } = "";
    public string IdentityNumber { get; set; } = "";
    public string Nationality { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public string ReceptionistName { get; set; } = "";
    public DateTime PrintDateTime { get; set; } = DateTime.Now;
    public string? GuestSignatureImageDataUrl { get; set; }
    public string? GuestGateConsentPdfPath { get; set; }
}
