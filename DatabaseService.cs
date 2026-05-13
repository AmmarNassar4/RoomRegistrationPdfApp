using Microsoft.Data.SqlClient;
using System.Data;

namespace RoomRegistrationPdfApp;

public sealed class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<GuestRegistrationData>> GetGuestRegistrationsByRoomAsync(string roomNo, string fallbackReceptionistName)
    {
        var registrations = new List<GuestRegistrationData>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(SqlText, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 120
        };
        command.Parameters.Add(new SqlParameter("@RoomNo", SqlDbType.VarChar, 20) { Value = roomNo.Trim() });

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var receptionistFromDb = DbValueFormatter.Text(reader["ReceptionistName"]);

            registrations.Add(new GuestRegistrationData
            {
                RegNo = DbValueFormatter.Text(reader["RegNo"]),
                FolioNo = DbValueFormatter.Text(reader["FolioNo"]),
                GuestCode = DbValueFormatter.Text(reader["GuestCode"]),
                RoomNo = DbValueFormatter.Text(reader["RoomNo"]),
                ConfirmationNo = DbValueFormatter.Text(reader["ConfirmationNo"]),
                ArrivalDate = DbValueFormatter.DateYmd(reader["ArrivalDate"]),
                ArrivalTime = DbValueFormatter.TimeHm(reader["ArrivalTime"]),
                DepartureDate = DbValueFormatter.DateYmd(reader["DepartureDate"]),
                DepartureTime = DbValueFormatter.TimeHm(reader["DepartureTime"]),
                MealPlan = DbValueFormatter.Text(reader["MealPlan"]),
                RoomRate = DbValueFormatter.Number(reader["RoomRate"], 2),
                Currency = DbValueFormatter.Text(reader["Currency"]),
                NoOfGuests = DbValueFormatter.Text(reader["NoOfGuests"]),
                RoomType = DbValueFormatter.Text(reader["RoomType"]),
                AdvanceDeposit = DbValueFormatter.Number(reader["AdvanceDeposit"], 2),
                GuestTitle = DbValueFormatter.Text(reader["GuestTitle"]),
                GuestName = BuildGuestName(reader),
                CompanyName = DbValueFormatter.Text(reader["CompanyName"]),
                LoyaltyNumber = DbValueFormatter.Text(reader["LoyaltyNumber"]),
                GuestAddress = BuildAddress(reader),
                Email = DbValueFormatter.Text(reader["Email"]),
                Telephone = DbValueFormatter.Text(reader["Telephone"]),
                BillingInstructions = DbValueFormatter.Text(reader["BillingInstructions"]),
                ModeOfPayment = DbValueFormatter.Text(reader["ModeOfPayment"]),
                PassportNo = DbValueFormatter.Text(reader["PassportNo"]),
                Nationality = DbValueFormatter.Text(reader["Nationality"]),
                DateOfBirth = DbValueFormatter.DateYmd(reader["DateOfBirth"]),
                ReceptionistName = string.IsNullOrWhiteSpace(receptionistFromDb) ? fallbackReceptionistName : receptionistFromDb,
                PrintDateTime = DateTime.Now
            });
        }

        return registrations;
    }

    private static string BuildGuestName(SqlDataReader reader)
    {
        var title = DbValueFormatter.Text(reader["GuestTitle"]);
        var first = DbValueFormatter.Text(reader["FirstName"]);
        var middle = DbValueFormatter.Text(reader["MiddleName"]);
        var last = DbValueFormatter.Text(reader["LastName"]);

        var name = string.Join(" ", new[] { first, middle, last }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(name))
            return $"{title}. {name}".Replace("..", ".").Trim();

        return string.IsNullOrWhiteSpace(name) ? title : name;
    }

    private static string BuildAddress(SqlDataReader reader)
    {
        var address = DbValueFormatter.Text(reader["GuestAddress"]);
        var city = DbValueFormatter.Text(reader["GuestCity"]);
        var zip = DbValueFormatter.Text(reader["GuestZip"]);
        var country = DbValueFormatter.Text(reader["GuestCountry"]);

        return string.Join(" ", new[] { address, city, zip, country }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }

    private const string SqlText = @"

;WITH RoomRegistrations AS
(
    SELECT
        M.[REG.#] AS RegNo,
        MIN(M.[FOLIO #]) AS FolioNo
    FROM PMS.VIEW_FOM_OCCUPANCY_MAIN M
    WHERE CONVERT(VARCHAR(20), M.[ROOM #]) = @RoomNo
    GROUP BY M.[REG.#]
)
SELECT
    COALESCE(CONVERT(VARCHAR(20), D.[ROOM#]), CONVERT(VARCHAR(20), M.[ROOM #])) AS RoomNo,
    COALESCE(NULLIF(CONVERT(VARCHAR(50), D.[ROOM TYPE LONG NAME]), ''), NULLIF(CONVERT(VARCHAR(50), M.[ROOM TYPE LONG NAME]), ''), CONVERT(VARCHAR(50), D.[ROOM TYPE])) AS RoomType,
    RR.RegNo AS RegNo,
    RR.FolioNo AS FolioNo,
    D.[GUEST CODE] AS GuestCode,
    D.[GUEST TITLE] AS GuestTitle,
    D.[LAST NAME] AS LastName,
    D.[MIDDLE NAME] AS MiddleName,
    D.[FIRST NAME] AS FirstName,
    D.[COMPANY NAME] AS CompanyName,
    D.[ADDRESS] AS GuestAddress,
    D.[CITY] AS GuestCity,
    D.[ZIP COD] AS GuestZip,
    D.[STATE] AS GuestState,
    D.[COUNTRY] AS GuestCountry,
    D.[TELEPHONE NO] AS Telephone,
    D.[ARRIVAL DATE] AS ArrivalDate,
    D.[ARRIVAL TIME] AS ArrivalTime,
    D.[DEPARTURE DATE] AS DepartureDate,
    D.[DEPARTURE TIME] AS DepartureTime,
    D.[RES.#] AS ConfirmationNo,
    COALESCE(M.[MEMBERSHIP ID], '') AS LoyaltyNumber,
    D.[EMAIL] AS Email,
    M.[PAYMENT MODE DESCRIPTION] AS ModeOfPayment,
    M.[BILLING INSTRUCTION DESCRIPTION] AS BillingInstructions,
    D.[NATION LONG NAME] AS Nationality,
    D.[PASSPORT #] AS PassportNo,
    D.[DATE OF BIRTH] AS DateOfBirth,
    COALESCE(NULLIF(CONVERT(VARCHAR(200), M.[PLAN LONG NAME]), ''), NULLIF(CONVERT(VARCHAR(200), M.[PLAN SHORT NAME]), ''), CONVERT(VARCHAR(200), M.[LOCAL CUR.PLAN])) AS MealPlan,
    COALESCE(
    MR.RoomRate,
    COALESCE(M.[LOCAL CUR.TARIFF], 0) + COALESCE(M.[LOCAL CUR.PLAN], 0)
) AS RoomRate,
    M.[CURRENCY] AS Currency,
    M.[TOTAL PAX] AS NoOfGuests,
    COALESCE(ADV.[ADVANCE AMOUNT], 0) AS AdvanceDeposit,
    COALESCE(NULLIF(CONVERT(VARCHAR(100), C.USERID), ''), CONVERT(VARCHAR(100), D.[USER ID])) AS ReceptionistName
FROM RoomRegistrations RR
INNER JOIN PMS.VIEW_FOM_OCCUPANCY_MAIN M
    ON M.[REG.#] = RR.RegNo
   AND M.[FOLIO #] = RR.FolioNo
OUTER APPLY
(
    SELECT TOP (1)
        D1.*
    FROM PMS.VIEW_FOM_OCCUPANCY_DETAILS D1
    WHERE D1.[REG.#] = RR.RegNo
    ORDER BY D1.[REG.#] DESC
) D
LEFT JOIN PMS.VIEW_FOM_OCCUPANCY_ADVANCE ADV
    ON ADV.[REG.#] = RR.RegNo
OUTER APPLY
(
    SELECT TOP (1)
        COALESCE(X.[MULTI DAY RATE], 0)
        + COALESCE(X.[MULTI DAY PLAN RATE], 0) AS RoomRate
    FROM PMS.VIEW_FOM_OCCUPANCY_MULTIRATE X
    WHERE X.[REG.#] = RR.RegNo
      AND X.[FOLIO #] = RR.FolioNo
    ORDER BY
        CASE WHEN X.[PRINT REG.CARD FLAG] = 1 THEN 0 ELSE 1 END,
        X.[MULTI DAY DATE]
) MR
OUTER APPLY
(
    SELECT TOP (1)
        C1.USERID
    FROM PMS.FMOCCTBL C1
    WHERE C1.REGNUB = RR.RegNo
    ORDER BY C1.REGNUB DESC
) C
ORDER BY RR.RegNo DESC;

";
}
