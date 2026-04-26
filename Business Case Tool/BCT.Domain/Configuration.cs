using static BCT.Domain.Const;

namespace BCT.Domain;

public static class Configuration
{
    public static class Project
    {
        public static HashSet<string> AdditionalCriteriaOptions = new()
        {
            "Verdienmodel",
            "Kwaliteit",
            "Onderhoud",
            "Digitaliseringgraad",
            "Werkomgeving",
            "Flexibiliteit",
        };

        public static HashSet<(string Key, AttributeType AttributeType, bool Calculated)> Attributes = new()
        {
            ("Baseline", AttributeType.String, false),
            ("Value", AttributeType.String, false),
            ("Risk", AttributeType.String, false),
            ("Conditionals", AttributeType.String, false),
            ("Means", AttributeType.String, false),

            ("VerdienmodelEnabled", AttributeType.Bool, false),
            ("Verdienmodel", AttributeType.String, false),
            ("VerdienmodelWeight", AttributeType.Double, false),

            ("KwaliteitEnabled", AttributeType.Bool, false),
            ("Kwaliteit", AttributeType.String, false),
            ("KwaliteitWeight", AttributeType.Double, false),

            ("OnderhoudEnabled", AttributeType.Bool, false),
            ("Onderhoud", AttributeType.String, false),
            ("OnderhoudWeight", AttributeType.Double, false),

            ("DigitaliseringgraadEnabled", AttributeType.Bool, false),
            ("Digitaliseringgraad", AttributeType.String, false),
            ("DigitaliseringgraadWeight", AttributeType.Double, false),

            ("WerkomgevingEnabled", AttributeType.Bool, false),
            ("Werkomgeving", AttributeType.String, false),
            ("WerkomgevingWeight", AttributeType.Double, false),

            ("FlexibiliteitEnabled", AttributeType.Bool, false),
            ("Flexibiliteit", AttributeType.String, false),
            ("FlexibiliteitWeight", AttributeType.Double, false),

            ("Interest", AttributeType.Double, false),

            ("Payback", AttributeType.Double, true),
            ("ROI", AttributeType.Double, true),

            ("Step1Notes", AttributeType.String, false),
            ("Step2Notes", AttributeType.String, false),
            ("Step3Notes", AttributeType.String, false),
            ("Step4Notes", AttributeType.String, false),
            ("Step4Evaluatie", AttributeType.String, false),

            //Residuals en cumulatieven:
            ("Investeringen (CAPEX) Risidual", AttributeType.Double, true),
            ("Nieuwe investeringen Risidual", AttributeType.Double, false),
            ("Vermeden investeringen Risidual", AttributeType.Double, false),
            ("Cumulatieve kasstroom Risidual", AttributeType.Double, true),
            ("Verdisconteerde kasstroom Risidual", AttributeType.Double, true),
            ("Cumulatieve verdisconteerde kasstroom Risidual", AttributeType.Double, true),
            ("Kasstroom Risidual", AttributeType.Double, true),

            //Totals:
            ("TotaalInvesteringen", AttributeType.Double, true),
            ("TotaalKosten", AttributeType.Double, true),
            ("TotaalInkomsten", AttributeType.Double, true),
            ("TotaalResultaat", AttributeType.Double, true),
        };

        public static HashSet<(string Key, bool Calculated)> OverTimeAttributes = new()
        {
            ("Investeringen (CAPEX)", true),
            ("Nieuwe investeringen", false),
            ("Vermeden investeringen", false),
            ("Kosten (OPEX)", true),
            ("Nieuwe kosten", true),
            ("ICT hardware", false),
            ("ICT software", false),
            ("Materieel", false),
            ("Materiaal / verbruiksmiddelen", false),
            ("Personeel", false),
            ("Energie", false),
            ("Overig", false),
            ("Vermeden kosten", true),
            ("Materieel2", false),
            ("Personeel2", false),
            ("Energie2", false),
            ("Overig2", false),
            ("Inkomsten", true),
            ("Extra inkomsten", false),
            ("Gederfde inkomsten", false),
            ("Kasstroom", true),
            ("Cumulatieve kasstroom", true),
            ("Verdisconteerde kasstroom", true),
            ("Cumulatieve verdisconteerde kasstroom", true),
        };

        public static Dictionary<string, string> OverTimeValueWizardMap = new()
        {
            { "Nieuwe investeringen", "NewInvestment" },
            { "Vermeden investeringen", "AvoidedInvestment" },
            { "ICT hardware", "ICTHardware" },
            { "ICT software", "ICTSoftware" },
            { "Materieel", "Equipment" },
            { "Materiaal / verbruiksmiddelen", "EquipmentUsage" },
            { "Personeel", "Personnel" },
            { "Energie", "Energy" },
            { "Overig", "Other" },
            { "Materieel2", "EquipmentOvoided" },
            { "Personeel2", "PersonnelOvoided" },
            { "Energie2", "EnergyOvoided" },
            { "Overig2", "OtherOvoided" },
            { "Extra inkomsten", "ExtraIncome" },
            { "Gederfde inkomsten", "LostIncome" },
        };

        public static HashSet<string> ProjectGridWizardCostCatagory = new ()
        {
            "ICT hardware",
            "ICT software",
            "Materieel",
            "Materiaal / verbruiksmiddelen",
            "Personeel",
            "Energie",
            "Overig",
        };

        public static HashSet<string> ProjectGridWizardAvoidedCostCatagory = new ()
        {
            "Materieel2",
            "Personeel2",
            "Energie2",
            "Overig2",
        };

        public static HashSet<string> SensitivityAttributesOptions = new()
        {
            "TotaalInvesteringen",
            "TotaalKosten",
            "TotaalInkomsten",
            "TotaalResultaat",
            "Payback"
        };

        public static HashSet<string> SensitivityAttributes = new ()
        {
            "Nieuwe investeringen",
            "Vermeden investeringen",
            "Materieel",
            "Materiaal / verbruiksmiddelen",
            "Personeel",
            "Energie",
            "Overig",
            "Materieel2",
            "Personeel2",
            "Energie2",
            "Overig2",
            "Extra inkomsten",
            "Gederfde inkomsten",
        };

        public static HashSet<string> SensitivityAttributesStartHorizon = new()
        {
            "Nieuwe investeringen",
            "Vermeden investeringen",
        };

        public static HashSet<string> SensitivityAttributesYearly = new()
        {
            "Materieel",
            "Materiaal / verbruiksmiddelen",
            "Personeel",
            "Energie",
            "Overig",
            "Materieel2",
            "Personeel2",
            "Energie2",
            "Overig2",
            "Extra inkomsten",
            "Gederfde inkomsten",
        };

        public static HashSet<string> GlobalTags = new()
        {
            "Verdienmodel",
            "Kwaliteit",
            "Onderhoud",
            "Digitaliseringgraad",
            "Werkomgeving",
            "Flexibiliteit"
        };
    }
}
