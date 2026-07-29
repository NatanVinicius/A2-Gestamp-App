using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage.Components;

public partial class Tools
{
  [Parameter]
  public CameraInspection? Camera { get; set; }
}
