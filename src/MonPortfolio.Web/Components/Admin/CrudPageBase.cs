using Microsoft.AspNetCore.Components;
using MonPortfolio.Application.Common;

namespace MonPortfolio.Web.Components.Admin;

public sealed record CrudMessages(string Created, string Updated, string Deleted);

/// <summary>
/// Comportement commun des écrans CRUD de l'administration (liste, création, édition, suppression
/// confirmée, notifications). Les pages ne déclarent que leur service, leur mapping et leur balisage.
/// </summary>
public abstract class CrudPageBase<TDto, TInput> : ComponentBase
    where TDto : class
    where TInput : class, new()
{
    protected IReadOnlyList<TDto>? Items { get; private set; }
    protected TInput Input { get; set; } = new();
    protected Guid? EditingId { get; private set; }
    protected bool EditorOpen { get; private set; }
    protected bool Saving { get; private set; }
    protected bool Deleting { get; private set; }
    protected IReadOnlyList<string> SaveErrors { get; private set; } = [];
    protected TDto? PendingDelete { get; private set; }
    protected Toast? Toast { get; set; }

    protected bool IsEditing => EditingId is not null;

    protected abstract ICrudService<TDto, TInput> Service { get; }
    protected abstract CrudMessages Messages { get; }
    protected abstract Guid IdOf(TDto item);
    protected abstract TInput ToInput(TDto item);
    protected virtual TInput NewInput() => new();

    protected override Task OnInitializedAsync() => ReloadAsync();

    protected async Task ReloadAsync() => Items = await Service.GetAllAsync();

    protected void OpenCreate()
    {
        Input = NewInput();
        EditingId = null;
        SaveErrors = [];
        EditorOpen = true;
    }

    protected void OpenEdit(TDto item)
    {
        Input = ToInput(item);
        EditingId = IdOf(item);
        SaveErrors = [];
        EditorOpen = true;
    }

    /// <summary>Fermeture sans enregistrement (Annuler, Échap, clic hors de la modale).</summary>
    protected async Task CloseEditorAsync()
    {
        await OnEditorClosingAsync(saved: false);
        EditorOpen = false;
    }

    /// <summary>
    /// Appelé juste avant la fermeture de l'éditeur, après un enregistrement réussi ou une annulation :
    /// permet par exemple de supprimer une image téléversée puis abandonnée.
    /// </summary>
    protected virtual Task OnEditorClosingAsync(bool saved) => Task.CompletedTask;

    protected async Task SaveAsync()
    {
        if (Saving)
            return;

        Saving = true;
        SaveErrors = [];
        try
        {
            var wasEditing = IsEditing;
            Result result = EditingId is { } id
                ? await Service.UpdateAsync(id, Input)
                : await Service.CreateAsync(Input);

            if (result.IsFailure)
            {
                SaveErrors = result.Errors;
                return;
            }

            await OnEditorClosingAsync(saved: true);
            EditorOpen = false;
            await ReloadAsync();
            Toast?.Show(wasEditing ? Messages.Updated : Messages.Created);
        }
        finally
        {
            Saving = false;
        }
    }

    protected void AskDelete(TDto item) => PendingDelete = item;

    protected void CancelDelete() => PendingDelete = null;

    protected async Task ConfirmDeleteAsync()
    {
        if (PendingDelete is null || Deleting)
            return;

        Deleting = true;
        try
        {
            var result = await Service.DeleteAsync(IdOf(PendingDelete));
            PendingDelete = null;
            await ReloadAsync();

            if (result.IsSuccess)
                Toast?.Show(Messages.Deleted);
            else
                Toast?.Show(result.Error, success: false);
        }
        finally
        {
            Deleting = false;
        }
    }
}
