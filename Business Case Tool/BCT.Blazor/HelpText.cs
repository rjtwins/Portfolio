namespace BCT.Blazor;

public static class HelpText
{
    public static class CompanyOverview
    {
        public static string LockHelpText = @"Alleen personen uit uw team hebben toegang tot de gegevens van uw organisatie.<br \>
            Gegevens worden veilig opgeslagen volgens hoogwaardige beveiligingsstandaarden en nooit met derden gedeeld.";
    }
    public static class Step1
    {
        public static string IntroText = @"<p><strong>Stap 2: Project<br /></strong>Hier maak je een nieuw project aan of bouw je voort op een bestaand project.<br />Je geeft de belangrijkste doelen en informatie op.<br />Dit is belangrijk omdat een goed gedefinieerd project de basis vormt voor een succesvolle investering.<br />Door duidelijke doelen te stellen, kun je gericht werken aan het behalen van resultaten.</p>";
        public static string Name = @"Geef je project een duidelijke en herkenbare naam om het gemakkelijk te kunnen identificeren.<br \>Voorbeeld: “IoT Implementatie voor Productieoptimalisatie” (Implementeren van een IoT-systeem om productieprocessen te optimaliseren)";
        public static string Tags = @"Tags: Voeg relevante tags toe om je project te categoriseren en later eenvoudig terug te vinden.<br \>Voorbeeld: Tags zoals “Digitale Innovatie”, “IoT”, “Productieoptimalisatie”";
        public static string Description = @"Definieer project: Beschrijf de belangrijkste doelen, scope en verwachte resultaten van het project.<br \>Dit helpt bij het richten van inspanningen en middelen.<br \>Voorbeeld: “Het doel is om een IoT-systeem te implementeren dat realtime data verzamelt en analyseert om de efficiëntie van productieprocessen te verbeteren.”";
        public static string Baseline = @"Baseline: Stel een referentiepunt vast voor het project, zoals huidige prestaties of omstandigheden, om toekomstige voortgang te meten.<br \>Voorbeeld: “Huidige productie-efficiëntie is 75%.”";
        public static string Value = @"Beoogde waarde: Wat wil je bereiken met dit project? <br />Beschrijf het concrete doel, bijvoorbeeld tijdsbesparing of kwaliteitsverbetering.<br />Denk aan: ""Supportvraag terugbrengen naar 8u per week"" of ""Foutreductie van 30% in orderverwerking""";
        public static string Conditionals = @"Randvoorwaarden: Identificeer de voorwaarden en beperkingen waarbinnen het project moet worden uitgevoerd, zoals budget, tijd en middelen.<br \>Voorbeeld: “Budget van €30.000, voltooiing binnen 4 maanden, gebruik van bestaande IT-infrastructuur.”";
        public static string Means = @"Middelen: Specificeer de benodigde middelen voor het project, zoals personeel, apparatuur en budget.<br \>Voorbeeld: “2 IT-specialisten, 1 projectmanager, IoT-apparatuur, en een budget van €30.000.”";
    }

    public static class Step2
    {
        public static string IntroText = @"<div><strong>Stap 3: Specificatie</strong></div>
            <div><strong>Welke aspecten vind je belangrijk bij je afweging?</strong></div>
            <div>Naast de financi&euml;le onderbouwing kun je ook andere aspecten meewegen in je besluit, zoals flexibiliteit of impact op de werkomgeving.</div>
            <div>Hier geef je aan welke van deze criteria voor jou relevant zijn, en hoe zwaar ze voor jou meewegen in de beoordeling van dit project.​</div>
            <div>De weging van deze criteria helpt je om bij de evaluatie breder te kijken dan alleen naar kosten en baten.</div>
            <div>Deze keuzes hebben geen invloed op de financi&euml;le berekening, maar worden op de uitkomstpagina getoond om je een completer beeld van het project te geven.</div>";

