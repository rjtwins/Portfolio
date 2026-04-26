using BCT.Domain.Entities;

namespace BCT.Domain;

public sealed class Calculation : ICalculation
{
    public Calculation()
    {

    }

    public List<DoubleValue> CalculateProjectValues(Project project, List<DoubleValue> storedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var calculatedAttributes = Configuration.Project.Attributes.Where(x => x.Calculated).ToList();

        result.AddRange(InvestmentCAPEX(storedValues, startYear, horizon));
        result.AddRange(NieuweKosten(storedValues, startYear, horizon));
        result.AddRange(VermedenKosten(storedValues, startYear, horizon));
        result.AddRange(Inkomsten(storedValues, startYear, horizon));

        //Takes only previously calculated values
        result.AddRange(KostenOPEX(result, startYear, horizon));
        result.AddRange(Kasstroom(result, startYear, horizon));
        result.AddRange(CumulatieveKasstroom(result, startYear, horizon));

        //Takes all values
        result.AddRange(VerdisconteerdeKasstroom(result.Union(storedValues).ToList(), startYear, horizon));

        //Takes only previously calculated values
        result.AddRange(CumulatieveVerdisconteerdeKasstroom(result, startYear, horizon));
        result.AddRange(Payback(result, startYear, horizon, project.InterestEnabled));
        result.Add(ROI(result.Union(storedValues).ToList(), startYear, horizon));

        result.Add(TotalIncome(result));
        result.Add(TotalInvestment(result));
        result.Add(TotalCost(result));
        result.Add(TotalResult(result, startYear + horizon - 1, project.InterestEnabled));

        return result;
    }

    private List<DoubleValue> InvestmentCAPEX(List<DoubleValue> storedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var newInvestments = storedValues
            .Where(x => x.Key == "Nieuwe investeringen")
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();


        var avoidedInvestments = storedValues
            .Where(x => x.Key == "Vermeden investeringen")
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var newInvestmentResidualValues = storedValues.Where(x => x.Key == "Nieuwe investeringen Risidual");
        var avoidedInvestmentResidualValues = storedValues.Where(x => x.Key == "Vermeden investeringen Risidual");

        var projectId = newInvestments.First().ProjectId;

        for (int i = startYear; i < startYear + horizon; i++)
        {
            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Investeringen (CAPEX)",
                Year = i,
                Value = newInvestments.Where(x => x.Year == i).Sum(x => x.Value) + (avoidedInvestments.Where(x => x.Year == i).Sum(x => x.Value) * -1)
            };

            result.Add(calculated);
        }

        var residual = new DoubleValue()
        {
            ProjectId = projectId,
            Key = "Investeringen (CAPEX) Risidual",
            Value = newInvestmentResidualValues.Sum(x => x.Value) + (avoidedInvestmentResidualValues.Sum(x => x.Value) * -1)
        };

        result.Add(residual);

