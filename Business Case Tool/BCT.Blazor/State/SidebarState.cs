using BCT.Application.EventManagement;

namespace BCT.Blazor.State;

public class SidebarState : Observable<SidebarExtended> { };
public class SidebarExtended
{
    public bool Value { get; set; } = true;
}
