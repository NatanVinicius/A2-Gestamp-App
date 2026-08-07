using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Application.Features.Ng;
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
  private INgState NgState { get; set; } = default!;

  [Inject]
  private IInspectionReviewService InspectionReviewService { get; set; } = default!;

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

  private void ChangeUserLogged()
  {
    NgState.Open();
  }

  private async Task SaveAsync()
  {
    if (_inspection is null ||
        UserState.User is null)
    {
      return;
    }

    await InspectionReviewService.SaveReviewAsync(
        _inspection,
        UserState.User);

    LogoutOperator();
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
    if (_inspection is null ||
        UserState.User is null)
    {
      return;
    }

    await InspectionReviewService.ApproveAsync(
        _inspection,
        UserState.User);

    LogoutOperator();
  }

  public void Dispose()
  {
    InspectionState.InspectionChanged -= OnInspectionChanged;
    UserState.StateChanged -= OnUserChanged;
  }
}
