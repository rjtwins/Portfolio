using Microsoft.AspNetCore.Components;
using Radzen;

namespace BCT.Blazor.Services;

public class HelpTextService : IHelpTextService
{
    private readonly TooltipService tooltipService;

    public HelpTextService(TooltipService tooltipService)
    {
        this.tooltipService = tooltipService;
    }

    public void ShowTooltip(ElementReference elementReference, TooltipOptions? options = null, string key = "", string text = "")
    {
        string tooltipText = string.Empty;
        if (!string.IsNullOrEmpty(text))
        {
            tooltipText = text;
        }
        else if(TryGetTextByKey(key, out string helpText))
        {
            tooltipText = helpText;
        }

        if (string.IsNullOrEmpty(tooltipText))
            return;

        tooltipService.Open(elementReference, tooltipText, options);
    }

    private bool TryGetTextByKey(string key, out string text)
    {
        text = string.Empty;

        if(string.IsNullOrEmpty(key))
            return false;

        if(HelpText.Step3.GridItems.TryGetValue(key, out text))
            return true;

        key = Domain.Configuration.Project.OverTimeValueWizardMap.FirstOrDefault(x => x.Value == key).Key;
        if (string.IsNullOrEmpty(key))
            return false;

        if (HelpText.Step3.GridItems.TryGetValue(key, out text))
            return true;

        return false;
    }

    public void CloseTooltip()
    {
        tooltipService.Close();
    }
}
