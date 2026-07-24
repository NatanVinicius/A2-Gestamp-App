using Features.Inspection.Domain;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage.Components;



public partial class Inspection
{
  [Inject]
  public IInspectionService? InspectionService { get; set; } = default;



}
