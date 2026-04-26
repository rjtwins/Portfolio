using static BCT.Domain.Const;

internal static class PresentationSettings
{
    //Moved to mouse on mouse off mode instaed of duration.
    internal static int HelpTextDuration = int.MaxValue;
    internal static int HelpTextDelay = 250;

    internal static Dictionary<string, string> AdditionalCriteriaLabels = new()
    {
        { "Verdienmodel", "Verdienmodel" },
        { "Kwaliteit", "Kwaliteit" },
        { "Onderhoud", "Onderhoud" },
        { "Digitaliseringgraad", "Digitaliseringgraad" },
        { "Werkomgeving", "Werkomgeving" },
        { "Flexibiliteit", "Flexibiliteit productie" },
    };

    //TODO: Take calculated flag from domain configuration instead here.
    internal static List<(string Key, string Label, bool Calculated, bool StrongText)> GridItems = new()
    {
        ("Investeringen (CAPEX)", "Investeringen (CAPEX)", true, true),
        ("Nieuwe investeringen", "Nieuwe investeringen", false, false),
        ("Vermeden investeringen", "Vermeden investeringen", false, false),
        ("Kosten (OPEX)", "Kosten (OPEX)", true, true),
        ("Nieuwe kosten", "Nieuwe kosten", true, true),
        ("ICT hardware", "ICT hardware", false, false),
        ("ICT software", "ICT software", false, false),
        ("Materieel", "Materieel", false, false),
        ("Materiaal / verbruiksmiddelen", "Materiaal / verbruiksmiddelen", false, false),
        ("Personeel", "Personeel", false, false),
        ("Energie", "Energie", false, false),
        ("Overig", "Overig", false, false),
        ("Vermeden kosten", "Vermeden kosten", true, true),
        ("Materieel2", "Materieel", false, false),
        ("Personeel2", "Personeel", false, false),
        ("Energie2", "Energie", false, false),
        ("Overig2", "Overig", false, false),
        ("Inkomsten", "Inkomsten", true, true),
        ("Extra inkomsten", "Extra inkomsten", false, false),
        ("Gederfde inkomsten", "Gederfde inkomsten", false, false),
        ("Kasstroom", "Kasstroom", true, true),
        ("Cumulatieve kasstroom", "Cumulatieve kasstroom" , true , false),
        ("Verdisconteerde kasstroom", "Verdisconteerde kasstroom" , true , true),
        ("Cumulatieve verdisconteerde kasstroom", "Cumulatieve verdisconteerde kasstroom" , true , false),
    };

    internal static Dictionary<string, string> PropertySelectorOptions = new()
    {
        {"Investeringen (CAPEX)", "Investeringen (CAPEX)" },
        {"Nieuwe investeringen", "Nieuwe investeringen" },
        {"Vermeden investeringen", "Vermeden investeringen" },
        {"Kosten (OPEX)", "Kosten (OPEX)" },
        {"Nieuwe kosten", "Nieuwe kosten" },
        {"ICT hardware", "ICT hardware kosten" },
        {"ICT software", "ICT software kosten" },
        {"Materieel", "Materieel kosten" },
        {"Materiaal / verbruiksmiddelen", "Materiaal / verbruiksmiddelen kosten" },
        {"Personeel", "Personeel kosten" },
        {"Energie", "Energie kosten" },
        {"Overig", "Overige kosten" },
        {"Vermeden kosten", "Vermeden kosten" },
        {"Materieel2", "Vermeden materieel kosten" },
        {"Personeel2", "Vermeden personeel kosten" },
        {"Energie2", "Vermeden energie kosten" },
        {"Overig2", "Vermeden overige kosten" },
        {"Inkomsten", "Inkomsten" },
        {"Extra inkomsten", "Extra inkomsten" },
        {"Gederfde inkomsten", "Gederfde inkomsten" },
        {"Kasstroom", "Kasstroom" },
        {"Cumulatieve kasstroom", "Cumulatieve kasstroom" },
        {"Verdisconteerde kasstroom", "Verdisconteerde kasstroom" },
        {"Cumulatieve verdisconteerde kasstroom", "Cumulatieve verdisconteerde kasstroom" },
    };

    public static List<string> GridInterestItems = new()
    {
        "Verdisconteerde kasstroom",
        "Cumulatieve verdisconteerde kasstroom"
    };

    internal static HashSet<string> HasResidual = new()
    {
        "Investeringen (CAPEX)",
        "Nieuwe investeringen",
        "Vermeden investeringen",
        "Kasstroom",
        "Cumulatieve kasstroom",
        "Verdisconteerde kasstroom",
        "Cumulatieve verdisconteerde kasstroom",
    };

