using A2GestampApp.Domain.Features.Inspection.Models;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage.Components;

public partial class Tools
{
  [Parameter]
  public CameraInspection? Camera { get; set; }
}