        public static string DecisionCriteria = @"Besliscriteria zijn de factoren die bedrijven gebruiken om investeringsbeslissingen te evalueren en te maken.<br \>Deze criteria helpen bij het beoordelen van de waarde en impact van een potentiële investering";
        public static string Verdienmodel = @"Bijdrage aan verdien/service model en/of betere financiële beheersing.<br \>Door bijvoorbeeld sneller de machines te kunnen instellen voor productie, waardoor het percentage gebruik verhoogt wordt.";
        public static string Kwaliteit = @"Verbetert de kwaliteit van producten of processen?<br \>Bijvoorbeeld: minder uitval of hogere klanttevredenheid.";
        public static string Onderhoud = @"Vermindert het onderhoud of maakt het onderhoud voorspelbaarder?<br \>Bijvoorbeeld: inzet sensoren voor voorspellend onderhoud.";
        public static string Digitaliseringgraad = @"Hoeveel digitalisering brengt dit project?<br \>Bijvoorbeeld: van papieren naar digitale werkorders.";
        public static string Werkomgeving = @"Heeft dit effect op veiligheid, ergonomie of werkplezier?<br \>Bijvoorbeeld: minder repeterend werk.";
        public static string Flexibiliteit = @"Maakt het je productie flexibeler?<br \>Bijvoorbeeld: sneller omschakelen tussen kleine series.";
    }

    public static class Step3
    {
        public static Dictionary<string, string> GridItems = new()
        {
            { 
                "Investeringen (CAPEX)",
                @"Tussenresultaat: saldo van nieuwe en vermeden investeringen als gevolg van dit project, per jaar.<br \>(EUR x 1000)"
            },
            {
                "Nieuwe investeringen",
                @"Vul in: totale investeringen die voor dit project gedaan moeten worden, per jaar.<br \>Denk aan aanschaf van apparatuur, bouwkundige aanpassingen, aanpassing van kabels en leidingen, etc.<br \>(EUR x 1000)"
            },
            {
                "Vermeden investeringen",
                @"Vul in: totale investeringen die vermeden worden als dit project wordt uitgevoerd, per jaar.<br \>Denk aan te vervallen vervangingsinvesteringen in oude apparatuur, etc.<br \>(EUR x 1000)"
            },
            {
                "Kosten (OPEX)",
                @"Tussenresultaat: saldo van nieuwe en vermeden kosten als gevolg van dit project, per jaar.<br \>(EUR x 1000)"
            },
            {
                "Nieuwe kosten",
                @"Tussenresultaat: totaal aan nieuwe kosten die gemaakt worden als gevolg van dit project, per jaar.<br \>(EUR x 1000)"
            },
            {
                "ICT hardware",
                @"Vul in: kosten per jaar voor de nieuwe ICT hardware.<br \>Denk aan kosten voor onderhoudscontracten, huur of lease van apparatuur, etc.<br \>(EUR x 1000)"
            },
            {
                "ICT software",
                @"Vul in: kosten per jaar voor de nieuwe ICT software.<br \>Denk aan licentiekosten, servicecontracten, etc.<br \>(EUR x 1000)"
            },
            {
                "Materieel",
                @"Vul in: kosten per jaar voor nieuw materieel.<br \>Denk aan kosten voor onderhoudscontracten, huur of lease van apparatuur, etc.<br \>(EUR x 1000)"
            },
            {
                "Materiaal / verbruiksmiddelen",
                @"Vul in: extra kosten per jaar voor materiaal en verbruiksmiddelen.<br \>Denk aan grijpvoorraad, bedrijfsmiddelen, etc.<br \>(EUR x 1000)"
            },
            {
                "Personeel",
                @"Vul in: kosten per jaar van personeel dat nodig is om de nieuwe apparatuur aan te schaffen en draaiend te houden.<br \>Denk aan operating, beheer, contractmanagement, etc.<br \>(EUR x 1000)"
            },
            {
                "Energie",
                @"Vul in: energiekosten per jaar voor de nieuwe apparatuur.<br \>(EUR x 1000)"
            },
            {
                "Overig",
                @"Vul in: overige kosten per jaar veroorzaakt door de nieuwe apparatuur.<br \>(EUR x 1000)"
            },
            {
                "Vermeden kosten",
                @"Tussenresultaat: totaal aan vermeden kosten als gevolg van dit project, per jaar.<br \>(EUR x 1000)"
            },
            {
                "Materieel2",
                @"Vul in: vermeden kosten per jaar voor af te stoten materieel.<br \>Denk aan kosten voor onderhoudscontracten, huur of lease van apparatuur, etc.<br \>(EUR x 1000)"
            },
            {
                "Personeel2",
                @"Vul in: vermeden kosten per jaar van personeel dat niet meer nodig is doordat de nieuwe apparatuur wordt aangeschaft.<br \>Denk aan operating, beheer, contractmanagement, etc.<br \>(EUR x 1000)"
            },
            {
                "Energie2",
                @"Vul in: vermeden energiekosten per jaar voor af te stoten apparatuur.<br \>(EUR x 1000)"
            },
            {
                "Overig2",
                @"Vul in: overige vermeden kosten per jaar veroorzaakt door de aanschaf van nieuwe apparatuur.<br \>(EUR x 1000)"
            },
            {
                "Inkomsten",
                @"Tussenresultaat: saldo van extra en gederfde inkomsten als gevolg van dit project, per jaar.<br \>(EUR x 1000)"
            },
            {
                "Extra inkomsten",
                @"Vul in: extra inkomsten die gegenereerd worden als gevolg van dit project.<br \>(EUR x 1000)"
            },
            {
                "Gederfde inkomsten",
                @"Vul in: inkomsten die gederfd worden als gevolg van dit project.<br \>(EUR x 1000)"
            },
            {
                "Kasstroom",
                @"Resultaat: totale kasstroom per jaar = inkomsten - (investeringen + kosten).<br \>(EUR x 1000)"
            },
            {
                "Cumulatieve kasstroom",
                @"Resultaat: cumulatieve kasstroom van het begin van het project t/m dat jaar.<br \>(EUR x 1000)"
            },
            {
                "Verdisconteerde kasstroom",
                @"Resultaat: totale kasstroom per jaar, inclusief effect van de rentevoet.<br \>(EUR x 1000)"
            },
            {
                "Cumulatieve verdisconteerde kasstroom",
                @"Resultaat: cumulatieve kasstroom van het begin van het project t/m dat jaar, inclusief effect van de rentevoet.<br \>(EUR x 1000)"
            }          
        };

