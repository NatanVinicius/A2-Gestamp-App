using A2GestampApp.Domain.Features.Inspection.Models;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage.Components;

public partial class Inspection
{
  [Parameter]
  public CameraInspection? Camera { get; set; }

  [Parameter]
  public int CameraCount { get; set; }

  [Parameter]
  public int CurrentIndex { get; set; }

  [Parameter]
  public EventCallback<int> CurrentIndexChanged { get; set; }

  private Task Next()
  {
    if (CameraCount <= 1)
    {
      return Task.CompletedTask;
    }

    return CurrentIndexChanged.InvokeAsync((CurrentIndex + 1) % CameraCount);
  }

  private Task Previous()
  {
    if (CameraCount <= 1)
    {
      return Task.CompletedTask;
    }

    var index = (CurrentIndex - 1 + CameraCount) % CameraCount;

    return CurrentIndexChanged.InvokeAsync(index);
  }

  private Task GoTo(int index)
  {
    return CurrentIndexChanged.InvokeAsync(index);
  }
}
