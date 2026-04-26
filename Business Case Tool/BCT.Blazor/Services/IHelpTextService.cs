using Microsoft.AspNetCore.Components;
using Radzen;

namespace BCT.Blazor.Services;
public interface IHelpTextService
{
    void ShowTooltip(ElementReference elementReference, TooltipOptions? options = null, string key = "", string text = "");
    void CloseTooltip();

    static TooltipOptions RightSide = new TooltipOptions() { Delay = PresentationSettings.HelpTextDelay, Duration = PresentationSettings.HelpTextDuration, Position = TooltipPosition.Top, CssClass = "rz-tooltip-rtl" };
    static TooltipOptions LeftSide = new TooltipOptions() { Delay = PresentationSettings.HelpTextDelay, Duration = PresentationSettings.HelpTextDuration, Position = TooltipPosition.Top, CssClass = "rz-tooltip-ltl" };
}