        public static string StartingYear = @"Het jaar waarop het project van start gaat.";
        public static string Horizon = @"Met horizon wordt hier de tijd bedoeld waarbinnen het project terugverdiend zou moeten zijn.<br \>Vervolgens wordt de terugverdientijd op basis van de ingevulde waarden berekend,<br \>en wordt gekeken of deze inderdaad binnen de horizon valt.";
        public static string Interest = @"De rentevoet wordt gebruikt om de bedragen netto contant te maken, en kan opgevat worden als een rendementseis voor de investering.<br \>De terugverdientijd wordt berekend inclusief dit geëiste rendement.​";
        public static string Payback = @"<div>Resultaat: terugverdientijd van het project in maanden.</div>
            <div>De terugverdientijd wordt berekend door de netto kasstroom bij elkaar op te tellen.</div>
            <div>De maand waarop de cumulatieve kasstroom 0 is sinds het begin van het project, bepaalt de terugverdientijd.</div>
            <div>&nbsp;</div>
            <div>Voorbeeld:</div>
            <div>In jaar 1 is de netto kasstroom -10.000 euro, in jaar 2 is de netto kasstroom +5.000 euro en in jaar 3 is de netto kasstroom +10.000 euro.</div>
            <div>Per jaar worden er 12 maanden gerekend dat betekent, halverwege jaar 3 is de nettokasstroom op 0, dus terugverdientijd is 30 maanden.</div>";