        return result;
    }

    private List<DoubleValue> NieuweKosten(List<DoubleValue> storedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var input = storedValues
            .Where(x =>
                x.Key == "ICT hardware"
                || x.Key == "ICT software"
                || x.Key == "ICT licenties"
                || x.Key == "Materieel"
                || x.Key == "Materiaal / verbruiksmiddelen"
                || x.Key == "Personeel"
                || x.Key == "Energie"
                || x.Key == "Overig"
                )
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var projectId = input.First().ProjectId;

        for (int i = startYear; i < startYear + horizon; i++)
        {
            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Nieuwe kosten",
                Year = i,
                Value = input.Where(x => x.Year == i).Sum(x => x.Value)
            };

            result.Add(calculated);
        }

        return result;
    }

    private List<DoubleValue> VermedenKosten(List<DoubleValue> storedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var input = storedValues
            .Where(x =>
                x.Key == "Materieel2"
                || x.Key == "Personeel2"
                || x.Key == "Energie2"
                || x.Key == "Overig2"
                )
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var projectId = input.First().ProjectId;

        for (int i = startYear; i < startYear + horizon; i++)
        {
            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Vermeden kosten",
                Year = i,
                Value = input.Where(x => x.Year == i).Sum(x => x.Value)
            };

            result.Add(calculated);
        }

        return result;
    }

    private List<DoubleValue> KostenOPEX(List<DoubleValue> calculatedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var newCosts = calculatedValues
            .Where(x => x.Key == "Nieuwe kosten")
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var avoidedCosts = calculatedValues
            .Where(x => x.Key == "Vermeden kosten")
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var projectId = newCosts.First().ProjectId;

        for (int i = startYear; i < startYear + horizon; i++)
        {
            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Kosten (OPEX)",
                Year = i,
                Value = newCosts.Where(x => x.Year == i).Sum(x => x.Value) + (avoidedCosts.Where(x => x.Year == i).Sum(x => x.Value) * -1)
            };

            result.Add(calculated);
        }

        return result;
    }

    private List<DoubleValue> Inkomsten(List<DoubleValue> storedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var extraIncome = storedValues
            .Where(x => x.Key == "Extra inkomsten")
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var lostIncome = storedValues
            .Where(x => x.Key == "Gederfde inkomsten")
            .Where(x => x.Year >= startYear && x.Year < startYear + horizon)
            .ToList();

        var projectId = extraIncome.First().ProjectId;

        for (int i = startYear; i < startYear + horizon; i++)
        {
            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Inkomsten",
                Year = i,
                Value = extraIncome.Where(x => x.Year == i).Sum(x => x.Value) + (lostIncome.Where(x => x.Year == i).Sum(x => x.Value) * -1)
            };

            result.Add(calculated);
        }

        return result;
    }

    private List<DoubleValue> Kasstroom(List<DoubleValue> calculatedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var projectId = calculatedValues.First().ProjectId;

        //We should not need to worry about years here because we are getting the values from the calculated values
        var investeringenCapex = calculatedValues.Where(x => x.Key == "Investeringen (CAPEX)");
        var kostenOPEX = calculatedValues.Where(x => x.Key == "Kosten (OPEX)");
        var inkomsten = calculatedValues.Where(x => x.Key == "Inkomsten");

        for (int i = startYear; i < startYear + horizon; i++)
        {
            //Defined as Inkomsten in a year - CAPEX in year - OPEX in year.
            var value = inkomsten.Single(x => x.Year == i).Value - (kostenOPEX.Single(x => x.Year == i).Value + investeringenCapex.Single(x => x.Year == i).Value);

            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Kasstroom",
                Year = i,
                Value = value
            };

            result.Add(calculated);
        }

        var risudual_capex = calculatedValues.Single(x => x.Key == "Investeringen (CAPEX) Risidual").Value;

        var residual = new DoubleValue()
        {
            ProjectId = projectId,
            Key = "Kasstroom Risidual",
            Value = risudual_capex,
        };

        result.Add(residual);

        return result;
    }

    private List<DoubleValue> CumulatieveKasstroom(List<DoubleValue> calculatedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var projectId = calculatedValues.First().ProjectId;

        //We should not need to worry about years here because we are getting the values from the calculated values
        var kasstroom = calculatedValues.Where(x => x.Key == "Kasstroom");

        for (int i = startYear; i < startYear + horizon; i++)
        {
            //Defined as the sum of all kasstroom values up to the current year.
            var value = kasstroom.Where(x => x.Year <= i).Sum(x => x.Value);

            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Cumulatieve kasstroom",
                Year = i,
                Value = value
            };

            result.Add(calculated);
        }

        var residual = new DoubleValue()
        {
            ProjectId = projectId,
            Key = "Cumulatieve kasstroom Risidual",
            Value = kasstroom.Sum(x => x.Value) + calculatedValues.Single(x => x.Key == "Kasstroom Risidual").Value
        };

        result.Add(residual);

        return result;
    }

    private List<DoubleValue> VerdisconteerdeKasstroom(List<DoubleValue> AllValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var projectId = AllValues.First().ProjectId;

        var kasstroom = AllValues.Where(x => x.Key == "Kasstroom").ToList();
        var interest = AllValues.Single(x => x.Key == "Interest");

        for (int i = startYear; i < startYear + horizon; i++)
        {
            //Defined as the Kasstroom value divided by (1 + interest)^(year - startYear)
            var value = kasstroom.Single(x => x.Year == i).Value / Math.Pow(1 + interest.Value, i - startYear);

            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Verdisconteerde kasstroom",
                Year = i,
                Value = value
            };

            result.Add(calculated);
        }

        var residual = new DoubleValue()
        {
            ProjectId = projectId,
            Key = "Verdisconteerde kasstroom Risidual",
            Value = AllValues.Single(x => x.Key == "Kasstroom Risidual").Value / Math.Pow(1 + interest.Value, horizon)
        };

        result.Add(residual);

        return result;
    }

    private List<DoubleValue> CumulatieveVerdisconteerdeKasstroom(List<DoubleValue> CalculatedValues, int startYear, int horizon)
    {
        var result = new List<DoubleValue>();
        var projectId = CalculatedValues.First().ProjectId;

        var verdisconteerdeKasstroom = CalculatedValues.Where(x => x.Key == "Verdisconteerde kasstroom");

        for (int i = startYear; i < startYear + horizon; i++)
        {
            //Defined as the sum of all kasstroom values up to the current year.
            var value = verdisconteerdeKasstroom.Where(x => x.Year <= i).Sum(x => x.Value);

            var calculated = new DoubleValue()
            {
                ProjectId = projectId,
                Key = "Cumulatieve verdisconteerde kasstroom",
                Year = i,
                Value = value
            };

            result.Add(calculated);
        }

        var residual = new DoubleValue()
        {
            ProjectId = projectId,
            Key = "Cumulatieve verdisconteerde kasstroom Risidual",
            Value = verdisconteerdeKasstroom.Sum(x => x.Value) + CalculatedValues.Single(x => x.Key == "Verdisconteerde kasstroom Risidual").Value
        };

        result.Add(residual);

        return result;
    }

    private List<DoubleValue> Payback(List<DoubleValue> CalculatedValues, int startYear, int horizon, bool interestEnabled)
    {
        var result = new List<DoubleValue>();
        var projectId = CalculatedValues.First().ProjectId;

        List<DoubleValue> valueSeries = new();

        if (interestEnabled)
            valueSeries = CalculatedValues
                .Where(x => x.Key == "Cumulatieve verdisconteerde kasstroom")
                .OrderBy(x => x.Year)
                .ToList();
        else
            valueSeries = CalculatedValues
                .Where(x => x.Key == "Cumulatieve kasstroom")
                .OrderBy(x => x.Year)
                .ToList();

        var positiveYear = valueSeries
                .FirstOrDefault(x => x.Value > 0)?.Year ?? (startYear - 1);

        var lastNegValue = valueSeries.FirstOrDefault(x => x.Year == positiveYear - 1)?.Value ?? 0;
        var firstPosValue = valueSeries.FirstOrDefault(x => x.Year == positiveYear)?.Value ?? 0;

        var dif = lastNegValue - firstPosValue;
        var fraction = lastNegValue / dif;

        if (!double.IsFinite(fraction))
            fraction = 0;

        var calculated = new DoubleValue()
        {
            ProjectId = projectId,
            Key = "Payback",
            Value = (positiveYear - startYear) + fraction
        };

        result.Add(calculated);
        return result;
    }

    private DoubleValue ROI(List<DoubleValue> AllValues, int startYear, int horizon)
    {
        var cumulatieveKasstroomResultaat = AllValues.Single(x => x.Key == "Cumulatieve kasstroom Risidual").Value;
        var newInvestmentStartHorizon = AllValues
            .Where(x => x.Key == "Nieuwe investeringen")
            .Where(x => x.Year == startYear)
            .Single().Value;

        double result = 0d;

        if (newInvestmentStartHorizon < 1)
            result = 0d;
        else
            result = cumulatieveKasstroomResultaat / newInvestmentStartHorizon;

        if (!double.IsFinite(result))
            result = 0;

        return new DoubleValue()
        {
            Key = "ROI",
            ProjectId = AllValues.First().ProjectId,
            Value = result
        };
    }

    public List<StringValue> GetAdditionalCriteria(List<BoolValue> enabled, List<DoubleValue> weights, List<StringValue> texts)
    {
        var enabledKeys = enabled.Where(x => x.Value == true).Select(x => x.Key.Replace("Enabled", "")).ToList();
        var orderedKeys = weights.OrderByDescending(x => x.Value).Select(x => x.Key.Replace("Weight", "")).ToList();

        var orderedEnabledStringValues = texts
            .Where(x => enabledKeys.Contains(x.Key))
            .OrderBy(x => orderedKeys.IndexOf(x.Key)).ToList();

        return orderedEnabledStringValues;
    }

    public DoubleValue TotalInvestment(List<DoubleValue> calculatedValues)
    {
        var result = calculatedValues.Where(x => x.Key == "Investeringen (CAPEX)").Sum(x => x.Value);

        return new DoubleValue()
        {
            Key = "TotaalInvesteringen",
            ProjectId = calculatedValues.First().ProjectId,
            Value = result
        };
    }

    public DoubleValue TotalCost(List<DoubleValue> calculatedValues)
    {
        var result = calculatedValues.Where(x => x.Key == "Kosten (OPEX)").Sum(x => x.Value);

        return new DoubleValue()
        {
            Key = "TotaalKosten",
            ProjectId = calculatedValues.First().ProjectId,
            Value = result
        };
    }

    public DoubleValue TotalIncome(List<DoubleValue> calculatedValues)
    {
        var result = calculatedValues.Where(x => x.Key == "Inkomsten").Sum(x => x.Value);

        return new DoubleValue()
        {
            Key = "TotaalInkomsten",
            ProjectId = calculatedValues.First().ProjectId,
            Value = result
        };
    }

    public DoubleValue TotalResult(List<DoubleValue> calculatedValues, int endYear, bool interestEnabled)
    {
        //double income = calculatedValues.Single(x => x.Key == "TotaalInkomsten").Value;
        //double investment = calculatedValues.Single(x => x.Key == "TotaalInvesteringen").Value;
        //double cost = calculatedValues.Single(x => x.Key == "TotaalKosten").Value;

        double investmentResidual = calculatedValues.Single(x => x.Key == "Investeringen (CAPEX) Risidual").Value;
        var key = interestEnabled ? "Cumulatieve verdisconteerde kasstroom Risidual" : "Cumulatieve kasstroom Risidual";
        double cumulative = calculatedValues.Single(x => x.Key == key).Value;
        //double result = cumulative + investmentResidual;

        return new DoubleValue()
        {
            Key = "TotaalResultaat",
            ProjectId = calculatedValues.First().ProjectId,
            Value = cumulative
        };
    }
}
