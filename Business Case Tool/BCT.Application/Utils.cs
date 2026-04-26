namespace BCT.Application;
internal static class Utils
{
    // Function to generate all quarter strings between two DateTime objects
    public static List<string> GetQuarterStrings(DateTime startDate, DateTime endDate)
    {
        List<string> quarters = new List<string>();

        // Starting year and quarter
        int startYear = startDate.Year;
        int startQuarter = GetQuarter(startDate);

        // Ending year and quarter
        int endYear = endDate.Year;
        int endQuarter = GetQuarter(endDate);

        // Iterate over years and quarters
        for (int year = startYear; year <= endYear; year++)
        {
            int startQtr = (year == startYear) ? startQuarter : 1;  // Starting quarter for the year
            int endQtr = (year == endYear) ? endQuarter : 4;        // Ending quarter for the year

            for (int quarter = startQtr; quarter <= endQtr; quarter++)
            {
                quarters.Add($"Q{quarter} {year}");
            }
        }

        return quarters;
    }

    // Function to determine the quarter of a given DateTime
    public static int GetQuarter(DateTime date)
    {
        int month = date.Month;
        if (month >= 1 && month <= 3) return 1;  // Q1
        if (month >= 4 && month <= 6) return 2;  // Q2
        if (month >= 7 && month <= 9) return 3;  // Q3
        return 4;  // Q4
    }
}