        public static string Residual = @"Vul in: eventuele restwaarde die de apparatuur heeft aan het eind van de horizon van de business case.<br \>(EUR x 1000)";
        public static string Risks = @"
            <p><em>Welke risico&rsquo;s kunnen jouw project be&iuml;nvloeden &ndash; en hoe ga je daarmee om?</em><br /><em>Denk actief na over mogelijke knelpunten in je project. Wat kan er misgaan? Wat zijn de gevolgen als dat gebeurt? <br />En wat kun je doen om die risico&rsquo;s te verkleinen of op te vangen?</em></p>
            <strong>Stap 1 &ndash; Benoem de risico&rsquo;s<br /></strong>Welke onzekerheden kunnen het succes van je project in gevaar brengen?<br /><strong>Bijvoorbeeld:</strong>
            <ul>
            <li>Technisch: onvoldoende data om AI goed te trainen.</li>
            <li>Organisatorisch: medewerkers gebruiken het nieuwe systeem niet.</li>
            <li>Financieel: opbrengsten komen later dan gepland.</li>
            <li>Afhankelijkheid: vertraging door externe leverancier.</li>
            </ul>
            <p><strong>Stap 2 &ndash; Denk na over de implicaties<br /></strong>Wat gebeurt er als dit risico werkelijkheid wordt? Denk aan: vertraging, hogere kosten, lagere impact of zelfs projectfalen.<strong><br /></strong></p>
            <p><strong>Stap 3 &ndash; Beschrijf mitigerende maatregelen<br /></strong>Wat kun je nu al doen om risico&rsquo;s te beperken of voor te bereiden op een plan B?<br /><strong>Bijvoorbeeld:</strong></p>
            <ul>
            <li>Een test- of pilotfase uitvoeren.</li>
            <li>Personeel trainen of meenemen in het proces.</li>
            <li>Duidelijke afspraken maken met leveranciers.</li>
            <li>Het project in fases uitvoeren.</li>
            </ul>
            <p><strong>Tip:</strong> Noem risico&rsquo;s die je echt moet bewaken en hoe je dit concreet aanpakt. <br />Dat maakt je businesscase sterker &eacute;n realistischer.</p>
        ";
    }

    public static class Step4
    {
        public static string Evaluation = @"
            <p><strong> Wat is je eindoordeel over dit project? <br /></strong> Gebruik dit veld om een weloverwogen oordeel te geven op basis van de financi&euml;le uitkomsten &eacute;n strategische relevantie.<br />Deze evaluatie helpt bij het nemen van een investeringsbeslissing,<br />intern overleg of voorbereiding op financieringsaanvragen.</p>
            <p><strong> Gebruik de volgende vragen als leidraad: </strong></p>
            <ol>
            <li>Is het project financieel aantrekkelijk?​​ <strong><br /></strong>
            <ul>
            <li>Valt de terugverdientijd binnen je beoogde horizon (bijvoorbeeld 3-5 jaar)?​</li>
            <li>Is het rendement (ROI) voldoende en concurrerend met andere opties?</li>
            <li>Zijn kosten, baten en kasstromen inzichtelijk en geloofwaardig?​</li>
            </ul>
            </li>
            <li>Draagt het project bij aan strategische doelen?​
            <ul>
            <li>Levert het meetbare voordelen op zoals tijdswinst, minder fouten, hogere klanttevredenheid of een betere werkomgeving?​</li>
            <li>Past het binnen de bredere digitaliseringsstrategie van je organisatie?​</li>
            </ul>
            </li>
            <li>Zijn de risico&rsquo;s acceptabel en voldoende gemitigeerd?​
            <ul>
            <li>Zijn de belangrijkste risico&rsquo;s benoemd?​</li>
            <li>Zijn er passende maatregelen genomen om ze te beperken?​</li>
            </ul>
            </li>
            <li>Wat is je samenvattende oordeel? <br />Formuleer een helder besluit: <br />
            <ul>
            <li>Positief: doorgaan met implementatie.</li>
            <li>Voorwaardelijk: alleen starten als specifieke risico&rsquo;s zijn afgedekt​.</li>
            <li>Negatief: te veel onzekerheid of onvoldoende rendement.</li>
            </ul>
            </li>
            </ol>
            <p><strong> Voorbeeld: <br />&ldquo;Het project biedt strategische meerwaarde en is financieel haalbaar. Wel starten met een pilot om risico&rsquo;s rond adoptie en datakwaliteit te beperken.&rdquo; <br /></strong></p>
            <p><strong> Tip: </strong> Houd het beknopt en feitelijk. Dit veld is bedoeld om je project af te ronden met een duidelijke conclusie en onderbouwing.</p>";

