using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies.ViewModels;

/// <summary>
/// ViewModel postinumero–toimipaikka -parin hallintaan ilman CommunityToolkit.MVVM‑kirjastoa.
/// </summary>
public class PostiViewModel : INotifyPropertyChanged
{
    private readonly IPostiDatabaseService _service;

    private string postinumero = string.Empty;
    public string Postinumero
    {
        get => postinumero;
        set
        {
            if (postinumero != value)
            {
                postinumero = value;
                OnPropertyChanged();
                RaiseCanExecuteChanged();
            }
        }
    }

    private string toimipaikka = string.Empty;
    public string Toimipaikka
    {
        get => toimipaikka;
        set
        {
            if (toimipaikka != value)
            {
                toimipaikka = value;
                OnPropertyChanged();
                RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TallennaCommand { get; }

    public PostiViewModel(IPostiDatabaseService service)
    {
        _service = service;
        TallennaCommand = new Command(async () => await TallennaAsync(), CanExecuteTallenna);
    }

    private bool CanExecuteTallenna()
        => !string.IsNullOrWhiteSpace(Postinumero) && !string.IsNullOrWhiteSpace(Toimipaikka);

    public async Task TallennaAsync()
    {
        var ok = await _service.LisaaTaiPaivitaAsync(new Posti
        {
            Postinumero = Postinumero.Trim(),
            Toimipaikka = Toimipaikka.Trim()
        });

        await Shell.Current.DisplayAlert(
            ok ? "Tallennettu" : "Virhe",
            ok ? "Postinumero lisättiin / päivitettiin." : "Tallennus epäonnistui.",
            "OK");
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RaiseCanExecuteChanged()
        => (TallennaCommand as Command)?.ChangeCanExecute();

    #endregion
}