    internal static Dictionary<string, string[]> GridHierarchy = new()
    {
        { "Investeringen (CAPEX)", new string[]{ "Nieuwe investeringen", "Vermeden investeringen" } },
        { "Kosten (OPEX)", new string[]{ "Nieuwe kosten", "Vermeden kosten", "ICT hardware", "ICT software", "Materieel", "Materiaal / verbruiksmiddelen", "Personeel", "Energie", "Overig", "Materieel2", "Personeel2", "Energie2", "Overig2" } },
        { "Nieuwe kosten", new string[]{ "ICT hardware", "ICT software", "Materieel", "Materiaal / verbruiksmiddelen", "Personeel", "Energie", "Overig" } },
        { "Vermeden kosten", new string[] { "Materieel2", "Personeel2", "Energie2", "Overig2" } },
        { "Inkomsten", new string[] { "Extra inkomsten", "Gederfde inkomsten" } },
        { "Kasstroom", new string[] { "Cumulatieve kasstroom", "Verdisconteerde kasstroom", "Cumulatieve verdisconteerde kasstroom" } }
    };

    internal static HashSet<string> Highlight = new()
    {
        "Investeringen (CAPEX)",
        "Kosten (OPEX)",
        "Inkomsten",
        "Kasstroom",
        "Verdisconteerde kasstroom",
    };

    internal static HashSet<string> HighlightSecondary = new()
    {
        "Nieuwe kosten",
        "Vermeden kosten",
    };

    internal static Dictionary<string, string> AttributeLabels = new()
    {
        {"Baseline", "Baseline" },
        {"Value", "Value" },
        {"Risk", "Risk" },
        {"Conditionals", "Conditionals" },
        {"Means", "Means" },

        {"VerdienmodelEnabled", "" },
        {"Verdienmodel", "Verdienmodel" },
        {"VerdienmodelWeight", "" },

        {"KwaliteitEnabled", "" },
        {"Kwaliteit", "Kwaliteit" },
        {"KwaliteitWeight", "" },

        {"OnderhoudEnabled", "" },
        {"Onderhoud", "Onderhoud" },
        {"OnderhoudWeight", "" },

        {"DigitaliseringgraadEnabled", "" },
        {"Digitaliseringgraad", "Digitaliseringgraad" },
        {"DigitaliseringgraadWeight", "" },

        {"WerkomgevingEnabled", "" },
        {"Werkomgeving", "Werkomgeving" },
        {"WerkomgevingWeight", "" },

        {"FlexibiliteitEnabled", "" },
        {"Flexibiliteit", "Flexibiliteit productie" },
        {"FlexibiliteitWeight", "" },

        {"Interest", "Rentevoet" },

        {"Payback", "Terugverdientijd" },
        {"ROI", "Return on investment" },

        {"Step1Notes", "" },
        {"Step2Notes", "" },
        {"Step3Notes", "" },
        {"Step4Notes", "" },
        {"Step4Evaluatie", "" },

        //Residuals:
        {"Investeringen (CAPEX) Risidual", "" },
        {"Nieuwe investeringen Risidual", "" },
        {"Vermeden investeringen Risidual", "" },
        {"Cumulatieve kasstroom Risidual", "" },
        {"Verdisconteerde kasstroom Risidual", "" },
        {"Cumulatieve verdisconteerde kasstroom Risidual", "" },
        {"Kasstroom Risidual", "" },

        //Totals:
        {"TotaalInvesteringen", "Totaal investeringen" },
        {"TotaalKosten", "Totaal kosten" },
        {"TotaalInkomsten", "Totaal inkomsten" },
        {"TotaalResultaat", "Totaal resultaat" },

        //Grid items:
        {"Investeringen (CAPEX)", "Investeringen (CAPEX)" },
        {"Nieuwe investeringen", "Nieuwe investeringen" },
        {"Vermeden investeringen", "Vermeden investeringen" },
        {"Kosten (OPEX)", "Kosten (OPEX)" },
        {"Nieuwe kosten", "Nieuwe kosten" },
        {"ICT hardware", "ICT hardware" },
        {"ICT software", "ICT software" },
        {"Materieel", "Materieel kosten" },
        {"Materiaal / verbruiksmiddelen", "Materiaal / verbruiksmiddelen kosten" },
        {"Personeel", "Personeel kosten" },
        {"Energie", "Energiekosten" },
        {"Overig", "Overige kosten" },
        {"Vermeden kosten", "Vermeden kosten" },
        {"Materieel2", "Vermeden materieel kosten" },
        {"Personeel2", "Vermeden personeel kosten" },
        {"Energie2", "Vermeden energiekosten" },
        {"Overig2", "Vermeden overige kosten" },
        {"Inkomsten", "Inkomsten" },
        {"Extra inkomsten", "Extra inkomsten" },
        {"Gederfde inkomsten", "Gederfde inkomsten" },
        {"Kasstroom", "Kasstroom" },
        {"Cumulatieve kasstroom", "Cumulatieve kasstroom" },
        {"Verdisconteerde kasstroom", "Verdisconteerde kasstroom" },
        {"Cumulatieve verdisconteerde kasstroom", "Cumulatieve verdisconteerde kasstroom" },
    };

    public enum SidebarMode
    {
        Planner,
        Charting
    }

    public static Dictionary<int, string> HarveyBalls = new()
    {
        { 0, "020-white" },
        { 1, "040-white" },
        { 2, "060-white" },
        { 3, "080-white" },
        { 4, "100-white" },
    };
}