        public static string ROI = @"Rendement van de investering, berekend als (Totaal resultaat) / (Totaal investeringen + Totaal kosten).<br />
            Hierbij zijn de nominale (niet-verdisconteerde) bedragen gebruikt.<br />Het totaal resultaat is: totaal inkomsten - (totaal investeringen + totaal kosten)<br /><br />
            Voorbeeld:<br />Bij €80.000 opbrengsten, €20.000 kosten en €40.000 investeringen is de ROI:<br />(€80.000 – €20.000 – €40.000) ÷ €60.000 = 33,3%.
        ";

        public static string ROIInterest = @"Rendement van de investering, berekend als (Totaal resultaat) / (Totaal investeringen + totale kosten).<br />
            Hierbij zijn de bedragen verdisconteerd met de rentevoet ('netto-contant gemaakt').<br />
            Het totaal resultaat is: totaal inkomsten - (totaal investeringen + totaal kosten), alle getallen zijn verdisconteerd.<br /><br />
            Voorbeeld:<br />
            Bij €65.000 verdisconteerde opbrengsten, €15.000 verdisconteerde kosten en €35.000 NCW-investeringen is de ROI:<br />
            (€65.000 – €15.000 – €35.000) ÷ €50.000 = 30,0%
        ";
        
        public static string Value = @"Wat wil je bereiken met dit project? <br />
            Beschrijf het concrete doel, bijvoorbeeld tijdsbesparing of kwaliteitsverbetering.<br />
            Denk aan: ""Supportvraag terugbrengen naar 8u per week"" of ""Foutreductie van 30% in orderverwerking""";
        
        public static string TotalResult = @"Totaal van alle inkomsten verminderd met het totaal van de investeringen en het totaal van alle kosten.<br />
            Hierbij zijn de nominale (niet-verdisconteerde) bedragen gebruikt.<br /><br />
            Voorbeeld:<br />
            Opbrengsten €100.000 – investeringen €50.000 – kosten €20.000 = €30.000 resultaat.";

        public static string TotalResultInterest = @"Totaal van alle inkomsten verminderd met het totaal van de investeringen en het totaal van alle kosten.<br />
            Hierbij zijn de bedragen verdisconteerd met de rentevoet ('netto-contant gemaakt').<br /><br />
            Voorbeeld:<br />
            Verdisconteerde opbrengsten €83.000 – verdisconteerde investeringen €35.000 – verdisconteerde kosten €17.000 = €31.000 resultaat.";
        
        public static string TotalInvestment = @"Totale investeringen over de looptijd van het project.<br />
            Hierbij zijn de nominale (niet-verdisconteerde) bedragen gebruikt.<br /><br />
            Voorbeeld:<br />
            In jaar 1: €25.000, jaar 3: €15.000 → totaal €40.000 aan investeringen.";

        public static string TotalCost = @"Totale kosten over de looptijd van het project.<br />
            Hierbij zijn de nominale (niet-verdisconteerde) bedragen gebruikt.<br /><br />
            Voorbeeld:<br />
            Jaarlijks €5.000 aan onderhoud over 5 jaar = €25.000.";

        public static string TotalIncome = @"Totale inkomsten over de looptijd van het project.<br />
            Hierbij zijn de nominale (niet-verdisconteerde) bedragen gebruikt.<br /><br />
            Voorbeeld:<br />
            Jaarlijkse opbrengst van €15.000 × 5 jaar = €75.000.";

        public static string ChartHelpText = @"<p>Met deze grafiek zie je in &eacute;&eacute;n oogopslag wanneer je project winst begint op te leveren.</p>
            <ul>
            <li>De zwarte balkjes tonen per jaar de kasstroom: opbrengsten min kosten.</li>
            <li>De oranje lijn laat zien hoe deze bedragen zich opstapelen over de tijd (cumulatief).&nbsp;</li>
            <li>Voorbeeld: <br />jaar 1 &ndash;10.000, jaar 2 +5.000, jaar 3 +10.000 &rarr; in jaar 3 kom je boven nul.</li>
            </ul>";
    }
}
