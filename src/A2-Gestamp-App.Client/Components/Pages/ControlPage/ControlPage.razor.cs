using System.Diagnostics;

using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Domain.Features.Inspection.Entities;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.ControlPage;

public partial class ControlPage : IDisposable
{
  [Inject]
  private IAuthenticatedUserState UserState { get; set; } = default!;

  [Inject]
  private IInspectionState InspectionState { get; set; } = default!;

  [Inject]
  private NavigationManager Navigation { get; set; } = default!;

  [Inject]
  private IConfirmationDialogState ConfirmationDialog { get; set; } = default!;

  private Inspection? _inspection;

  private int _currentIndex;

  private CameraInspection? CurrentCamera =>
      _currentIndex switch
      {
        0 => _inspection?.Camera1,
        1 => _inspection?.Camera2,
        2 => _inspection?.Camera3,
        _ => null
      };

  private bool CanSave =>
      UserState.User is not null;

  private bool CanChangeJudgment =>
      UserState.User?.Role == UserRole.AuxiliaryManager;

  protected override void OnInitialized()
  {
    _inspection = InspectionState.CurrentInspection;

    InspectionState.InspectionChanged += OnInspectionChanged;
    UserState.StateChanged += OnUserChanged;
  }

  private void OnInspectionChanged()
  {
    _inspection = InspectionState.CurrentInspection;

    InvokeAsync(StateHasChanged);
  }

  private void OnUserChanged()
  {
    InvokeAsync(StateHasChanged);
  }

  private void LogoutOperator()
  {
    UserState.Clear();

    Navigation.NavigateTo("/");
  }

  private async Task SaveAsync()
  {
    // TODO: salvar inspeção

    LogoutOperator();

    await Task.CompletedTask;
  }

  private Task ChangeJudgmentAsync()
  {
    ConfirmationDialog.Open(
        title: "Alterar Julgamento",
        message:
            "Deseja realmente alterar o julgamento desta inspeção para APROVADA?",
        confirmText: "Confirmar",
        onConfirm: ConfirmChangeJudgmentAsync);

    return Task.CompletedTask;
  }

  private async Task ConfirmChangeJudgmentAsync()
  {
    _inspection?.Approve();

    if (_inspection is not null)
    {
      InspectionState.SetInspection(_inspection);
    }


    Debug.WriteLine(_inspection?.Result);

    LogoutOperator();

    await Task.CompletedTask;
  }

  public void Dispose()
  {
    InspectionState.InspectionChanged -= OnInspectionChanged;
    UserState.StateChanged -= OnUserChanged;
  }
}
